using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EscapeRoom.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsDialogContent.xaml
    /// </summary>
    public partial class SettingsDialogContent : UserControl
    {
        XeZrunner.UI.Theming.ThemeManager ThemeManager;
        Configuration.EscapeRoomConfig Config;
        Configuration.ConfigurationManager ConfigurationManager;
        ControllerWindow MainWindow;

        public SettingsDialogContent()
        {
            InitializeComponent();

            MainWindow = (ControllerWindow)Application.Current.MainWindow;
            ThemeManager = MainWindow.ThemeManager;
            Config = MainWindow.Config;
            ConfigurationManager = MainWindow.ConfigurationManager;
        }
        bool _isLoaded;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;

            switch (Config.Theme)
            {
                case XeZrunner.UI.Theming.ThemeManager.Theme.Light:
                    themeToggleButton.IsActive = false; break;
                case XeZrunner.UI.Theming.ThemeManager.Theme.Dark:
                    themeToggleButton.IsActive = true; break;
            }

            foreach (XeZrunner.UI.Controls.RadioButton button in accentStackPanel.Children)
            {
                if (Config.UserAccent == null & button.Text == "Default")
                { button.IsActive = true; break; }
                else if (button.Text == Config.Accent.ToString())
                    button.IsActive = true;
            }

#if DEBUG
            debugStackPanel.Visibility = Visibility.Visible;
#endif

            if (Config.DebugFeatures)
                debugFeaturesCheckbox.IsActive = Config.DebugFeatures;

            _isLoaded = true;
        }
        void ValidateThemeChanges()
        {
            if (!_isLoaded)
                return;

            Config.Theme = themeToggleButton.IsActive ? XeZrunner.UI.Theming.ThemeManager.Theme.Dark : XeZrunner.UI.Theming.ThemeManager.Theme.Light;

            ConfigurationManager.SerializeConfigJSON(Config);
            MainWindow.CheckThemeChanges();
        }
        void ValidateAccentChanges()
        {
            if (!_isLoaded)
                return;

            foreach (XeZrunner.UI.Controls.RadioButton button in accentStackPanel.Children)
            {
                if (button.Text == "Default" & button.IsActive)
                    Config.UserAccent = null;
                else if (button.IsActive)
                    Config.UserAccent = ThemeManager.GetAccentFromString(button.Text);
            }

            ConfigurationManager.SerializeConfigJSON(Config);
            MainWindow.CheckThemeChanges();
        }

        // Dialog content handling
        private void ThemeToggleButton_IsActiveChanged(object sender, EventArgs e)
        {
            ValidateThemeChanges();
        }
        private void accent_Click(object sender, EventArgs e)
        {
            ValidateAccentChanges();
        }
        private void debugFeaturesCheckbox_IsActiveChanged(object sender, EventArgs e)
        {
            Config.DebugFeatures = debugFeaturesCheckbox.IsActive;
            ConfigurationManager.SerializeConfigJSON(Config);
        }

        private void ResetConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationManager.ResetConfiguration();
        }
    }
}
