namespace WebTests.DTOs
{
    public class QuestionDto
    {
        public string Text { get; set; } = "None";
        public List<AnswerOptionDto> Options { get; set; }
    }
}
