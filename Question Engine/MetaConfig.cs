using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom
{
    public class MetaConfig
    {
        // Ending
        public enum GameEndingType { None, ImageText }
        public GameEndingType EndingType { get; set; } = GameEndingType.None;
        public string EndingMediaPath { get; set; }
        public string EndingText { get; set; }

        // Default question properties
        public string DefaultQuestionSuccessText { get; set; } = "Helyes válasz!";
        public string DefaultQuestionFailureText { get; set; } = "Helytelen válasz!";
        public string DefaultEndingText { get; set; } = "JÁTÉK VÉGE";
    }
}
