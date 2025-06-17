namespace TechnologiesAPI.DTOs
{
    public class QuestionDto
    {
        public string ShortName { get; set; }
        public string Text { get; set; }
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new List<AnswerOptionDto>();
    }

}
