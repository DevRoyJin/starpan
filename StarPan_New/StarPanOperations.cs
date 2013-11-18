using System;
using System.IO;
using System.Linq;
using DiskAPIBase;
using Dokan;


namespace StarPan
{
    class StarPanOperations : DokanOperations
    {

        private int Context = 0;
        private int count_;
        private string rootPath = "/apps/sjtupan/";

        private ICloudDiskAccessUtility CloudDiskAccessUtility
        {
            get
            {
                return CloudDiskManager.Instance.GetCloudDisk(list => list.FirstOrDefault());

            }
        }

        #region DokanOperations member


      

        //private byte[] totalUploadByte;

        public StarPanOperations()
        {
            count_ = 1;
        }

        public int Cleanup(string filename, DokanFileInfo info)
        {
            //Console.WriteLine("Dokan CleanUp");
            return 0;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            //Console.WriteLine("Dokan CloseFile");
            return 0;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
           Console.WriteLine("Dokan CreateDirectory: " + filename);
           string targetPath = filename.Replace("\\", "/");
           if (CloudDiskAccessUtility.CreateDirectory(targetPath))
               return DokanNet.DOKAN_SUCCESS;
           else return DokanNet.DOKAN_ERROR;
        }

        public static string GetPathPart(string sourcePath)
        {
            return sourcePath.Substring(0, sourcePath.LastIndexOf('/')) + "/";
        }

        public static string GetFileName(string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
        }

        public int CreateFile(
            string filename,
            System.IO.FileAccess access,
            System.IO.FileShare share,
            System.IO.FileMode mode,
            System.IO.FileOptions options,
            DokanFileInfo info)
        {
            Console.WriteLine("Dokan CreateFile, filename is " + filename);
            Dokan.DokanNet.DokanResetTimeout(1000 * 30, info);
            info.Context = Context++;
            string targetPath = filename.Replace("\\", "/");
            String name = GetFileName(targetPath);
            
            #region 13/11/1添加改用新方法

            if (filename == "\\")
            {

                //info.IsDirectory = true;
                return DokanNet.DOKAN_SUCCESS;

            }

            if (mode == FileMode.Open || mode == FileMode.OpenOrCreate)
            {
                //FileElement file = CloudDiskAccessUtility.GetFileInfo(targetPath);
                //if (file != null)
                //{
                //    if (file.isdir == System.IO.FileAttributes.Normal)
                //    {
                //        return DokanNet.DOKAN_SUCCESS;
                //    }

                //    if (file.isdir == System.IO.FileAttributes.Directory)
                //    {
                //        info.IsDirectory = true;
                //        return DokanNet.DOKAN_SUCCESS;
                //    }
                //}

                return -DokanNet.ERROR_FILE_NOT_FOUND;
            }

            return DokanNet.DOKAN_SUCCESS;

            #endregion
        }




        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            string targetPath = filename.Replace("\\", "/");
            if (CloudDiskAccessUtility.DeleteFile(targetPath))
            {
                return DokanNet.DOKAN_SUCCESS;
            }

            return DokanNet.DOKAN_ERROR;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            string targetPath = filename.Replace("\\", "/");
            if (CloudDiskAccessUtility.DeleteFile(targetPath))
            {
                return DokanNet.DOKAN_SUCCESS;
            }

            return DokanNet.DOKAN_ERROR;
        }



        public int FlushFileBuffers(
            string filename,
            DokanFileInfo info)
        {
            return -1;
        }

        public int FindFiles(
            string filename,
            System.Collections.ArrayList files,
            DokanFileInfo info)
        {
            //SjtuPan code
            string targetPath = filename.Replace("\\", "/");
            Console.WriteLine("Dokan FindFiles, file name is " + filename);

            #region 通过AllinOne类中的List<FileElement>遍历查询目录下文件
            //foreach (FileElement file in allinone.returnAllFiles())
            //{
            //    if (file.parentPath == targetPath || file.parentPath == targetPath + "/") //不清楚传入的filename后面是否带"/" - TBD
            //    {
            //        FileInformation finfo = new FileInformation();
            //        finfo.Attributes = file.isdir;
            //        finfo.FileName = file.name;
            //        finfo.LastAccessTime = file.accessTime;
            //        finfo.LastWriteTime = file.motifyTime;
            //        finfo.CreationTime = file.createTime;
            //        finfo.Length = file.size;
            //        files.Add(finfo);
            //    }
            //}
            #endregion

            return DokanNet.DOKAN_SUCCESS;


        }

        public static DateTime getRightTime(long tmpTime)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long ITime = tmpTime * 10000000;
            TimeSpan toNow = new TimeSpan(ITime);
            return dtStart.Add(toNow);
        }

        public int GetFileInformation(
            string filename,
            FileInformation fileinfo,
            DokanFileInfo info)
        {


                Console.WriteLine("Dokan GetFileInformation, filename is " + filename);
                string targetPath = filename.Replace("\\", "/");
                String name = targetPath.Substring(targetPath.LastIndexOf('/') + 1);
                if (name == "desktop.ini" || name == "folder.jpg" || name == "folder.gif") return -1;
                if (filename == "\\")
                {
                    fileinfo.Attributes = System.IO.FileAttributes.Directory;
                    fileinfo.LastAccessTime = DateTime.Now;
                    fileinfo.LastWriteTime = DateTime.Now;
                    fileinfo.CreationTime = DateTime.Now;
                    //fileinfo.FileName = targetPath.Substring(targetPath.LastIndexOf('/') + 1);
                    //fileinfo.Length = 0;


                    return DokanNet.DOKAN_SUCCESS;

                }
                 else if (info.IsDirectory)
                {
                    fileinfo.Attributes = System.IO.FileAttributes.Directory;
                    fileinfo.LastAccessTime = DateTime.Now;
                    fileinfo.LastWriteTime = DateTime.Now;
                    fileinfo.CreationTime = DateTime.Now;
                    fileinfo.FileName = targetPath.Substring(targetPath.LastIndexOf('/') + 1);
                    fileinfo.Length = 0;


                    return DokanNet.DOKAN_SUCCESS;
                }
                else
                {
                    #region 查找List<FileElement>中的对应文件信息
                    //FileElement file = allinone.searchFile(targetPath);
                    //if (file == null) return DokanNet.ERROR_FILE_NOT_FOUND;
                    //else
                    //{
                    //    fileinfo.FileName = file.name;
                    //    fileinfo.LastAccessTime = file.accessTime;
                    //    fileinfo.LastWriteTime = file.motifyTime;
                    //    fileinfo.CreationTime = file.createTime;
                    //    fileinfo.Length = file.size;
                    //    fileinfo.Attributes = file.isdir;
                    //    return DokanNet.DOKAN_SUCCESS;
                    //}
                    return DokanNet.DOKAN_SUCCESS;

                    #endregion

                } 
        }

        public int LockFile(
            string filename,
            long offset,
            long length,
            DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int MoveFile(
            string filename,
            string newname,
            bool replace,
            DokanFileInfo info)
        {
            string fromPath = filename.Replace("\\", "/");
            string toPath = newname.Replace("\\", "/");
            if (CloudDiskAccessUtility.Move(fromPath, toPath)) return DokanNet.DOKAN_SUCCESS;
            else return DokanNet.DOKAN_ERROR;
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan OpenDirectory, filename is " + filename);
            info.Context = Context++;
            //string targetPath = filename.Replace("\\", "/");
            //if(allinone.getSingleFileInfo(targetPath)!=null)return DokanNet.DOKAN_SUCCESS;
            //return DokanNet.ERROR_PATH_NOT_FOUND;
            return DokanNet.DOKAN_SUCCESS;
        }

        public int ReadFile(
            string filename,
            byte[] buffer,
            ref uint readBytes,
            long offset,
            DokanFileInfo info)
        {
            Console.WriteLine("Dokan ReadFile, filename is " + filename + "; offset is " + offset);
            Dokan.DokanNet.DokanResetTimeout(1000 * 30, info);
            string targetPath = filename.Replace("\\", "/");

            #region 利用List先判断哪个网盘再读取
            try
            {
                bool result = true;
                byte[] readFileBuffer=null;
                result =CloudDiskAccessUtility.DownloadFile(targetPath, out readFileBuffer);


                MemoryStream ms = new MemoryStream(readFileBuffer, true);
                if (null == ms)
                {
                    return -1;
                }
                ms.Seek(offset, SeekOrigin.Begin);
                readBytes = (uint)ms.Read(buffer, 0, buffer.Length);
                return result?0:-1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
            #endregion
        }





        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetFileAttributes(
            string filename,
            System.IO.FileAttributes attr,
            DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetFileTime(
            string filename,
            DateTime ctime,
            DateTime atime,
            DateTime mtime,
            DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int Unmount(DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int GetDiskFreeSpace(
           ref ulong freeBytesAvailable,
           ref ulong totalBytes,
           ref ulong totalFreeBytes,
           DokanFileInfo info)
        {
            //long[] spaceResult = BaiduPan.getQuata();
            //freeBytesAvailable = (ulong)spaceResult[0] - (ulong)spaceResult[1];
            //totalBytes = (ulong)spaceResult[0];
            //totalFreeBytes = (ulong)spaceResult[0] - (ulong)spaceResult[1];
            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return 0;
        }

        public int WriteFile(
            string filename,
            byte[] buffer,
            ref uint writtenBytes,
            long offset,
            DokanFileInfo info)
        {
            

            Console.WriteLine("Dokan WriteFile, filename is " + filename + " offset is " + offset + " buffer length is "+buffer.Length);

            string targetPath = filename.Replace("\\", "/");

            if (CloudDiskAccessUtility.UploadFile(targetPath,buffer))
            {
                writtenBytes = (uint)buffer.Length;

                return DokanNet.DOKAN_SUCCESS;
            }

            return DokanNet.DOKAN_ERROR;

        }

        #endregion

    }
}
