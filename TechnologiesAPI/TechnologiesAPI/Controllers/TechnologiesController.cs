using Microsoft.AspNetCore.Mvc;
using TechnologiesAPI.DTOs;

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

        [HttpPost]
        public async Task<IActionResult> AddTechnology([FromBody] TechnologyDto dto)
        {
            return await _service.AddTechnology(dto);
        }

        [HttpGet("{technologyName}")]
        public async Task<IActionResult> GetAllQuestions(string technologyName)
        {
            return await _service.GetAllQuestions(technologyName);
        }
    }
}