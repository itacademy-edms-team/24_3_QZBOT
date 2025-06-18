using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTG.DTO;

namespace BotTG
{
    public class ConfirmationAdd
    {
        public TechnologyDto Technology { get; set; }
        public ConfirmToAdd ConfirmToAdd { get; set; }
    }
}
