using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BaiduCloudAPI;

namespace StarPan
{

    class FileElement
    {
        //struct of a single file property
        public string parentPath { get; set; } //contians '/' at the end of string , does not contain app_root part
        public DateTime createTime { get; set; }
        public DateTime motifyTime { get; set; }
        public DateTime accessTime { get; set; }
        public long size { get; set; }
        public FileAttributes isdir { get; set; } //System.IO.FileAttributes.Directory and System.IO.FileAttributes.Normal;
        public string name { get; set; } //only name, does not contain any '/' symbol
        public int origin { get; set; } //0=baidu; 1=kuaipan ; 2=?
    }

    class AllinOne
    {
        public List<FileElement> AllFiles { get; set; }
        private KuaiPan KuaiPan;
        private BaiduPan BaiduPan;
        private BaiduPCSDiskUtility BaiduSdk;

        public AllinOne()
        {
            //initialize and get all files property from each cloud storage and save to List<FileElement>
            KuaiPan = new KuaiPan();
            this.BaiduPan = new BaiduPan();
            AllFiles = new List<FileElement>();
            getFiles_Baidu(""); //从根遍历文件
            getFiles_KuaiPan("");//从根遍历文件
            Console.WriteLine("Get files list completed");
        }

        #region 小工具

        public DateTime getRightTime(long tmpTime)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long ITime = tmpTime * 10000000;
            TimeSpan toNow = new TimeSpan(ITime);
            return dtStart.Add(toNow);
        }

        public string GetFileName(string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
        }

        public string GetPathPart(string sourcePath)
        {
            return sourcePath.Substring(0, sourcePath.LastIndexOf('/')) + "/";
        }
        #endregion

        #region 获取目录下文件列表信息

        public void getFiles_Baidu(string path)
        {
            FilesList myFiles = BaiduPan.getFilesList(path);
            for (int i = 0; i < myFiles.list.Length; i++)
            {
                FileElement finfo = new FileElement();
                //int nameIndex = myFiles.list[i].path.Split(new Char[] { '/' }).Length;
                //string name = myFiles.list[i].path.Split(new Char[] { '/' })[nameIndex - 1];
                finfo.name = GetFileName(myFiles.list[i].path);

                if (myFiles.list[i].isdir == 1)
                {
                    finfo.isdir = System.IO.FileAttributes.Directory;
                    getFiles_Baidu(path + "/" + finfo.name);

                }
                else
                {
                    finfo.isdir = System.IO.FileAttributes.Normal;
                }
                finfo.accessTime = getRightTime(myFiles.list[i].mtime);
                finfo.motifyTime = getRightTime(myFiles.list[i].mtime);
                finfo.createTime = getRightTime(myFiles.list[i].ctime);
                finfo.parentPath = path + "/";
                finfo.size = (long)myFiles.list[i].size;
                finfo.origin = 0;
                AllFiles.Add(finfo);
            }
        }

        public void getFiles_KuaiPan(string path)
        {
            KuaiPanSDK.Model.MetaData filesList = KuaiPan.getFilesList(path);

            foreach (KuaiPanSDK.Model.FileData file in filesList.Files)
            {
                FileElement finfo = new FileElement();
                finfo.name = file.Name;
                if (file.Type == KuaiPanSDK.TypeEnum.Folder)
                {
                    finfo.isdir = System.IO.FileAttributes.Directory;
                    getFiles_KuaiPan(path + "/" + finfo.name);
                }
                else
                {
                    finfo.isdir = System.IO.FileAttributes.Normal;
                }

                finfo.accessTime = file.ModifyTime;
                finfo.motifyTime = file.ModifyTime;
                finfo.createTime = file.Createtime;
                finfo.origin = 1;
                finfo.parentPath = path + "/";
                finfo.size = file.Size;

                AllFiles.Add(finfo);
            }
        }

        #endregion

        public List<FileElement> returnAllFiles()
        {
            return AllFiles;
        }

        public FileElement searchFile(string path) //按照文件名查找List中的文件
        {
            string name = GetFileName(path);
            foreach (FileElement file in AllFiles)
            {
                if (file.name == name) return file;
            }
            return null;
        }

        public FileElement getSingleFileInfo(string path) //按照绝对路径查询List中的文件
        {

            foreach (FileElement file in AllFiles)
            {
                if (file.parentPath + file.name == path) return file;
            }
            return null;
        }

        public bool readFile(string path, out byte[] filebuffer) //read file content from cloud
        {
            //为防止出现在不同网盘的不同目录中存在相同文件名的文件，这里首先要判断所取文件的parentFolder在哪个网盘中

            byte[] buffer = null;
            bool result = true;
            if (GetPathPart(path) == "/") //若为根目录，则直接根据文件名查询文件
            {
                foreach (FileElement file in AllFiles) //find file info from List
                {
                    if (file.parentPath + file.name == path) //path could be found in List
                    {
                        if (file.origin == 0) //from Baidu pan
                        {
                            result = BaiduPan.DownloadFile(path, out buffer);

                        }
                        if (file.origin == 1) //from KuaiPan
                        {
                            result = KuaiPan.DownloadFile(path, out buffer);

                        }

                        if (file.origin == 2)
                        {

                        }
                    }
                }
            }
            else //非根目录，则取parentFolder，查询其在哪个网盘中再读文件
            {
                foreach (FileElement file in AllFiles)
                {
                    if (file.parentPath + file.name + "/" == GetPathPart(path)) //上传文件时，传入的path型如"/hellosjtu/ss.txt"，ss.txt为新文件名
                    {
                        if (file.origin == 0)
                        {

                            Console.WriteLine("Read file from Baidu, file name is " + path);
                            result = BaiduPan.DownloadFile(path, out buffer);

                        }
                        else if (file.origin == 1)
                        {
                            Console.WriteLine("Read file from KuaiPan, file name is " + path);
                            result = KuaiPan.DownloadFile(path, out buffer);


                        }
                        else if (file.origin == 2) { }


                    }
                }
            }
            filebuffer = buffer;
            return result ? true : false;
        }

        public bool uploadFile(string path, byte[] buffer) //upload file
        {
            FileElement newfile = new FileElement();
            if (GetPathPart(path) == "/")
            {
                Random rd = new Random();
                int i = rd.Next(0, 2);
                Console.WriteLine("Random number is " + i);
                if (i == 0)
                {
                    Console.WriteLine("New file create in Baidu");
                    BaiduPan.UploadFile(path, buffer);
                }
                else if (i == 1)
                {
                    Console.WriteLine("New file create in KuaiPan");
                    KuaiPan.UploadFile(path, buffer);
                }
                else if (i == 2) { }

                newfile.origin = i;
            }
            else
            {
                foreach (FileElement file in AllFiles)
                {
                    if (file.parentPath + file.name + "/" == GetPathPart(path)) //上传文件时，传入的path型如"/hellosjtu/ss.txt"，ss.txt为新文件名
                    {
                        if (file.origin == 0)
                        {

                            Console.WriteLine("New file create in Baidu");
                            BaiduPan.UploadFile(path, buffer);
                        }
                        else if (file.origin == 1)
                        {
                            Console.WriteLine("New file create in KuaiPan");
                            KuaiPan.UploadFile(path, buffer);
                        }
                        else if (file.origin == 2) { }

                        newfile.origin = file.origin;
                    }
                }
            }


            newfile.name = GetFileName(path);
            newfile.accessTime = DateTime.Now;
            newfile.createTime = DateTime.Now;
            newfile.motifyTime = DateTime.Now;
            newfile.isdir = FileAttributes.Normal;
            newfile.parentPath = GetPathPart(path);

            newfile.size = buffer.Length;
            AllFiles.Add(newfile);

            return true;
        }

        public bool removeFile(string path)
        {
            bool result = true;
            FileElement fileTobeRemoved = null;
            foreach (FileElement file in AllFiles)
            {
                if (file.parentPath + file.name == path)
                {
                    if (file.origin == 0)
                    {
                        result = BaiduPan.DeleteFile(path);
                    }

                    else if (file.origin == 1)
                    {
                        result = KuaiPan.DeleteFile(path);
                    }
                    else if (file.origin == 2)
                    {
                        //others
                    }
                    fileTobeRemoved = file;
                }
            }

            AllFiles.Remove(fileTobeRemoved);
            return result;
        }

        public bool CreateDirectory(string path)
        {
            bool result = true;
            FileElement newfile = new FileElement();
            if (GetPathPart(path) == "/")
            {
                Random rd = new Random();
                int i = rd.Next(0, 2);
                //Console.WriteLine("Random number is " + i);
                if (i == 0)
                {
                    Console.WriteLine("New directory create in Baidu");
                    result=BaiduPCSDiskUtility.Instance.CreateDirectory(path);
                }
                else if (i == 1)
                {
                    Console.WriteLine("New directory create in KuaiPan");
                    result=KuaiPan.CreateDirectory(path);
                }
                else if (i == 2) { }

                newfile.origin = i;
            }
            else
            {
                foreach (FileElement file in AllFiles)
                {
                    if (file.parentPath + file.name + "/" == GetPathPart(path)) //上传文件时，传入的path型如"/hellosjtu/ss.txt"，ss.txt为新文件名
                    {
                        if (file.origin == 0)
                        {

                            Console.WriteLine("New directory create in Baidu");
                            result=BaiduPCSDiskUtility.Instance.CreateDirectory(path);
                        }
                        else if (file.origin == 1)
                        {
                            Console.WriteLine("New directory create in KuaiPan");
                            result=KuaiPan.CreateDirectory(path);
                        }
                        else if (file.origin == 2) { }

                        newfile.origin = file.origin;
                    }
                }


            }
            newfile.name = GetFileName(path);
            newfile.accessTime = DateTime.Now;
            newfile.createTime = DateTime.Now;
            newfile.motifyTime = DateTime.Now;
            newfile.isdir = FileAttributes.Directory;
            newfile.parentPath = GetPathPart(path);

            newfile.size = 0;
            AllFiles.Add(newfile);

            return result;

        }


    }

    public interface IDiskUtility
    {
        long GetQuota();

        long GetUsedSpace();

        long GetFreeSpace();

        bool UploadFile(string path, byte[] buffer);

        bool DownloadFile(string path, out byte[] filebuffer);

        bool CreateDirectory(string path);

        bool DeleteDirectory(string path);

        bool DeleteFile(string path);

    }
}
