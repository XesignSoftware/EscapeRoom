using EscapeRoom.QuestionHandling;
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
using System.Windows.Shapes;
using XeZrunner.UI.Controls.Buttons;
using XeZrunner.UI.Theming;

namespace EscapeRoom.Windows
{
    public partial class GameWindow : Window
    { 
        public Animation Animation = new Animation();
        ControllerWindow ControllerWindow = (ControllerWindow)Application.Current.MainWindow;
        ThemeManager ThemeManager;
        QuestionManager QuestionManager = new QuestionManager();
        public Question Question { get; set; }

        public GameWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG // show debug titlebar
            titleBar.Visibility = Visibility.Visible;
#else
            titlebar.Visibility =  Visibility.Collapsed;
#endif
            DebugFeatures = ControllerWindow.DebugFeatures;

            videoGrid.Visibility = Visibility.Collapsed;
        }

        public GameWindow(Question quest)
        {
            InitializeComponent();

            Question = quest;
            LoadQuestion();
        }

        public GameWindow(Exception ex)
        {
            InitializeComponent();

            Exception(ex);
        }

        void Exception(Exception ex)
        {
            QuestionTitle = "An error occured while loading the question.";
            QuestionDescription = "Error: " + ex.Message;
            input_TextField.Visibility = Visibility.Collapsed;
            input_choicesGrid.Visibility = Visibility.Collapsed;

            titleBar.SetResourceReference(BackgroundProperty, "Red");
        }

        bool _debugFeatures;
        public bool DebugFeatures
        {
            get { return _debugFeatures; }
            set
            {
                _debugFeatures = value;

                if (!value)
                    titleBar.Visibility = Visibility.Collapsed;
                else
                    titleBar.Visibility = Visibility.Visible;
            }
        }

        void LoadQuestion()
        {
            if (Question == null)
            {
                Exception(new System.Exception("The passed question is null."));
                return;
            }

            QuestionTitle = Question.QuestionTitle;
            QuestionDescription = Question.QuestionDescription;
            QuestionMediaPath = Question.QuestionMediaPath;
            PrepareQuestInput();

            Animation.FadeIn(title_TextBlock);
            Animation.FadeIn(desc_TextBlock);
            Animation.FadeIn(input_TextField);
            Animation.FadeIn(input_choicesGrid);
        }

        public string QuestionTitle
        {
            get { return title_TextBlock.Text; }
            set { title_TextBlock.Text = value; }
        }

        public string QuestionDescription
        {
            get { return desc_TextBlock.Text; }
            set { desc_TextBlock.Text = value; }
        }

        string _mediaPath;
        public string QuestionMediaPath
        {
            get { return _mediaPath; }
            set
            {
                _mediaPath = value;
                UpdateMedia();
            }
        }

        public string QuestionInputSolution { get; set; }

        async void UpdateMedia()
        {
            mediaContainer.Children.Clear();

            if (_mediaPath != null & _mediaPath != "")
            {
                if (!File.Exists(_mediaPath))
                    return;

                progress.Visibility = Visibility.Visible;

                UIElement elementToAdd = await CreateMedia(_mediaPath);

                await Task.Delay(TimeSpan.FromSeconds(.5));

                if (elementToAdd != null)
                    mediaContainer.Children.Add(elementToAdd);

                Animation.FadeIn(mediaContainer);
                await Animation.FadeOutAsync(progress);
            }
        }

        async Task<UIElement> CreateMedia(string mediaPath)
        {
            switch (QuestionManager.GetFileExtension(mediaPath))
            {
                case ".jpg":
                case ".png":
                    {
                        BitmapImage image = new BitmapImage();
                        byte[] bytes = null;

                        try
                        {
                            using (FileStream stream = new FileStream(mediaPath, FileMode.Open, FileAccess.Read))
                            {
                                bytes = new byte[stream.Length];
                                await stream.ReadAsync(bytes, 0, (int)stream.Length);
                            }

                            if (bytes.Length != 0)
                            {
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.None;
                                image.StreamSource = new MemoryStream(bytes);
                                image.EndInit();
                            }
                            else
                                throw new Exception("The image is null.");
                        }
                        catch (Exception ex)
                        {
                            Exception(ex);
                            return null;
                        }

                        return new Image() { Source = image, Stretch = Stretch.UniformToFill };
                    }
                default:
                    return null;
            }
        }

        void PrepareQuestInput()
        {
            switch (Question.QuestionInputType)
            {
                case Question.QuestInputType.Input:
                    {
                        input_TextField.Visibility = Visibility.Visible;
                        input_choicesGrid.Visibility = Visibility.Collapsed;

                        QuestionInputSolution = Question.QuestionInputSolution;
                        break;
                    }
                case Question.QuestInputType.Choices:
                    {
                        input_TextField.Visibility = Visibility.Collapsed;
                        input_choicesStackPanel.Visibility = Visibility.Visible;

                        // clear choice buttons
                        input_choicesStackPanel.Children.Clear();

                        // create choice buttons
                        foreach (string choice in Question.QuestionChoices)
                        {
                            if (choice.StartsWith("*"))
                            {
                                string finalChoice = choice.Substring(1);
                                input_choicesStackPanel.Children.Add(CreateChoiceButton(finalChoice, true));
                            }
                            else
                                input_choicesStackPanel.Children.Add(CreateChoiceButton(choice));
                        }

                        break;
                    }
            }
        }

        NavMenuItem CreateChoiceButton(string text, bool solution = false)
        {
            NavMenuItem button = new NavMenuItem() { Icon = "", Text = text };

            if (solution)
                button.Tag = "solution";

            button.Click += input_choiceClicked;
            return button;
        }

        public event RoutedEventHandler ChoiceClicked;

        private void input_choiceClicked(object sender, RoutedEventArgs e)
        {
            ChoiceClicked?.Invoke(sender, e);

            var button = (NavMenuItem)sender;

            if ((string)button.Tag == "solution")
                Success();
        }

        private void input_TextField_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (input_TextField.Text == QuestionInputSolution)
                    Success();
                else
                    Failure();
            }
        }

        public void Success()
        {
            videoGrid.Visibility = Visibility.Visible;
            titleBar.ClearValue(BackgroundProperty);
            titleBar.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4CAF50"));
        }

        public void Failure()
        {

        }

        private void titleBar_CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void titleBar_MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void titleBar_MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
            {
                this.WindowState = WindowState.Normal;
                this.Width = 1024;
                this.Height = 768;
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Key == Key.Q || e.Key == Key.E)
                {
                    if (ThemeManager == null)
                        ThemeManager = ControllerWindow.ThemeManager;
                }

                if (e.Key == Key.Q)
                {
                    ThemeManager.Config_SetTheme(ThemeManager.Theme.Light);
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Blue);
                }
                if (e.Key == Key.E)
                {
                    ThemeManager.Config_SetTheme(ThemeManager.Theme.Dark);
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Pink);
                }
            }
        }
    }
}
