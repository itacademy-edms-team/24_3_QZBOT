using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTests.Data;
using WebTests.DTOs;

namespace WebTests.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public IActionResult GetAllTests()
        {
            var tests = _context.Tests.ToList();
            return Ok(tests);
        }

        [HttpGet("{title}")]
        public IActionResult GetTest(string title)
        {
            var questions = _context.Tests
                .Where(t => t.Title == title)
                .SelectMany(t => t.Questions)
                .Include(q => q.Options)
                .ToList();

            return Ok(questions);
        }

        [HttpGet("exist/{title}")]
        public IActionResult CheckTestExists(string title)
        {
            var test = _context.Tests.FirstOrDefault(t => t.Title == title);

            if (test == null)
            {
                return Ok(false);
            } 
            else
            {
                return Ok(true);
            }
        }

        [HttpPost("check")]
        public IActionResult CheckAnswer([FromBody] AnswerCheckDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Неверный запрос.");

            var question = _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Test)
                .FirstOrDefault(q =>
                    q.Id == dto.QuestionId &&
                    q.Test.Title.ToLower() == dto.Title.ToLower());

            if (question == null)
                return NotFound("Вопрос или тест не найден.");

            if (dto.SelectedOptionIndex < 0 || dto.SelectedOptionIndex >= question.Options.Count)
                return BadRequest("Некорректный индекс варианта.");

            var selectedOption = question.Options[dto.SelectedOptionIndex];
            bool isCorrect = selectedOption.IsCorrect;

            return Ok( isCorrect );
        }
    }
}