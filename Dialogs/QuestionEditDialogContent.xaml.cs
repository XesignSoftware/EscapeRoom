using EscapeRoom.QuestionHandling;
using EscapeRoom.Windows;
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
using XeZrunner.UI.Controls.Buttons;
using XeZrunner.UI.Popups;
using static EscapeRoom.Question;

namespace EscapeRoom.Dialogs
{
    /// <summary>
    /// Interaction logic for QuestionEditDialogContent.xaml
    /// </summary>
    public partial class QuestionEditDialogContent : UserControl
    {
        public QuestionEditDialogContent()
        {
            InitializeComponent();
        }

        QuestionManager QuestionManager = new QuestionManager();
        Question _question;

        public QuestionEditDialogContent(Question quest)
        {
            InitializeComponent();

            _question = quest;
            LoadDialog();
        }

        void LoadDialog()
        {
            modify_TitleBox.Text = _question.QuestionTitle;
            modify_DescBox.Text = _question.QuestionDescription;
            SetDialogQuestVariant(_question.QuestionType);
            media_pathTextField.Text = _question.QuestionMediaPath;
            modify_inputTextField.Text = _question.QuestionInputSolution;
            UpdateQuestionMedia();
            UpdateQuestionInputType();
        }

        void UpdateQuestionInputType()
        {
            switch (_question.QuestionInputType)
            {
                case QuestInputType.Input:
                    {
                        modify_inputTextField.Visibility = Visibility.Visible;
                        modify_choicesGrid.Visibility = Visibility.Collapsed;
                        modify_inputRadioButton.ActivateButton();
                        break;
                    }
                case QuestInputType.Choices:
                    {
                        modify_inputTextField.Visibility = Visibility.Collapsed;
                        modify_choicesGrid.Visibility = Visibility.Visible;
                        modify_choicesRadioButton.ActivateButton();
                        break;
                    }
            }

            UpdateQuestionChoices();
        }

        /// <summary>
        /// Returns the appropriate RadioButton for the QuestionType
        /// </summary>
        string TagFromQuestVariant(Question.QuestType type)
        {
            switch (type)
            {
                case Question.QuestType.TextQuestion:
                    return "Text";
                case Question.QuestType.ImageQuestion:
                    return "Image";

                default:
                    throw new Exception("Invalid question type!");
            }
        }

        Question.QuestType QuestVariantFromTag(string tag)
        {
            switch (tag)
            {
                case "Text":
                    return Question.QuestType.TextQuestion;
                case "Image":
                    return Question.QuestType.ImageQuestion;

                default:
                    return Question.QuestType.TextQuestion;
            }
        }

        /// <summary>
        /// Sets the QuestionType RadioButtons in the dialog
        /// </summary>
        void SetDialogQuestVariant(Question.QuestType questVariant, bool update = false)
        {
            if (!update)
            {
                foreach (XeZrunner.UI.Controls.RadioButton button in modify_TypeRadioButtonStackPanel.Children)
                {
                    string buttonTag = (string)button.Tag;

                    if (buttonTag == TagFromQuestVariant(questVariant))
                        button.IsActive = true;
                }
            }

            if (questVariant == Question.QuestType.ImageQuestion)
            {
                modify_mediaStackPanel.Visibility = Visibility.Visible; // show media controls
                UpdateQuestionMedia();
            }
            else
                modify_mediaStackPanel.Visibility = Visibility.Collapsed; // hide media controls
        }

        void UpdateQuestionMedia()
        {
            if (GetDialogQuestVariant() != QuestType.ImageQuestion)
                return;

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

        string GetFileExtension(string filePath)
        {
            return filePath.Substring(filePath.Length - 4);
        }

        UIElement CreateMedia(string mediaPath)
        {
            switch (GetFileExtension(mediaPath))
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

        /// <summary>
        /// Gets the user-customized new quest varaint to build the new Question with.
        /// </summary>
        Question.QuestType? GetDialogQuestVariant()
        {
            foreach (XeZrunner.UI.Controls.RadioButton button in modify_TypeRadioButtonStackPanel.Children)
            {
                if (button.IsActive)
                {
                    switch (button.Tag)
                    {
                        case "Text":
                            return Question.QuestType.TextQuestion;
                        case "Image":
                            return Question.QuestType.ImageQuestion;

                        default:
                            throw new Exception("Invalid question type RadioButton Tag!");
                    }
                }
            }

            throw new Exception("None of the question type RadioButtons were active!");
        }

        public Question BuildQuestion()
        {
            string finalMediaPath = "";

            if (GetDialogQuestVariant().Value == QuestType.ImageQuestion)
            {
                if (!File.Exists(media_pathTextField.Text))
                    throw new FileNotFoundException();
                if (!QuestionManager.IsValidMediaFile(media_pathTextField.Text))
                    throw new Exception("Invalid media type! - currently supported: .jpg, .png");
                finalMediaPath = media_pathTextField.Text;
            }

            /*
            if (modify_inputRadioButton.IsActive)
                if (modify_inputTextField.Text == "")
                    throw new Exception("No input solution was given!");
                    */

            if (modify_choicesRadioButton.IsActive)
                if (modify_choicesTextField.Text == "")
                    throw new Exception("No choices were given!");

            return new Question()
            {
                QuestID = _question.QuestID,
                QuestionTitle = modify_TitleBox.Text,
                QuestionDescription = modify_DescBox.Text,
                QuestionType = GetDialogQuestVariant().Value,
                QuestionMediaPath = finalMediaPath, // return empty when not selected, but don't delete textfield entry
                QuestionInputSolution = modify_inputTextField.Text,
                QuestionInputType = GetDialogQuestInputType(),
                QuestionChoices = GetDialogChoices()
            };
        }

        Question.QuestInputType GetDialogQuestInputType()
        {
            foreach (XeZrunner.UI.Controls.RadioButton button in modify_inputRadioButtonSP.Children)
            {
                if (button.IsActive)
                {
                    switch (button.Tag)
                    {
                        default:
                            return QuestInputType.Input;

                        case "Input":
                            return QuestInputType.Input;
                        case "Choices":
                            return QuestInputType.Choices;
                    }
                }
            }

            throw new Exception("None of the question input type RadioButtons were active!");
        }

        List<string> GetDialogChoices()
        {
            if (modify_choicesTextField.Text == "")
                return null;
            else
            {
                return new List<string>(
                           modify_choicesTextField.Text.Split(new string[] { "\n" },
                           StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private void RadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            var button = (XeZrunner.UI.Controls.RadioButton)sender;
            string buttonTag = (string)button.Tag;

            SetDialogQuestVariant(QuestVariantFromTag(buttonTag), true);
        }

        public OpenFileDialog openfileDialog = new OpenFileDialog()
        {
            Title = "Browse for media...",
            Filter = "JPG image (*.jpg)|*.jpg|PNG image (*.png)|*.png|All files (*.*)|*.*",
            DefaultExt = "jpg",
        };

        private void media_browseButton_Click(object sender, RoutedEventArgs e)
        {
            bool? result = openfileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            media_pathTextField.Text = openfileDialog.FileName;
        }

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

        private void InputRadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            var button = (XeZrunner.UI.Controls.RadioButton)sender;

            switch (button.Tag)
            {
                case "Input":
                    _question.QuestionInputType = QuestInputType.Input; break;
                case "Choices":
                    _question.QuestionInputType = QuestInputType.Choices; break;
            }

            UpdateQuestionInputType();
        }

        void UpdateQuestionChoices()
        {
            if (_question.QuestionChoices == null)
                return;

            // clear text
            modify_choicesTextField.Clear();

            int counter = 0;
            foreach (string choice in _question.QuestionChoices)
            {
                if (counter != 0)
                    modify_choicesTextField.Text += "\n";

                modify_choicesTextField.Text += choice;
                counter++;
            }
        }

        private async void modify_choicesAddButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog() { /*Title = "Create new question choice",*/ PrimaryButtonText = "Add", SecondaryButtonText = "Cancel" };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

            TextField textfield = new TextField() { Title = "Enter question choice text: " };

            StackPanel stackpanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(8, 15, 0, 0) };
            var checkbox = new XeZrunner.UI.Controls.CheckBox();
            TextBlock checkbox_block = new TextBlock() { Text = "Solution", Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };

            stackpanel.Children.Add(checkbox);
            stackpanel.Children.Add(checkbox_block);

            grid.Children.Add(textfield);
            grid.Children.Add(stackpanel);
            Grid.SetColumn(stackpanel, 1);

            dialog.Content = grid;

            if (await contentdialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Primary)
            {
                if (modify_choicesTextField.Text != "")
                    modify_choicesTextField.Text += "\n";
                if (checkbox.IsActive)
                    modify_choicesTextField.Text += "*";

                modify_choicesTextField.Text += textfield.Text;
            }
        }

        private void tryButton_Click(object sender, RoutedEventArgs e)
        {
            Question trialQuestion;
            GameWindow gameWindow;

            try
            {
                trialQuestion = BuildQuestion();
                gameWindow = new GameWindow(trialQuestion);
            }
            catch (Exception ex)
            {
                gameWindow = new GameWindow(ex);
            }

            gameWindow.Show();
        }

        private void main_Loaded(object sender, RoutedEventArgs e)
        {
            modify_TitleBox.Focus();
        }
    }
}
