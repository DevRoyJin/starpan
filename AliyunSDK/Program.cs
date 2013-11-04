using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AliyunSDK
{
    class Program
    {
        [STAThread()]
        private static void Main(string[] args)
        {
            ////GetUsedSpace
            //Console.WriteLine("Aliyuan used space is:{0}",AliyunOssUtility.Instance.GetUsedSpace());



            //upload
            Console.WriteLine("Please input a file path:");
            string path = Console.ReadLine();
            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    byte[] fd = null;

                    using (var ms = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            var buf = new byte[1024];
                            count = fs.Read(buf, 0, 1024);
                            ms.Write(buf, 0, count);
                        } while (fs.CanRead && count > 0);
                        fd = ms.ToArray();
                    }
                    
                    if (AliyunOssUtility.Instance.UploadFile(string.Format("app/{0}", Path.GetFileName(path)), fd))
                    {
                        Console.WriteLine("UploadFile {0} succeeded.", path);
                    }
                    else
                    {
                        Console.WriteLine("UploadFile {0} failed.", path);
                    }
                }
            }
            

            //download
           


            ////mkdir
            //string path = "app/test/";
            //if (AliyunOssUtility.Instance.CreateDirectory("app/test/"))
            //{
            //    Console.WriteLine("The path {0} created succussfully.",path);
            //}
            //else
            //{
            //    Console.WriteLine("The path {0} wasn't created.", path);
            //}
            
          

            Console.ReadLine();
        }
    }
}
