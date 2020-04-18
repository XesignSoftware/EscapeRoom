using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom.FeatureControl
{
    public class Feature
    {
        /// <summary>
        /// The friendly name of the feature
        /// ex. Camera View Animation Experiment 1
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The dev name of the feature
        /// ex. CameraViewAnimExperiment1
        /// </summary>
        public string DevName { get; set; }

        /// <summary>
        /// The value of the "feature"
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The kind of "feature"
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FeatureCategory Category { get; set; }

        public enum FeatureCategory
        {
            Feature,
            Feature_WIP,
            FeatureVariable,
            Experiment,
            ExperimentVariable,
            SpecialEvent,
            SpecialEventVariable,
            InternalFeature,
            InternalFeatureVariable
        }
    }
}
