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
}
