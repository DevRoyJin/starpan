using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Aliyun.OpenServices;
using DiskAPIBase;
using Aliyun.OpenServices.OpenStorageService;

namespace AliyunSDK
{
    public class AliyunOssUtility : ICloudDiskAccessUtility
    {
        private const string EndPoint = "http://oss.aliyuncs.com";
        private const string AccessKey = "Y8HLBRjlRgKp5SXV";
        private const string AccessSecret = "6nRLip0xiul2CZVsTDe36TYoqD09YT";
        private const string BucketName = "localclouddisk";

        private readonly OssClient _ossClient;

        public AliyunOssUtility()
        {
            //代理
            if (WebUtiltiy.IsProxyEnable)
            {
                var proxy = WebRequest.GetSystemWebProxy();
                var clientConfig = new ClientConfiguration();
                var edUri = new Uri(EndPoint);
                var proxyUri = proxy.GetProxy(edUri);

                clientConfig.ProxyHost = proxyUri.Host;
                clientConfig.ProxyPort = proxyUri.Port;
                if (proxy.Credentials != null)
                {
                    var credential = proxy.Credentials.GetCredential(edUri, "Basic");
                    if (credential != null)
                    {
                        clientConfig.ProxyUserName = credential.UserName;
                        clientConfig.ProxyPassword = credential.Password;
                        clientConfig.ProxyDomain = credential.Domain;
                    }
                }

                //clientConfig.ConnectionTimeout = HttpWebResponseUtility.DefaultRequestTimeout;


                _ossClient = new OssClient(edUri, AccessKey, AccessSecret, clientConfig);
            }
            //非代理
            else
            {
                _ossClient = new OssClient(EndPoint, AccessKey, AccessSecret);

            }
        }

        private static AliyunOssUtility _utility;

        public static AliyunOssUtility Instance
        {
            get
            {
                if (_utility == null)
                {
                    _utility = new AliyunOssUtility();
                    return _utility;
                }
                return _utility;
            }
        }

        #region [IDiskUtility members]
        public long GetQuota()
        {
            return Int64.MaxValue;
        }

        public long GetUsedSpace()
        {
            try
            {
                return _ossClient.ListObjects(BucketName).ObjectSummaries.Sum(o => o.Size);
            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ", ossEx.ErrorCode, ossEx.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Generic error: {0}", exception.Message);
            }
            return -1;
        }
        
        public long GetFreeSpace()
        {
            return Int64.MaxValue;
        }

        public bool UploadFile(string path,byte[] fileData)
        {
            try
            {
                var metaData = new ObjectMetadata();
                metaData.ContentEncoding = "utf-8";
                _ossClient.PutObject(BucketName, path, new MemoryStream(fileData), metaData);
                return true;
            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ", ossEx.ErrorCode, ossEx.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Generic error: {0}", exception.Message);
            }
            return false;
        }

        public bool DownloadFile(string path, out byte[] fileData)
        {
            try
            {
                var getObjReq = new GetObjectRequest(BucketName, path);
                using (var stream = new MemoryStream())
                {
                    _ossClient.GetObject(getObjReq,stream);
                    fileData = stream.ToArray();
                    return true;
                }
                
            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ",ossEx.ErrorCode, ossEx.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Generic error: {0}", exception.Message);
            }
            fileData = null;
            return false;
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                var metaData = new ObjectMetadata();
                metaData.ContentType = "application/x-www-form-urlencoded";
                _ossClient.PutObject(BucketName, path, new MemoryStream(), metaData);
                return true;
            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ", ossEx.ErrorCode, ossEx.Message);
                
            }
            catch (Exception exception)
            {          
                Console.WriteLine("Generic error: {0}",exception.Message);
            }
            return false;


        }

        public bool DeleteDirectory(string path)
        {
            try
            {
                //if (path.Contains("/"))
                //{
                //    var arr = path.EndsWith("/")?path.Substring(0,path.Length-1).Split('/'):path.Split('/');
                //    var folderInfo = _ossClient.GetObjectMetadata(BucketName,arr[arr.Length-1]);
                //}
                _ossClient.DeleteObject(BucketName,path);
                return true;
            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ", ossEx.ErrorCode, ossEx.Message);

            }
            catch (Exception exception)
            {
                Console.WriteLine("Generic error: {0}", exception.Message);
            }
            return false;
        }

        public bool DeleteFile(string path)
        {
            try
            {
                _ossClient.DeleteObject(BucketName,path);
                return true;
            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ", ossEx.ErrorCode, ossEx.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Generic error: {0}", exception.Message);
            }
            return false;
        }

        public IList<DiskAPIBase.File.CloudFileInfo> GetFileList(string dirPath)
        {
            throw new NotImplementedException();
        }
        #endregion



    }
}
