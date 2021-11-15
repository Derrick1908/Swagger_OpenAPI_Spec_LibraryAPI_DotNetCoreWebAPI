using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Produces("application/json", "application/xml")]
    [Route("api/v{version:apiVersion}/authors")]
    //[Route("api/v2.0/authors")]                           //This was added at the Start when it was difficult to differentiate the Requests for the Swagger Doc API Sepcification
    [ApiController]
    [ApiVersion("2.0")]
    public class AuthorsControllerV2 : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsControllerV2(IAuthorRepository authorsRepository, IMapper mapper)
        {
            _authorsRepository = authorsRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets a List of Authors (V2)
        /// </summary>
        /// <returns>An Action Result of Type IEnumerable of Author</returns>
        /// <response code="200">Returns the List of Authors</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
            return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
        }
    }
}
