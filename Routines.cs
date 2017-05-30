using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListViewInteraction.Routines
{
    public class Routines
    {
        public enum dirtype
        {
            file,
            folder
        };

        public static String getImageType(String imageType)
        {
            String returnString;
            switch (imageType)
            {
                case ".docx":
                case ".rtf":
                case ".doc":
                    returnString =  "Images/word.png";
                    break;
                case ".pptx":
                case ".ppt":
                    returnString =  "Images/ppt.png";
                    break;
                case ".pub":
                    returnString =  "Images/pub.png";
                    break;
                case ".xlsx":
                case ".xls":
                    returnString =  "Images/excel.png";
                    break;
                case ".c":
                case ".cs":
                case ".cpp":
                    returnString =  "Images/c.png";
                    break;
                case ".java":
                    returnString =  "Images/java.png";
                    break;
                case ".js":
                    returnString =  "Images/js.png";
                    break;
                case ".pdf":
                case ".ps":
                    returnString =  "Images/adobe.png";
                    break;
                case ".htm":
                case ".mhtml":
                case ".html":
                case ".chm":
                case ".asp":
                case ".aspx":
                    returnString =  "Images/htm.png";
                    break;
                case ".jpg":
                case ".png":
                case ".jpeg":
                case ".gif":
                case ".bmp":
                case ".tiff":
                    returnString =  "Images/pic.png";
                    break;
                case ".txt":
                case ".lst":
                case ".list":
                case ".log":
                    returnString =  "Images/txt.png";
                    break;
                case ".ini":
                case ".dll":
                case ".o":
                case ".sln":
                case ".bin":
                case ".dat":
                case ".ocx":
                    returnString =  "Images/conf.png";
                    break;
                default:
                    returnString =  "Images/un.png";
                    break;
            }
            return returnString;
        }
    }
}
