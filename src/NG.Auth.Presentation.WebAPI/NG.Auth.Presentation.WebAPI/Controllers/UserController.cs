using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NG.Auth.Business.Contract;
using NG.Auth.Domain;
using NG.Common.Library.Filters;
using NG.DBManager.Infrastructure.Contracts.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NG.Auth.Presentation.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new User
        /// </summary>
        /// <param name="userToRegister">The new User to be registered</param>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - User successfully created.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        [HttpPost("Register")]
        [ProducesResponseType(typeof(ApiError), 543)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Register(User user, [FromQuery] UserToRegister userToRegister)
        {
            var response = await _userService.RegisterAsync(user);
            return Ok(user);
        }


        /// <summary>
        /// Generate a token for the given User
        /// </summary>
        /// <param name="Credentials">The user credentials to log in</param>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - Token succesfully created.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        /// <returns></returns>
        [HttpPost("Login")]
        [ProducesResponseType(typeof(ApiError), 543)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public IActionResult Login(Credentials credentials)
        {
            string token = _userService.Authenticate(credentials);

            return Ok(token);
        }
    }
}
