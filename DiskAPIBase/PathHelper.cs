using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DiskAPIBase
{
    public class PathHelper
    {
        public static string ConvertToWebPath(string path)
        {
            if (path.Contains("\\"))
            {
                path = path.Replace("\\", "/");
            }
            return path;
        }

        public static string ConvertToLocalPath(string path)
        {
            if (path.Contains("/"))
            {
                path = path.Replace("/", ""+Path.DirectorySeparatorChar);
            }
            return path;
        }

        public static string CombineWebPath(string path1, string path2)
        {
            path1 = ConvertToLocalPath(path1);
            path2 = ConvertToLocalPath(path2);

            while(path2.StartsWith("\\"))
            {
                path2 = path2.Substring(1);
            }

            var path = Path.Combine(path1, path2);
            return ConvertToWebPath(path);
        }
    }
}
