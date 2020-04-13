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
        public string QuestsJSON = "EscapeRoom_Quests.json";
        public QuestionManager()
        {

        }
        public event EventHandler QuestionsChanged;
        public event EventHandler RemoveFailed;
        public event EventHandler ModifyFailed;

        #region JSON Read & get
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
        public string ReadQuestsListFromJSON_Literal()
        {
            return File.ReadAllText(GetPathForJSON(QuestsJSON));
        }
        
        #endregion

        #region Question functions
        public List<Question> GetQuestsFromJSON()
        {
            string file = File.ReadAllText(GetPathForJSON(QuestsJSON));
            return JsonConvert.DeserializeObject<List<Question>>(file);
        }
        public int GetQuestionCount()
        {
            return GetQuestsFromJSON().Count;
        }
        public Question GetQuestionByID(int id)
        {
            List<Question> list = GetQuestsFromJSON();

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

        public void AddQuestion(Question Question, bool invokeEvent = true)
        {
            List<Question> list = GetQuestsFromJSON();

            // check if quest has an ID
            if (!Question.QuestID.HasValue & Question.QuestionType != Question.QuestType.MetaQuestion) // if there's no quest ID, assign +1 based on list
            {
                int itemCount = list.Count -1;
                Question.QuestID = itemCount;
            }

            list.Add(Question);

            // Serialize the list into JSON
            SerializeQuestsJSON(list);

            // add meta quest if doesn't exist
            Question metaQuest = null;
            foreach (Question quest in list)
            {
                if (quest.QuestionType == Question.QuestType.MetaQuestion)
                    metaQuest = quest;
            }

            if (metaQuest == null)
                AddQuestion(new Question() { QuestionTitle = "Game configuration", QuestionType = Question.QuestType.MetaQuestion });

            // call event
            if (invokeEvent)
                QuestionsChanged?.Invoke(null, null);
        }
        public void RemoveQuestion(int QuestionID, bool invokeEvent = true)
        {
            List<Question> list = GetQuestsFromJSON();
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

            if (invokeEvent)
                QuestionsChanged?.Invoke(null, null);
        }
        public void RemoveQuestion(Question question, bool invokeEvent = true)
        {
            if (question.QuestID.HasValue)
                RemoveQuestion(question.QuestID.Value, invokeEvent);
            else
                throw new Exception("QuestID is null!");
        }
        public void RemoveLastQuestion()
        {
            List<Question> list = GetQuestsFromJSON();

            // find ID of last question
            if (list.Count > 1)
                RemoveQuestion(list.Count - 1);
            else if (list.Count == 1)
                RemoveQuestion(0);
            else
                throw new Exception("No question to delete!");
        }
        public void OrderQuestion(int oldID, int newID)
        {
            List<Question> list = GetQuestsFromJSON();

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
        public void ModifyQuestion(Question newQuestion, bool invokeEvent = true)
        {
            List<Question> list = GetQuestsFromJSON();

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

            if (invokeEvent)
                QuestionsChanged?.Invoke(null, null);
        }
        public void DuplicateQuestion(bool invokeEvent = true)
        {
            List<Question> list = GetQuestsFromJSON();
            Question quest = GetQuestionByID((GetQuestionCount() - 1));
            if (quest == null) // last entry is meta
                quest = GetQuestionByID(GetQuestionCount() - 2);

            DuplicateQuestion(quest, invokeEvent);
        }
        public void DuplicateQuestion(Question questToDuplicate, bool invokeEvent = true)
        {
            var newQuestion = questToDuplicate;
            newQuestion.QuestID += 1;

            AddQuestion(newQuestion, invokeEvent);
        }
        #endregion

        #region JSON functions
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
        void WriteEmptyQuestsJSON(string path)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented };
                ser.Serialize(file, new List<Question>());
            }
        }
        #endregion

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
