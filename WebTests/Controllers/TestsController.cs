using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTests.Data;
using WebTests.Models;
using WebTests.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Published
                })
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

        [HttpPost("add")]
        public IActionResult AddTest([FromBody] TestDto dto)
        {
            try
            {
                var test = new Test();

                test.Title = dto.Title;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized("Не удалось определить пользователя");
                }

                test.CreatorId = userId;
                

                if (dto.Questions == null)
                {
                    return BadRequest("Questions пусты");
                }

                foreach (var question in dto.Questions)
                {
                    if (question.Options == null)
                    {
                        continue;
                    }

                    var quest = new Question()
                    {
                        Text = question.Text
                    };

                    if (quest.Options == null)
                    {
                        quest.Options = new List<AnswerOption>();
                    }

                    foreach (var option in question.Options)
                    {
                        var opt = new AnswerOption()
                        {
                            Text = option.Text,
                            IsCorrect = option.IsCorrect
                        };

                        quest.Options.Add(opt);
                    }

                    if (test.Questions == null)
                    {
                        test.Questions = new List<Question>();
                    }

                    test.Questions.Add(quest);
                }

                _context.Add(test);
                _context.SaveChanges();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
                throw;
            }
        }

        [HttpPost("edit/{id}")]
        public IActionResult EditTest(int id, [FromBody] TestDto dto)
        {
            var test = _context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(t => t.Id == id);

            if (test == null)
                return NotFound("Тест не найден");

            if (!string.IsNullOrWhiteSpace(dto.Title))
                test.Title = dto.Title;

            var incomingQuestionTexts = dto.Questions.Select(q => q.Text).ToList();

            var questionsToRemove = test.Questions
                .Where(q => !incomingQuestionTexts.Contains(q.Text))
                .ToList();

            _context.Questions.RemoveRange(questionsToRemove);

            foreach (var qDto in dto.Questions)
            {
                var existingQuestion = test.Questions
                    .FirstOrDefault(q => q.Text == qDto.Text);

                if (existingQuestion == null)
                {
                    var newQuestion = new Question
                    {
                        Text = qDto.Text,
                        TestId = test.Id,
                        Options = qDto.Options.Select(o => new AnswerOption
                        {
                            Text = o.Text,
                            IsCorrect = o.IsCorrect,
                        }).ToList()
                    };

                    test.Questions.Add(newQuestion);
                }
                else
                {
                    existingQuestion.Text = qDto.Text;

                    _context.AnswerOptions.RemoveRange(existingQuestion.Options);

                    existingQuestion.Options = qDto.Options.Select(o => new AnswerOption
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect,
                    }).ToList();
                }
            }

            _context.SaveChanges();

            return Ok(true);
        }
    }
}