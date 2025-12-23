using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using WebTests.Data;
using WebTests.DTOs;
using WebTests.Models;

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
            var tests = _context.Tests
                .Include(q => q.Questions)
                .ThenInclude(o => o.Options)
                .ToList();

            return Ok(tests);
        }

        [HttpGet("my")]
        [Authorize]
        public IActionResult GetMyTests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tests = _context.Tests
                .Where(t => t.CreatorId == userId)
                .ToList();

            return Ok(tests);
        }

        [HttpGet("published")]
        public IActionResult GetPublishedTests()
        {
            var tests = _context.Tests
                .Where(t => t.Published == true)
                .Select(t => new
                {
                    t.Id,
                    t.Title
                })
                .ToList();

            return Ok(tests);
        }

        [Authorize]
        [HttpGet("passed")]
        public async Task<IActionResult> GetPassedTests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tests = await _context.UserTests
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Test)
                .ToListAsync();

            return Ok(tests);
        }

        [HttpGet("{title}")]
        public IActionResult GetTestByTitle(string title)
        {
            var questions = _context.Tests
                .Where(t => t.Title == title)
                .Include(t => t.Types)
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault();

            return Ok(questions);
        }

        [HttpGet("id/{id}")]
        public IActionResult GetTestById(int id)
        {
            var questions = _context.Tests
                .Where(t => t.Id == id)
                .Include(t => t.Types)
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault();

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

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddTest([FromBody] TestDto dto)
        {
            if (dto == null)
                return BadRequest("DTO is null");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required");

            if (dto.Questions == null || dto.Questions.Count == 0)
                return BadRequest("Questions are required");


            if (!User.Identity!.IsAuthenticated)
                return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var test = TestFactory.FromDto.Create(dto);

            test.CreatorId = userId;
            test.CreatedDate = DateTime.UtcNow;

            if (!dto.Published)
                test.PublishDate = null;
            else
                test.PublishDate = DateTime.UtcNow;

            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTestById), new { id = test.Id }, test.Id);
        }

        [Authorize]
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditTest(int id, [FromBody] TestDto updated)
        {
            if (updated == null)
                return BadRequest("DTO is null");

            if (string.IsNullOrEmpty(updated.Title))
                return BadRequest("Title is required");

            if (updated.Questions == null || !updated.Questions.Any())
                return BadRequest("Questions are required");

            if (updated.Questions.Any(q => q.Options == null || !q.Options.Any()))
                return BadRequest("Each question must have options");


            var test = await _context.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
                return NotFound();


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            if (userId != test.CreatorId)
                return Forbid();


            TestFactory.FromDto.Update(test, updated);


            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPost("pass/{testId}")]
        public async Task<IActionResult> PassTest(int testId, [FromBody] int score)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var testExists = await _context.Tests.AnyAsync(t => t.Id == testId);
            if (!testExists)
                return NotFound("Тест не найден");

            var test = await _context.Tests
                .Where(t => t.Id == testId)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();

            var existing = await _context.UserTests
                .FirstOrDefaultAsync(ut => ut.TestId == testId && ut.UserId == userId);

            if (existing != null)
                return BadRequest("Уже была попытка прохождения теста");

            int totalQuestions = test.Questions.Count;
            bool isPassed = score >= totalQuestions / 2.0;

            var entity = new UserTest
            {
                UserId = userId,
                TestId = testId,
                Score = score,
                IsPassed = isPassed
            };

            _context.UserTests.Add(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet("isPassed/{testId}")]
        public async Task<IActionResult> IsTestPassed(int testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var test = await _context.Tests
                .FirstOrDefaultAsync(t => t.Id == testId);
            
            if (test == null)
                return NotFound();

            var record = await _context.UserTests
                .Where(ut => ut.TestId == testId && ut.UserId == userId)
                .Include(t => t.Test)
                .FirstOrDefaultAsync();

            return Ok(record);
        }
    }
}