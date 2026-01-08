using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebTests.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = "None";
        public bool IsMultiple { get; set; } = false;
        public List<AnswerOption> Options { get; set; }

        [JsonIgnore]
        public int TestId { get; set; }

        [JsonIgnore]
        public Test Test { get; set; }
    }
}
