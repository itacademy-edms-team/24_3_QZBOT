using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security;
using System.Security.Claims;
using System.Text.Json;
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
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || dto.SelectedOptionIndexes == null || dto.SelectedOptionIndexes.Count == 0)
                return BadRequest("Неверный запрос.");

            var question = _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Test)
                .FirstOrDefault(q =>
                    q.Id == dto.QuestionId &&
                    q.Test.Title.ToLower() == dto.Title.ToLower());

            if (question == null)
                return NotFound("Вопрос или тест не найден.");

            var selectedIndexes = dto.SelectedOptionIndexes.Distinct().ToList();

            if (selectedIndexes.Any(i => i < 0 || i >= question.Options.Count))
                return BadRequest("Некорректный индекс варианта.");


            var result = selectedIndexes
                .Select(i => question.Options[i].IsCorrect)
                .ToList();

            return Ok(result);
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

            return Ok(true);
        }

        [Authorize]
        [HttpPost("start/{testId}")]
        public async Task<IActionResult> StartTest(int testId) 
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

            var attempt = await _context.UserTests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(ut => ut.TestId == testId && ut.UserId == userId && ut.IsFinished == false); 
            
            if (attempt == null) 
            { 
                attempt = new UserTest 
                { 
                    UserId = userId, 
                    TestId = testId, 
                    StartedAt = DateTime.UtcNow,
                    IsFinished = false 
                }; 
                
                _context.UserTests.Add(attempt); 
                await _context.SaveChangesAsync(); 
            } 
            
            return Ok(UserTestDto.MapToDto(attempt)); 
        }

        [Authorize]
        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var userTest = await _context.UserTests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t =>
                    t.Id == dto.UserTestId &&
                    t.UserId == userId &&
                    t.IsFinished == false);

            if (userTest == null)
                return NotFound("Попытка не найдена или уже завершена");

            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q =>
                    q.Id == dto.QuestionId &&
                    q.TestId == userTest.TestId);

            if (question == null)
                return BadRequest("Вопрос не принадлежит этому тесту");

            if (userTest.Answers.Any(a => a.QuestionId == dto.QuestionId))
                return BadRequest("На этот вопрос уже был дан ответ");

            var validOptionIds = question.Options.Select(o => o.Id).ToHashSet();

            if (dto.SelectedOptionIds.Any(id => !validOptionIds.Contains(id)))
                return BadRequest("Один или несколько вариантов не принадлежат этому вопросу");

            var correctIds = question.Options
                .Where(o => o.IsCorrect)
                .Select(o => o.Id)
                .ToHashSet();

            bool isCorrect =
                dto.SelectedOptionIds.Count == correctIds.Count &&
                dto.SelectedOptionIds.All(id => correctIds.Contains(id));

            var answer = new UserTestAnswer
            {
                UserTestId = userTest.Id,
                QuestionId = dto.QuestionId,
                SelectedOptionsJson = JsonSerializer.Serialize(dto.SelectedOptionIds),
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.UtcNow
            };

            _context.UserTestAnswers.Add(answer);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                isCorrect,
                answeredQuestions = userTest.Answers.Count + 1
            });
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
            bool isPassed = (score / totalQuestions) * 100 >= test.MinSuccessPercent;


            // здесь костыль со временем, чтобы не делать миграцию для возврата предыдущей модели.

            // isFinished должен показывать закончил ли пользователь проходить тест, а сейчас
            // он показывает прошел ли (ответил правильно),
            // потому что о попытке прохождения говорит сама запись в UserTest

            var entity = new UserTest
            {
                UserId = userId,
                TestId = testId,
                Score = score,
                StartedAt = DateTime.UtcNow,
                FinishedAt = DateTime.UtcNow,
                IsFinished = isPassed
            };

            _context.UserTests.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(true);
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