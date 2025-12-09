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

        public DateTime PassedAt { get; set; } = DateTime.UtcNow;
        public int Score { get; set; }
        public bool IsPassed { get; set; }
    }
}
