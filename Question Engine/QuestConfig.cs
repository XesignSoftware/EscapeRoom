using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom
{
    public class QuestConfig
    {
        public List<Question> QuestList { get; set; } = new List<Question>();
        public MetaConfig MetaConfig { get; set; } = new MetaConfig();
    }
}
