namespace WebTests.Models
{
    public class UserTestAnswer
    {
        public int Id { get; set; }
        public int UserTestId { get; set; }
        public UserTest UserTest { get; set; }
        public int QuestionId { get; set; }
        public string SelectedOptionsJson { get; set; }
        public bool? IsCorrect { get; set; }                    
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
