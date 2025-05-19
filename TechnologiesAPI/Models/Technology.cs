using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Technology
    {
        public int Id { get; set; }
        public string Title { get; set; } // заметка: нужно будет сделать уникальным
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
