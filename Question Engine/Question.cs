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
        public enum QuestType { TextQuestion, ImageQuestion, MetaQuestion }
        public enum QuestInputType { Input, Choices }

        public enum QuestSuccessType { None, ImageText, UI, }
        public enum QuestFailureType { ShakePlayGrid, UI }

        public int? QuestID { get; set; }
        public QuestType QuestionType { get; set; }

        public string QuestionTitle { get; set; }
        public string QuestionDescription { get; set; }

        public string QuestionMediaPath { get; set; }

        public QuestInputType QuestionInputType { get; set; }
        public string QuestionInputSolution { get; set; }
        public List<string> QuestionChoices { get; set; }

        public QuestSuccessType QuestionSuccessType { get; set; } = QuestSuccessType.ImageText;
        public QuestFailureType QuestionFailureType { get; set; } = QuestFailureType.ShakePlayGrid;
        public string QuestionSuccessMediaPath { get; set; }
        public bool SkipOnFailure { get; set; } = false;
    }
}
