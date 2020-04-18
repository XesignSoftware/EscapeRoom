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

        // TODO: client requirements, change for production
        public Question.QuestSuccessType DefaultQuestSuccessType { get; set; } = Question.QuestSuccessType.ImageText;
        public Question.QuestFailureType DefaultQuestFailureType { get; set; } = Question.QuestFailureType.ShakePlayGrid;

        public bool Animations { get; set; } = true;

        ThemeManager.Theme? _Theme;
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

        public ThemeManager.Accent? UserAccent = null;

        ThemeManager.Accent _Accent;
        public ThemeManager.Accent? Accent
        {
            get
            {
                if (UserAccent == null)
                {
                    if (Theme == ThemeManager.Theme.Dark)
                        return Accent_Dark;
                    else
                        return Accent_Light;
                }
                else
                    return UserAccent;
            }
            set
            {
                if (!value.HasValue)
                    UserAccent = null;
                else
                    _Accent = value.Value;
            }
        }
        public ThemeManager.Accent Accent_Dark { get; set; } = ThemeManager.Accent.Pink;
        public ThemeManager.Accent Accent_Light { get; set; } = ThemeManager.Accent.Blue;
    }
}
