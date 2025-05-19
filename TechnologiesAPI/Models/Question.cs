using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Question
    {
        public int Id { get; set; }
        public string ShortName { get; set; } // заметка: сделать это поле уникальным
        public int TechnologyId { get; set; }
        public string Text { get; set; }
        public Technology Technology { get; set; }
        public ICollection<AnswerOption> AnswerOption { get; set; } = new List<AnswerOption>();
    }
}
