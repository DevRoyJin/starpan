using System;
using System.Collections.Generic;
using System.Text;

namespace StarPan
{
    class Program
    {
        static void Main(string[] args)
        {
            var instantce = CloudDiskManager.Instance;
            instantce.PrintFreeSpace();
            Console.ReadLine();
        }
    }
}
