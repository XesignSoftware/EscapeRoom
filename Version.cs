using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom
{
    public class Version
    {
        public string VersionNumber = "20200403-01";
        public string BuildType
        {
            get
            {
#if DEBUG
                return "dev";
#else
                return "prod";
#endif
            }
        }

        public string Channel
        {
            get
            {
#if DEBUG
                return "dev-master";
#else
                return "rel-master";
#endif
            }
        }
    }
}
