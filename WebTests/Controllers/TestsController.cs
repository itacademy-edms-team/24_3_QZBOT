using Microsoft.AspNetCore.Mvc;

namespace WebTests.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        [HttpGet("{tech}")]
        public IActionResult GetTest(string tech)
        {
            if (tech.ToLower() == "python")
            {
                var questions = new[]
                {
                    new {
                        Id = 1,
                        Question = "Что делает функция len() в Python?",
                        Options = new[] { "Измеряет длину объекта", "Удаляет элемент", "Создает список" },
                        CorrectOption = 0
                    },
                    new
                    {
                        Id = 2,
                        Question = "Переменная в Python",
                        Options = new[] { "run", "get", "int" },
                        CorrectOption = 2
                    }
                };

                return Ok(questions);
            }

            else if (tech.ToLower() == "csharp")
            {
                var questions = new[]
                {
                    new
                    {
                        Id = 1,
                        Question = "Что делает функция len() в C#?",
                        Options = new[] { "Измеряет длину объекта", "Удаляет элемент", "Нет такой функции" },
                        CorrectOption = 2
                    },
                    new
                    {
                        Id = 2,
                        Question = "Что делает джун на C#?",
                        Options = new[] { "Измеряет длину объекта", "Ищет работу", "Нет такой функции" },
                        CorrectOption = 2
                    }
                };

                return Ok(questions);
            }
            return NotFound();
        }
    }
}
