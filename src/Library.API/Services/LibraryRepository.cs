﻿using Library.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Helpers;
using Library.API.Models;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly LibraryContext _context;
        private readonly IPropertyMappingService mappingService;

        public LibraryRepository(LibraryContext context, IPropertyMappingService mappingService)
        {
            _context = context;
            this.mappingService = mappingService;
        }

        public void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();
            _context.Authors.Add(author);

            // the repository fills the id (instead of using identity columns)
            if (author.Books.Any())
            {
                foreach (var book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }
        }

        public void AddBookForAuthor(Guid authorId, Book book)
        {
            var author = GetAuthor(authorId);
            if (author != null)
            {
                // if there isn't an id filled out (ie: we're not upserting),
                // we should generate one
                if (book.Id == null)
                {
                    book.Id = Guid.NewGuid();
                }
                author.Books.Add(book);
            }
        }

        public bool AuthorExists(Guid authorId)
        {
            return _context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            _context.Authors.Remove(author);
        }

        public void DeleteBook(Book book)
        {
            _context.Books.Remove(book);
        }

        public Author GetAuthor(Guid authorId)
        {
            return _context.Authors.FirstOrDefault(a => a.Id == authorId);
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters resourceParameters)
        {
            //var collectionBeforePaging = _context.Authors
            //                            .OrderBy(a => a.FirstName)
            //                            .ThenBy(a => a.LastName)
            //                            .AsQueryable();

            var collectionBeforePaging = _context.Authors
                .ApplySort(resourceParameters.OrderBy, mappingService.GetPropertyMapping<AuthorDto, Author>());

            if (!string.IsNullOrEmpty(resourceParameters.Genre))
            {
                var genreForWhereClause = resourceParameters.Genre.Trim().ToLowerInvariant();
                collectionBeforePaging =
                    collectionBeforePaging.Where(c => c.Genre.ToLowerInvariant() == genreForWhereClause);
            }

            if (!string.IsNullOrEmpty(resourceParameters.SearchQuery))
            {
                var searchForWhereClause = resourceParameters.SearchQuery.Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging.Where(
                    c => c.Genre.ToLowerInvariant().Contains(searchForWhereClause)
                    || c.FirstName.ToLowerInvariant().Contains(searchForWhereClause)
                    || c.LastName.ToLowerInvariant().Contains(searchForWhereClause));
            }
            return PagedList<Author>.Create(collectionBeforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return _context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .ToList();
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public Book GetBookForAuthor(Guid authorId, Guid bookId)
        {
            return _context.Books
              .Where(b => b.AuthorId == authorId && b.Id == bookId).FirstOrDefault();
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
        {
            return _context.Books
                        .Where(b => b.AuthorId == authorId).OrderBy(b => b.Title).ToList();
        }

        public void UpdateBookForAuthor(Book book)
        {
            // no code in this implementation
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}
