using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BaiduCloudSDK
{
    class Program
    {
        [STAThread()]
        private static void Main(string[] args)
        {
            #region upload
            ////upload
            using (var fs = File.Open(@"C:\Users\Roy\Desktop\2013-2014(1)kb.doc", FileMode.Open))
            {
                byte[] buf = new byte[fs.Length];
                fs.Read(buf, 0, (int)fs.Length);
                var res = BaiduPCSDiskUtility.Instance.UploadFile("/apps/测试应用/test/2013-2014(1)kb.doc", buf);
                Console.WriteLine(res);
            }
            
            #endregion

            #region download
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
            #endregion

            #region mkdir
            ////mkdir
            //Console.WriteLine("Please input the path of directory:");
            //var path =  @"/apps/测试应用/aaa";//Console.ReadLine();
            //var ret = BaiduPCSDiskUtility.Instance.CreateDirectory(path);
            //if (ret)
            //{
            //    Console.WriteLine("Create directory succeeded.-->"+path);
            //}
            //else
            //{
            //    Console.WriteLine("Create directory failed.-->" + path);
            //}
            #endregion

            #region GetFileList

            //var dirPath = @"/apps/测试应用";
            //var flist = BaiduPCSDiskUtility.Instance.GetFileList(dirPath);
            //if (flist != null && flist.Count > 0)
            //{
            //    foreach (var f in flist)
            //    {                            
            //        Console.WriteLine("FileName:{0}\r\nCreationTime:{1}\r\nModificationTime:{2}\r\nSize:{3}\r\nIsDir:{4}\r\n------------------------------------",
            //            f.Path,new DateTime(f.CreateTime),new DateTime(f.ModifiyTime),f.Size,f.IsDir);
            //    }
            //}


            #endregion


            Console.ReadLine();
        }
    }
}
