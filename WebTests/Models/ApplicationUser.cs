using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTests.Models;

namespace WebTests.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
        public DateTime? BirthDate { get; set; }
        public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
        public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
    }
}
