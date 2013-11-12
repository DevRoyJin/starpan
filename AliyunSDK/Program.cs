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
            //Just uncomment the code which you want to test.

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


            //string folderPath = "app/test/";
            //if (AliyunOssUtility.Instance.DeleteDirectory(folderPath))
            //{
            //    Console.WriteLine("Delete folder {0} succeeded.", folderPath);
            //}
            //else
            //{
            //    Console.WriteLine("Delete folder {0} failed.", folderPath);
            //}
            #endregion

            //get file list
            #region Get File List

            var list = AliyunOssUtility.Instance.GetFileList("app/test/");
            foreach (var f in list)
            {
                Console.WriteLine("FileName:{0}\r\nCreationTime:{1}\r\nModificationTime:{2}\r\nSize:{3}\r\nIsDir:{4}\r\n------------------------------------",
                        f.Path,new DateTime(f.CreateTime),new DateTime(f.ModifiyTime),f.Size,f.IsDir);
            }
            #endregion

            Console.ReadLine();
        }
    }
}
