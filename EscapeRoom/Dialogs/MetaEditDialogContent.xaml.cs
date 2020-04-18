using EscapeRoom.Configuration;
using EscapeRoom.QuestionHandling;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using XeZrunner.UI.Controls;

namespace EscapeRoom.Dialogs
{
    /// <summary>
    /// Interaction logic for EndgameEditDialogContent.xaml
    /// </summary>
    public partial class MetaEditDialogContent : UserControl
    {
        ConfigurationManager ConfigurationManager = new ConfigurationManager();
        MetaConfig MetaConfig;

        public MetaEditDialogContent()
        {
            InitializeComponent();
        }
        public MetaEditDialogContent(MetaConfig ending)
        {
            InitializeComponent();

            MetaConfig = ending;
            LoadDialog();
        }
        private void main_Loaded(object sender, RoutedEventArgs e)
        {

        }
        void LoadDialog()
        {
            SetDialogType(MetaConfig.EndingType);
            media_pathTextField.Text = MetaConfig.EndingMediaPath;
            UpdateQuestionMedia();
            modify_endtextTextField.Text = MetaConfig.EndingText;
            DefaultSuccess_TextField.Text = MetaConfig.DefaultQuestionSuccessText;
            DefaultFailure_TextField.Text = MetaConfig.DefaultQuestionFailureText;
        }
        public MetaConfig Build()
        {
            string finalMediaPath = "";

            if (media_pathTextField.Text != "")
            {
                if (!File.Exists(media_pathTextField.Text))
                    throw new FileNotFoundException();
                if (!ConfigurationManager.IsValidMediaFile(media_pathTextField.Text))
                    throw new Exception("Invalid media type! - currently supported: .jpg, .png");
                finalMediaPath = media_pathTextField.Text;
            }

            return new MetaConfig()
            {
                EndingType = DialogGameEndingType,
                EndingMediaPath = finalMediaPath, // return empty when not selected, but don't delete textfield entry
                EndingText = modify_endtextTextField.Text,
                DefaultQuestionSuccessText = DefaultSuccess_TextField.Text,
                DefaultQuestionFailureText = DefaultFailure_TextField.Text
            };
        }

        UIElement CreateMedia(string mediaPath)
        {
            switch (ConfigurationManager.GetFileExtension(mediaPath))
            {
                case ".jpg":
                case ".png":
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(mediaPath);
                    bitmap.DecodePixelWidth = 250;
                    bitmap.EndInit();

                    return new Image() { Source = bitmap, Stretch = Stretch.UniformToFill, Margin = new Thickness(0, 8, 0, 8) };
                }
                default:
                    return null;
            }
        }
        void UpdateQuestionMedia()
        {
            /*
            if (_question.QuestionType != Question.QuestType.MetaQuestion)
                throw new Exception("Invalid MetaQuestion!");
                */

            string mediaPath = media_pathTextField.Text;

            mediaContainer.Children.Clear();

            if (mediaPath != null & mediaPath != "")
            {
                if (!File.Exists(mediaPath))
                    return;

                UIElement elementToAdd = CreateMedia(mediaPath);
                if (elementToAdd != null)
                    mediaContainer.Children.Add(elementToAdd);
            }
        }

        // Dialog content handling
        public OpenFileDialog openfileDialog = new OpenFileDialog()
        {
            Title = "Browse for media...",
            Filter = "JPG image (*.jpg)|*.jpg|PNG image (*.png)|*.png|All files (*.*)|*.*",
            DefaultExt = "jpg",
        };
        private async void media_pathTextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            string originalText = media_pathTextField.Text;

            UpdateQuestionMedia();

            // stop processing input for 1 sec
            media_pathTextField.TextChangedHandled = false;
            await Task.Delay(TimeSpan.FromSeconds(1));
            media_pathTextField.TextChangedHandled = true;

            if (media_pathTextField.Text != originalText)
                media_pathTextField_TextChanged(sender, e);
        }
        private void media_browseButton_Click(object sender, RoutedEventArgs e)
        {
            bool? result = openfileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            media_pathTextField.Text = openfileDialog.FileName;
        }

        void SetDialogType(MetaConfig.GameEndingType type)
        {
            foreach (XeZrunner.UI.Controls.RadioButton button in modify_typeStackPanel.Children)
                if (type == StringToEndingType(button.Tag.ToString()))
                    button.IsActive = true;
        }

        MetaConfig.GameEndingType StringToEndingType(string str)
        {
            switch (str)
            {
                case "None":
                    return MetaConfig.GameEndingType.None;
                case "ImageText":
                    return MetaConfig.GameEndingType.ImageText;
                default:
                    throw new Exception("StringToEndingType(): invalid string (\"" + str + "\")");
            }
        }

        MetaConfig.GameEndingType DialogGameEndingType;
        private void TypeRadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            var button = (FrameworkElement)sender;
            switch (button.Tag)
            {
                case "None":
                    DialogGameEndingType = MetaConfig.GameEndingType.None;
                    modify_mediaStackPanel.Visibility = Visibility.Collapsed;
                    break;
                case "ImageText":
                    DialogGameEndingType = MetaConfig.GameEndingType.ImageText;
                    modify_mediaStackPanel.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
