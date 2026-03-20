namespace WebTests.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Status { get; set; }
    }
}
