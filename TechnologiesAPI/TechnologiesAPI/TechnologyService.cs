using Microsoft.AspNetCore.Mvc;
using Models;
using Data;
using Data.Repository;
using TechnologiesAPI.DTOs;

namespace TechnologiesAPI
{
    public class TechnologyService
    {
        private readonly IQuestionRepository _questionRepo;
        private readonly ITechnologyRepository _technologyRepo;
        private readonly IAnswerOptionRepository _answerOptionRepo;

        public TechnologyService(IQuestionRepository questionRepo, 
            ITechnologyRepository technologyRepo, 
            IAnswerOptionRepository answerOptionRepo)
        {
            _questionRepo = questionRepo;
            _technologyRepo = technologyRepo;
            _answerOptionRepo = answerOptionRepo;
        }

        public async Task<IActionResult> GetAllQuestions(string technologyName)
        {
            if (string.IsNullOrWhiteSpace(technologyName))
            {
                return new BadRequestObjectResult("Название технологии не может быть пустым.");
            }

            technologyName = technologyName.ToLower();

            var questions = await _technologyRepo.GetAllQuestionsByTechnologyName(technologyName);
            if (questions == null || !questions.Any())
            {
                return new NotFoundObjectResult($"Технология '{technologyName}' не найдена.");
            }


            var result = new Dictionary<string, object>();

            foreach (var question in questions)
            {
                result.Add(
                    question.ShortName,
                    new
                    {
                        question.Text,
                        question.AnswerOption
                    }
                );
            }

            return new OkObjectResult(new { Technology = technologyName, Questions = result });
        }

        public async Task<IActionResult> CheckAnswer(string questionShortName, string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(questionShortName) || string.IsNullOrWhiteSpace(userAnswer))
            {
                return new BadRequestObjectResult("Название вопроса и ответ должны быть заполнены");
            }

            questionShortName = questionShortName.ToLower();
            userAnswer = userAnswer.Trim();

            var answerOptions = await _answerOptionRepo.GetAllByQuestionShortName(questionShortName);

            if (answerOptions == null || !answerOptions.Any())
            {
                return new NotFoundObjectResult($"Вопроса {questionShortName} не существует");
            }

            var correctAnswer = answerOptions.FirstOrDefault(x => x.IsCorrect);

            var isCorrect = string.Equals(correctAnswer.Text.Trim(), userAnswer, StringComparison.OrdinalIgnoreCase);

            return new OkObjectResult(new
            {
                isCorrect
            });
        }

        public async Task<IActionResult> AddTechnology(TechnologyDto dto)
        {
            var technology = new Technology
            {
                Title = dto.Title,
                ParentTechnologyId = dto.ParentTechnologyId,
                Questions = dto.Questions.Select(q => new Question
                {
                    ShortName = q.ShortName,
                    Text = q.Text,
                    AnswerOption = q.AnswerOptions.Select(a => new AnswerOption
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };

            if (await _technologyRepo.CheckExistsTechnologyByTitle(technology.Title))
            {
                return new BadRequestObjectResult($"Данная технология уже существует!");
            }

            await _technologyRepo.AddAsync(technology);
            return new OkObjectResult(technology);
        }
    }
}
