using Microsoft.AspNetCore.Mvc;

namespace TGBot
{
    public class TechnologyService
    {
        private readonly DataRepository _repository;

        public TechnologyService(DataRepository repository)
        {
            _repository = repository;
        }

        public IActionResult GetAllQuestions(string technologyName)
        {
            if (string.IsNullOrWhiteSpace(technologyName))
            {
                return new BadRequestObjectResult("Название технологии не может быть пустым.");
            }

            technologyName = technologyName.ToLower();

            var questions = _repository.GetQuestions(technologyName);
            if (questions == null)
            {
                return new NotFoundObjectResult($"Технология '{technologyName}' не найдена.");
            }

            var result = questions.ToDictionary(
                q => q.Key,
                q => new Question()
                {
                    Text = q.Value.Text,
                    Answers = q.Value.Answers
                });

            return new OkObjectResult(new { Technology = technologyName, Questions = result });
        }

        public IActionResult CheckAnswer(string technologyName, string questionSlug, string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(technologyName) || string.IsNullOrWhiteSpace(questionSlug) || string.IsNullOrWhiteSpace(userAnswer))
            {
                return new BadRequestObjectResult("Имя технологии, название вопроса и ответ не могут быть пустыми.");
            }

            technologyName = technologyName.ToLower();
            questionSlug = questionSlug.ToLower();

            var question = _repository.GetQuestion(technologyName, questionSlug);
            if (question == null)
            {
                return new NotFoundObjectResult($"Вопрос '{questionSlug}' для технологии '{technologyName}' не найден.");
            }

            var isCorrect = string.Equals(userAnswer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            return new OkObjectResult(new { IsCorrect = isCorrect });
        }
    }
}
