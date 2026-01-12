namespace WebTests.DTOs
{
    public class AnswerCheckDto
    {
        public string Title { get; set; }
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIndexes { get; set; } = new List<int>();
    }
}
