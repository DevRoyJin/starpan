using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Dokan;
using DiskAPIBase;
using System.Net;
using AliyunSDK;


namespace StarPan
{
    class Program
    {
        private static ICloudDiskAccessUtility GetUtiltiy(IEnumerable<ICloudDiskAccessUtility> list)
        {
            return list.OrderBy(li => li.GetFreeSpace()).LastOrDefault();
        }

        private static void Main(string[] args)
        {
            //get disk utility by some strategy
            //var utility = CloudDiskManager.Instance.GetClouDisk(GetUtiltiy);
            //Console.WriteLine("Get Utility {0}", utility.GetType());



            DokanOptions opt = new DokanOptions();
            opt.MountPoint = "r:\\";
            opt.DebugMode = true;
            opt.UseStdErr = true;
            opt.VolumeLabel = "SjtuPan";
            opt.RemovableDrive = true;
            int status = DokanNet.DokanMain(opt, new StarPanOperations());



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
