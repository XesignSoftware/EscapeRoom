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

        public ThemeManager.Theme? _Theme;
        public ThemeManager.Theme? Theme
        {
            get
            {
                if (_Theme == null)
                    return Theme_Default;
                else
                    return _Theme;
            }
            set { _Theme = value; }
        }

        public ThemeManager.Theme Theme_Default { get; set; } = ThemeManager.Theme.Dark;

        public ThemeManager.Accent? _UserAccent;
        public ThemeManager.Accent? _Accent;
        public ThemeManager.Accent? Accent
        {
            get
            {
                if (_UserAccent == null)
                {
                    if (Theme == ThemeManager.Theme.Dark)
                        return Accent_Dark;
                    else
                        return Accent_Light;
                }
                else
                    return _UserAccent;
            }
            set { _Accent = value; }
        }
        public ThemeManager.Accent Accent_Dark { get; set; } = ThemeManager.Accent.Pink;
        public ThemeManager.Accent Accent_Light { get; set; } = ThemeManager.Accent.Blue;
    }
}
