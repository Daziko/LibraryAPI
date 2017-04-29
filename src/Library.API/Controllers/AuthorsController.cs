using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository libaryRepository;

        public AuthorsController(ILibraryRepository libaryRepository)
        {
            this.libaryRepository = libaryRepository;
        }

        [HttpGet]      
        public IActionResult GetAuthors()
        {
            var authorsFromRepository = libaryRepository.GetAuthors();
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepository);

            return Ok(authors);

        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetAuthor(Guid id)
        {           
            var authorFromRepository = libaryRepository.GetAuthor(id);

            if (authorFromRepository == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorFromRepository);

            return new JsonResult(author);
        }
    }
}
