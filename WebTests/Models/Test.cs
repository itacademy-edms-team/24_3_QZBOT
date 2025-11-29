using System.Text.Json.Serialization;
﻿using Microsoft.AspNetCore.Identity;

namespace WebTests.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; } = "None";
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<TestTypes>? Types { get; set; }
        public string? CreatorId { get; set; }
        public IdentityUser? Creator { get; set; }
    }
}
