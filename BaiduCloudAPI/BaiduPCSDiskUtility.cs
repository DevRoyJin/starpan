using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DiskAPIBase;


namespace BaiduCloudAPI
{
    public class BaiduPCSDiskUtility : IDiskUtility
    {
        #region Constant
        private const string PcsPath = @"/apps/sjtupan";
        private const long SingleFileLengthLimitation = 2147483648L;
        private const string DefaultUrl = "";
        private const string AccessToken = "3.610e76fb259ee1c0cee211b548ff1a40.2592000.1385016898.1477197110-1567359";
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



        public bool UploadFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("{0} doesn't exist.", path);
            }
            try
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.Length > GetFreeSpace())
                {
                    throw new InvalidOperationException("No enough space.");
                }
                var json = "";
                if (fileInfo.Length > SingleFileLengthLimitation)
                {
                    //分片上传（大文件）
                    Console.WriteLine("The file is too large, don't supported now!");
                    return false;
                }
                else
                {
                    //一般文件上传
                    json = UploadInternal(path);
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
                Console.WriteLine("Delete directory {0} succeeds-->" + ret,pcsPath);
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
                Console.WriteLine("Delete file {0} succeed-->" + ret,pcsPath);
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
        #endregion

        #region Private Methods

        private string GetQuotaDataInternal()
        {
            string url = "https://pcs.baidu.com/rest/2.0/pcs/quota?method={0}&access_token={1}";
            url = string.Format(url, BaiduCloudCommand.GetInfoCommand, AccessToken);
            var response = HttpWebResponseUtility.CreateGetHttpResponse(url, HttpWebResponseUtility.DefaultRequestTimeout, null, null);
            return HttpWebResponseUtility.ConvertReponseToString(response);
        }

        private string UploadInternal(string path)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            string pcsPath = PcsPath;
            string[] pathComponents = path.Split(Path.DirectorySeparatorChar);

            int i = 0;
            foreach (var pathComponent in pathComponents)
            {
                if (i > 0)
                    pcsPath += @"/" + pathComponent;
                i++;
            }
            string url =
                "https://pcs.baidu.com/rest/2.0/pcs/file?method={0}&path={1}&access_token={2}";

            url = string.Format(url, BaiduCloudCommand.UploadCommand, Uri.EscapeDataString(pcsPath), AccessToken);
            string contentType = "multipart/form-data; boundary=" + boundary + "\r\n";
            var response = HttpWebResponseUtility.CreatePostHttpResponse(url, contentType, HttpWebResponseUtility.ConstructFileUploadPostData(path, pcsPath, boundary), HttpWebResponseUtility.DefaultRequestTimeout, null, Encoding.UTF8, null);
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
        #endregion
    }

}
