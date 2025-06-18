using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BotTG.DTO
{
    public class TechnologyDto
    {
        public string Title { get; set; }
        public string? ParentTechnologyTitle { get; set; }

        [JsonIgnore]
        public Technology? ParentTechnology { get; set; }

        [JsonIgnore]
        public ICollection<Technology> ChildTechnologies { get; set; } = new List<Technology>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<UsersTechnologies> UsersTechnologies { get; set; } = new List<UsersTechnologies>();
    }
}
