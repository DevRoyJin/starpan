using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;
using DiskAPIBase;

namespace StarPan
{
    class Program
    {
        static void Main(string[] args)
        {
            //string root = "/apps";
            //string path = "111/22/3.txt";
            //path = PathHelper.CombineWebPath(root, path);
            
            var instantce = CloudDiskManager.Instance;
            instantce.PrintFreeSpace();
            Console.ReadLine();
        }
    }
}
