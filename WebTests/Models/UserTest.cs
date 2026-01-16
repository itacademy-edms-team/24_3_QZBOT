using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.Eventing.Reader;

namespace WebTests.Models
{
    public class UserTest
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public int TestId { get; set; }
        public Test Test { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int Score { get; set; }
        public bool IsFinished { get; set; } = false;
        public bool IsPassed { get; set; } = false;
        public ICollection<UserTestAnswer> Answers { get; set; }
    }
}
