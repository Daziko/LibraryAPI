using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{AuthorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository libaryRepository;

        public BooksController(ILibraryRepository libaryRepository)
        {
            this.libaryRepository = libaryRepository;
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

            if (!libaryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = libaryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            Mapper.Map(book, bookFromRepo);

            libaryRepository.UpdateBookForAuthor(bookFromRepo);

            if (!libaryRepository.Save())
            {
                throw new Exception("DB Error");
            }

            return NoContent();
        }
    }
}
