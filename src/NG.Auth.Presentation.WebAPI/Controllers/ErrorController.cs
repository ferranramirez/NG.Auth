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
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult ErrorView()
        {
            return View("Error");
        }
    }
}
