using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DiskAPIBase;
using System.Net;
using System.IO;

namespace KuaiPanSDK
{
    class KuaiPanUtility : ICloudDiskAccessUtility
    {

        #region 私有方法
        private readonly KuaiPanSDK.KuaiPan _sdk;
        private readonly string _name;
        private readonly string _root;

        public KuaiPanUtility()
        {
            _sdk = new KuaiPanSDK.KuaiPan("xcRYiOnHwra5Lb5o", "l5sS04iA862dm00u", "0059a7586c815f96a1f8aedd", "b5ffa76e063b4ff58e41e7843feab2ed");

        }

        public KuaiPanUtility(string name, string root, string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            _name = name;
            _root = root;
            _sdk = new KuaiPanSDK.KuaiPan(consumerKey, consumerSecret, token, tokenSecret);
        }

        public string GetFileName(string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
        }

        public string GetPathPart(string sourcePath)
        {
            return sourcePath.Substring(0, sourcePath.LastIndexOf('/')) + "/";
        }

        public static string Search_string(string s, string s1, string s2)  //截取2个字符串之间的字符串
        {
            int n1, n2;
            n1 = s.IndexOf(s1, 0) + s1.Length;   //开始位置
            n2 = s.IndexOf(s2, n1);               //结束位置
            return s.Substring(n1, n2 - n1);   //取搜索的条数，用结束的位置-开始的位置,并返回
        }

        #endregion

        #region ICloudDiskAccessUtility Members
        public string Name
        {
            get { return _name; }
        }

        public string Root
        {
            get { return _root; }
        }


        public long GetQuota()
        {
            return _sdk.GetAccountInfo().QuotaTotal;
        }

        public long GetUsedSpace()
        {
            return _sdk.GetAccountInfo().QuotaUsed;
        }

        public long GetFreeSpace()
        {
            return _sdk.GetAccountInfo().QuotaTotal - _sdk.GetAccountInfo().QuotaUsed;
        }

        public bool UploadFile(string path, byte[] fileData)
        {
            path = PathHelper.CombineWebPath(_root, path);
            var result = _sdk.UpLoadFile(path, true, GetFileName(path), fileData);
            if (result != null)
            {
                return true;
            }
            return false; ;
        }

        public bool DownloadFile(string path, out byte[] fileData)
        {
            try
            {
                path = PathHelper.CombineWebPath(_root, path);

                string downloadPath = _sdk.Download(path);

                //WebClient wc = new WebClient();
                //string myCookie = @"token=0059a75882e3435a92343bd59c42802c-342602031; logindate=2013-10-30; ksc_suv=2025338563.1383101596179; relogintoken=0059a758633140bf84e328f16e342846-41119f36bbd44177872ead5a199bf55a-5875544; _et=0338322ea3d0101d517f62fc9fe13568910cac8668c473c16094f3ad61484ccbd28579e49c44241265f9; _ga=GA1.2.194446616.1376518040; _fs=b9e71c18; kphelper-inst=1; __cdl_snk=daa50229ca76ec07998e52a105b6ca7a";
                //wc.Headers.Add(HttpRequestHeader.Accept, @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                //wc.Headers.Add(HttpRequestHeader.UserAgent, @"Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36");
                //wc.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip,deflate,sdch");
                //wc.Headers.Add(HttpRequestHeader.AcceptLanguage, @"en-US,en;q=0.8");
                //wc.Headers.Add(HttpRequestHeader.Cookie, myCookie);

                Uri uri = new Uri(downloadPath);

                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                CookieContainer cc = new CookieContainer();
                request.CookieContainer = cc;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36";
                request.AllowAutoRedirect = true;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (Stream reader = response.GetResponseStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            var offset = 0;
                            var count = 0;
                            byte[] buff = new byte[32 * 1024];
                            do
                            {
                                count = reader.Read(buff, offset, buff.Length);
                                ms.Write(buff, offset, count);

                            } while (reader.CanRead && count > 0);
                            fileData = ms.ToArray();
                        }
                    }

                   
                    return true;
                    //readFileBuff = new Byte[response.ContentLength];

                    //reader.Read(readFileBuff, 0, (int)response.ContentLength);
                    //reader.Close();
                    //reader.Dispose();
                    //return readFileBuff;

                }



            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                fileData = null;
                return false;
            }
        }

        public bool CreateDirectory(string path)
        {
            path = PathHelper.CombineWebPath(_root, path);
            if (_sdk.Create(path) != null) return true;
            return false;
        }

        public bool DeleteDirectory(string path)
        {
            path = PathHelper.CombineWebPath(_root, path);
            return _sdk.Delete(path) == null ? false : true;
        }

        public bool DeleteFile(string filePath)
        {
            filePath = PathHelper.CombineWebPath(_root, filePath);
            return _sdk.Delete(filePath) == null ? false : true;
        }

        public IList<DiskAPIBase.File.CloudFileInfo> GetFileList(string dirPath)
        {
            dirPath = PathHelper.CombineWebPath(_root, dirPath);
            KuaiPanSDK.Model.MetaData FilesLists = _sdk.GetMetaData(dirPath, null);
            List<DiskAPIBase.File.CloudFileInfo> AllFiles = null;
            foreach (var f in FilesLists.Files)
            {
                DiskAPIBase.File.CloudFileInfo file = new DiskAPIBase.File.CloudFileInfo();
                file.Path = dirPath + "/" + f.Name;
                file.CreateTime = f.Createtime.Ticks;
                file.ModifiyTime = f.ModifyTime.Ticks;
                file.Size = f.Size;
                if (f.Type == KuaiPanSDK.TypeEnum.Folder)
                {
                    file.IsDir = true;

                }
                else
                {
                    file.IsDir = false;
                }

            }


            return AllFiles;
        }

        public DiskAPIBase.File.CloudFileInfo GetFileInfo(string path)
        {
            path = PathHelper.CombineWebPath(_root, path);
            KuaiPanSDK.Model.MetaData fileData = _sdk.GetMetaData(path, null);
            DiskAPIBase.File.CloudFileInfo file = new DiskAPIBase.File.CloudFileInfo();
            file.Path = path + "/" + fileData.Name;
            file.CreateTime = fileData.Createtime.Ticks;
            file.ModifiyTime = fileData.ModifyTime.Ticks;
            file.Size = fileData.Size;
            if (fileData.Type == KuaiPanSDK.TypeEnum.Folder)
            {
                file.IsDir = true;

            }
            else
            {
                file.IsDir = false;
            }
            return file;
        }

        public bool Move(string path, string newName)
        {
            path = PathHelper.CombineWebPath(_root, path);
            if (_sdk.Move(path, newName) != null) return true;
            return false;
        }

        #endregion
    }
}
