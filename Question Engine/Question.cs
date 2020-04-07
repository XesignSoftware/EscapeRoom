using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace EscapeRoom
{
    public class Question
    {
        public int? QuestID { get; set; }

        public enum QuestType
        {
            TextQuestion,
            ImageQuestion,
        }

        public enum QuestInputType
        {
            Input,
            Choices
        }

        public QuestType QuestionType { get; set; }

        public string QuestionTitle { get; set; }

        public string QuestionDescription { get; set; }

        public string QuestionMediaPath { get; set; }

        public string QuestionInputSolution { get; set; }

        public QuestInputType QuestionInputType { get; set; }

        public List<string> QuestionChoices { get; set; }
    }
}
