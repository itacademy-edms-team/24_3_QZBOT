namespace WebTests.DTOs
{
    public class SubmitAnswerDto
    {
        public int UserTestId { get; set; }
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
    }
}
