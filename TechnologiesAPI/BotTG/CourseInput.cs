using Models;


namespace BotTG
{
    public class CourseInput
    {
        public string Title { get; set; }
        public string? TitleOfParentCourse { get; set; }
        public List<Question> Questions { get; set; }
    }
}