using Microsoft.AspNetCore.Mvc;

namespace TechnologiesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnologiesController : ControllerBase
    {
        private readonly TechnologyService _service;

        public TechnologiesController(TechnologyService service)
        {
            _service = service;
        }

        [HttpPost("{technologyName}/{questionSlug}/check")]
        public IActionResult CheckAnswer(string technologyName, string questionSlug, [FromBody] string userAnswer)
        {
            return _service.CheckAnswer(technologyName, questionSlug, userAnswer);
        }

        [HttpGet("{technologyName}")]
        public IActionResult GetAllQuestions(string technologyName)
        {
            return _service.GetAllQuestions(technologyName);
        }
    }
}