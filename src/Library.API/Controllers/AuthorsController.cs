using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository libaryRepository;
        private readonly IUrlHelper urlHelper;
        private readonly IPropertyMappingService propertyMappingService;
        private readonly ITypeHelperService typeHelperService;

        public AuthorsController(
            ILibraryRepository libaryRepository, 
            IUrlHelper urlHelper, 
            IPropertyMappingService propertyMappingService, 
            ITypeHelperService typeHelperService)
        {
            this.libaryRepository = libaryRepository;
            this.urlHelper = urlHelper;
            this.propertyMappingService = propertyMappingService;
            this.typeHelperService = typeHelperService;
        }

        [HttpGet(Name = "GetAuthors")]     
        public IActionResult GetAuthors(AuthorsResourceParameters authorsQueryParameters)
        {
            if (!propertyMappingService.ValidMappingExistFor<AuthorDto, Author>(authorsQueryParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!typeHelperService.TypeHasProperties<AuthorDto>(authorsQueryParameters.Fields))
            {
                return BadRequest();
            }  
                 
            var authorsFromRepository = libaryRepository.GetAuthors(authorsQueryParameters);

            var previousLink = authorsFromRepository.HasPrevious
                ? CreateAuthorsResourceUri(authorsQueryParameters, ResourceUriType.PreviousPage)
                : null;

            var nextLink = authorsFromRepository.HasNext
                ? CreateAuthorsResourceUri(authorsQueryParameters, ResourceUriType.NextPage)
                : null;

            var paginationMetadata = new
            {               
                totalCount = authorsFromRepository.TotalCount,
                pageSize = authorsFromRepository.PageSize,
                currentPage = authorsFromRepository.CurrentPage,
                totalPages = authorsFromRepository.TotalPages,
                previousPageLink = previousLink,
                nextPagelink = nextLink
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepository);

            return Ok(authors.ShapeData(authorsQueryParameters.Fields));
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage :
                    return urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = resourceParameters.Fields,
                            orderBy = resourceParameters.OrderBy,
                            searchQuery = resourceParameters.SearchQuery,
                            genre = resourceParameters.Genre,
                            pageNumber = resourceParameters.PageNumber - 1,
                            pageSize = resourceParameters.PageSize
                        });
                case ResourceUriType.NextPage :
                    return urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = resourceParameters.Fields,
                            orderBy = resourceParameters.OrderBy,
                            searchQuery = resourceParameters.SearchQuery,
                            genre = resourceParameters.Genre,
                            pageNumber = resourceParameters.PageNumber + 1,
                            pageSize = resourceParameters.PageSize
                        });
                default:
                    return urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = resourceParameters.Fields,
                            orderBy = resourceParameters.OrderBy,
                            searchQuery = resourceParameters.SearchQuery,
                            genre = resourceParameters.Genre,
                            pageNumber = resourceParameters.PageNumber,
                            pageSize = resourceParameters.PageSize
                        });
            }
        }

        [HttpGet]
        [Route("{id}", Name = "GetAuthor")]
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
        [HttpPost]
        public IActionResult CrteateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);

            libaryRepository.AddAuthor(authorEntity);
            if (!libaryRepository.Save())
            {
                throw new Exception("DB Error");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new {id = authorToReturn.Id}, authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (libaryRepository.AuthorExists(id))
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepository = libaryRepository.GetAuthor(id);
            if (authorFromRepository == null)
            {
                return NotFound();
            }

            libaryRepository.DeleteAuthor(authorFromRepository);

            if (!libaryRepository.Save())
            {
                throw new Exception("DB Error");
            }

            return NoContent();
        }
    }
}
