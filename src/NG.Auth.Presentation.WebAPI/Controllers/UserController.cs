using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NG.Auth.Business.Contract;
using NG.Auth.Domain;
using NG.Common.Library.Filters;
using System;
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
        /// <param name="userDto">The new User to be registered</param>
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
        [ProducesResponseType(typeof(AuthenticationResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Register(RegisterRequest userDto)
        {
            return Ok(await _userService.RegisterAsync(userDto));
        }

        /// <summary>
        /// Generate a token for the given User
        /// </summary>
        /// <param name="credentials">The user credentials to log in</param>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - Token succesfully created.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        [HttpPost("Authenticate")]
        [ProducesResponseType(typeof(ApiError), 543)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AuthenticationResponse), (int)HttpStatusCode.OK)]
        public IActionResult Authenticate(AuthenticationRequest credentials)
        {
            var authenticationResponse = _userService.Authenticate(credentials);

            // SetTokenCookie(authenticationResponse.RefreshToken);

            return Ok(authenticationResponse);
        }

        /// <summary>
        /// Give a new token
        /// </summary>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - Token succesfully created.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        [HttpPost("RefreshToken")]
        public IActionResult RefreshToken()
        {
            // var refreshToken = Request.Cookies["refreshToken"];
            var refreshToken = Request.Headers["refreshToken"];

            var authenticationResponse = _userService.RefreshToken(refreshToken);

            // SetTokenCookie(authenticationResponse.RefreshToken);

            return Ok(authenticationResponse);
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}
