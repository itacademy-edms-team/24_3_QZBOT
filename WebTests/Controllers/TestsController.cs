using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTests.Data;
using WebTests.Models;
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

        [HttpPost("add")]
        public IActionResult AddTest([FromBody] TestDto dto)
        {
            try
            {
                var test = new Test();

                test.Title = dto.Title;

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

        [HttpPost("edit/{title}")]
        public IActionResult EditTest(string title, [FromBody] List<QuestionDto> dtos)
        {
            var test = _context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(t => t.Title == title);

            if (test == null)
                return NotFound("Тест не найден");

            foreach (var dto in dtos)
            {
                var existingQuestion = test.Questions.FirstOrDefault(q => q.Text == dto.Text);

                if (existingQuestion == null)
                {
                    var newQ = new Question
                    {
                        Text = dto.Text,
                        TestId = test.Id,
                        Options = dto.Options.Select(o => new AnswerOption
                        {
                            Text = o.Text,
                            IsCorrect = o.IsCorrect,
                        }).ToList()
                    };

                    test.Questions.Add(newQ);
                }
                else
                {
                    existingQuestion.Options.Clear();
                    existingQuestion.Options = dto.Options.Select(o => new AnswerOption
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList();
                }
            }

            var newTexts = dtos.Select(q => q.Text).ToList();
            var toDelete = test.Questions.Where(q => !newTexts.Contains(q.Text)).ToList();
            _context.Questions.RemoveRange(toDelete);

            var emptyOptions = _context.AnswerOptions.Where(o => o.QuestionId == null).ToList();
            _context.AnswerOptions.RemoveRange(emptyOptions);

            _context.SaveChanges();

            var pendingDeletions = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            return Ok(true);
        }

    }
}