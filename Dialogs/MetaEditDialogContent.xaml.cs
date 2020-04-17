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
        GameEnding _ending;

        public MetaEditDialogContent()
        {
            InitializeComponent();
        }
        public MetaEditDialogContent(GameEnding ending)
        {
            InitializeComponent();

            _ending = ending;
            LoadDialog();
        }
        private void main_Loaded(object sender, RoutedEventArgs e)
        {

        }
        void LoadDialog()
        {
            SetDialogType(_ending.Type);
            media_pathTextField.Text = _ending.MediaPath;
            modify_endtextTextField.Text = _ending.EndingText;
            UpdateQuestionMedia();
        }
        public GameEnding Build()
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

            return new GameEnding()
            {
                Type = DialogGameEndingType,
                MediaPath = finalMediaPath, // return empty when not selected, but don't delete textfield entry
                EndingText = modify_endtextTextField.Text
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

        void SetDialogType(GameEnding.EndingType type)
        {
            foreach (XeZrunner.UI.Controls.RadioButton button in modify_typeStackPanel.Children)
                if (type == StringToEndingType(button.Tag.ToString()))
                    button.IsActive = true;
        }

        GameEnding.EndingType StringToEndingType(string str)
        {
            switch (str)
            {
                case "None":
                    return GameEnding.EndingType.None;
                case "ImageText":
                    return GameEnding.EndingType.ImageText;
                default:
                    throw new Exception("StringToEndingType(): invalid string (\"" + str + "\")");
            }
        }

        GameEnding.EndingType DialogGameEndingType;
        private void TypeRadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            var button = (FrameworkElement)sender;
            switch (button.Tag)
            {
                case "None":
                    DialogGameEndingType = GameEnding.EndingType.None;
                    modify_mediaStackPanel.Visibility = Visibility.Collapsed;
                    break;
                case "ImageText":
                    DialogGameEndingType = GameEnding.EndingType.ImageText;
                    modify_mediaStackPanel.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
