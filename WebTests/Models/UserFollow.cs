namespace WebTests.Models
{
    public class UserFollow
    {
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        public string FollowingId { get; set; }
        public ApplicationUser Following { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
