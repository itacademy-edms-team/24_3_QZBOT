using WebTests.Data;
using WebTests.DTOs;
using WebTests.Models;

namespace WebTests.TestFactory
{
    public static class FromDto
    {
        public static Test Create(TestDto dto, AppDbContext context)
        {
            var test = new Test();
            test.Title = dto.Title;
            test.CreatedDate = DateTime.UtcNow;

            if (dto.Published)
                test.Published = true;
            else
                test.Published = false;


            var types = context.TestTypes
                .Where(t => dto.Types.Contains(t.Name))
                .ToList();

            test.Types = types;

            test.MinSuccessPercent = dto.MinimumSuccessPercent;


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

        public static void Update(Test test, TestDto dto, AppDbContext context)
        {
            test.Title = dto.Title;

            bool wasPublished = test.Published;
            test.Published = dto.Published;

            if (!wasPublished && dto.Published)
                test.PublishDate = DateTime.UtcNow;

            test.EditTime = DateTime.UtcNow;



            test.Types.Clear();

            var types = context.TestTypes
                .Where(t => dto.Types.Contains(t.Name))
                .ToList();

            test.Types = types;

            test.MinSuccessPercent = dto.MinimumSuccessPercent;


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
