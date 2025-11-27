using System.Text.Json.Serialization;

namespace WebTests.Models
{
    public class TestTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        public List<Test> Tests { get; set; }
    }
}