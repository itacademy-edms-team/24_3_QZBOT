using Microsoft.AspNetCore.Mvc;

namespace TGBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnologiesController : ControllerBase
    {
        [HttpPost("{technologyName}/{questionSlug}/check")]
        public IActionResult CheckAnswer(string technologyName, string questionSlug, [FromBody] string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(technologyName) || string.IsNullOrWhiteSpace(questionSlug) || string.IsNullOrWhiteSpace(userAnswer))
            {
                return BadRequest("Имя технологии, название вопроса и ответ не могут быть пустыми.");
            }
            technologyName = technologyName.ToLower();
            questionSlug = questionSlug.ToLower();

            var data = new Dictionary<string, Dictionary<string, Question>>()
            {
                {
                    "python",
                    new Dictionary<string, Question>
                    {
                        {
                            "variables",
                            new Question()
                            {
                                Text = "Как объявить переменную в Python?",
                                Answers = new List<string> { "x = 10", "var x = 10", "let x = 10" },
                                CorrectAnswer = "x = 10"
                            }
                        },
                        {
                            "functions",
                            new Question()
                            {
                                Text = "Как объявить функцию в Python?",
                                Answers = new List<string> { "function", "def", "lambda" },
                                CorrectAnswer = "def"
                            }
                        }
                    }
                },
                {
                    "java",
                    new Dictionary<string, Question>
                    {
                        {
                            "inheritance",
                            new Question()
                            {
                                Text = "Какой оператор используется для наследования?",
                                Answers = new List<string> { "extends", "implements", "inherits" },
                                CorrectAnswer = "extends"
                            }
                        },
                        {
                            "variables",
                            new Question()
                            {
                                Text = "Как объявить переменную в Java?",
                                Answers = new List<string> { "int x = 10;", "var x = 10;", "let x = 10;" },
                                CorrectAnswer = "int x = 10;"
                            }
                        }
                    }
                }
            };

            if (!data.ContainsKey(technologyName))
            {
                return NotFound($"Технология '{technologyName}' не найдена.");
            }

            var questions = data[technologyName];

            if (!questions.ContainsKey(questionSlug))
            {
                return NotFound($"Вопрос '{questionSlug}' для технологии '{technologyName}' не найден.");
            }

            var correctAnswer = questions[questionSlug].CorrectAnswer;
            var isCorrect = string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);

            return Ok(new { IsCorrect = isCorrect });
        }

        [HttpGet("{technologyName}")]
        public IActionResult GetAllQuestions(string technologyName)
        {
            if (string.IsNullOrWhiteSpace(technologyName))
            {
                return BadRequest("Название технологии не может быть пустым.");
            }

            technologyName = technologyName.ToLower();

            var data = new Dictionary<string, Dictionary<string, Question>>()
            {
                {
                    "python",
                    new Dictionary<string, Question>
                    {
                        {
                            "variables",
                            new Question()
                            {
                                Text = "Как объявить переменную в Python?",
                                Answers = new List<string> { "x = 10", "var x = 10", "let x = 10" },
                                CorrectAnswer = "x = 10"
                            
                            }
                        },
                        {
                            "functions",
                            new Question()
                            {
                                Text = "Как объявить функцию в Python?",
                                Answers = new List<string> { "function", "def", "lambda" },
                                CorrectAnswer = "def"
                            }
                        }
                    }
                },
                {
                    "java",
                    new Dictionary<string, Question>
                    {
                        {
                            "inheritance",
                            new Question()
                            {
                                Text = "Какой оператор используется для наследования?",
                                Answers = new List<string> { "extends", "implements", "inherits" },
                                CorrectAnswer = "extends"
                            }
                        },
                        {
                            "variables",
                            new Question()
                            {
                                Text = "Как объявить переменную в Java?",
                                Answers = new List<string> { "int x = 10;", "var x = 10;", "let x = 10;" },
                                CorrectAnswer = "int x = 10;"
                            }
                        }
                    }
                }
            };

            if (!data.ContainsKey(technologyName))
            {
                return NotFound($"Технология '{technologyName}' не найдена.");
            }

            var questions = data[technologyName]
            .ToDictionary(
                q => q.Key,
                q => new Question()
                {
                    Text = q.Value.Text,
                    Answers = q.Value.Answers
                });

            return Ok(new { Technology = technologyName, Questions = questions });
        }
    }
}