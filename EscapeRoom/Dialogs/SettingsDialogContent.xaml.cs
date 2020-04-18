using EscapeRoom.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XeZrunner.UI.Controls;

namespace EscapeRoom.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsDialogContent.xaml
    /// </summary>
    public partial class SettingsDialogContent : UserControl
    {
        XeZrunner.UI.Theming.ThemeManager ThemeManager;
        public Configuration.EscapeRoomConfig Config;
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
                else if (button.Text == Config.UserAccent.ToString())
                { button.IsActive = true; break; }
            }

            animationsCheckBox.IsActive = Config.Animations;

            foreach (XeZrunner.UI.Controls.RadioButton button in blurStackPanel.Children)
                if (button.Tag.ToString() == Config.BlurLevel)
                    button.IsActive = true;

#if DEBUG
            debugStackPanel.Visibility = Visibility.Visible;
#endif

            if (Config.DebugFeatures)
                debugFeaturesCheckbox.IsActive = Config.DebugFeatures;

            _isLoaded = true;
        }

        // Dialog content handling
        private void ThemeToggleButton_IsActiveChanged(object sender, EventArgs e)
        {
            if (!_isLoaded)
                return;

            Config.Theme = themeToggleButton.IsActive ? XeZrunner.UI.Theming.ThemeManager.Theme.Dark : XeZrunner.UI.Theming.ThemeManager.Theme.Light;

            ConfigurationManager.Save(Config);
            MainWindow.CheckThemeChanges();
        }
        private void accent_Click(object sender, EventArgs e)
        {
            if (!_isLoaded)
                return;

            foreach (XeZrunner.UI.Controls.RadioButton button in accentStackPanel.Children)
            {
                if (button.Text == "Default" & button.IsActive)
                { Config.UserAccent = null; break; }
                else if (button.IsActive)
                { Config.UserAccent = ThemeManager.GetAccentFromString(button.Text); break; }
            }

            ConfigurationManager.Save(Config);
            MainWindow.CheckThemeChanges();
        }

        private void ResetConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationManager.ResetConfiguration();
        }

        private void animationsCheckBox_IsActiveChanged(object sender, bool e)
        {
            Config.Animations = e;
        }

        private void blurRadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            int counter = 0;
            foreach (XeZrunner.UI.Controls.RadioButton btn in blurStackPanel.Children)
            {
                if (btn.IsActive)
                    Config.BlurLevel = btn.Tag.ToString();
                counter++;
            }

            // animate blur in main window
            DoubleAnimation bluranim = new DoubleAnimation(MainWindow.UIBlurUtils.GetBlurLevel(Config.BlurLevel), TimeSpan.FromSeconds(.3));
            MainWindow.contentDialogHost_BlurEffect.BeginAnimation(BlurEffect.RadiusProperty, bluranim);
        }

        private void debugFeaturesCheckbox_IsActiveChanged(object sender, bool e)
        {
            Config.DebugFeatures = e;
        }
    }
}
