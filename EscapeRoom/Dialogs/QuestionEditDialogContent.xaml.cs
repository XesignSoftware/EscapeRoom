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
        QuestionManager QuestionManager = new QuestionManager();
        Question _question;

        public QuestionEditDialogContent()
        {
            InitializeComponent();
        }
        public QuestionEditDialogContent(Question quest)
        {
            InitializeComponent();

            _question = quest;
            LoadDialog();
        }
        private void main_Initialized(object sender, EventArgs e)
        {
            advancedStackPanel.Visibility = Visibility.Collapsed;
        }
        private void main_Loaded(object sender, RoutedEventArgs e)
        {
            modify_TitleBox.Focus();
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

            // success
            SetDialogSuccessType(_question.QuestionSuccessType);
            success_mediapathTextField.Text = _question.QuestionSuccessMediaPath;
            success_TextTextField.Text = _question.QuestionSuccessText;

            // failure
            SetDialogFailureType(_question.QuestionFailureType);
            skipOnFailureCheckbox.IsActive = _question.SkipOnFailure;
        }
        public Question BuildQuestion()
        {
            string finalMediaPath = "";
            string success_finalMediaPath = "";

            if (GetDialogQuestVariant().Value == QuestType.ImageQuestion)
            {
                if (!File.Exists(media_pathTextField.Text))
                    throw new FileNotFoundException();
                if (!QuestionManager.IsValidMediaFile(media_pathTextField.Text))
                    throw new Exception("Invalid media type! - currently supported: .jpg, .png");
                finalMediaPath = media_pathTextField.Text;
            }

            if (DialogSuccessType == QuestSuccessType.ImageText)
            {
                if (!File.Exists(success_mediapathTextField.Text))
                    throw new FileNotFoundException();
                if (!QuestionManager.IsValidMediaFile(success_mediapathTextField.Text))
                    throw new Exception("Invalid media type! - currently supported: .jpg, .png");
                success_finalMediaPath = success_mediapathTextField.Text;
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
                QuestionChoices = GetDialogChoices(),

                QuestionSuccessType = DialogSuccessType,
                QuestionSuccessMediaPath = success_finalMediaPath,
                QuestionSuccessText = success_TextTextField.Text,

                QuestionFailureType = DialogFailureType,
                SkipOnFailure = skipOnFailureCheckbox.IsActive
            };
        }

        // Question properties
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
        QuestType QuestVariantFromTag(string tag)
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

        // Dialog content functions
        /// <summary>
        /// Sets the QuestionType RadioButtons in the dialog
        /// </summary>
        void SetDialogQuestVariant(Question.QuestType questVariant, bool activateButton = true)
        {
            if (activateButton)
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

        UIElement CreateMedia(string mediaPath)
        {
            switch (QuestionManager.GetFileExtension(mediaPath))
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
        void UpdateQuestionSuccessMedia()
        {
            if (DialogSuccessType != QuestSuccessType.ImageText)
                return;

            string mediaPath = success_mediapathTextField.Text;

            success_mediaContainer.Children.Clear();

            if (mediaPath != null & mediaPath != "")
            {
                if (!File.Exists(mediaPath))
                    return;

                UIElement elementToAdd = CreateMedia(mediaPath);
                if (elementToAdd != null)
                    success_mediaContainer.Children.Add(elementToAdd);
            }
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

        // Dialog content handling
        // Top buttons
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

        // Question Type
        private void TypeRadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            var button = (XeZrunner.UI.Controls.RadioButton)sender;
            string buttonTag = (string)button.Tag;

            SetDialogQuestVariant(QuestVariantFromTag(buttonTag), false);
        }

        // Question Media
        #region OpenFileDialog openfileDialog ...
        public OpenFileDialog openfileDialog = new OpenFileDialog()
        {
            Title = "Browse for media...",
            Filter = "JPG image (*.jpg)|*.jpg|PNG image (*.png)|*.png|All files (*.*)|*.*",
            DefaultExt = "jpg",
        };
        #endregion
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

        // Question input
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

        // Advanced
        private void advanced_ChevronButton_Click(object sender, RoutedEventArgs e)
        {
            if (!advancedStackPanel.IsVisible)
            {
                advancedStackPanel.Visibility = Visibility.Visible;
                advanced_ChevronButton.Icon = "\ue70e";
            }
            else
            {
                advancedStackPanel.Visibility = Visibility.Collapsed;
                advanced_ChevronButton.Icon = "\ue70d";
            }
        }

        // Success
        Question.QuestSuccessType DialogSuccessType;
        Question.QuestSuccessType StringToSuccessType(string str)
        {
            switch (str)
            {
                case "None":
                    return QuestSuccessType.None;
                case "UI":
                    return QuestSuccessType.UI;
                case "ImageText":
                    return QuestSuccessType.ImageText;
                default:
                    throw new Exception("SuccessTypeFromString(): invalid string (\"" + str + "\")");
            }
        }
        void SetDialogSuccessType(QuestSuccessType type, bool activateButton = true)
        {
            if (activateButton)
            {
                foreach (XeZrunner.UI.Controls.RadioButton button in successTypeStackPanel.Children)
                {
                    string buttonTag = (string)button.Tag;

                    if (type == StringToSuccessType(buttonTag))
                        button.IsActive = true;
                }
            }

            switch (type)
            {
                case QuestSuccessType.None:
                    success_mediaStackPanel.Visibility = Visibility.Collapsed;
                    success_TextTextField.Visibility = Visibility.Collapsed;
                    break;
                case QuestSuccessType.UI:
                    success_mediaStackPanel.Visibility = Visibility.Collapsed;
                    success_TextTextField.Visibility = Visibility.Visible;
                    break;
                case QuestSuccessType.ImageText:
                    success_mediaStackPanel.Visibility = Visibility.Visible;
                    success_TextTextField.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void SuccessRadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            var button = (FrameworkElement)sender;
            DialogSuccessType = StringToSuccessType(button.Tag.ToString());
            SetDialogSuccessType(DialogSuccessType, false);
        }
        private async void success_mediapathTextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            string originalText = success_mediapathTextField.Text;

            UpdateQuestionSuccessMedia();

            // stop processing input for 1 sec
            success_mediapathTextField.TextChangedHandled = false;
            await Task.Delay(TimeSpan.FromSeconds(1));
            success_mediapathTextField.TextChangedHandled = true;

            if (success_mediapathTextField.Text != originalText)
                success_mediapathTextField_TextChanged(sender, e);
        }
        private void success_mediabrowseButton_Click(object sender, RoutedEventArgs e)
        {
            bool? result = openfileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            success_mediapathTextField.Text = openfileDialog.FileName;
        }

        // Failure
        Question.QuestFailureType DialogFailureType;
        Question.QuestFailureType StringToFailureType(string str)
        {
            switch (str)
            {
                case "ShakePlayGrid":
                    return QuestFailureType.ShakePlayGrid;
                case "UI":
                    return QuestFailureType.UI;
                default:
                    throw new Exception("FailureTypeFromString(): invalid string (\"" + str + "\")");
            }
        }
        void SetDialogFailureType(QuestFailureType type, bool activateButton = true)
        {
            if (activateButton)
            {
                foreach (XeZrunner.UI.Controls.RadioButton button in failureTypeStackPanel.Children)
                {
                    string buttonTag = (string)button.Tag;

                    if (type == StringToFailureType(buttonTag))
                        button.IsActive = true;
                }
            }

            switch (type)
            {
                case QuestFailureType.ShakePlayGrid:
                case QuestFailureType.UI:
                    break;
            }
        }
        private void FailureRadioButton_IsActiveChanged(object sender, EventArgs e)
        {
            var button = (FrameworkElement)sender;
            DialogFailureType = StringToFailureType(button.Tag.ToString());
            SetDialogFailureType(DialogFailureType, false);
        }
    }
}
