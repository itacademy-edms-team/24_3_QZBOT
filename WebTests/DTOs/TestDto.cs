namespace WebTests.DTOs
{
    public class TestDto
    {
        public string Title { get; set; } = string.Empty;
        public List<QuestionDto> Questions { get; set; }
        public List<string> Types { get; set; } = new();
        public bool Published { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime EditTime { get; set; } = DateTime.UtcNow;
    }
}