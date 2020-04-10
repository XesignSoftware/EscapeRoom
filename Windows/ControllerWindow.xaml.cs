using EscapeRoom.QuestionHandling;
using EscapeRoom.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public QuestionManager QuestionManager;
        Version Version = new Version();

        public ControllerWindow()
        {
            InitializeComponent();

            Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            // hide UI elements by default
            shiftdeletewarningTextBlock.Visibility = Visibility.Hidden;

            splashGrid.Visibility = Visibility.Visible;

            ThemeManager = new ThemeManager(Application.Current.Resources);
            ThemeManager.ConfigChanged += ThemeManager_ConfigChanged;

            // Set dark theme
            ThemeManager.Config_SetTheme(ThemeManager.Theme.Dark);
            ThemeManager.Config_SetAccent(ThemeManager.Accent.Pink);

            // Init QuestionHandler
            QuestionManager = new QuestionManager();
            QuestionManager.QuestionsChanged += QuestionManager_QuestionsChanged;
        }

        private void QuestionManager_QuestionsChanged(object sender, EventArgs e)
        {
            LoadQuestions();
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            contentDialogHost.TextContentDialog("An error has occured!", e.Exception.Message, true, "Continue anyway");
            e.Handled = true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            DebugFeatures = true;
            debugRow.Height = new GridLength(1, GridUnitType.Auto);
            this.Title += string.Format(" ({0}) [development]", Version.VersionNumber);
#else
            DisableDebugFeatures();
#endif

            if (Keyboard.IsKeyDown(Key.LeftShift) & Keyboard.IsKeyDown(Key.F12))
                DisableDebugFeatures();

            // Load version info
            LoadVersionInfo();

            // Load Question list
            LoadQuestions();

            await Task.Delay(TimeSpan.FromSeconds(.82));
            await Animation.FadeOutAsync(splashGrid);
        }

        public bool DebugFeatures = false;

        void DisableDebugFeatures()
        {
            DebugFeatures = false;
            debugRow.Height = new GridLength(0, GridUnitType.Pixel);
            debugButton.Visibility = Visibility.Collapsed;
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
                control.PlayClick += Control_PlayClick; ;
                questionListStackPanel.Children.Add(control); // add control to StackPanel
            }
        }

        async Task CallQuestionEditDialog(bool create, Question question, Dialogs.QuestionEditDialogContent comingfromPrev = null)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = create ? "Új kérdés létrehozása" : "Kérdés módosítása",
                PrimaryButtonText = create ? "Létrehozás" : "Mentés",
                SecondaryButtonText = "Mégse"
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

            // reload questions

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
            // reload questions list
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

        private void ThemeManager_ConfigChanged(object sender, EventArgs e)
        {

        }

        private async void Window_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
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
            }

            if (e.Key == Key.LeftShift)
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

            if (e.Key == Key.LeftShift)
            {
                newButton.Icon = "\ue8c8"[0].ToString();
                newButton.Text = "Utolsó duplikálása";

                shiftdeletewarningTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async void debugButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog() { Title = "", Content = "" };
            if (await contentDialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Primary)
            {

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
    }
}
