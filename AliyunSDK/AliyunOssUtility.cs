﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Aliyun.OpenServices;
using DiskAPIBase;
using DiskAPIBase.File;
using Aliyun.OpenServices.OpenStorageService;

namespace AliyunSDK
{
    public class AliyunOssUtility : ICloudDiskAccessUtility
    {
        private const string EndPoint = "http://oss.aliyuncs.com";
        private string BucketName;

        private readonly string _name;
        private readonly string _root;

        private readonly OssClient _ossClient;

        private Stack<string> _filesFroDelStack = new Stack<string>();

        public AliyunOssUtility()
            : this("app/", "aliyun", "Y8HLBRjlRgKp5SXV", "6nRLip0xiul2CZVsTDe36TYoqD09YT", "localclouddisk")
        {
        }

        public AliyunOssUtility(string root, string name, string key, string secret, string bucketName)
        {
            _name = name;
            _root = root;
            this.BucketName = bucketName;
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


                _ossClient = new OssClient(edUri, key, secret, clientConfig);
            }
                //非代理
            else
            {
                _ossClient = new OssClient(EndPoint, key, secret);

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
            return ~(1<<31);//2G
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
            return GetQuota() - GetUsedSpace();
        }

        public bool UploadFile(string path,byte[] fileData)
        {
            try
            {
                path = PathHelper.CombineWebPath(_root, path);
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
                path = PathHelper.CombineWebPath(_root, path);
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
                path = PathHelper.CombineWebPath(_root, path);
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
                path = PathHelper.CombineWebPath(_root, path);
                //递归删除所有子文件
                DeleteDirectoryInternal(path);

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
                path = PathHelper.CombineWebPath(_root, path);
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
            try
            {
                dirPath = PathHelper.CombineWebPath(_root, dirPath);
                var oList = _ossClient.ListObjects(BucketName, dirPath);
                return oList.ObjectSummaries.Where(oos => oos.Key != dirPath &&
                    (!oos.Key.Substring(dirPath.Length).Contains("/") || oos.Key.Substring(dirPath.Length).Split('/')[1] == ""))
                    .Select(oos => new CloudFileInfo
                    {
                        //Path = oos.Key.Substring(_root.Length),
                        //remove "/" at the end of directory path
                        Path = oos.Key.Substring(_root.Length, oos.Key.EndsWith("/") ? oos.Key.Length - _root.Length - 1 : oos.Key.Length - _root.Length),
                        CreateTime = oos.LastModified.Ticks,
                        ModifiyTime = oos.LastModified.Ticks,
                        IsDir = oos.Key.EndsWith("/"),
                        Size = oos.Size
                    }).ToList();

            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ", ossEx.ErrorCode, ossEx.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Generic error: {0}", exception.Message);
            }
            return null;
        }

        public CloudFileInfo GetFileInfo(string path)
        {
            try
            {
                path = PathHelper.CombineWebPath(_root, path);
                var listObjReq = new ListObjectsRequest(BucketName);
                listObjReq.Prefix = path;
                listObjReq.MaxKeys = 1;

                var oList = _ossClient.ListObjects(listObjReq);
                var objSummary = oList.ObjectSummaries.FirstOrDefault(oos => oos.Key == path);
                if (objSummary != null)
                {
                    return new CloudFileInfo()
                    {
                        Path = objSummary.Key.Substring(_root.Length),
                        CreateTime = objSummary.LastModified.Ticks,
                        ModifiyTime = objSummary.LastModified.Ticks,
                        IsDir = objSummary.Key.EndsWith("/"),
                        Size = objSummary.Size
                    };
                }
            }
            catch (OssException ossEx)
            {
                Console.WriteLine("OSS operation failed, error code:{0},error msg:{1} ", ossEx.ErrorCode, ossEx.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Generic error: {0}", exception.Message);
            }
            return null;
        }

        public bool Move(string path, string newPath)
        {         
            try
            {
                path = PathHelper.CombineWebPath(_root, path);
                newPath = PathHelper.CombineWebPath(_root, newPath);
                //Copy first
                var copyReq = new CopyObjectRequest(BucketName, path, BucketName, newPath);
                var copyResult = _ossClient.CopyObject(copyReq);
                if (copyResult != null)
                {
                    //copy succeed, delete the origin object
                    _ossClient.DeleteObject(BucketName,path);
                    return true;
                }
                
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
        #endregion

        private void DeleteDirectoryInternal(string dirPath)
        {
            if (_filesFroDelStack.Count > 0)
            {
                _filesFroDelStack.Clear();
            }
            var oList = _ossClient.ListObjects(BucketName, dirPath);//路径下所有文件
            foreach (var obj in oList.ObjectSummaries)
            {
                if (obj.Key.EndsWith("/"))
                {
                    _filesFroDelStack.Push(obj.Key);
                }
                else
                {
                    _ossClient.DeleteObject(BucketName,obj.Key);
                }
            }

            while (_filesFroDelStack.Count>0)
            {
                var delKey = _filesFroDelStack.Pop();
                _ossClient.DeleteObject(BucketName, delKey);
            }

        }
    }
}
