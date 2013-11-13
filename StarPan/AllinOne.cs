using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BaiduCloudSDK;
using AliyunSDK;
using DiskAPIBase;

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
        
        private byte[] uploadTotalBuffer=null;
        private FileElement ToUploadFile;

        public AllinOne()
        {
            //initialize and get all files property from each cloud storage and save to List<FileElement>
            KuaiPan = new KuaiPan();
            this.BaiduPan = new BaiduPan();
            AllFiles = new List<FileElement>();
            getFiles_Baidu(""); //从根遍历文件
            getFiles_KuaiPan("");//从根遍历文件
            getFiles_OSS("");//从根遍历文件
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


        #region 定义网盘写策略
        private static ICloudDiskAccessUtility GetUtiltiy_ByRandom(IEnumerable<ICloudDiskAccessUtility> list)
        {
            Random rd = new Random();
            int i = rd.Next(0, 3);
            
            int count = 0;
            foreach (var f in list)
            {
                if (i == count) return f;
                count++;
            }


            return list.LastOrDefault();
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

        public void getFiles_OSS(string path)
        {
            
            var list = AliyunOssUtility.Instance.GetFileList(path);
            foreach (var f in list)
            {
                f.Path = "/" + f.Path;
                FileElement finfo = new FileElement();
               
                if (f.IsDir == true)
                {
                    finfo.name = GetFileName(f.Path.Substring(0, f.Path.LastIndexOf('/')));
                    finfo.parentPath = GetPathPart(f.Path.Substring(0, f.Path.LastIndexOf('/')));
                    finfo.isdir = System.IO.FileAttributes.Directory;
                    //getFiles_OSS(f.Path.Substring(0, f.Path.LastIndexOf('/'))); //如果是目录，则截取目录路径，OSS返回的f.path为folder/的形式，注意最后带有/符号
                }
                else
                {
                    finfo.isdir = System.IO.FileAttributes.Normal;
                    finfo.parentPath = GetPathPart(f.Path);
                    finfo.name = GetFileName(f.Path);
                }
                finfo.accessTime = new DateTime(f.ModifiyTime);
                finfo.motifyTime = new DateTime(f.ModifiyTime);
                finfo.createTime = new DateTime(f.CreateTime);
                finfo.origin = 2;
                
                finfo.size = f.Size;
                AllFiles.Add(finfo);
            }
        
        }

        #endregion

        public List<FileElement> returnAllFiles()
        {
            return AllFiles;
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
                            result = AliyunOssUtility.Instance.DownloadFile(path.Substring(path.IndexOf("/") + 1), out buffer);
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
                        else if (file.origin == 2) 
                        {
                            Console.WriteLine("Read file from OSS, file name is " + path);
                            result = AliyunOssUtility.Instance.DownloadFile(path.Substring(path.IndexOf("/") + 1), out buffer);
                        }


                    }
                }
            }
            filebuffer = buffer;
            return result ? true : false;
        }

        public FileElement initialUploadFile(string path)
        {
            FileElement newfile = new FileElement();
            newfile.name = GetFileName(path);
            newfile.accessTime = DateTime.Now;
            newfile.createTime = DateTime.Now;
            newfile.motifyTime = DateTime.Now;
            newfile.isdir = FileAttributes.Normal;
            newfile.parentPath = GetPathPart(path);
            newfile.size = 0;
            AllFiles.Add(newfile);
            return newfile;
        }

        public bool uploadFile(string path, byte[] buffer) //upload file
        {
            bool result = true;

            //为了支持大于64KB的文件上传，这里先检察传入的buffer大小，如果为65536则说明文件结束，继续等待下次的buffer传入并累加
            if (ToUploadFile == null) ToUploadFile = initialUploadFile(path);

            if (buffer.Length == 65536)
            {
                if (uploadTotalBuffer == null) uploadTotalBuffer = buffer;
                else
                    uploadTotalBuffer = uploadTotalBuffer.Concat(buffer).ToArray();
                AllFiles.Remove(ToUploadFile);
                ToUploadFile.size = uploadTotalBuffer.Length;
                AllFiles.Add(ToUploadFile);
                return true;
            }
            else
            {
                if (uploadTotalBuffer == null) uploadTotalBuffer = buffer;
                else
                uploadTotalBuffer = uploadTotalBuffer.Concat(buffer).ToArray();
                AllFiles.Remove(ToUploadFile);
                ToUploadFile.size = uploadTotalBuffer.Length;
                
            }
            
            //当buffer size小于65536时，说明文件尾已到，则将合并后的buffer传递至api并上传至网盘
            if (GetPathPart(path) == "/")
            {
                Random rd = new Random();
                int i = rd.Next(0, 3);
                Console.WriteLine("Random number is " + i);
                if (i == 0)
                {
                    Console.WriteLine("New file create in Baidu");
                    result = BaiduPan.UploadFile(path, uploadTotalBuffer);
                }
                else if (i == 1)
                {
                    Console.WriteLine("New file create in KuaiPan");
                    result = KuaiPan.UploadFile(path, uploadTotalBuffer);
                }
                else if (i == 2) 
                {
                    Console.WriteLine("New file create in OSS");
                    result = AliyunOssUtility.Instance.UploadFile(path.Substring(path.IndexOf("/") + 1),uploadTotalBuffer);
                }

                ToUploadFile.origin = i;
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
                            result = BaiduPan.UploadFile(path, uploadTotalBuffer);
                        }
                        else if (file.origin == 1)
                        {
                            Console.WriteLine("New file create in KuaiPan");
                            result = KuaiPan.UploadFile(path, uploadTotalBuffer);
                        }
                        else if (file.origin == 2) 
                        {
                            Console.WriteLine("New file create in OSS");
                            result = AliyunOssUtility.Instance.UploadFile(path.Substring(path.IndexOf("/") + 1), uploadTotalBuffer);
                        }

                        ToUploadFile.origin = file.origin;
                    }
                }
            }

            if (result == true)
            {

                AllFiles.Add(ToUploadFile);
            }
            else
            {
               
            }
            uploadTotalBuffer = null;
            ToUploadFile = null;
            return result;
        }

        public bool removeFile(string path)
        {
            Console.WriteLine("To remove file, file name is {0}", path);
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
                        result = AliyunOssUtility.Instance.DeleteFile(path.Substring(path.IndexOf("/") + 1));
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
            if (path == "/") return true;
            else
            {
                foreach (FileElement file in AllFiles)
                {
                    if (file.parentPath + file.name == path) return true;
                }
            }
            //经调式，当上传文件时，此方法也会被调用，所以必须使用以上代码判断文件的父目录，若存在则不用新建直接返回true



            if (GetPathPart(path) == "/")
            {
                Random rd = new Random();
                int i = rd.Next(0, 3);
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
                else if (i == 2) 
                {
                    Console.WriteLine("New directory create in OSS");
                    result = AliyunOssUtility.Instance.CreateDirectory(path.Substring(path.IndexOf("/") + 1)+"/");
                }

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
                        else if (file.origin == 2) 
                        {
                            Console.WriteLine("New directory create in OSS");
                            result = AliyunOssUtility.Instance.CreateDirectory(path.Substring(path.IndexOf("/") + 1)+"/");
                        }

                        newfile.origin = file.origin;
                    }
                }


            }
            if (result == true)
            {
                newfile.name = GetFileName(path);
                newfile.accessTime = DateTime.Now;
                newfile.createTime = DateTime.Now;
                newfile.motifyTime = DateTime.Now;
                newfile.isdir = FileAttributes.Directory;
                newfile.parentPath = GetPathPart(path);
                
                newfile.size = 0;
                AllFiles.Add(newfile);
            }
            return result;

        }

        public bool MoveFile(string fromPath, string toPath)
        {
            bool result=true;
            FileElement fileToBeMoved=null;
            FileElement newFile = null ;

            foreach (FileElement file in AllFiles) //find file info from List
            {
                if (file.parentPath + file.name == fromPath) //path could be found in List
                {
                    fileToBeMoved = file;
                    newFile = file;
                    if (file.origin == 0) //from Baidu pan
                    {
                        result = BaiduPan.MoveFile(fromPath, toPath);
                        
                    }
                    if (file.origin == 1) //from KuaiPan
                    {
                        result = KuaiPan.MoveFile(fromPath, toPath);

                    }

                    if (file.origin == 2)
                    {
                        
                    }
                    if (result)
                    {
                        newFile.name = GetFileName(toPath);
                        newFile.parentPath = GetPathPart(toPath);
                        newFile.motifyTime = DateTime.Now;
                        newFile.createTime = DateTime.Now;
                        newFile.accessTime = DateTime.Now;
                        newFile.origin = file.origin;
                        newFile.size = file.size;
                        newFile.isdir = file.isdir;

                    }
                    else
                    {
                        return false;
                    }


                }
            }
            AllFiles.Remove(fileToBeMoved);
            AllFiles.Add(newFile);

            


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

        bool MoveFile(string fromPath, string toPath);

    }
}
