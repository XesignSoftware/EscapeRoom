using System;
using System.Collections.Generic;
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
using XeZrunner.UI.Controls.Buttons;

namespace EscapeRoom
{
    public partial class QuestionControl : UserControl
    {
        public QuestionControl()
        {
            InitializeComponent();
        }

        public Question Question { get; set; }

        public QuestionControl(Question inc)
        {
            InitializeComponent();

            Question = inc;

            if (inc.QuestID.HasValue)
                ID = inc.QuestID.Value;
            Type = inc.QuestionType.ToString();
            Title = inc.QuestionTitle;
            Description = inc.QuestionDescription;
        }

        int _ID;
        public int ID
        {
            get { return _ID; }
            set { _ID = value; ID_TextBlock.Text = value.ToString(); }
        }

        public int NewID
        {
            set { ID_TextBlock.Text = string.Format("(was {0}, will be {1})", ID.ToString(),  value.ToString()); }
        }

        public string Type
        {
            get { return Type_TextBlock.Text; }
            set { Type_TextBlock.Text = value; }
        }

        public string Title
        {
            get { return Title_TextBlock.Text; }
            set { Title_TextBlock.Text = value; }
        }

        public string Description
        {
            get { return Desc_TextBlock.Text; }
            set { Desc_TextBlock.Text = value; }
        }

        public event EventHandler Click;
        public event EventHandler PlayClick;
        public event EventHandler DeleteClick;
        public event EventHandler<string> OrderingClick;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(Question, null);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteClick?.Invoke(Question, null);
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            PlayClick?.Invoke(Question, null);
        }

        private void orderingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (AppBarButton)sender;

            OrderingClick?.Invoke(this, (string)button.Tag);
        }
    }
}
