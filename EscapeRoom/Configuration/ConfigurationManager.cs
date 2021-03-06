﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EscapeRoom.Configuration
{
    public class ConfigurationManager
    {
        public MediaUtils MediaUtils = new MediaUtils();
        public string ConfigJSON = "EscapeRoom_Configuration.json";
        public ConfigurationManager()
        {

        }

        // JSON Read & get
        public string GetPathForJSON(string file)
        {
            // Configuration directory path
            string configDir = AppDomain.CurrentDomain.BaseDirectory + @"Configuration\";
            string filePath = configDir + file;

            // If the configuration directory doesn't exist, create it.
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            if (!File.Exists(filePath))
                CreateConfigFile(filePath, new EscapeRoomConfig());

            return filePath;
        }
        public string ReadConfigFromJSON_Literal()
        {
            return File.ReadAllText(GetPathForJSON(ConfigJSON));
        }

        void CreateConfigFile(string path, EscapeRoomConfig config)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented };
                ser.Serialize(file, config);
            }
        }
        public EscapeRoomConfig ReadConfigFromJSON()
        {
            string file = File.ReadAllText(GetPathForJSON(ConfigJSON));

            // if the config file is empty, create a new empty config file and return that
            if (file == "" || file == "[]")
            {
                ResetConfiguration();
                return ReadConfigFromJSON();
            }

            return JsonConvert.DeserializeObject<EscapeRoomConfig>(file);
        }
        public void ResetConfiguration()
        {
            SerializeConfigJSON(new EscapeRoomConfig());
        }
        void SerializeConfigJSON(EscapeRoomConfig config)
        {
            CreateConfigFile(GetPathForJSON(ConfigJSON), config);
        }
        public void Save(EscapeRoomConfig config) { SerializeConfigJSON(config); }

        public bool IsValidMediaFile(string mediaPath)
        {
            return MediaUtils.IsValidMediaFile(mediaPath);
        }
        public string GetFileExtension(string filePath)
        {
            return MediaUtils.GetFileExtension(filePath);
        }
    }
}
