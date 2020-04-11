using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EscapeRoom.QuestionHandling
{
    public class QuestionManager
    {
        public QuestionManager()
        {

        }

        public List<Question> ReadQuestsListFromJSON()
        {
            string file = File.ReadAllText(GetPathForJSON(QuestsJSON));
            return JsonConvert.DeserializeObject<List<Question>>(file);
        }

        public string ReadQuestsListFromJSON_Literal()
        {
            return File.ReadAllText(GetPathForJSON(QuestsJSON));
        }

        public string QuestsJSON = "EscapeRoom_Quests.json";

        public int GetQuestionCount()
        {
            return ReadQuestsListFromJSON().Count;
        }

        public event EventHandler QuestionsChanged;

        public void AddQuestion(Question Question)
        {
            List<Question> list = ReadQuestsListFromJSON();

            // check if quest has an ID
            if (!Question.QuestID.HasValue) // if there's no quest ID, assign +1 based on list
            {
                int itemCount = list.Count;
                Question.QuestID = itemCount;
            }

            list.Add(Question);

            // Serialize the list into JSON
            SerializeQuestsJSON(list);

            // call event
            QuestionsChanged?.Invoke(null, null);
        }

        public event EventHandler RemoveFailed;

        public void RemoveQuestion(int QuestionID)
        {
            List<Question> list = ReadQuestsListFromJSON();
            bool removeSuccessful = false;

            // find ID of Question in list
            int counter = 0;
            foreach (Question quest in list)
            {
                if (quest.QuestID == QuestionID) // if we found the ID, remove from list
                {
                    list.RemoveAt(counter);
                    removeSuccessful = true;
                    break;
                }

                counter++;
            }

            if (!removeSuccessful)
            {
                RemoveFailed.Invoke(null, null);
                return;
            }

            // Serialize the list into JSON
            SerializeQuestsJSON(list);

            QuestionsChanged?.Invoke(null, null);
        }

        public void RemoveLastQuestion()
        {
            List<Question> list = ReadQuestsListFromJSON();

            // find ID of last question
            if (list.Count > 1)
                RemoveQuestion(list.Count - 1);
            else if (list.Count == 1)
                RemoveQuestion(0);
            else
                throw new Exception("No question to delete!");
        }

        public void RemoveQuestion(Question question)
        {
            if (question.QuestID.HasValue)
                RemoveQuestion(question.QuestID.Value);
            else
                throw new Exception("QuestID is null!");
        }

        public void OrderQuestion(int oldID, int newID)
        {
            List<Question> list = ReadQuestsListFromJSON();

            var quest = list[oldID];
            var tbreplacedQuest = list[newID];

            if (newID == 0)
                list.Insert(newID, quest);
            else if (newID < oldID)
                list.Insert(newID - 1, quest);
            else if (newID > oldID)
                list.Insert(newID + 1, quest);

            if (newID < oldID)
                list.RemoveAt(oldID + 1);
            else
                list.RemoveAt(oldID);

            quest.QuestID = newID;
            tbreplacedQuest.QuestID = oldID;

            SerializeQuestsJSON(list);

            /* Don't invoke QuestionsChanged, as we want to manually re-arrange the items in the controller
             * for performance!
            //QuestionsChanged?.Invoke(null, null);
            */
        }

        public event EventHandler ModifyFailed;

        public void ModifyQuestion(Question newQuestion)
        {
            List<Question> list = ReadQuestsListFromJSON();

            // find the Question to modify
            int counter = 0;
            foreach (Question quest in list)
            {
                if (quest.QuestID == newQuestion.QuestID)
                    break;

                counter++;
            }

            Question targetQuestion = list[counter];

            if (targetQuestion == null)
            {
                ModifyFailed.Invoke(null, null);
                return;
            }

            // Modify the Question
            list.RemoveAt(counter);
            list.Insert(counter, newQuestion);

            // Serialize the list
            SerializeQuestsJSON(list);

            QuestionsChanged?.Invoke(null, null);
        }

        public void DuplicateQuestion()
        {
            DuplicateQuestion(GetQuestionByID(GetQuestionCount() - 1));
        }

        public void DuplicateQuestion(Question questToDuplicate)
        {
            var newQuestion = questToDuplicate;
            newQuestion.QuestID += 1;

            AddQuestion(newQuestion);
        }

        public void ClearQuestionList()
        {
            SerializeQuestsJSON(new List<Question>());
            QuestionsChanged?.Invoke(null, null);
        }

        public void SerializeQuestsJSON(List<Question> list)
        {
            using (StreamWriter file = File.CreateText(GetPathForJSON(QuestsJSON)))
            {
                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented };
                ser.Serialize(file, list);
            }
        }

        public string GetPathForJSON(string file)
        {
            string configDir = AppDomain.CurrentDomain.BaseDirectory + @"Configuration\";
            string filePath = configDir + file;

            // If the configuration directory doesn't exist, create it.
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            if (!File.Exists(filePath))
                WriteEmptyQuestsJSON(filePath);

            return configDir + file;
        }

        void WriteEmptyQuestsJSON(string path)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented };
                ser.Serialize(file, new List<Question>());
            }
        }

        public Question GetQuestionByID(int id)
        {
            List<Question> list = ReadQuestsListFromJSON();

            bool successful = false;

            int counter = 0;
            foreach (Question quest in list)
            {
                if (quest.QuestID == id)
                {
                    successful = true; break;
                }
                counter++;
            }

            if (successful)
                return list[counter];
            else
                return null;
        }

        public bool IsValidMediaFile(string mediaPath)
        {
            switch (GetFileExtension(mediaPath))
            {
                case ".jpg":
                case ".png":
                    return true;
                default:
                    return false;
            }
        }

        public string GetFileExtension(string filePath)
        {
            return filePath.Substring(filePath.Length - 4);
        }

        /// <summary>
        /// Creates a test Question JSON file.
        /// </summary>
        public void DEBUG_CreateTestQuestionList()
        {
            // Remove all Questions
            ClearQuestionList();

            // Create 10 test Questions and add them to the list
            for (int i = 0; i <= 0; i++)
            {
                AddQuestion(new Question
                {
                    QuestionType = Question.QuestType.TextQuestion,
                    QuestionTitle = "Hányféle macskafajta létezik a Földön?",
                    QuestionDescription = "Ez egy nagyon fontos kérdés - ne rontsd el!"
                });
            }

            QuestionsChanged?.Invoke(null, null);
        }
    }
}
