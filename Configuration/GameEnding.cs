using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom.Configuration
{
    public class GameEnding
    {
        public enum EndingType { None, ImageText }
        public EndingType Type { get; set; } = EndingType.None;
        public string MediaPath { get; set; }
        public string EndingText { get; set; }
    }
}
