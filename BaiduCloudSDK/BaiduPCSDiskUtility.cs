using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using DiskAPIBase.File;
using Newtonsoft;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DiskAPIBase;


namespace BaiduCloudSDK
{
    public class BaiduPCSDiskUtility : ICloudDiskAccessUtility
    {
        #region Constant
        private const string PcsPath = @"/apps/sjtupan";
        private const long SingleFileLengthLimitation = 2147483648L;
        private const string DefaultUrl = "";
        private const string AccessToken = "3.a0fdecf6c512ce56ff547b1fcbdc9750.2592000.1386781278.4195463253-248414";
        #endregion

        private static BaiduPCSDiskUtility _utility;

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
        public long GetQuota()
        {
            var quotaJson = GetQuotaDataInternal();
            if (quotaJson == null)
            {
                return 0;
            }
            JObject jo = (JObject)JsonConvert.DeserializeObject(quotaJson);
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
            var quotaJson = GetQuotaDataInternal();
            if (quotaJson == null)
            {
                return 0;
            }
            JObject jo = (JObject)JsonConvert.DeserializeObject(quotaJson);
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
            var quotaJson = GetQuotaDataInternal();
            if (quotaJson == null)
            {
                return 0;
            }
            JObject jo = (JObject)JsonConvert.DeserializeObject(quotaJson);
            long lSpace,usedSpace;
            if (Int64.TryParse(jo["quota"].ToString(), out lSpace) && Int64.TryParse(jo["used"].ToString(), out usedSpace))
            {
                return lSpace - usedSpace;
            }
            Console.WriteLine("GetFreeSpace failed.");
            return -1;
        }



        public bool UploadFile(string path,byte[] fileData)
        {
            try
            {
                if (fileData.Length > GetFreeSpace())
                {
                    throw new InvalidOperationException("No enough space.");
                }
                var json = "";
                if (fileData.Length > SingleFileLengthLimitation)
                {
                    //分片上传（>2G大文件）
                    Console.WriteLine("The file is too large, don't supported now!");
                    return false;
                }
                else
                {
                    //一般文件上传
                    json = UploadInternal(path,fileData);
                }
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
                var msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    var sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject) JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Upload failed-->error_msg:" + jsRes["error_msg"].ToString();
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
                Console.WriteLine("Upload file failed:"+exception.Message);
                //Console.WriteLine(exception.StackTrace);
            }
            return false;

        }

        public bool DownloadFile(string pcsPath,out byte[] filebuffer)
        {
            try
            {
                filebuffer = DownloadFieInternal(pcsPath);
                return true;
            }
            catch (WebException we)
            {
                var msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    var sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject)JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Download failed:" + jsRes["error_msg"].ToString();
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
                Console.WriteLine("Download file {0} failed:"+ex.ToString(),pcsPath);
                
            }
            filebuffer = null;
            return false;


        }

        public bool CreateDirectory(string pcsPath)
        {
            pcsPath = PcsPath+ pcsPath;
            try
            {
               var ret = CreateDirectoryInternal(pcsPath);
               Console.WriteLine("Make dir succeeds-->"+ret);
                return true;
            }
            catch (WebException we)
            {
                var msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    var sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject)JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Make dir failed:" + jsRes["error_msg"].ToString();
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
                var ret = DeleteDirectoryInternal(pcsPath);
                Console.WriteLine("Delete directory {0} succeeded-->" + ret,pcsPath);
                return true;
            }
            catch (WebException we)
            {
                var msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    var sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject)JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Delete directory {0} failed:" + jsRes["error_msg"].ToString();
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
                var ret = DeleteDirectoryInternal(pcsPath);
                Console.WriteLine("Delete file {0} succeeded-->" + ret,pcsPath);
                return true;
            }
            catch (WebException we)
            {
                var msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    var sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject)JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Delete file {0} failed:" + jsRes["error_msg"].ToString();
                    }
                }
                if (string.IsNullOrEmpty(msg))
                {
                    msg = "Delete file {0} failed:" + we.Message;
                }
                Console.WriteLine(msg,pcsPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        public IList<DiskAPIBase.File.CloudFileInfo> GetFileList(string dirPath)
        {
            try
            {
                var ret = GetFileListInternal(dirPath);
                var responseJo = 
                    (JObject)JsonConvert.DeserializeObject(ret);
                var sFileList = JsonConvert.DeserializeObject <List<JObject>>(responseJo["list"].ToString());
                var fileList = sFileList.Select(jo => new CloudFileInfo
                {
                    Path = jo["path"].ToString(),
                    CreateTime = jo["ctime"].ToObject<long>(),
                    ModifiyTime =jo["mtime"].ToObject<long>(),
                    Size =jo["size"].ToObject<long>(),
                    IsDir = jo["isdir"].ToObject<int>()==1

                }).ToList();

                return fileList;
            }
            catch (WebException we)
            {
                var msg = "";
                var res = we.Response as HttpWebResponse;
                if (res != null)
                {
                    var sRes = HttpWebResponseUtility.ConvertReponseToString(res);
                    var jsRes = (JObject)JsonConvert.DeserializeObject(sRes);
                    if (jsRes != null && jsRes["error_msg"] != null)
                    {
                        msg = "Get file list {0} failed:" + jsRes["error_msg"].ToString();
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
        #endregion

        #region Private Methods

        private string GetQuotaDataInternal()
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/quota?method={0}&access_token={1}";
            url = string.Format(url, BaiduCloudCommand.GetInfoCommand, AccessToken);
            var response = HttpWebResponseUtility.CreateGetHttpResponse(url, HttpWebResponseUtility.DefaultRequestTimeout, null, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private string UploadInternal(string path,byte[] fileData)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

            string url =
                "https://c.pcs.baidu.com/rest/2.0/pcs/file?method={0}&path={1}&access_token={2}";
            //string appPath = Path.GetDirectoryName(path) ?? "";
            //appPath = string.IsNullOrEmpty(appPath) ? appPath : appPath.Replace("\\", "/");
            url = string.Format(url, BaiduCloudCommand.UploadCommand, Uri.EscapeDataString(path), AccessToken);
            string contentType = "multipart/form-data; boundary=" + boundary + "\r\n";
            var response = HttpWebResponseUtility.CreatePostHttpResponse(url, contentType, HttpWebResponseUtility.ConstructFileUploadPostData(Path.GetFileName(path),fileData,path,boundary), HttpWebResponseUtility.DefaultRequestTimeout, null, Encoding.UTF8, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);

        }

        private byte[] DownloadFieInternal(string pcsPath)
        {
            string url = "https://d.pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}&path={2}";
            url = string.Format(url, BaiduCloudCommand.DownloadCommand, AccessToken, Uri.EscapeDataString(pcsPath));
            var response = HttpWebResponseUtility.CreateGetHttpResponse(url, HttpWebResponseUtility.DefaultRequestTimeout, "", null);

            byte[] ret = null;
            using (var stream = response.GetResponseStream())
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
            var url = "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}";
            url = string.Format(url, BaiduCloudCommand.MakeDirCommand, AccessToken);
            var formData = new Dictionary<string, string>();
            formData.Add("path",Uri.EscapeDataString(pcsPath));
            var response = HttpWebResponseUtility.CreatePostHttpResponse(url, formData, HttpWebResponseUtility.DefaultRequestTimeout, null,
                                                                         Encoding.UTF8, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }


        private string DeleteDirectoryInternal(string pcsPath)
        {
            return null;
        }

        private string GetFileListInternal(string pcsPath)
        {
            var url = "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&access_token={1}&path={2}";
            url = string.Format(url,BaiduCloudCommand.GetFileListCommand , AccessToken,pcsPath);
            var response = HttpWebResponseUtility.CreateGetHttpResponse(url,
                HttpWebResponseUtility.DefaultRequestTimeout, null, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }
        #endregion

    }

}
