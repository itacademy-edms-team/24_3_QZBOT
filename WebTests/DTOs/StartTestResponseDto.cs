namespace WebTests.DTOs
{
    public class StartTestResponseDto
    {
        public string Status { get; set; }
        public int UserTestId { get; set; }
        public QuestionDto? NextQuestion { get; set; }
        public List<int> AnsweredQuestionIds { get; set; }
    }
}
