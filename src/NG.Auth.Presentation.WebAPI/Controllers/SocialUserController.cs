using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NG.Auth.Business.Contract;
using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using NG.Common.Library.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NG.Auth.Presentation.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SocialUserController : Controller
    {
        private readonly ISocialUserService _SocialUserService;
        private readonly ILogger<SocialUserController> _logger;
        private readonly IConfiguration _configuration;

        public SocialUserController(
            ISocialUserService SocialUserService,
            ILogger<SocialUserController> logger,
            IConfiguration configuration)
        {
            _SocialUserService = SocialUserService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Create a new SocialUser
        /// </summary>
        /// <param name="SocialUserDto">The new SocialUser to be registered</param>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - SocialUser successfully created.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        [HttpPost("Register")]
        [ProducesResponseType(typeof(ApiError), 543)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AuthenticationResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Register(SocialRegisterRequest SocialUserDto)
        {
            return Ok(await _SocialUserService.RegisterAsync(SocialUserDto));
        }
    }
}
