using Microsoft.AspNetCore.Mvc;

namespace TGBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnologiesController : ControllerBase
    {
        [HttpGet("{technologyName}")]
        public IActionResult Get(string technologyName)
        {
            if (string.IsNullOrWhiteSpace(technologyName))
            {
                return BadRequest("Имя технологии не может быть пустым.");
            }
            technologyName = technologyName.ToLower();

            var data = new Dictionary<string, List<Question>>()
            {
                {
                    "python",
                    new List<Question>
                    {
                        new Question()
                        {
                            question = "сейчас или вообще?",
                            answers = new List<string>
                            {
                                "сейчас",
                                "вообще"
                            }
                        },
                        new Question()
                        {
                            question = "второй вопрос по пайтон",
                            answers = new List<string>
                            {
                                "Ответ 1",
                                "Ответ 2"
                            }
                        }
                    }
                },

                {
                    "java",
                    new List<Question>
                    {
                        new Question()
                        {
                            question = "первый вопрос по джаве",
                            answers = new List<string>
                            {
                                "Ответ 1",
                                "Ответ 2"
                            }
                        },
                        new Question()
                        {
                            question = "второй вопрос по джаве",
                            answers = new List<string>
                            {
                                "Ответ 1",
                                "Ответ 2"
                            }
                        }
                    }
                },
            };

            if (data.ContainsKey(technologyName.ToLower()))
            {
                return Ok(new {TechnologiesClass = technologyName, Question = data[technologyName] });
            }

            return NotFound($"Технология '{technologyName}' не найдена.");
        }
    }
}