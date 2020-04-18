using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom.FeatureControl
{
    public class FeatureUniverse
    {
        public string UniverseName { get; set; }
        public List<Feature> Features { get; set; }

        public void AddFeature(Feature feature)
        {
            Features.Add(feature);
        }
        public void RemoveFeature(Feature feature)
        {
            Features.Remove(feature);
        }
    }
}
