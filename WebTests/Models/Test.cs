using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace WebTests.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; } = "None";
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<TestTypes>? Types { get; set; }
        public string? CreatorId { get; set; }
        public ApplicationUser? Creator { get; set; }
        public bool Published { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime EditTime { get; set; } = DateTime.UtcNow;
        public int MinSuccessPercent { get; set; } = 70;
        public bool isDeleted { get; set; } = false;
        public string? Description { get; set; }
        public int? Difficult { get; set; }
        public string? CoverUrl { get; set; }
        public int? TimeLimitSeconds { get; set; }
        [JsonIgnore]
        public bool IsPublic { get; set; } = false;
        [JsonIgnore]
        public string? AccessToken { get; set; }
    }
}
