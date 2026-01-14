using System.Data.Entity.Core.Mapping;

namespace WebTests.DTOs
{
    public class TestReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CreatorId { get; set; }
        public List<string> Types { get; set; }
        public List<QuestionDto> Questions { get; set; }
        public bool Published { get; set; }
        public int MinimumSuccessPercent { get; set; }
    }
}
