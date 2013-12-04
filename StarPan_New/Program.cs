using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;
using DiskAPIBase;
using Dokan;

namespace StarPan
{
    class Program
    {
        static void Main(string[] args)
        {
            var instantce = CloudDiskManager.Instance;
            instantce.PrintFreeSpace();

            var proxy = new DokanOperationProxy();
            proxy.PrintFileCount();
            proxy.PrintNode(proxy.Root);
            DokanOptions opt = new DokanOptions();
            opt.MountPoint = UtilityMethods.GetFirstAvailableDriveLetter() + ":\\";//"r:\\";
            opt.DebugMode = true;
            opt.UseStdErr = true;
            opt.VolumeLabel = "NetDisk";
            opt.RemovableDrive = true;
            //int status = DokanNet.DokanMain(opt, new StarPanOperations());
            int status = DokanNet.DokanMain(opt, new DokanCloudDiskOperation());



            switch (status)
            {
                case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                    Console.WriteLine("Drvie letter error");
                    break;
                case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                    Console.WriteLine("Driver install error");
                    break;
                case DokanNet.DOKAN_MOUNT_ERROR:
                    Console.WriteLine("Mount error");
                    break;
                case DokanNet.DOKAN_START_ERROR:
                    Console.WriteLine("Start error");
                    break;
                case DokanNet.DOKAN_ERROR:
                    Console.WriteLine("Unknown error");
                    break;
                case DokanNet.DOKAN_SUCCESS:
                    Console.WriteLine("Success");
                    break;
                default:
                    Console.WriteLine("Unknown status: %d", status);
                    break;
            }

        }
    }
}
