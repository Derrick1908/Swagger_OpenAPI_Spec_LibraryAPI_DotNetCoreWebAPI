using AutoMapper;
using Library.API.Attributes;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Produces("application/json","application/xml")]        //This Overwrites any Defaults set at the Startup Level.
    [Route("api/authors/{authorId}/books")]
    [ApiController]   
    public class BooksController : ControllerBase
    { 
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }
       
        /// <summary>
        /// Gets the Books for a Specific Author
        /// </summary>
        /// <param name="authorId">The Id of the Book Author</param>
        /// <returns>An ActionResult of Type IEnumerable of Book</returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
        Guid authorId )
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId); 
            return Ok(_mapper.Map<IEnumerable<Book>>(booksFromRepo));
        }

        /// <summary>
        /// Get a Book by Id for a Specific Author
        /// </summary>
        /// <param name="authorId">The Id of the Book Author</param>
        /// <param name="bookId">The Id of the Book</param>
        /// <returns>An Action Result of Type Book</returns>
        /// <response code="200">Returns the Requested Book</response>
        [HttpGet("{bookId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.marvin.book+json")]      //This Overrides any Deafult Levels set either at the Controller Level or at the Defautls Statup Level
        [RequestHeaderMatchesMediaType(HeaderNames.Accept,
            "application/json",
            "application/vnd.marvin.books+json")]
        public async Task<ActionResult<Book>> GetBook(
            Guid authorId,
            Guid bookId)
        {
            if (! await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Book>(bookFromRepo));
        }


        /// <summary>
        /// Creates a Book for a Specific Author
        /// </summary>
        /// <param name="authorId">The Id of the Author Book</param>
        /// <param name="bookForCreation">The Book to Create</param>
        /// <returns>An Action Result of Type Book</returns>
        /// <response code="422">Validation Error</response>
        [HttpPost()]
        [Consumes("application/json", "application/vnd.marvin.book+json")]
        [RequestHeaderMatchesMediaType(HeaderNames.ContentType,
           "application/json", "application/vnd.marvin.bookforcreation+json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary),StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Book>> CreateBook(
            Guid authorId,
            [FromBody] BookForCreation bookForCreation)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }
    }
}
