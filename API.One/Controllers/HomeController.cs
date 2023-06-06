using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.One.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        using (_logger.BeginScope("Processing {path} with id = {id} ", "Home/Get", id))
        {
            _logger.LogInformation("Starting Get action");

            if (id == 0)
                throw new Exception("Invalid Id");

            _logger.LogInformation("Ending Get action");

            return Ok(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}
