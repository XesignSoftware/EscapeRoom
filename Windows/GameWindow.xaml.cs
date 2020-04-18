using EscapeRoom.Configuration;
using EscapeRoom.QuestionHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XeZrunner.UI.Controls.Buttons;
using XeZrunner.UI.Theming;

namespace EscapeRoom.Windows
{
    public partial class GameWindow : Window
    {
        EscapeRoomConfig Config;
        public Animation Animation = new Animation();
        ControllerWindow ControllerWindow = (ControllerWindow)Application.Current.MainWindow;
        QuestionManager QuestionManager = new QuestionManager();
        MetaConfig MetaConfig;

        public GameWindow()
        {
            InitializeComponent();
        }
        public GameWindow(Question quest)
        {
            InitializeComponent();

            Question = quest;
        }
        public GameWindow(List<Question> questList)
        {
            InitializeComponent();

            QuestionList = questList;
        }
        public GameWindow(Exception ex)
        {
            InitializeComponent();

            ThrowException(ex);
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            Config = ControllerWindow.Config;
            MetaConfig = QuestionManager.GetMetaConfigFromJSON();

            // hook to OnLoading
            OnLoading += GameWindow_OnLoading;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // fullscreen
            this.WindowState = WindowState.Maximized;

            // assign storyboards
            AssignStoryboards();

#if DEBUG   // show debug titlebar
            titleBar.Visibility = Visibility.Visible;
#else
            titlebar.Visibility =  Visibility.Collapsed;
#endif

            // debug features
            DebugFeatures = ControllerWindow.DebugFeatures;

            // hide elements that need to be hidden
            resultGrid.Visibility = Visibility.Collapsed;

            if (Question != null & QuestionList != null)
                throw EX_INIT_AMBIGIOUS;

            if (QuestionList != null)
                Auto();
            else
                LoadQuestion();
        }
        void ThrowException(Exception ex)
        {
            QuestionTitle = "An error occured while loading the question.";
            QuestionDescription = "Error: " + ex.Message;
            input_TextField.Visibility = Visibility.Collapsed;
            input_choicesGrid.Visibility = Visibility.Collapsed;

            titleBar.SetResourceReference(BackgroundProperty, "Red");
        }
        void LoadQuestion(Question quest = null)
        {
            if (quest != null)
                Question = quest;

            if (Question == null)
            {
                ThrowException(EX_NULLQUEST);
                titleBar.AppTitle = "EscapeRoom: [null or invalid question]";
                return;
            }

            QuestionTitle = Question.QuestionTitle;
            QuestionDescription = Question.QuestionDescription;
            QuestionMediaPath = Question.QuestionMediaPath;
            PrepareQuestInput();

            QuestionSuccessMediaPath = Question.QuestionSuccessMediaPath; // create solution media
            QuestionSuccessText = Question.QuestionSuccessText;

            EndingMediaPath = MetaConfig.EndingMediaPath;
            EndingText = MetaConfig.EndingText;

            titleBar.AppTitle = string.Format("EscapeRoom: " + // debug titlebar info
                    "[QuestID: {0} | QuestionType: {1} | QuestionInputType: {2} | QuestionSolution: \"{3}\"]"
                    , Question.QuestID, Question.QuestionType, Question.QuestionInputType, QuestionInputSolution);

            Animation.FadeIn(title_TextBlock);
            Animation.FadeIn(desc_TextBlock);
            Animation.FadeIn(input_TextField);
            Animation.FadeIn(input_choicesGrid);
        }
        void GameWindow_OnLoading(object sender, Question e)
        {
            LoadQuestion(e);
        }

        #region Public properties
        public Question Question { get; set; }
        public List<Question> QuestionList { get; set; }

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
        public string QuestionSuccessMediaPath
        {
            get { return succ_img_Image.Source.ToString(); }
            set { CreateImageSource(value); }
        }
        string _successText;
        public string QuestionSuccessText
        {
            get { return _successText; }
            set
            {
                _successText = value;
                succ_img_TextBlock.Text = value;
                success_textblock.Text = value != "" ? value : MetaConfig.DefaultQuestionSuccessText;
            }
        }
        public string EndingMediaPath
        {
            get { return ending_Image.Source.ToString(); }
            set { ending_Image.Source = CreateImageSource(value); }
        }
        public string EndingText
        {
            get { return ending_TextBlock.Text; }
            set { ending_TextBlock.Text = value != "" ? value : MetaConfig.DefaultEndingText; }
        }
        #endregion
        #region Animations
        Storyboard playGrid_ShakeAnim;

        Storyboard result_In;
        Storyboard result_Out;

        Storyboard result_Success_Img_In;
        Storyboard result_Success_Img_Out;

        void AssignStoryboards()
        {
            playGrid_ShakeAnim = (Storyboard)FindResource(nameof(playGrid_ShakeAnim));

            result_In = (Storyboard)FindResource(nameof(result_In));

            result_Success_Img_In = (Storyboard)FindResource(nameof(result_Success_Img_In));
            result_Success_Img_Out = (Storyboard)FindResource(nameof(result_Success_Img_Out));

            result_Out = (Storyboard)FindResource(nameof(result_Out));
        }

        void PlayStoryboard(Storyboard board)
        {
            board.Begin();

            if (!Config.Animations)
                board.SkipToFill();

            if (!DebugFeatures)
                return;
            if (Keyboard.IsKeyDown(Key.LeftShift))
                board.SetSpeedRatio(0.1);
        }
        #endregion
        #region Exceptions
        Exception EX_NULLQUEST = new Exception("The passed question is null.");
        Exception EX_UIRESULT_TODO = new Exception("UI results are not yet implemented!");
        Exception EX_INIT_AMBIGIOUS = new Exception("[init] Both a Question and a List<Question> were passed!");
        Exception EX_MEDIA_NULL = new Exception("Media is null.");
        Exception EX_MEDIA_INEXISTENT = new Exception("Media file does not exist.");
        #endregion

        async void UpdateMedia()
        {
            mediaContainer.Children.Clear();

            progress.Visibility = Visibility.Visible;

            UIElement elementToAdd = await CreateMedia(QuestionMediaPath);

            await Task.Delay(TimeSpan.FromSeconds(.5));

            if (elementToAdd != null)
                mediaContainer.Children.Add(elementToAdd);

            Animation.FadeIn(mediaContainer);
            await Animation.FadeOutAsync(progress);
        }
        async Task<UIElement> CreateMedia(string mediaPath)
        {
            if (mediaPath == null || mediaPath == "")
                return new Image();
            if (!File.Exists(mediaPath))
                return new Image();

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
                        ThrowException(ex);
                        return null;
                    }

                    return new Image() { Source = image, Stretch = Stretch.UniformToFill };
                }
                default:
                    return null;
            }
        }
        BitmapImage CreateImageSource(string mediaPath)
        {
            if (mediaPath == null || mediaPath == "")
                return new BitmapImage();
            if (!File.Exists(mediaPath))
                return new BitmapImage();

            return new BitmapImage(new Uri(mediaPath));
        }

        // Input & choices handling
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
                        string finalChoice = Regex.Replace(choice, @"\t|\n|\r", "");
                        if (choice.StartsWith("*"))
                        {
                            finalChoice = finalChoice.Substring(1);
                            input_choicesStackPanel.Children.Add(CreateChoiceButton(finalChoice, true));

                            // set global solution string
                            QuestionInputSolution = finalChoice;
                        }
                        else
                            input_choicesStackPanel.Children.Add(CreateChoiceButton(finalChoice));
                    }

                    break;
                }
            }
        }
        public event RoutedEventHandler ChoiceClicked;
        NavMenuItem CreateChoiceButton(string text, bool solution = false)
        {
            NavMenuItem button = new NavMenuItem() { Icon = "", Text = text };

            if (solution)
                button.Tag = "solution";

            button.Click += input_choiceClicked;
            return button;
        }
        private async void input_choiceClicked(object sender, RoutedEventArgs e)
        {
            ChoiceClicked?.Invoke(sender, e);

            var button = (NavMenuItem)sender;

            if ((string)button.Tag == "solution")
                await Success();
            else
                await Failure();
        }
        private async void input_TextField_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (input_TextField.Text == QuestionInputSolution)
                    await Success();
                else
                    await Failure();
            }
        }

        // Result handling
        public enum ResultEvent { Success, Failure, Forwards, Dismiss, Ending }

        /// <summary>
        /// Handles the results view for specific events.
        /// </summary>
        /// <param name="event">The action ( to handle.</param>
        void HandleResultView(ResultEvent @event)
        {
            switch (@event)
            {
                case ResultEvent.Success:
                    PlayStoryboard(result_In); break;
                case ResultEvent.Forwards:
                case ResultEvent.Dismiss:
                    PlayStoryboard(result_Out); break;
                case ResultEvent.Ending: // TODO: Animation for ending
                    break;
            }

            switch (@event)
            {
                case ResultEvent.Success:
                {
                    // TODO: handle UI success type with animations
                    successGrid.Visibility = Visibility.Collapsed;

                    switch (Question.QuestionSuccessType)
                    {
                        case Question.QuestSuccessType.ImageText:
                            PlayStoryboard(result_Success_Img_In);
                            break;
                        case Question.QuestSuccessType.UI:
                            PlayStoryboard(result_In);
                            successGrid.Visibility = Visibility.Visible;
                            break;
                    }
                    break;
                }
                case ResultEvent.Failure:
                {
                    switch (Question.QuestionFailureType)
                    {
                        case Question.QuestFailureType.ShakePlayGrid:
                            PlayStoryboard(playGrid_ShakeAnim); break;
                        case Question.QuestFailureType.UI:
                            PlayStoryboard(result_In); throw EX_UIRESULT_TODO;
                    }
                    break;
                }
                case ResultEvent.Forwards:
                    if (Question.QuestionSuccessType == Question.QuestSuccessType.ImageText)
                        PlayStoryboard(result_Success_Img_Out);
                    break;
                case ResultEvent.Ending:
                {
                    switch (MetaConfig.EndingType)
                    {
                        case MetaConfig.GameEndingType.None:
                            break;
                        case MetaConfig.GameEndingType.ImageText:
                            // TODO: Proper animations &/ handling
                            endingGrid.Visibility = Visibility.Visible;
                            Animation.FadeIn(endingGrid);
                            break;
                    }
                    break;
                }
            }
        }

        public event EventHandler OnEnding;
        public event EventHandler<Question> OnLoading;
        public event EventHandler<Question> OnSuccess;
        public event EventHandler<Question> OnFailure;

        /// <summary>
        /// The amount of time to wait upon succeeding. (Auto)
        /// </summary>
        public TimeSpan SuccessTime { get; set; } = TimeSpan.FromSeconds(3);
        TimeSpan? _failureTime = null;
        public TimeSpan FailureTime
        {
            get { if (!_failureTime.HasValue) return SuccessTime; else return _failureTime.Value; }
            set { _failureTime = value; }
        }
        public async Task Success()
        {
            HandleResultView(ResultEvent.Success);

            SetTitlebarColor("Success");

            OnSuccess?.Invoke(null, Question);

            if (!IsGameAuto)
                return;

            await Task.Delay(SuccessTime);
            Forwards();
        }
        public async Task Failure()
        {
            HandleResultView(ResultEvent.Failure);

            SetTitlebarColor("Failure");

            OnFailure?.Invoke(null, Question);

            if (!IsGameAuto)
                return;

            await Task.Delay(FailureTime);

            SetTitlebarColor("Accent");
            // TODO: Display media on failure?
            //Forwards();
        }
        public void Forwards()
        {
            input_TextField.Clear();
            SetTitlebarColor("Accent");

            if (!IsGameAuto)
                HandleResultView(ResultEvent.Dismiss);
            else
            {
                // increase auto counter and invoke next question loading
                auto_counter++;
                if (auto_counter != QuestionList.Count)
                {
                    HandleResultView(ResultEvent.Forwards);
                    Invoke_Loading(auto_counter);
                }
                else
                    Ending();
            }
        }

        // Automatic gamemode
        int auto_counter = 0;
        void Auto(List<Question> questList = null)
        {
            if (questList != null)
                QuestionList = questList;

            // sort the question list by QuestIDs
            QuestionList = SortQuestionList(QuestionList);

            auto_counter = 0;
            Invoke_Loading(0);
        }

        /// <summary>
        /// Gets the questions' IDs in a List form.
        /// </summary>
        List<Question> SortQuestionList(List<Question> questList)
        {
            List<Question> finalQuestList = new List<Question>();
            List<int> questIDList = new List<int>();

            foreach (Question quest in questList)
                questIDList.Add(quest.QuestID.Value);

            foreach (int questID in questIDList)
                finalQuestList.Add(questList[questID]);

            return finalQuestList;
        }

        public bool IsGameAuto
        {
            get { if (QuestionList == null) return false; else return true; }
        }
        public void Ending()
        {
            OnEnding?.Invoke(null, null);

            HandleResultView(ResultEvent.Ending);

            SetTitlebarColor("Success");
        }
        public void UnFinish()
        {
            Animation.FadeOut(endingGrid);

            SetTitlebarColor("Accent");
        }
        void Invoke_Loading(Question quest)
        {
            OnLoading?.Invoke(null, quest);
        }
        void Invoke_Loading(int questID)
        {
            Question quest = QuestionList[questID];
            Invoke_Loading(quest);
        }

        #region Debug
        bool _debugFeatures;
        public bool DebugFeatures
        {
            get { return _debugFeatures; }
            set
            {
                _debugFeatures = value;

                if (value)
                {
                    titleBar.Visibility = Visibility.Visible;
                    SuccessTime = TimeSpan.FromSeconds(1);
                }
                else
                {
                    titleBar.Visibility = Visibility.Collapsed;
                    SuccessTime = TimeSpan.FromSeconds(3);
                }
            }
        }

        public void SetTitlebarColor(string resourcename)
        {
            titleBar.ClearValue(BackgroundProperty);
            titleBar.SetResourceReference(BackgroundProperty, resourcename);
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
                if (e.Key == Key.Q)
                {
                    ControllerWindow.SetTheme(ThemeManager.Theme.Light);
                }
                if (e.Key == Key.E)
                {
                    ControllerWindow.SetTheme(ThemeManager.Theme.Dark);
                }
            }
        }
        #endregion
    }
}
