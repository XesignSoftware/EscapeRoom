using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom.Configuration
{
    public class ConfigurationManager
    {
        public EscapeRoomConfig ReadConfigFromJSON()
        {
            string file = File.ReadAllText(GetPathForJSON(ConfigJSON));
            return JsonConvert.DeserializeObject<EscapeRoomConfig>(file);
        }

        public string ReadConfigFromJSON_Literal()
        {
            return File.ReadAllText(GetPathForJSON(ConfigJSON));
        }

        public string ConfigJSON = "EscapeRoom_Configuration.json";

        public string GetPathForJSON(string file)
        {
            string configDir = AppDomain.CurrentDomain.BaseDirectory + @"Configuration\";
            string filePath = configDir + file;

            // If the configuration directory doesn't exist, create it.
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            if (!File.Exists(filePath))
                CreateConfigFile(filePath, new EscapeRoomConfig());

            return configDir + file;
        }

        public void ResetConfiguration()
        {
            SerializeConfigJSON(new EscapeRoomConfig());
        }

        void CreateConfigFile(string path, EscapeRoomConfig config)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented };
                ser.Serialize(file, config);
            }
        }

        public void SerializeConfigJSON(EscapeRoomConfig config)
        {
            CreateConfigFile(GetPathForJSON(ConfigJSON), config);
        }
    }
}
