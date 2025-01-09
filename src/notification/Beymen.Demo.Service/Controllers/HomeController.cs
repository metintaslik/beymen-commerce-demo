using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beymen.Demo.Service.Controllers;

[Route("/")]
[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return StatusCode((int)HttpStatusCode.OK, "Beymen.Demo.Service is running.");
    }
}
