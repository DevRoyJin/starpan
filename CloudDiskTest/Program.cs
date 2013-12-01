using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AliyunSDK;
using BaiduCloudSDK;

namespace CloudDiskTest
{
    class Program
    {
        static void Main(string[] args)
        {        
            #region Baidu

            #region upload
            ////upload
            //using (var fs = File.Open(@"C:\Users\Roy\Desktop\2013-2014(1)kb.doc", FileMode.Open))
            //{
            //    byte[] buf = new byte[fs.Length];
            //    fs.Read(buf, 0, (int)fs.Length);
            //    var res = BaiduPCSDiskUtility.Instance.UploadFile("/apps/测试应用/test/2013-2014(1)kb.doc", buf);
            //    Console.WriteLine(res);
            //}

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
            //            f.Path, new DateTime(f.CreateTime), new DateTime(f.ModifiyTime), f.Size, f.IsDir);
            //    }
            //}


            #endregion

            #region GetFileInfo

            //var filePath = @"/apps/测试应用/sysclean.bat";
            //var f = BaiduPCSDiskUtility.Instance.GetFileInfo(filePath);
            //Console.WriteLine("FileName:{0}\r\nCreationTime:{1}\r\nModificationTime:{2}\r\nSize:{3}\r\nIsDir:{4}\r\n------------------------------------",
            //            f.Path, new DateTime(f.CreateTime), new DateTime(f.ModifiyTime), f.Size, f.IsDir);


            #endregion

            #endregion

            #region Aliyun
            ////GetUsedSpace
            #region GetQuota
            //Console.WriteLine("Aliyuan used space is:{0}",AliyunOssUtility.Instance.GetUsedSpace());
            #endregion

            //upload
            #region Upload
            //Console.WriteLine("Please input a file path:");
            //string path = Console.ReadLine();
            //if (File.Exists(path))
            //{
            //    using (var fs = new FileStream(path, FileMode.Open))
            //    {
            //        byte[] fd = null;

            //        using (var ms = new MemoryStream())
            //        {
            //            int count = 0;
            //            do
            //            {
            //                var buf = new byte[1024];
            //                count = fs.Read(buf, 0, 1024);
            //                ms.Write(buf, 0, count);
            //            } while (fs.CanRead && count > 0);
            //            fd = ms.ToArray();
            //        }

            //        if (AliyunOssUtility.Instance.UploadFile(string.Format("app/{0}", Path.GetFileName(path)), fd))
            //        {
            //            Console.WriteLine("UploadFile {0} succeeded.", path);
            //        }
            //        else
            //        {
            //            Console.WriteLine("UploadFile {0} failed.", path);
            //        }
            //    }
            //}
            #endregion

            //download
            #region Download
            //Console.WriteLine("Please input the file path you want to download from your bucket:");
            //string path = Console.ReadLine();
            //byte[] fileData;
            //if (AliyunOssUtility.Instance.DownloadFile(path, out fileData))
            //{
            //    var fileName = "";
            //    if (!string.IsNullOrEmpty(path))
            //    {
            //        var array = path.Split('/');
            //        fileName = array[array.Length - 1];
            //    }
            //    if (fileData != null && fileData.Length > 0)
            //    {
            //        string localPath = Path.Combine(@"d:\apps\", fileName);
            //        if (File.Exists(localPath))
            //        {
            //            File.Delete(localPath);
            //        }
            //        var fs = new FileStream(localPath, FileMode.CreateNew, FileAccess.ReadWrite);
            //        fs.BeginWrite(fileData, 0, fileData.Length, ar =>
            //            {
            //                fs.EndWrite(ar);
            //                fs.Close();
            //                fs = null;
            //            }, localPath);
            //    }

            //    Console.WriteLine("Download {0} succeeded.", path);
            //}
            //else
            //{
            //    Console.WriteLine("Download {0} failed.", path);
            //}
            #endregion

            ////mkdir
            #region Create folder
            //string path1 = "app/test3/";
            //if (AliyunOssUtility.Instance.CreateDirectory(path1))
            //{
            //    Console.WriteLine("The path {0} created succussfully.", path1);
            //}
            //else
            //{
            //    Console.WriteLine("The path {0} wasn't created.", path1);
            //}
            #endregion

            //delete folder/file
            #region Delete Folder
            //string filePath = "app/test/2013-2014(1)kb.doc";


            //if (AliyunOssUtility.Instance.DeleteFile(filePath))
            //{
            //    Console.WriteLine("Delete file {0} succeeded.", filePath);
            //}
            //else
            //{
            //    Console.WriteLine("Delete file {0} failed.", filePath);
            //}


            string folderPath = "test/";
            if (AliyunOssUtility.Instance.DeleteDirectory(folderPath))
            {
                Console.WriteLine("Delete folder {0} succeeded.", folderPath);
            }
            else
            {
                Console.WriteLine("Delete folder {0} failed.", folderPath);
            }
            #endregion

            //get file list
            #region Get File List

            //var list = AliyunOssUtility.Instance.GetFileList("test/");
            //foreach (var f in list)
            //{
            //    Console.WriteLine("FileName:{0}\r\nCreationTime:{1}\r\nModificationTime:{2}\r\nSize:{3}\r\nIsDir:{4}\r\n------------------------------------",
            //            f.Path, new DateTime(f.CreateTime), new DateTime(f.ModifiyTime), f.Size, f.IsDir);
            //}
            #endregion

            //get file info
            #region Get fileinfo
            //var f = AliyunOssUtility.Instance.GetFileInfo("app/test/云打印.docx");
            //Console.WriteLine("FileName:{0}\r\nCreationTime:{1}\r\nModificationTime:{2}\r\nSize:{3}\r\nIsDir:{4}\r\n------------------------------------",
            //            f.Path, new DateTime(f.CreateTime), new DateTime(f.ModifiyTime), f.Size, f.IsDir);
            #endregion
            #endregion

            Console.ReadLine();
        }
    }
}
