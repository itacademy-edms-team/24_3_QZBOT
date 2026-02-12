namespace WebTests.DTOs
{
    public class StartTestResponseDto
    {
        public int UserTestId { get; set; }
        public QuestionDto? NextQuestion { get; set; }
        public List<int> AnsweredQuestionIds { get; set; }
    }
}
