using System;
using System.Collections.Generic;
using AutoMapper;
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
        [Route("{id}")]
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
    }
}
