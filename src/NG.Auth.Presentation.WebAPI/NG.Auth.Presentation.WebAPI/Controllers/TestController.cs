using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NG.Auth.Business.Contract;

namespace NG.Auth.Presentation.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IUserService userService,
            ILogger<TestController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Everyone()
        {
            _logger.LogInformation("Everyone can access here!");
            return Ok("Everyone can access here!!");
        }

        [Authorize]
        [HttpGet("BasicUser")]
        public ActionResult BasicUser()
        {
            return Ok("You are an authorised user");
        }

        [Authorize(Roles = "Premium")]
        [HttpGet("Premium")]
        public ActionResult Premium()
        {
            return Ok("You are a PREMIUM user");
        }
    }
}
