﻿namespace TechnologiesAPI.DTOs
{
    public class TechnologyDto
    {
        public string Title { get; set; }
        public string? ParentTechnologyTitle { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }
}
