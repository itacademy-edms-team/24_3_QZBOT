using WebTests.Models;

namespace WebTests.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<AnswerOptionDto> Options { get; set; }
        public bool isMultiple { get; set; } = false;

        public static QuestionDto FromEntity(Question question)
        {
            return new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                isMultiple = question.IsMultiple,
                Options = question.Options
                    .Select(o => new AnswerOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text
                    })
                    .ToList()
            };
        }
    }
}
