using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTG
{
    public class UserLocalState
    {
        public string Technology { get; set; }
        public int CurrentQuestionIndex { get; set; } = 0;
        public int LastQuestionMessageId { get; set; } = -1;
        public int CurrentScore { get; set; } = 0;
    }
}
