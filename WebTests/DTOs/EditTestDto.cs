namespace WebTests.DTOs
{
    public class EditTestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public List<QuestionDto> Questions { get; set; }
        public List<string> Types { get; set; } = new();
        public string CreatorId { get; set; }
        public bool Published { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime EditTime { get; set; } = DateTime.UtcNow;
        public int MinimumSuccessPercent { get; set; }
        public int Difficult { get; set; }
    }
}
