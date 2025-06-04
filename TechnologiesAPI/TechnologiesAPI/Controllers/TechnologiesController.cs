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

        [HttpPost("{questionSlug}/check")]
        public async Task<IActionResult> CheckAnswer(string questionSlug, [FromBody] string userAnswer)
        {
            return await _service.CheckAnswer(questionSlug, userAnswer);
        }

        [HttpGet("{technologyName}")]
        public async Task<IActionResult> GetAllQuestions(string technologyName)
        {
            return await _service.GetAllQuestions(technologyName);
        }
    }
}