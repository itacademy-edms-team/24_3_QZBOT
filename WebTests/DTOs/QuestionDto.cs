namespace WebTests.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<AnswerOptionDto> Options { get; set; }
        public bool isMultiple { get; set; } = false;
    }
}
