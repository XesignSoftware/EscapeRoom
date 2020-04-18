using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom.FeatureControl
{
    public class EscapeRoomFeatureControl : List<FeatureUniverse>
    {
        public EscapeRoomFeatureControl()
        {
            foreach (FeatureUniverse feature in list)
                this.Add(feature);
        }

        List<FeatureUniverse> list = new List<FeatureUniverse>()
        {
            new FeatureUniverse()
            {
                UniverseName = "Controller universe",
                Features = new List<Feature>()
                {
                    new Feature()
                    {
                        Name = "Blur application background in dialogs",
                        DevName = "ContentDialogHostBlur2020Q2",
                        Category = Feature.FeatureCategory.Experiment,
                        Value = true
                    }
                }
            }
        };
    }
}