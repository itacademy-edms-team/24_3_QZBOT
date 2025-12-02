namespace WebTests.DTOs
{
    public class TestDto
    {
        public string Title { get; set; } = string.Empty;
        public bool Published { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }
}