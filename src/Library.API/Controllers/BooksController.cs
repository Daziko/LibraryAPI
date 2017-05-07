using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api/authors/{AuthorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository libaryRepository;
        private readonly ILogger<BooksController> logger;

        public BooksController(ILibraryRepository libaryRepository, ILogger<BooksController> logger)
        {
            this.libaryRepository = libaryRepository;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!libaryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo = libaryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            return Ok(booksForAuthor);
        }

        [HttpGet]
        [Route("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAutor(Guid authorId, Guid id)
        {
            if (!libaryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }            

            var bookFromRepo = libaryRepository.GetBookForAuthor(authorId, id);

            if (bookFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = Mapper.Map<BookDto>(bookFromRepo);

            return Ok(bookForAuthor);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto),"The provided description shouldn't be different from the title");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var bookEntity = Mapper.Map<Book>(book);

            if (!libaryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromCreation = Mapper.Map<Book>(bookEntity);

            libaryRepository.AddBookForAuthor(authorId, bookFromCreation);

            if (!libaryRepository.Save())
            {
                throw new Exception("DB Error");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id}, bookToReturn);
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!libaryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = libaryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            libaryRepository.DeleteBook(bookFromRepo);

            if (!libaryRepository.Save())
            {
                throw new Exception("DB Error");
            }

            logger.LogInformation(100, $"Book {id} for author {authorId} was deleted");
            return NoContent();
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description shouldn't be different from the title");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!libaryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = libaryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;

                libaryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!libaryRepository.Save())
                {
                    throw new Exception("DB Error");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn .Id} , bookToReturn);
            }

            Mapper.Map(book, bookFromRepo);

            libaryRepository.UpdateBookForAuthor(bookFromRepo);

            if (!libaryRepository.Save())
            {
                throw new Exception("DB Error");
            }

            return NoContent();
        }

        [HttpPatch]
        [Route("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!libaryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = libaryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto, ModelState);

                if (bookDto.Description == bookDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description shouldn't be different from the title");
                }

                TryValidateModel(bookDto);

                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                libaryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!libaryRepository.Save())
                {
                    throw new Exception("DB Error");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);

            }

            var bookTopatch = Mapper.Map<BookForUpdateDto>(bookFromRepo);

            patchDoc.ApplyTo(bookTopatch, ModelState);

            if (bookTopatch.Description == bookTopatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description shouldn't be different from the title");
            }

            TryValidateModel(bookTopatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            Mapper.Map(bookTopatch, bookFromRepo);

            libaryRepository.UpdateBookForAuthor(bookFromRepo);

            if (!libaryRepository.Save())
            {
                throw new Exception("DB Error");
            }

            return NoContent();
        }
    }
}
