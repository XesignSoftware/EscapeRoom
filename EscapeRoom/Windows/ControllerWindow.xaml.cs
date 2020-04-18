using EscapeRoom.Configuration;
using EscapeRoom.Dialogs;
using EscapeRoom.FeatureControl;
using EscapeRoom.QuestionHandling;
using EscapeRoom.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using XeZrunner.UI.Controls;
using XeZrunner.UI.Popups;
using XeZrunner.UI.Theming;

namespace EscapeRoom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ControllerWindow : Window
    {
        public ThemeManager ThemeManager;
        Version Version = new Version();
        public Animation Animation = new Animation();
        public EscapeRoomConfig Config = new EscapeRoomConfig();
        public QuestionManager QuestionManager = new QuestionManager();
        public ConfigurationManager ConfigurationManager = new ConfigurationManager();
        public UIBlurUtils UIBlurUtils = new UIBlurUtils();
        public FeatureControlManager FeatureControlManager = new FeatureControlManager();

        public ControllerWindow()
        {
            InitializeComponent();

            Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            ThemeManager = new ThemeManager(Application.Current.Resources);

            splashGrid.Visibility = Visibility.Visible;

            if (Keyboard.IsKeyDown(Key.LeftShift) & Keyboard.IsKeyDown(Key.F10))
            {
                this.Focus();
                QuestionManager.WriteEmptyQuestsJSON();
                contentDialogHost.TextContentDialog("Quests reset", string.Format("The quests have been reset!\n\n{0}", QuestionManager.ReadJSON_Literal()));
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) & Keyboard.IsKeyDown(Key.F11))
            {
                this.Focus();
                ConfigurationManager.ResetConfiguration();
                contentDialogHost.TextContentDialog("Configuration reset", string.Format("The configuration has been reset!\n\n{0}", ConfigurationManager.ReadConfigFromJSON_Literal()));
            }

            // hide UI elements by default
            shiftdeletewarningTextBlock.Visibility = Visibility.Hidden;
            splash_errorTextBlock.Text = "";

            Config = ConfigurationManager.ReadConfigFromJSON();

            // Init QuestionHandler
            QuestionManager.QuestionsChanged += QuestionManager_QuestionsChanged;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ConfigChanged += ThemeManager_ConfigChanged;

            // Load version info
            LoadVersionInfo();

            // Load Configuration
            LoadConfiguration();

            // Load Question list
            LoadQuestions();

            await Task.Delay(TimeSpan.FromSeconds(.6));
            await Animation.FadeOutAsync(splashGrid);
        }
        public void LoadConfiguration()
        {
            Config = ConfigurationManager.ReadConfigFromJSON();
            DebugFeatures = Config.DebugFeatures;
            CheckThemeChanges();
        }
        void LoadVersionInfo()
        {
            debug_versionString.Text = string.Format("{0} [{1}]", Version.VersionNumber, Version.BuildType);
            release_versionString.Text = string.Format("version: {0}", Version.VersionNumber);
        }
        event EventHandler<Exception> UnhandledException;
        private async void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            await contentDialogHost.TextContentDialogAsync("An error has occured!", e.Exception.Message, true, "Continue anyway");
            splash_errorTextBlock.Text = string.Format("Error: {0}", e.Exception.Message);

            UnhandledException?.Invoke(this, e.Exception);
        }

        // Question engine
        public void LoadQuestions()
        {
            List<Question> list = QuestionManager.GetQuestsFromJSON(); // get Question list
            List<int> questIDList = new List<int>();

            foreach (Question quest in list)
                questIDList.Add(quest.QuestID.Value);

            //questIDList.Sort();

            // clear containers
            questionListStackPanel.Children.Clear();

            foreach (int questID in questIDList)
                questionListStackPanel.Children.Add(CreateQuestionControl(list[questID])); // add controls to StackPanel
        }
        private void QuestionManager_QuestionsChanged(object sender, EventArgs e)
        {
            LoadQuestions();
        }
        QuestionControl CreateQuestionControl(Question quest)
        {
            QuestionControl control = new QuestionControl(quest); // create new Question control
            control.Click += Control_Click;
            control.DeleteClick += Control_DeleteClick;
            control.PlayClick += Control_PlayClick;
            if (quest.QuestID != null)
                control.OrderingClick += Control_OrderingClick;

            return control;
        }
        async Task CallQuestionEditDialog(bool create, Question question, UIElement comingfromPrev = null)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = create ? "Új kérdés létrehozása" : "Kérdés módosítása",
                PrimaryButtonText = create ? "Létrehozás" : "Mentés",
                SecondaryButtonText = "Mégse",
                IsDismissableByTouchBlocker = false
            };

#if DEBUG
            dialog.IsDismissableByTouchBlocker = true;
#endif

            UIElement dialogContent;

            if (comingfromPrev != null)
                dialogContent = comingfromPrev;
            else
                dialogContent = new Dialogs.QuestionEditDialogContent(question);

            dialog.Content = dialogContent;

            if (await contentDialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Primary)
            {
                Question newQuestion;

                try
                {
                    var dialogContentForEditBuild = (QuestionEditDialogContent)dialogContent;
                    newQuestion = dialogContentForEditBuild.BuildQuestion();
                }
                catch (Exception ex)
                {
                    await contentDialogHost.TextContentDialogAsync("Could not save question!", ex.Message, true);

                    // There was an error.
                    // Re-call the exact same dialog that the user just witnessed.

                    // detach the previous dialog's content so we can re-use it
                    dialog.Content = null;

                    // re-call the dialog with the same Content
                    await CallQuestionEditDialog(create, question, dialogContent);

                    return;
                }

                if (create) // creating a new question
                    QuestionManager.AddQuestion(newQuestion);
                else // modifying an existing question
                    QuestionManager.ModifyQuestion(newQuestion);
            }
        }

        // Game ending configuration
        async Task CallMetaEditDialog(MetaConfig ending, UIElement comingfromPrev = null)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Játékbeállítások módosítása",
                PrimaryButtonText = "Mentés",
                SecondaryButtonText = "Mégse",
                IsDismissableByTouchBlocker = false
            };

#if DEBUG
            dialog.IsDismissableByTouchBlocker = true;
#endif

            UIElement dialogContent;

            if (comingfromPrev != null)
                dialogContent = comingfromPrev;
            else
                dialogContent = new MetaEditDialogContent(ending);

            dialog.Content = dialogContent;

            if (await contentDialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Primary)
            {
                MetaConfig newMeta;

                try
                {
                    var dialogContentForEditBuild = (MetaEditDialogContent)dialogContent;
                    newMeta = dialogContentForEditBuild.Build();
                }
                catch (Exception ex)
                {
                    await contentDialogHost.TextContentDialogAsync("Could not save meta configuration!", ex.Message, true);

                    // There was an error.
                    // Re-call the exact same dialog that the user just witnessed.

                    // detach the previous dialog's content so we can re-use it
                    dialog.Content = null;

                    // re-call the dialog with the same Content
                    await CallMetaEditDialog(ending, dialogContent);
                    return;
                }

                QuestionManager.UpdateMeta(newMeta);
            }
        }

        // Game
        /// <summary>
        /// Creates and shows a game window without linking any automatic behaviors.
        /// </summary>
        /// <param name="question"></param>
        void TestQuestion(Question question)
        {
            var gameWindow = CreateGame(question);
        }
        GameWindow CreateGame(Question question)
        {
            GameWindow window = new GameWindow(question);
            UnhandledException = window.OnUnhandledException;
            window.Show(); window.Activate();

            return window;
        }
        /// <summary>
        /// Creates a game window that will cycle through the questions in an automatic fashion.
        /// </summary>
        /// <param name="question"></param>
        GameWindow CreateGameAuto(List<Question> questList)
        {
            GameWindow window = new GameWindow(questList);
            UnhandledException = window.OnUnhandledException;
            window.Show(); window.Activate();

            return window;
        }
        void LaunchAuto()
        {
            List<Question> questList = QuestionManager.GetQuestsFromJSON();
            GameWindow gameWindow_Auto = CreateGameAuto(questList);

            // hook up events
            gameWindow_Auto.OnLoading += GameWindow_Auto_OnLoading;
            gameWindow_Auto.OnSuccess += GameWindow_Auto_OnSuccess;
            gameWindow_Auto.OnFailure += GameWindow_Auto_OnFailure;
        }
        private void GameWindow_Auto_OnLoading(object sender, Question e)
        {

        }
        private void GameWindow_Auto_OnSuccess(object sender, Question e)
        {

        }
        private void GameWindow_Auto_OnFailure(object sender, Question e)
        {

        }

       // Button events
        private async void newButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift))
                await CallQuestionEditDialog(true, new Question());
            else
            {
                if (QuestionManager.GetQuestionCount() == 0)
                {
                    contentDialogHost.TextContentDialog("Could not create question", "There isn't a question to duplicate.", true);
                    return;
                }
                QuestionManager.DuplicateQuestion();

            }
        }
        private async void Control_Click(object sender, EventArgs e)
        {
            Question question = (Question)sender;
            await CallQuestionEditDialog(create: false, question);
        }
        private void Control_PlayClick(object sender, EventArgs e)
        {
            Question targetQuestion = (Question)sender;

            GameWindow overlay = new GameWindow(targetQuestion);
            overlay.Show(); overlay.Activate();
        }
        private async void Control_DeleteClick(object sender, EventArgs e)
        {
            Question targetQuestion = (Question)sender;

            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Törlöd az incidenst?",
                    Content = "",
                    PrimaryButtonText = "Törlés",
                    SecondaryButtonText = "Mégse",
                    IsErrorDialog = true
                };

                if (await contentDialogHost.ShowDialogAsync(dialog) != ContentDialogHost.ContentDialogResult.Primary)
                {
                    return;
                }
            }

            QuestionManager.RemoveQuestion(targetQuestion);
        }
        private void Control_OrderingClick(object sender, string direction)
        {
            var button = (QuestionControl)sender;
            QuestionControl tbreplacedButton;
            var quest = button.Question;

            int ID = questionListStackPanel.Children.IndexOf(button);
            int newID = questionListStackPanel.Children.IndexOf(button);

            if (ID == 0 & direction == "Up")
            {
                contentDialogHost.TextContentDialog("Can't move this question up", "It is already at the top!", true);
                return;
            }
            if (ID == questionListStackPanel.Children.Count - 1 & direction == "Down")
            {
                contentDialogHost.TextContentDialog("Can't move this question down", "It is already at the bottom!", true);
                return;
            }

            if (direction == "Up")
                newID--;
            else
                newID++;

            tbreplacedButton = (QuestionControl)questionListStackPanel.Children[newID];
            questionListStackPanel.Children.RemoveAt(ID);

            // move button
            questionListStackPanel.Children.Insert(newID, button);
            // change Question ID
            QuestionManager.OrderQuestion(ID, newID);
            // update button ID
            button.ID = newID;
            tbreplacedButton.ID = ID;
        }
        private async void debugOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Test a question",
                PrimaryButtonText = "Test!",
                SecondaryButtonText = "Cancel"
            };

            TextField idbox = new TextField() { Title = "Enter ID", Margin = new Thickness(5), Text = "0" };
            dialog.Content = idbox;

            if (await contentDialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Primary)
            {
                Question quest = QuestionManager.GetQuestionByID(int.Parse(idbox.Text));
                TestQuestion(quest);
            }
        }
        private void automaticButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchAuto();
        }
        private async void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog() { Title = "Settings", PrimaryButtonText = "Accept" };
            dialog.Content = new SettingsDialogContent();

            await contentDialogHost.ShowDialogAsync(dialog);
            LoadConfiguration();
        }
        private async void configureMetaButton_Click(object sender, RoutedEventArgs e)
        {
            await CallMetaEditDialog(QuestionManager.GetMetaConfigFromJSON());
        }

        #region Theming engine
        public void SetTheme(ThemeManager.Theme theme)
        {
            Config.Theme = theme;
            ConfigurationManager.Save(Config);

            CheckThemeChanges();
        }
        public void CheckThemeChanges()
        {
            ThemeManager_ConfigChanged(null, null);
        }

        bool Theming_Handled = true;
        private async void ThemeManager_ConfigChanged(object sender, EventArgs e)
        {
            if (!Theming_Handled)
                return;

            Theming_Handled = false;
            bool theme_unchanged = false;
            bool accent_unchanged = false;

            if (Config.Theme != ThemeManager.GetThemeFromString(ThemeManager.Config.theme))
                ThemeManager.Config_SetTheme(Config.Theme.Value);
            else
                theme_unchanged = true;

            if (Config.Accent != ThemeManager.GetAccentFromString(ThemeManager.Config.accent))
                ThemeManager.Config_SetAccent(Config.Accent.Value);
            else
                accent_unchanged = true;

            if (!theme_unchanged || !accent_unchanged)
            {
                //ConfigurationManager.Save(Config);

                // Screenshot!
                themechangeImage.Source = Screenshot(this);

                themechangeImage.Visibility = Visibility.Visible;

                DoubleAnimation anim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3));
                themechangeImage.BeginAnimation(OpacityProperty, anim);

                await Task.Delay(TimeSpan.FromSeconds(.3));
                themechangeImage.Visibility = Visibility.Hidden;
            }

            Theming_Handled = true;
        }
        RenderTargetBitmap Screenshot(FrameworkElement element)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(element);

            //await Task.Delay(1);

            return renderTargetBitmap;
        }
        #endregion

        // Window events
        private async void contentDialogHost_DialogRequested(object sender, bool e)
        {
            if (!(bool)FeatureControlManager.GetFeature("ContentDialogHostBlur2020Q2").Value)
                return;

            if (e)
            {
                contentDialogHost_BlurGrid.Visibility = Visibility.Visible;
                DoubleAnimation anim_in = new DoubleAnimation(UIBlurUtils.GetBlurLevel(Config.BlurLevel), TimeSpan.FromSeconds(.3));
                contentDialogHost_BlurEffect.BeginAnimation(BlurEffect.RadiusProperty, anim_in);
            }
            else
            {
                DoubleAnimation anim_out = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
                contentDialogHost_BlurEffect.BeginAnimation(BlurEffect.RadiusProperty, anim_out);
                await Task.Delay(TimeSpan.FromSeconds(.3));
                contentDialogHost_BlurGrid.Visibility = Visibility.Hidden;
            }
        }
        private async void Window_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
#if DEBUG
            if (Keyboard.IsKeyDown(Key.LeftShift) & e.Key == Key.F12)
            {
                DebugFeatures = !DebugFeatures;

                if (DebugFeatures)
                    contentDialogHost.TextContentDialog("Debug features re-enabled!", "", true);
            }
#endif

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                // new question
                if (e.Key == Key.N & !DebugFeatures)
                    newButton_Click(null, null);

                // Debug shortcuts
                if (!DebugFeatures)
                    return;

                if (e.Key == Key.V)
                    debug_ClearJSON_Click(null, null);
                if (e.Key == Key.B)
                    debug_ReadQuestsJSONFile_Click(null, null);
                if (e.Key == Key.N & Keyboard.IsKeyDown(Key.LeftAlt))
                    await CallQuestionEditDialog(true, new Question());
                else if (e.Key == Key.N)
                    QuestionManager.AddQuestion(new Question());
                if (e.Key == Key.M)
                    QuestionManager.RemoveLastQuestion();
                if (e.Key == Key.L)
                    LoadQuestions();

                if (e.Key == Key.Q & Keyboard.IsKeyDown(Key.LeftShift))
                {
                    ConfigurationManager.ResetConfiguration();
                    contentDialogHost.TextContentDialog("Configuration reset", "", true);

                    CheckThemeChanges();
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (e.Key == Key.Q)
                        Config.Theme = ThemeManager.Theme.Light;
                    else if (e.Key == Key.E)
                        Config.Theme = ThemeManager.Theme.Dark;

                    if (e.Key == Key.Q || e.Key == Key.E)
                        CheckThemeChanges();
                }
            }

            if (Keyboard.IsKeyUp(Key.LeftShift))
            {
                newButton.Icon = "\ue109"[0].ToString();
                newButton.Text = "Új kérdés";

                shiftdeletewarningTextBlock.Visibility = Visibility.Hidden;

                debug_ClearJSON.Text = "(DEBUG) Delete all user questions";
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (contentDialogHost.Visibility == Visibility.Visible)
                return;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                newButton.Icon = "\ue8c8"[0].ToString();
                newButton.Text = "Utolsó duplikálása";

                shiftdeletewarningTextBlock.Visibility = Visibility.Visible;

                debug_ClearJSON.Text = "(DEBUG) Clear Quests JSON file";
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Close the game window(s) we create.
            Application.Current.Shutdown();
        }

        #region Debug
        public bool DebugFeatures
        {
            get { return Config.DebugFeatures; }
            set
            {
                if (value)
                {
                    Config.DebugFeatures = true;
                    debugRow.Height = new GridLength(1, GridUnitType.Auto);
                    debugOverlayButton.Visibility = Visibility.Visible;

                    debug_versionBlock.Visibility = Visibility.Visible;
                    release_versionString.Visibility = Visibility.Collapsed;

                    this.Title = string.Format("EscapeRoom ({0}) [development]", Version.VersionNumber);
                }
                else
                {
                    Config.DebugFeatures = false;
                    debugRow.Height = new GridLength(0, GridUnitType.Pixel);
                    debugOverlayButton.Visibility = Visibility.Collapsed;

                    debug_versionBlock.Visibility = Visibility.Collapsed;
                    release_versionString.Visibility = Visibility.Visible;

                    this.Title = "EscapeRoom";
                }

                ConfigurationManager.Save(Config);
            }
        }

        private void debug_ReadQuestsJSONFile_Click(object sender, RoutedEventArgs e)
        {
            contentDialogHost.TextContentDialog("", QuestionManager.ReadJSON_Literal());
        }
        private void debug_ClearJSON_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                QuestionManager.WriteEmptyQuestsJSON(); LoadQuestions();
            }
            else
            {
                List<Question> list = QuestionManager.GetQuestsFromJSON();
                foreach (Question quest in list)
                    QuestionManager.RemoveQuestion(quest, false);
                LoadQuestions();
            }
        }
        #endregion
    }
}