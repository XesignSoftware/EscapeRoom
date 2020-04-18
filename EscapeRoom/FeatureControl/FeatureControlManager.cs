using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom.FeatureControl
{
    public class FeatureControlManager
    {
        string GitHubFileLink = "";
        public string FeatureControlJSON = "EscapeRoom_FeatureControl.json";
        public List<Feature> allFeatures;
        public List<FeatureUniverse> universes;
        public FeatureControlManager(bool preloadFeatureGroups = true)
        {
            if (preloadFeatureGroups)
                LoadFeatureControl();
        }
        public void LoadFeatureControl()
        {
            universes = GetFeatureGroups();
        }
        public string GetPathForJSON(string file)
        {
            // Configuration directory path
            string configDir = AppDomain.CurrentDomain.BaseDirectory + @"Configuration\";
            string filePath = configDir + file;

            // If the configuration directory doesn't exist, create it.
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            if (!File.Exists(filePath))
                CreateFeatureControlFile(filePath, new EscapeRoomFeatureControl());

            return filePath;
        }
        public string GetPathForJSON()
        {
            return GetPathForJSON(FeatureControlJSON);
        }
        public string ReadJSON_Literal()
        {
            return File.ReadAllText(GetPathForJSON());
        }
        public List<FeatureUniverse> GetFeatureGroups()
        {
            if (universes != null)
                return universes;

            string file = File.ReadAllText(GetPathForJSON());
            return JsonConvert.DeserializeObject<List<FeatureUniverse>>(file);
        }
        public List<Feature> GetFeaturesFromUniverse(string universeName)
        {
            List<Feature> features = null;

            foreach (FeatureUniverse group in GetFeatureGroups())
                if (group.UniverseName == universeName)
                    features = group.Features;

            if (features == null)
                throw new Exception("FeatureControl: Couldn't find universe!");
            else
                return features;
        }
        public List<Feature> GetFeaturesFromAllUniverses()
        {
            if (allFeatures != null)
                return allFeatures;

            List<Feature> finalList = new List<Feature>();
            foreach (FeatureUniverse group in GetFeatureGroups())
            {
                foreach (Feature feature in group.Features)
                    finalList.Add(feature);
            }

            allFeatures = finalList;
            return finalList;
        }
        public Feature GetFeatureFromUniverse(string universeName, string featureName)
        {
            Feature targetFeature = null;

            foreach (FeatureUniverse group in GetFeatureGroups())
                if (group.UniverseName == universeName)
                    foreach (Feature feature in group.Features)
                        if (feature.DevName == featureName)
                            targetFeature = feature;

            return targetFeature;
        }
        public Feature GetFeature(string featureName)
        {
            Feature targetFeature = null;

            foreach (FeatureUniverse group in GetFeatureGroups())
            {
                foreach (Feature feature in group.Features)
                    if (feature.DevName == featureName)
                        targetFeature = feature;
            }

            return targetFeature;
        }
        public List<string> GetUniverseNames()
        {
            List<string> universeNames = new List<string>();
            List<FeatureUniverse> groups = GetFeatureGroups();

            foreach (FeatureUniverse group in groups)
                universeNames.Add(group.UniverseName);

            return universeNames;
        }
        public void ChangeUniverseFeatureValue(string universeName, string featureName, object newValue)
        {
            // change the Feautre in the global featuregroups
            foreach (FeatureUniverse group in GetFeatureGroups())
            {
                if (universeName == group.UniverseName)
                {
                    foreach (Feature feature in group.Features)
                        if (feature.DevName == featureName)
                            feature.Value = newValue;
                }
            }

            //// Get the ID for the group we're targetting
            //int targetGroupID = 0;
            //foreach (FeatureGroup group in GetFeatureGroups())
            //{
            //    if (group.Universe == universeName)
            //        break;
            //    else
            //        targetGroupID++;
            //}

            //// Get the ID for the feature we're targetting
            //int targetFeatureID = 0;
            //foreach (Feature feature in GetFeatureGroups()[targetGroupID].Features)
            //{
            //    if (feature.DevName == featureName || feature.Name == featureName)
            //        break;
            //    else
            //        targetFeatureID++;
            //}

            //// Change the value of the feature
            //GetFeatureGroups()[targetGroupID].Features[targetFeatureID].Value = newValue;

            // Re-serialize the JSON
            using (StreamWriter file = File.CreateText(GetPathForJSON()))
            {
                JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
                serializer.Serialize(file, universes);
            }
        }
        public void ChangeFeature(string featureName, object newValue)
        {
            // change the Feautre in the global featuregroups
            foreach (FeatureUniverse group in GetFeatureGroups())
            {
                foreach (Feature feature in group.Features)
                    if (feature.DevName == featureName)
                        feature.Value = newValue;
            }

            // Re-serialize the global JSON
            using (StreamWriter file = File.CreateText(GetPathForJSON()))
            {
                JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
                serializer.Serialize(file, universes);
            }
        }
        void CreateFeatureControlFileFromGitHub(string path)
        {

        }
        void CreateFeatureControlFile(string path, List<FeatureUniverse> groups)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented };
                ser.Serialize(file, groups);
            }
        }
        void CreateFeatureControlFile(string path, EscapeRoomFeatureControl featurecontrol)
        {
            CreateFeatureControlFile(path, (List<FeatureUniverse>)featurecontrol);
        }
        public void SerializeFeatureControl(List<FeatureUniverse> groups)
        {
            CreateFeatureControlFile(GetPathForJSON(), groups);
        }
        public void SerializeFeatureControl(EscapeRoomFeatureControl featurecontrol)
        {
            CreateFeatureControlFile(GetPathForJSON(), featurecontrol);
        }
        public void AddFeatureToUniverse(string universeName, Feature featuretoAdd)
        {
            foreach (FeatureUniverse group in GetFeatureGroups())
                if (group.UniverseName == universeName)
                    group.AddFeature(featuretoAdd);

            SerializeFeatureControl(universes);
        }

        // AppDirectory\Configuration\...
        public enum ConfigCategories { Application, User }
        public void DEBUG_CreateFeatureConfig()
        {
            List<FeatureUniverse> grouplist = new List<FeatureUniverse>();
            FeatureUniverse group = new FeatureUniverse() { UniverseName = "TestUniverse" };
            FeatureUniverse group2 = new FeatureUniverse() { UniverseName = "UniverseTest" };
            List<Feature> featurelist = new List<Feature>();

            for (int i = 0; i <= 10; i++)
            {
                Feature newfeature = new Feature()
                {
                    Name = "Test feature #" + i,
                    DevName = "TestFeature" + i,
                    Category = Feature.FeatureCategory.Experiment,
                    Value = true
                };

                featurelist.Add(newfeature);
            }

            group.Features = featurelist;
            group2.Features = featurelist;
            grouplist.Add(group);
            grouplist.Add(group2);

            using (StreamWriter file = File.CreateText(GetPathForJSON()))
            {
                JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
                serializer.Serialize(file, grouplist);
            }
        }
    }
}
