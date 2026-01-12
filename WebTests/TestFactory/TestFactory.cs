using WebTests.DTOs;
using WebTests.Models;

namespace WebTests.TestFactory
{
    public static class FromDto
    {
        public static Test Create(TestDto dto)
        {
            var test = new Test();
            test.Title = dto.Title;


            foreach (var question in dto.Questions)
            {
                if (question.Options == null)
                {
                    continue;
                }

                var quest = new Question()
                {
                    Text = question.Text,
                    IsMultiple = question.isMultiple
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

            return test;
        }

        public static void Update(Test test, TestDto dto)
        {
            test.Title = dto.Title;

            bool wasPublished = test.Published;
            test.Published = dto.Published;

            if (!wasPublished && dto.Published)
                test.PublishDate = DateTime.UtcNow;

            test.EditTime = DateTime.UtcNow;

            test.Questions.Clear();

            foreach (var q in dto.Questions)
            {
                var question = new Question
                {
                    Text = q.Text,
                    Options = q.Options.Select(o => new AnswerOption
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect,
                    }).ToList(),
                    IsMultiple = q.isMultiple
                };

                test.Questions.Add(question);
            }
        }
    }
}
