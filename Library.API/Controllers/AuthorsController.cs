﻿using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Library.API.Controllers
{

    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsController(
            IAuthorRepository authorsRepository,
            IMapper mapper)
        {
            _authorsRepository = authorsRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
            return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
        }

        /// <summary>
        /// Gets an Author by his/her Id
        /// </summary>
        /// <param name="authorId">The Id of the Author you want to Retrieve</param>
        /// <returns>An ActionResult of Type Author</returns>
        /// Old return Comment: An Author with Id, FirstName and LastName Fields
        [HttpGet("{authorId}")]
        public async Task<ActionResult<Author>> GetAuthor(
            Guid authorId)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Author>(authorFromRepo));
        }

        [HttpPut("{authorId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Author>> UpdateAuthor(
            Guid authorId,
            AuthorForUpdate authorForUpdate)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(authorForUpdate, authorFromRepo);

            //// update & save
            _authorsRepository.UpdateAuthor(authorFromRepo);
            await _authorsRepository.SaveChangesAsync();

            // return the author
            return Ok(_mapper.Map<Author>(authorFromRepo)); 
        }

        /// <summary>
        /// Partially Updates an Author
        /// </summary>
        /// <param name="authorId">The Id of the Author you want to retrieve</param>
        /// <param name="patchDocument">The set of Operations you want to apply to the Author</param>
        /// <returns>An ActionResult of type Author</returns>
        /// <remarks>
        /// Sample Request (this request updates the Author's First Name) \
        /// PATCH /authors/id      \
        /// [   \
        ///     {   \
        ///       "op": "replace ", \
        ///       "path": "/firstname", \
        ///       "value": "new First Name" \
        ///     }   \
        /// ]   
        /// </remarks>
        /// \ -> for New Line
        [HttpPatch("{authorId}")]
        public async Task<ActionResult<Author>> UpdateAuthor(
            Guid authorId,
            JsonPatchDocument<AuthorForUpdate> patchDocument)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            // map to DTO to apply the patch to
            var author = _mapper.Map<AuthorForUpdate>(authorFromRepo);
            patchDocument.ApplyTo(author, ModelState);

            // if there are errors when applying the patch the patch doc 
            // was badly formed  These aren't caught via the ApiController
            // validation, so we must manually check the modelstate and
            // potentially return these errors.
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // map the applied changes on the DTO back into the entity
            _mapper.Map(author, authorFromRepo);

            // update & save
            _authorsRepository.UpdateAuthor(authorFromRepo);
            await _authorsRepository.SaveChangesAsync();

            // return the author
            return Ok(_mapper.Map<Author>(authorFromRepo));
        }
    }
}
