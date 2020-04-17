using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom.Configuration
{
    public class GameEnding
    {
        public enum GameEndingType { None, ImageText }
        public GameEndingType EndingType { get; set; } = GameEndingType.None;
        public string Media { get; set; }
        public string EndingText { get; set; }
    }
}
