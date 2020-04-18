using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoom
{
    public class MediaUtils
    {
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
    }

    public class UIBlurUtils
    {
        public double GetBlurLevel(string level)
        {
            switch (level)
            {
                case "Off":
                    return 0;
                case "Low":
                    return 15;
                case "High":
                    return 30;
                default:
                    return 15;
            }
        }
    }
}
