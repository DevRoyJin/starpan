using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiskAPIBase;
using Aliyun.OpenServices.OpenStorageService;

namespace AliyunSDK
{
    public class AliyunOssUtility : IDiskUtility
    {
        private const string EndPoint = "http://oss.aliyuncs.com";
        private const string AccessKey = "Y8HLBRjlRgKp5SXV";
        private const string AccessSecret = "6nRLip0xiul2CZVsTDe36TYoqD09YT";
        private const string BucketName = "localclouddisk";

        private OssClient _ossClient;

        private AliyunOssUtility()
        {
            _ossClient = new OssClient(EndPoint,AccessKey, AccessSecret);
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
            return _ossClient.ListObjects(BucketName).ObjectSummaries.Sum(o => o.Size);
        }

        public long GetFreeSpace()
        {
            return Int64.MaxValue;
        }

        public bool UploadFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool DownloadFile(string pcsPath, out byte[] filebuffer)
        {
            throw new NotImplementedException();
        }

        public bool CreateDirectory(string pcsPath)
        {
            throw new NotImplementedException();
        }

        public bool DeleteDirectory(string pcsPath)
        {
            throw new NotImplementedException();
        }

        public bool DeleteFile(string pcsPath)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
