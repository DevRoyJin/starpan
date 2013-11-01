using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BaiduCloudAPI
{
    class Program
    {
        [STAThread()]
        private static void Main(string[] args)
        {
            ////upload
            //var res = BaiduPCSDiskUtility.Instance.UploadFile(@"C:\Users\10165298\Desktop\222.txt");
            //Console.WriteLine(res);
            

            ////download
            //try
            //{
            //    var filePcsPath = @"/apps/测试应用/Merge-0150_updated1.sql";
            //    var tempPath = filePcsPath.Replace('/', '\\');
            //    string path = Path.Combine("D:\\",tempPath.StartsWith("\\")?tempPath.Substring(1):tempPath);
            //    if (File.Exists(path))
            //    {
            //        File.Delete(path);
            //    }
            //    else
            //    {
            //        string folder = Path.GetDirectoryName(path);
            //        if(!Directory.Exists(folder))
            //        {
            //            Directory.CreateDirectory(folder);
            //        }
            //    }
            //    using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            //    {
            //        byte[] buf ;  
            //        if(BaiduPCSDiskUtility.Instance.DownloadFile(filePcsPath,out buf))
            //        {
            //            fs.Write(buf, 0, buf.Length);
            //        }

                    
            //    }
            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine(exception.ToString());
                
            //}


            //mkdir
            Console.WriteLine("Please input the path of directory:");
            var path =  @"/apps/测试应用/aaa";//Console.ReadLine();
            var ret = BaiduPCSDiskUtility.Instance.CreateDirectory(path);
            if (ret)
            {
                Console.WriteLine("Create directory succeeded.-->"+path);
            }
            else
            {
                Console.WriteLine("Create directory failed.-->" + path);
            }
            
          

            Console.ReadLine();
        }
    }
}
