using Microsoft.AspNetCore.Mvc;

namespace TGBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnologiesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var data = new TechnologiesClass
            {
                Technologies = new List<string> { "python", "java" }
            };

            return Ok(data);
        }
    }
}
