namespace WebTests.Models
{
    public class LikedTest
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int TestId { get; set; }
        public Test Test { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
