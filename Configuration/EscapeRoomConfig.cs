using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XeZrunner.UI.Theming;

namespace EscapeRoom.Configuration
{
    public class EscapeRoomConfig
    {
#if DEBUG
        public bool DebugFeatures = true;
#else
        public bool DebugFeatures = false;
#endif

        public bool IsThemingFirstTimeDone { get; set; } = false;

        public ThemeManager.Theme? Theme { get; set; }
        public ThemeManager.Theme Theme_Default { get; set; } = ThemeManager.Theme.Dark;

        public ThemeManager.Accent? Accent { get; set; }
        public ThemeManager.Accent Accent_Dark { get; set; } = ThemeManager.Accent.Pink;
        public ThemeManager.Accent Accent_Light { get; set; } = ThemeManager.Accent.Blue;
    }
}
