using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dokan;
using System.IO;
using Newtonsoft.Json.Linq;


namespace StarPan
{
    class StarPanOperations : DokanOperations
    {
        #region DokanOperations member


        private int Context = 0;
        private int count_;
        private string rootPath = "/apps/sjtupan/";
        private KuaiPan KuaiPan;
        private AllinOne allinone;
        private byte[] totalUploadByte;

        public StarPanOperations()
        {
            count_ = 1;
            //long[] spaceResult = BaiduPan.getQuata();
            KuaiPan = new KuaiPan();
            allinone  = new AllinOne();

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
           if (allinone.CreateDirectory(targetPath))
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

            #region 老方法 - 废弃，参考
            //try
            //{
            //    if (filename == "\\")
            //    {

            //        //info.IsDirectory = true;
            //        return DokanNet.DOKAN_SUCCESS;

            //    }


            //    if (name == "desktop.ini" || name == "folder.jpg" || name == "folder.gif" || name == @".svn") return -DokanNet.ERROR_FILE_NOT_FOUND;





            //    #region 通过List<FileElement>查询文件
            //    FileElement file = allinone.getSingleFileInfo(targetPath);
            //    if (file == null)
            //    {

            //    }

            //    else if ((access & System.IO.FileAccess.Read) == System.IO.FileAccess.Read)
            //    {


            //        if (file.isdir == System.IO.FileAttributes.Directory)
            //        {
            //            info.IsDirectory = true;
            //            return DokanNet.DOKAN_SUCCESS;
            //        }
            //        else
            //        {
            //            return DokanNet.DOKAN_SUCCESS;
            //        }

            //    }

            //    #endregion

            //    #region Mem盘的方法  - 废弃
            //    //FileElement file = allinone.getSingleFileInfo(targetPath);
            //    //if (file!=null)
            //    //{
            //    //    //this is a folder?
            //    //    info.IsDirectory = true;
            //    //    if (mode == FileMode.Open || mode == FileMode.OpenOrCreate)
            //    //    {
            //    //        //info.IsDirectory = true;
            //    //        return DokanNet.DOKAN_SUCCESS;
            //    //    }

            //    //    // you can't make a file with the same name as a folder;
            //    //    return -DokanNet.ERROR_ALREADY_EXISTS;
            //    //}
            //    #endregion
            //    // there's no folder with this name, the parent exists;
            //    // attempt to use the file
            //    switch (mode)
            //    {
            //        // Opens the file if it exists and seeks to the end of the file, 
            //        // or creates a new file
            //        case FileMode.Append:

            //            return DokanNet.DOKAN_SUCCESS;

            //        // Specifies that the operating system should create a new file. 
            //        // If the file already exists, it will be overwritten. 
            //        case FileMode.Create:
            //            //if (!thisFile.Exists())

            //            //else
            //            //	thisFile.Content = new Thought.Research.AweBuffer(1024); //MemoryStream();
            //            return DokanNet.DOKAN_SUCCESS;

            //        // Specifies that the operating system should create a new file. 
            //        // If the file already exists, an IOException is thrown.
            //        case FileMode.CreateNew:
            //            Console.WriteLine("try to create new file, file name is " + name);
            //            return DokanNet.DOKAN_SUCCESS;

            //        // Specifies that the operating system should open an existing file. 
            //        // A System.IO.FileNotFoundException is thrown if the file does not exist.
            //        case FileMode.Open:

            //            return DokanNet.DOKAN_SUCCESS;

            //        // Specifies that the operating system should open a file if it exists; 
            //        // otherwise, a new file should be created.
            //        case FileMode.OpenOrCreate:

            //            return DokanNet.DOKAN_SUCCESS;

            //        // Specifies that the operating system should open an existing file. 
            //        // Once opened, the file should be truncated so that its size is zero bytes
            //        case FileMode.Truncate:


            //            return DokanNet.DOKAN_SUCCESS;
            //    }





            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}

            //return DokanNet.DOKAN_SUCCESS;

            #endregion
            
            #region 13/11/1添加改用新方法

            if (filename == "\\")
            {

                //info.IsDirectory = true;
                return DokanNet.DOKAN_SUCCESS;

            }

            if (mode == FileMode.Open || mode == FileMode.OpenOrCreate)
            {
                FileElement file = allinone.getSingleFileInfo(targetPath);
                if (file != null)
                {
                    if (file.isdir == System.IO.FileAttributes.Normal)
                    {
                        return DokanNet.DOKAN_SUCCESS;
                    }

                    if (file.isdir == System.IO.FileAttributes.Directory)
                    {
                        info.IsDirectory = true;
                        return DokanNet.DOKAN_SUCCESS;
                    }
                }

                return -DokanNet.ERROR_FILE_NOT_FOUND;
            }

            return DokanNet.DOKAN_SUCCESS;

            #endregion
        }




        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            string targetPath = filename.Replace("\\", "/");
            if (allinone.removeFile(targetPath))
            {
                return DokanNet.DOKAN_SUCCESS;
            }

            return DokanNet.DOKAN_ERROR;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            string targetPath = filename.Replace("\\", "/");
            if (allinone.removeFile(targetPath))
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

            #region 百度盘获取文件列表 - 废弃
            //if (filename == "\\")
            //{
            //    try
            //    {
            //        FilesList myFiles = BaiduPan.getFilesList("/apps/sjtupan");
            //        for (int i = 0; i < myFiles.list.Length; i++)
            //        {
            //            FileInformation finfo = new FileInformation();
            //            int nameIndex = myFiles.list[i].path.Split(new Char[] { '/' }).Length;

            //            string name = myFiles.list[i].path.Split(new Char[] { '/' })[nameIndex - 1];
            //            finfo.FileName = name;

            //            if (myFiles.list[i].isdir == 1)
            //            {
            //                finfo.Attributes = System.IO.FileAttributes.Directory;
            //            }
            //            else
            //            {
            //                finfo.Attributes = System.IO.FileAttributes.Normal;
            //            }
            //            finfo.LastAccessTime = getRightTime(myFiles.list[i].mtime);
            //            finfo.LastWriteTime = getRightTime(myFiles.list[i].mtime);
            //            finfo.CreationTime = getRightTime(myFiles.list[i].ctime);
            //            //finfo.LastAccessTime = DateTime.Now;
            //            //finfo.LastWriteTime = DateTime.Now;
            //            //finfo.CreationTime = DateTime.Now;
            //            finfo.Length = (long)myFiles.list[i].size;


            //            files.Add(finfo);
            //        }
            //    }
            //    catch { }
            //}
            //else
            //{

            //    FilesList myFiles = BaiduPan.getFilesList("/apps/sjtupan" + targetPath);
            //    if (myFiles.list.Length == 0) return -1;
            //    for (int i = 0; i < myFiles.list.Length; i++)
            //    {
            //        FileInformation finfo = new FileInformation();
            //        int nameIndex = myFiles.list[i].path.Split(new Char[] { '/' }).Length;

            //        string name = myFiles.list[i].path.Split(new Char[] { '/' })[nameIndex - 1];
            //        finfo.FileName = name;

            //        if (myFiles.list[i].isdir == 1)
            //        {
            //            finfo.Attributes = System.IO.FileAttributes.Directory;
            //        }
            //        else
            //        {
            //            finfo.Attributes = System.IO.FileAttributes.Normal;
            //        }
            //        finfo.LastAccessTime = getRightTime(myFiles.list[i].mtime);
            //        finfo.LastWriteTime = getRightTime(myFiles.list[i].mtime);
            //        finfo.CreationTime = getRightTime(myFiles.list[i].ctime);
            //        //finfo.LastAccessTime = DateTime.Now;
            //        //finfo.LastWriteTime = DateTime.Now;
            //        //finfo.CreationTime = DateTime.Now;
            //        finfo.Length = (long)myFiles.list[i].size;

            //        files.Add(finfo);
            //    }

            //}

            #endregion

            #region 快盘获取文件列表  - 废弃
            //if (filename == "\\")
            //{
                //KuaiPanSDK.Model.MetaData filesList = KuaiPan.getFilesList("/");

                //foreach (KuaiPanSDK.Model.FileData file in filesList.Files)
                //{
                //    FileInformation finfo = new FileInformation();
            //        if (file.Type == KuaiPanSDK.TypeEnum.Folder)
            //        {
            //            finfo.Attributes = System.IO.FileAttributes.Directory;
            //        }
            //        else
            //        {
            //            finfo.Attributes = System.IO.FileAttributes.Normal;
            //        }
            //        finfo.FileName = file.Name;
            //        finfo.LastAccessTime = file.ModifyTime;
            //        finfo.LastWriteTime = file.ModifyTime;
            //        finfo.CreationTime = file.Createtime;
            //        finfo.Length = file.Size;
            //        files.Add(finfo);
            //    }

            //}
            //else
            //{
            //    KuaiPanSDK.Model.MetaData filesList = KuaiPan.getFilesList(targetPath);
            //    foreach (KuaiPanSDK.Model.FileData file in filesList.Files)
            //    {
            //        FileInformation finfo = new FileInformation();
            //        if (file.Type == KuaiPanSDK.TypeEnum.Folder)
            //        {
            //            finfo.Attributes = System.IO.FileAttributes.Directory;
            //        }
            //        else
            //        {
            //            finfo.Attributes = System.IO.FileAttributes.Normal;
            //        }
            //        finfo.FileName = file.Name;
            //        finfo.LastAccessTime = file.ModifyTime;
            //        finfo.LastWriteTime = file.ModifyTime;
            //        finfo.CreationTime = file.Createtime;
            //        finfo.Length = file.Size;
            //        files.Add(finfo);
            //    }
            //}

            #endregion

            #region 通过AllinOne类中的List<FileElement>遍历查询目录下文件
            foreach (FileElement file in allinone.returnAllFiles())
            {
                if (file.parentPath == targetPath || file.parentPath == targetPath + "/") //不清楚传入的filename后面是否带"/" - TBD
                {
                    FileInformation finfo = new FileInformation();
                    finfo.Attributes = file.isdir;
                    finfo.FileName = file.name;
                    finfo.LastAccessTime = file.accessTime;
                    finfo.LastWriteTime = file.motifyTime;
                    finfo.CreationTime = file.createTime;
                    finfo.Length = file.size;
                    files.Add(finfo);
                }
            }
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
                    #region 直接去网盘查询文件信息 - 废弃
                    //FilesList BaiduFiles = BaiduPan.searchFile("/apps/sjtupan" + targetPath);
                    //KuaiPanSDK.Model.MetaData KuaiPanFiles = KuaiPan.getFilesList(targetPath);
                    ////Console.WriteLine("BaiduFiles content: " + BaiduFiles + " filename is " + targetPath);

                    //if (BaiduFiles == null && KuaiPanFiles == null) return DokanNet.ERROR_FILE_NOT_FOUND;
                    //else
                    //{
                    //    if (BaiduFiles != null)
                    //    {
                    //        fileinfo.FileName = name;
                    //        fileinfo.LastAccessTime = getRightTime(BaiduFiles.list[0].mtime);
                    //        fileinfo.LastWriteTime = getRightTime(BaiduFiles.list[0].mtime);
                    //        fileinfo.CreationTime = getRightTime(BaiduFiles.list[0].ctime);
                    //        fileinfo.Length = (long)BaiduFiles.list[0].size;
                    //        if (BaiduFiles.list[0].isdir == 1)
                    //        {
                    //            fileinfo.Attributes = System.IO.FileAttributes.Directory;
                    //        }
                    //        else
                    //        {
                    //            fileinfo.Attributes = System.IO.FileAttributes.Normal;
                    //        }
                    //    }
                    //    else if (KuaiPanFiles != null)
                    //    {
                    //        if (KuaiPanFiles.Type == KuaiPanSDK.TypeEnum.Folder)
                    //        {
                    //            fileinfo.Attributes = System.IO.FileAttributes.Directory;
                    //        }
                    //        else
                    //        {
                    //            fileinfo.Attributes = System.IO.FileAttributes.Normal;
                    //        }
                    //        fileinfo.FileName = KuaiPanFiles.Name;
                    //        fileinfo.LastAccessTime = KuaiPanFiles.ModifyTime;
                    //        fileinfo.LastWriteTime = KuaiPanFiles.ModifyTime;
                    //        fileinfo.CreationTime = KuaiPanFiles.Createtime;
                    //        fileinfo.Length = KuaiPanFiles.Size;

                    //    }

                    //}

                    #endregion

                    #region 查找List<FileElement>中的对应文件信息
                    FileElement file = allinone.searchFile(targetPath);
                    if (file == null) return DokanNet.ERROR_FILE_NOT_FOUND;
                    else
                    {
                        fileinfo.FileName = file.name;
                        fileinfo.LastAccessTime = file.accessTime;
                        fileinfo.LastWriteTime = file.motifyTime;
                        fileinfo.CreationTime = file.createTime;
                        fileinfo.Length = file.size;
                        fileinfo.Attributes = file.isdir;
                        return DokanNet.DOKAN_SUCCESS;
                    }


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
            if (allinone.MoveFile(fromPath, toPath)) return DokanNet.DOKAN_SUCCESS;
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

            #region 直接从网盘读取字节流 - 废弃
            //try
            //{
            //    byte[] readFileBuffer = null;
            //        byte[] BaiduFileByte = BaiduPan.readFile("/apps/sjtupan" + targetPath);
            //        byte[] KuaiPanByte = KuaiPan.readFile(targetPath);
            //        if (BaiduFileByte != null) readFileBuffer = BaiduFileByte;
            //        else if (KuaiPanByte != null) readFileBuffer = KuaiPanByte;
            //        else
            //        {
            //            return DokanNet.DOKAN_ERROR;
            //        }
                



            //    MemoryStream ms = new MemoryStream(readFileBuffer, true);
            //    if (null == ms)
            //    {
            //        return -1;
            //    }
            //    //ms.Seek(offset, SeekOrigin.Begin);
            //    readBytes = (uint)ms.Read(buffer, 0, buffer.Length);
            //    return 0;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //    return -1;
            //}
            #endregion

            #region 利用List先判断哪个网盘再读取
            try
            {
                bool result = true;
                byte[] readFileBuffer=null;
                result =allinone.readFile(targetPath, out readFileBuffer);


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

            if (allinone.uploadFile(targetPath,buffer))
            {
                writtenBytes = (uint)buffer.Length;

                return DokanNet.DOKAN_SUCCESS;
            }

            return DokanNet.DOKAN_ERROR;

        }

        #endregion

    }
}
