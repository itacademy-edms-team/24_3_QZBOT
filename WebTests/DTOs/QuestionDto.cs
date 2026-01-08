namespace WebTests.DTOs
{
    public class QuestionDto
    {
        public string Text { get; set; }
        public List<AnswerOptionDto> Options { get; set; }
        public bool isMultiple { get; set; } = false;
    }
}
