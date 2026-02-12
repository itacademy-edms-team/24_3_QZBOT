using WebTests.Models;

namespace WebTests.DTOs
{
    public class UserTestDto
    {
        public int UserTestId { get; set; }
        public DateTime StartedAt { get; set; }
        public bool IsFinished { get; set; }
        public List<UserAnswerDto> Answers { get; set; }

        public List<int> AnsweredQuestionIds { get; set; }

        public static UserTestDto MapToDto(UserTest attempt)
        {
            return new UserTestDto
            {
                UserTestId = attempt.Id,
                StartedAt = attempt.StartedAt,
                IsFinished = attempt.IsFinished,
                AnsweredQuestionIds = attempt.Answers
                .Select(a => a.QuestionId)
                .ToList()
            };
        }
    }
}