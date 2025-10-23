﻿namespace WebTests.Models
{
    public class AnswerOption
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
