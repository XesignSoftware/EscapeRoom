using EscapeRoom.Configuration;
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
        public Animation Animation = new Animation();
        public ThemeManager ThemeManager;
        public QuestionManager QuestionManager = new QuestionManager();
        public ConfigurationManager ConfigurationManager = new ConfigurationManager();

        Version Version = new Version();
        public EscapeRoomConfig Config = new EscapeRoomConfig();

        public ControllerWindow()
        {
            InitializeComponent();

            Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            // hide UI elements by default
            shiftdeletewarningTextBlock.Visibility = Visibility.Hidden;
            splash_errorTextBlock.Text = "";

            splashGrid.Visibility = Visibility.Visible;

            ThemeManager = new ThemeManager(Application.Current.Resources);

            Config = ConfigurationManager.ReadConfigFromJSON();

            // Set theme
            if (!Config.Theme.HasValue)
                ThemeManager.Config_SetTheme(Config.Theme_Default);
            if (!Config.Accent.HasValue)
            {
                if (Config.Theme == ThemeManager.Theme.Dark)
                    ThemeManager.Config_SetAccent(Config.Accent_Dark);
                else
                    ThemeManager.Config_SetAccent(Config.Accent_Light);
            }

            // Init QuestionHandler
            QuestionManager.QuestionsChanged += QuestionManager_QuestionsChanged;
        }

        private void QuestionManager_QuestionsChanged(object sender, EventArgs e)
        {
            LoadQuestions();
        }

        private async void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            await contentDialogHost.TextContentDialogAsync("An error has occured!", e.Exception.Message, true, "Continue anyway");
            splash_errorTextBlock.Text = string.Format("Error: {0}", e.Exception.Message);
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

            await Task.Delay(TimeSpan.FromSeconds(.82));
            await Animation.FadeOutAsync(splashGrid);
        }

        public void LoadConfiguration()
        {
            Config = ConfigurationManager.ReadConfigFromJSON();
            DebugFeatures = Config.DebugFeatures;
            CheckThemeChanges();
        }

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

                    this.Title = string.Format("EscapeRoom ({0}) [development]", Version.VersionNumber);
                }
                else
                {
                    Config.DebugFeatures = false;
                    debugRow.Height = new GridLength(0, GridUnitType.Pixel);
                    debugOverlayButton.Visibility = Visibility.Collapsed;

                    this.Title = "EscapeRoom";
                }

                ConfigurationManager.SerializeConfigJSON(Config);
            }
        }

        void LoadVersionInfo()
        {
            versionString.Text = string.Format("{0} [{1}]", Version.VersionNumber, Version.BuildType);
        }

        public void LoadQuestions()
        {
            List<Question> list = QuestionManager.ReadQuestsListFromJSON(); // get Question list
            List<int> questIDList = new List<int>();

            foreach (Question quest in list)
                questIDList.Add(quest.QuestID.Value);

            //questIDList.Sort();

            // clear StackPanel
            questionListStackPanel.Children.Clear();

            foreach (int questID in questIDList)
            {
                QuestionControl control = new QuestionControl(list[questID]); // create new Question control
                control.Click += Control_Click;
                control.DeleteClick += Control_DeleteClick;
                control.PlayClick += Control_PlayClick;
                control.OrderingClick += Control_OrderingClick;
                questionListStackPanel.Children.Add(control); // add control to StackPanel
            }

            /*
            foreach (Question inc in list)
            {
                QuestionControl control = new QuestionControl(inc); // create new Question control
                control.Click += Control_Click;
                control.DeleteClick += Control_DeleteClick;
                control.PlayClick += Control_PlayClick;
                control.OrderingClick += Control_OrderingClick;
                questionListStackPanel.Children.Add(control); // add control to StackPanel
            }
            */
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

        async Task CallQuestionEditDialog(bool create, Question question, Dialogs.QuestionEditDialogContent comingfromPrev = null)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = create ? "Új kérdés létrehozása" : "Kérdés módosítása",
                PrimaryButtonText = create ? "Létrehozás" : "Mentés",
                SecondaryButtonText = "Mégse",
                IsDismissableByTouchBlocker = false
            };

            Dialogs.QuestionEditDialogContent dialogContent;

            if (comingfromPrev == null)
                dialogContent = new Dialogs.QuestionEditDialogContent(question);
            else
                dialogContent = comingfromPrev;

            dialog.Content = dialogContent;

            if (await contentDialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Primary)
            {
                Question newQuestion;

                try
                {
                    newQuestion = dialogContent.BuildQuestion();
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

            if (Config.Theme == null)
                ThemeManager.Config_SetTheme(Config.Theme_Default);
            else if (Config.Theme != ThemeManager.GetThemeFromString(ThemeManager.Config.theme))
                ThemeManager.Config_SetTheme(Config.Theme.Value);

            if (Config.Accent == null)
            {
                if (Config.Theme == ThemeManager.Theme.Dark || Config.Theme == null)
                    ThemeManager.Config_SetAccent(Config.Accent_Dark);
                else
                    ThemeManager.Config_SetAccent(Config.Accent_Light);
            }
            else if (Config.Accent != ThemeManager.GetAccentFromString(ThemeManager.Config.accent))
                ThemeManager.Config_SetAccent(Config.Accent.Value);

            ConfigurationManager.SerializeConfigJSON(Config);

            // Screenshot!
            themechangeImage.Source = Screenshot(this);

            themechangeImage.Visibility = Visibility.Visible;

            DoubleAnimation anim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3));
            themechangeImage.BeginAnimation(OpacityProperty, anim);

            await Task.Delay(TimeSpan.FromSeconds(.3));
            themechangeImage.Visibility = Visibility.Hidden;

            Theming_Handled = true;
        }

        RenderTargetBitmap Screenshot(FrameworkElement element)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(element);

            //await Task.Delay(1);

            return renderTargetBitmap;
        }

        private async void Window_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {// new question
                if (e.Key == Key.N & !DebugFeatures)
                    newButton_Click(null, null);

#if DEBUG
                if (e.Key == Key.F12)
                {
                    DebugFeatures = !DebugFeatures;

                    if (DebugFeatures)
                        contentDialogHost.TextContentDialog("Debug features re-enabled!", "", true);
                }
#endif

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
            }

            if (Keyboard.IsKeyUp(Key.LeftShift))
            {
                newButton.Icon = "\ue109"[0].ToString();
                newButton.Text = "Új kérdés";

                shiftdeletewarningTextBlock.Visibility = Visibility.Hidden;
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
            }
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
                GameWindow overlay = new GameWindow(QuestionManager.GetQuestionByID(int.Parse(idbox.Text)));
                overlay.Show(); overlay.Activate();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Close the game window(s) we create.
            Application.Current.Shutdown();
        }

        private void debug_ReadQuestsJSONFile_Click(object sender, RoutedEventArgs e)
        {
            contentDialogHost.TextContentDialog("", QuestionManager.ReadQuestsListFromJSON_Literal());
        }

        private void debug_ClearJSON_Click(object sender, RoutedEventArgs e)
        {
            QuestionManager.ClearQuestionList();
        }

        private void automaticButton_Click(object sender, RoutedEventArgs e)
        {
            contentDialogHost.TextContentDialog("Todo",
                "This should automatically go through all questions in order, by ID.");
        }

        }
    }
}
