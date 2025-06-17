using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class Technology
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? ParentTechnologyId { get; set; }

        [JsonIgnore]
        public Technology? ParentTechnology { get; set; }

        [JsonIgnore]
        public ICollection<Technology> ChildTechnologies { get; set; } = new List<Technology>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<UsersTechnologies> UsersTechnologies { get; set; } = new List<UsersTechnologies>();
    }
}
