namespace WebTests.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; } = "None";
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
