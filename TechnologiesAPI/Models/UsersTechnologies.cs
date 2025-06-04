using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class UsersTechnologies
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public int TechnologyId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public Technology Technology { get; set; }
    }
}
