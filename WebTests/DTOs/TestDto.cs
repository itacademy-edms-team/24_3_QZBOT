namespace WebTests.DTOs
{
    public class TestDto
    {
        public string Title { get; set; } = "None";
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }
}