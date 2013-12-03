using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DiskAPIBase;
using DiskAPIBase.File;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaiduCloudSDK
{
    public class BaiduPCSDiskUtility : ICloudDiskAccessUtility
    {
        #region Constant

        private const string PcsPath = @"/apps/sjtupan";
        private const long SingleFileLengthLimitation = 2147483648L;
        private const string DefaultUrl = "";
        private readonly string _accessToken;
        private readonly string _name;
        private readonly string _root;

        #endregion

        //测试用
        private static BaiduPCSDiskUtility _utility;

        public BaiduPCSDiskUtility()
        {
            _name = "baidu";
            _root = "/apps/测试应用";
            _accessToken = "3.a0fdecf6c512ce56ff547b1fcbdc9750.2592000.1386781278.4195463253-248414";
        }

        public BaiduPCSDiskUtility(string root, string name, string acessToken)
        {
            _name = name;
            _root = root;
            _accessToken = acessToken;
        }


        public static BaiduPCSDiskUtility Instance
        {
            get
            {
                if (_utility == null)
                {
                    _utility = new BaiduPCSDiskUtility();
                    return _utility;
                }
                return _utility;
            }
        }

        #region IDiskUtility Members

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
            string quotaJson = GetQuotaDataInternal();
            Console.WriteLine("Baidu : Got reponse-->>" + quotaJson);
            Console.WriteLine("Baidu : GetQuota succeeded");
            if (quotaJson == null)
            {
                return 0;
            }
            var jo = (JObject) JsonConvert.DeserializeObject(quotaJson);
            long lSpace;
            if (Int64.TryParse(jo["quota"].ToString(), out lSpace))
            {
                return lSpace;
            }
            Console.WriteLine("GetQuota failed.");
            return -1;
        }

        public long GetUsedSpace()
        {
            string quotaJson = GetQuotaDataInternal();
            if (quotaJson == null)
            {
                return 0;
            }
            var jo = (JObject) JsonConvert.DeserializeObject(quotaJson);
            long lSpace;
            if (Int64.TryParse(jo["used"].ToString(), out lSpace))
            {
                return lSpace;
            }
            Console.WriteLine("GetUsedSpace failed.");
            return -1;
        }


        public long GetFreeSpace()
        {
            string quotaJson = GetQuotaDataInternal();
            if (quotaJson == null)
            {
                return 0;
            }
            var jo = (JObject) JsonConvert.DeserializeObject(quotaJson);
            long lSpace, usedSpace;
            if (Int64.TryParse(jo["quota"].ToString(), out lSpace) &&
                Int64.TryParse(jo["used"].ToString(), out usedSpace))
            {
                return lSpace - usedSpace;
            }
            Console.WriteLine("GetFreeSpace failed.");
            return -1;
        }


        public bool UploadFile(string path, byte[] fileData)
        {
            try
            {
                path = PathHelper.CombineWebPath(_root, path);
                if (fileData.Length > GetFreeSpace())
                {
                    throw new InvalidOperationException("No enough space.");
                }
                string json = "";
                if (fileData.Length > SingleFileLengthLimitation)
                {
                    //分片上传（>2G大文件）
                    Console.WriteLine("The file is too large, don't supported now!");
                    return false;
                }
                //一般文件上传
                json = UploadInternal(path, fileData);
                Console.WriteLine("Baidu : Got reponse-->>" + json);
                Console.WriteLine("Baidu : UploadFile {0} succeeded", path);
                var jo = (JObject) JsonConvert.DeserializeObject(json);
                if (jo == null)
                {
                    Console.WriteLine("Can't resolve the response data!");
                    return false;
                }
                return true;
            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Upload failed-->error_msg:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = we.Message;
                }
                Console.WriteLine(msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Upload file failed:" + exception.Message);
                //Console.WriteLine(exception.StackTrace);
            }
            return false;
        }

        public bool DownloadFile(string pcsPath, out byte[] filebuffer)
        {
            try
            {
                pcsPath = PathHelper.CombineWebPath(_root, pcsPath);
                filebuffer = DownloadFieInternal(pcsPath);
                return true;
            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Download failed:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = we.Message;
                }
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Download file {0} failed:" + ex, pcsPath);
            }
            filebuffer = null;
            return false;
        }

        public bool CreateDirectory(string pcsPath)
        {
            try
            {
                pcsPath = PathHelper.CombineWebPath(_root, pcsPath);
                if (GetFileInfo(pcsPath) != null)
                    return true;
                string ret = CreateDirectoryInternal(pcsPath);
                Console.WriteLine("Baidu : Got reponse-->>" + ret);
                Console.WriteLine("Baidu : CreateDirectory {0} succeeded", pcsPath);
                return true;
            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Make dir failed:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = we.Message;
                }
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }


        public bool DeleteDirectory(string pcsPath)
        {
            try
            {
                pcsPath = PathHelper.CombineWebPath(_root, pcsPath);
                string ret = DeleteDirectoryInternal(pcsPath);
                Console.WriteLine("Baidu : Got reponse-->>" + ret);
                Console.WriteLine("Baidu : DeleteDirectory {0} succeeded", pcsPath);
                return true;
            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Delete directory {0} failed:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = "Delete directory {0} failed:" + we.Message;
                }
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        public bool DeleteFile(string pcsPath)
        {
            try
            {
                pcsPath = PathHelper.CombineWebPath(_root, pcsPath);
                string ret = DeleteDirectoryInternal(pcsPath);
                Console.WriteLine("Baidu : Got reponse-->>" + ret);
                Console.WriteLine("Baidu : Delete file {0} succeeded", pcsPath);
                return true;
            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Delete file {0} failed:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = "Delete file {0} failed:" + we.Message;
                }
                Console.WriteLine(msg, pcsPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        public IList<CloudFileInfo> GetFileList(string dirPath)
        {
            try
            {
                dirPath = PathHelper.CombineWebPath(_root, dirPath);
                string ret = GetFileListInternal(dirPath);
                Console.WriteLine("Baidu :Got reponse-->>" + ret);
                var responseJo =
                    (JObject) JsonConvert.DeserializeObject(ret);
                var sFileList = JsonConvert.DeserializeObject<List<JObject>>(responseJo["list"].ToString());
                List<CloudFileInfo> fileList = sFileList.Select(jo =>
                {
                    bool isDir = jo["isdir"].ToObject<int>() == 1;
                    return new CloudFileInfo
                    {
                        //Path = jo["path"].ToString().Substring(_root.Length) + (isDir ? "/" : ""),
                        //remove "/" at the end of directory path
                        Path = jo["path"].ToString().Substring(_root.Length),
                        CreateTime = GetRightTime(jo["ctime"].ToObject<long>()).Ticks,
                        ModifiyTime = GetRightTime(jo["mtime"].ToObject<long>()).Ticks,
                        Size = jo["size"].ToObject<long>(),
                        IsDir = isDir
                    };
                }).ToList();
                Console.WriteLine("Baidu :GetFileList of dir {0}", dirPath);
                return fileList;
            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Get file list {0} failed:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = "Get file list {0} failed:" + we.Message;
                }
                Console.WriteLine(msg, dirPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public CloudFileInfo GetFileInfo(string path)
        {
            try
            {
                path = PathHelper.CombineWebPath(_root, path);
                string ret = GetFileInfoInternal(path);
                Console.WriteLine("Baidu :Got reponse-->>" + ret);
                var responseJo =
                    (JObject) JsonConvert.DeserializeObject(ret);
                var sFileList = JsonConvert.DeserializeObject<List<JObject>>(responseJo["list"].ToString());
                JObject jo = sFileList[0];
                bool isDir = jo["isdir"].ToObject<int>() == 1;
                Console.WriteLine("Baidu :GetFileInfo of file {0}", path);
                return new CloudFileInfo
                {
                    Path = jo["path"].ToString().Substring(_root.Length) + (isDir ? "/" : ""),
                    CreateTime = jo["ctime"].ToObject<long>(),
                    ModifiyTime = jo["mtime"].ToObject<long>(),
                    Size = jo["size"].ToObject<long>(),
                    IsDir = isDir
                };
            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Get file info {0} failed:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = "Get file list {0} failed:" + we.Message;
                }
                Console.WriteLine(msg, path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public bool Move(string oldPath, string newPath)
        {
            try
            {
                oldPath = PathHelper.CombineWebPath(_root, oldPath);
                newPath = PathHelper.CombineWebPath(_root, newPath);
                var newFolder = PathHelper.GetParentDirectory(newPath);

                CreateDirectoryInternal(newFolder);              
                string ret = MoveInternal(oldPath, newPath);
                Console.WriteLine("Baidu: Got resposne-->>" + ret);
                Console.WriteLine("Baidu :Move file from {0} to {1} ", oldPath, newPath);

                return true;
                

            }
            catch (WebException we)
            {
                string msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    string sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Move file from {0} to {1} failed:" + jsRes["error_msg"];
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = "Move file from {0} to {1} failed:" + we.Message;
                }
                Console.WriteLine(msg, oldPath, newPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }


        #endregion

        #region Private Methods

        private string GetQuotaDataInternal()
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/quota?method={0}&access_token={1}";
            url = string.Format(url, BaiduCloudCommand.GetInfoCommand, _accessToken);
            HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(url,
                HttpWebResponseUtility.DefaultRequestTimeout, null, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private string UploadInternal(string path, byte[] fileData)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

            string url =
                "https://c.pcs.baidu.com/rest/2.0/pcs/file?method={0}&path={1}&access_token={2}";
            //string appPath = Path.GetDirectoryName(path) ?? "";
            //appPath = string.IsNullOrEmpty(appPath) ? appPath : appPath.Replace("\\", "/");
            url = string.Format(url, BaiduCloudCommand.UploadCommand, Uri.EscapeDataString(path), _accessToken);
            string contentType = "multipart/form-data; boundary=" + boundary + "\r\n";
            HttpWebResponse response = HttpWebResponseUtility.CreatePostHttpResponse(url, contentType,
                HttpWebResponseUtility.ConstructFileUploadPostData(Path.GetFileName(path), fileData, path, boundary),
                HttpWebResponseUtility.DefaultRequestTimeout, null, Encoding.UTF8, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private byte[] DownloadFieInternal(string pcsPath)
        {
            string url = "https://d.pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}&path={2}";
            url = string.Format(url, BaiduCloudCommand.DownloadCommand, _accessToken, Uri.EscapeDataString(pcsPath));
            HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(url,
                HttpWebResponseUtility.DefaultRequestTimeout, "", null);

            byte[] ret = null;
            using (Stream stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            var buf = new byte[1024];
                            count = stream.Read(buf, 0, 1024);
                            ms.Write(buf, 0, count);
                        } while (stream.CanRead && count > 0);
                        ret = ms.ToArray();
                    }
                }
            }
            return ret;
        }

        private string CreateDirectoryInternal(string pcsPath)
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}";
            url = string.Format(url, BaiduCloudCommand.MakeDirCommand, _accessToken);
            var formData = new Dictionary<string, string>();
            formData.Add("path", Uri.EscapeDataString(pcsPath));
            HttpWebResponse response = HttpWebResponseUtility.CreatePostHttpResponse(url, formData,
                HttpWebResponseUtility.DefaultRequestTimeout, null,
                Encoding.UTF8, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }


        private string DeleteDirectoryInternal(string pcsPath)
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}";
            url = string.Format(url, BaiduCloudCommand.DeleteCommand, _accessToken);
            var formData = new Dictionary<string, string>();
            formData.Add("path", Uri.EscapeDataString(pcsPath));
            HttpWebResponse response = HttpWebResponseUtility.CreatePostHttpResponse(url, formData,
                HttpWebResponseUtility.DefaultRequestTimeout, null, Encoding.UTF8, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private string GetFileListInternal(string pcsPath)
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}&path={2}";
            url = string.Format(url, BaiduCloudCommand.GetFileListCommand, _accessToken, Uri.EscapeDataString(pcsPath));
            HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(url,
                HttpWebResponseUtility.DefaultRequestTimeout, null, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private string GetFileInfoInternal(string pcsPath)
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}&path={2}";
            url = string.Format(url, BaiduCloudCommand.GetFileInfoCommand, _accessToken, Uri.EscapeDataString(pcsPath));
            HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(url,
                HttpWebResponseUtility.DefaultRequestTimeout, null, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private string MoveInternal(string oldPath, string newPath)
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}";
            url = string.Format(url, BaiduCloudCommand.MoveCommand, _accessToken);
            var formData = new Dictionary<string, string>();
            formData.Add("from", Uri.EscapeDataString(oldPath));
            formData.Add("to", Uri.EscapeDataString(newPath));
            HttpWebResponse response = HttpWebResponseUtility.CreatePostHttpResponse(url, formData,
                HttpWebResponseUtility.DefaultRequestTimeout, null, Encoding.UTF8, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private DateTime GetRightTime(long tmpTime)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long ITime = tmpTime * 10000000;
            var toNow = new TimeSpan(ITime);
            return dtStart.Add(toNow);
        }


        #endregion
    }
}