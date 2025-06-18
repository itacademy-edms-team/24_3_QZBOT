using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class Admins
    {
        public long UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }
    }
}
