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
using WebTests.TestFactory;

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
                .Where(t => t.CreatorId == userId && t.isDeleted == false)
                .ToList();

            return Ok(tests);
        }

        [HttpGet("published")]
        public IActionResult GetPublishedTests()
        {
            var tests = _context.Tests
                .Where(t => t.Published == true && t.isDeleted == false)
                .Include(t => t.Types)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Questions,
                    Types = t.Types.Select(tt => tt.Name).ToList(),
                    t.CreatorId,
                    t.Creator,
                    t.Published,
                    t.CreatedDate,
                    t.PublishDate,
                    t.EditTime,
                    t.MinSuccessPercent
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
                .Where(ut => ut.UserId == userId && ut.IsFinished == true)
                .Include(ut => ut.Test)
                    .ThenInclude(q => q.Questions)
                .ToListAsync();

            return Ok(tests);
        }

        [HttpGet("{title}")]
        public IActionResult GetTestByTitle(string title)
        {
            var test = _context.Tests
                .Where(t => t.Title == title && t.isDeleted == false)
                .Include(t => t.Types)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefault();

            if (test == null)
                return NotFound();

            var dto = new TestReadDto
            {
                Id = test.Id,
                Title = test.Title,
                Published = test.Published,
                CreatorId = test.CreatorId,
                MinimumSuccessPercent = test.MinSuccessPercent,
                Types = test.Types.Select(t => t.Name).ToList(),
                Questions = test.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    isMultiple = q.IsMultiple,
                    Options = q.Options.Select(o => new AnswerOptionDto
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("id/{id}")]
        public IActionResult GetTestById(int id)
        {
            var test = _context.Tests
                .Where(t => t.Id == id && t.isDeleted == false)
                .Include(t => t.Types)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefault(t => t.Id == id);

            if (test == null)
                return NotFound();

            var dto = new TestReadDto
            {
                Id = test.Id,
                Title = test.Title,
                Published = test.Published,
                CreatorId = test.CreatorId,
                MinimumSuccessPercent = test.MinSuccessPercent,
                Types = test.Types.Select(t => t.Name).ToList(),
                Questions = test.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    isMultiple = q.IsMultiple,
                    Options = q.Options.Select(o => new AnswerOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("exist/{title}")]
        public IActionResult CheckTestExists(string title)
        {
            var test = _context.Tests.FirstOrDefault(t => t.Title == title && t.isDeleted == false);

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

            var test = TestFactory.FromDto.Create(dto, _context);

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
                .Where(t => t.isDeleted == false)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .Include(t => t.Types)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
                return NotFound();


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            if (userId != test.CreatorId)
                return Forbid();


            TestFactory.FromDto.Update(test, updated, _context);


            await _context.SaveChangesAsync();

            return Ok(true);
        }

        [Authorize]
        [HttpPost("{testId}/checkstart")]
        public async Task<IActionResult> CheckStart(int testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var test = await _context.Tests
                .Where(t => t.isDeleted == false)
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
                return NotFound();

            var activeAttempt = await _context.UserTests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.TestId == testId && t.UserId == userId && !t.IsFinished);

            var finishedAttempt = await _context.UserTests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.TestId == testId && t.UserId == userId && t.IsFinished);

            if (activeAttempt == null && finishedAttempt == null)
                return Ok(true);

            return Ok(false);
        }

        [Authorize]
        [HttpPost("{testId}/start")]
        public async Task<IActionResult> StartTest(int testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var test = await _context.Tests
                .Where(t => t.isDeleted == false)
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
                return NotFound();


            var activeAttempt = await _context.UserTests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.TestId == testId && t.UserId == userId && !t.IsFinished);

            if (activeAttempt != null)
            {
                return Ok(new UserTestDto
                {
                    Status = "Active",
                    UserTestId = activeAttempt.Id,
                    StartedAt = activeAttempt.StartedAt,
                    Answers = activeAttempt.Answers.Select(a => new UserAnswerDto
                    {
                        QuestionId = a.QuestionId,
                        SelectedOptionIds = JsonSerializer.Deserialize<List<int>>(a.SelectedOptionsJson)!
                    }).ToList()
                });
            }

            var finishedAttempt = await _context.UserTests
                .Where(t => t.TestId == testId && t.UserId == userId && t.IsFinished)
                .OrderByDescending(t => t.FinishedAt)
                .FirstOrDefaultAsync();

            if (finishedAttempt != null)
            {
                return Ok(new UserTestDto // было StartTestResponseDto
                {
                    Status = "Finished"
                });
            }

            var newAttempt = new UserTest
            {
                UserId = userId,
                TestId = testId,
                StartedAt = DateTime.UtcNow,
                IsFinished = false,
            };

            _context.UserTests.Add(newAttempt);
            await _context.SaveChangesAsync();

            return Ok(new UserTestDto
            {
                Status = "New",
                UserTestId = newAttempt.Id,
                StartedAt = newAttempt.StartedAt,
                Answers = new List<UserAnswerDto>()
            });
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

            var validOptionIds = question.Options.Select(o => o.Id).ToHashSet();

            if (dto.SelectedOptionIds.Any(id => !validOptionIds.Contains(id)))
                return BadRequest("Один или несколько вариантов не принадлежат этому вопросу");


            var correctIds = question.Options
                .Where(o => o.IsCorrect)
                .Select(o => o.Id)
                .ToList();

            var score = TestFactory.AnswerFactory.CalculateScore(dto.SelectedOptionIds, correctIds);

            var existing = userTest.Answers
                .FirstOrDefault(a => a.QuestionId == dto.QuestionId);

            if (existing == null)
            {
                userTest.Answers.Add(new UserTestAnswer
                {
                    QuestionId = dto.QuestionId,
                    SelectedOptionsJson = JsonSerializer.Serialize(dto.SelectedOptionIds),
                    Score = score,
                    AnsweredAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.SelectedOptionsJson = JsonSerializer.Serialize(dto.SelectedOptionIds);
                existing.Score = score;
                existing.AnsweredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                score,
                answeredQuestions = userTest.Answers.Count
            });
        }

        [Authorize]
        [HttpPost("{testId}/finish")]
        public async Task<IActionResult> FinishTest(int testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var test = await _context.Tests
                .Where(t => t.isDeleted == false)
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
                return NotFound();

            var userTest = await _context.UserTests
                .Include(t => t.Answers)
                .Include(t => t.Test)
                    .ThenInclude(t => t.Questions)
                .FirstOrDefaultAsync(t =>
                t.TestId == testId &&
                t.UserId == userId &&
                !t.IsFinished);

            if (userTest == null)
                return NotFound();

            double totalScore = userTest.Answers.Sum(a => a.Score);
            int totalQuestions = userTest.Test.Questions.Count;

            double percent =
                totalQuestions == 0
                    ? 0
                    : totalScore / totalQuestions * 100;

            bool isPassed =
                percent >= userTest.Test.MinSuccessPercent;

            userTest.Score = totalScore;
            userTest.IsPassed = isPassed;
            userTest.IsFinished = true;
            userTest.FinishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new FinishTestResultDto
            {
                Score = totalScore,
                MaxScore = totalQuestions,
                IsPassed = isPassed
            });
        }

        [HttpPost("pass/{testId}")]
        public async Task<IActionResult> PassTest(int testId, [FromBody] int score)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var testExists = await _context.Tests.AnyAsync(t => t.Id == testId);
            if (!testExists)
                return NotFound("Тест не найден");

            var test = await _context.Tests
                .Where(t => t.Id == testId && t.isDeleted == false)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();

            var existing = await _context.UserTests
                .FirstOrDefaultAsync(ut => ut.TestId == testId && ut.UserId == userId);


            int totalQuestions = test.Questions.Count;
            bool isPassed = ((float)score / totalQuestions) * 100 >= test.MinSuccessPercent;

            if (userId == null)
                return Ok(isPassed);

            if (userId == test.CreatorId)
                return Ok(isPassed);


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

            return Ok(entity.IsFinished); // возвращает пройден тест или нет
        }

        [Authorize]
        [HttpGet("{testId}/attempt")]
        public async Task<IActionResult> GetAttempt(int testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var test = await _context.Tests
                .Where(t => t.isDeleted == false)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
                return NotFound();

            var attempt = _context.UserTests
                .FirstOrDefaultAsync(x => x.UserId == userId && x.TestId == testId && !x.IsFinished);

            return Ok(attempt);
        }

        [Authorize]
        [HttpGet("isPassed/{testId}")]
        public async Task<IActionResult> IsTestPassed(int testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var test = await _context.Tests
                .Where(t => t.isDeleted == false)
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