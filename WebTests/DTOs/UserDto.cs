namespace WebTests.DTOs
{
    public class UserProfileDto
    {
        public IFormFile? Avatar { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Status { get; set; }
    }
}
