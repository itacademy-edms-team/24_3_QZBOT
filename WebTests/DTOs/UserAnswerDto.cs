namespace WebTests.DTOs
{
    public class UserAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; }
    }
}
