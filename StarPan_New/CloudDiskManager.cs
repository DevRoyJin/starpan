using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using DiskAPIBase;
using StarPan.Configuration;

namespace StarPan
{
    public class CloudDiskManager
    {
        private readonly IDictionary<string,ICloudDiskAccessUtility> _mCloudDisks;

        private CloudDiskManager()
        {
            _mCloudDisks = new Dictionary<string, ICloudDiskAccessUtility>();
            //获取网盘操作接口实例 

            var section = ConfigurationManager.GetSection("cloudDiskConfiguration") as CloudDisksConfigurationSection;
            if (section == null)
            {
                throw new Exception("Error:Please config the cloud disk first.");

            }

            foreach (var item in section.CloudDiskGroup)
            {
                var cloudDiskInstance = item as CloudDiskInstance;
                if (cloudDiskInstance == null)
                {
                    Console.WriteLine("Error: Read cloud disk configuration failed.");
                    continue;
                }
                var dll = cloudDiskInstance.DllName;
                var cn = cloudDiskInstance.ClassName;
                var accessToken = cloudDiskInstance.AccessToken;
                var accessKey = cloudDiskInstance.AccessKey;
                var accessSerect = cloudDiskInstance.AccessSerect;
                var args = !string.IsNullOrEmpty(accessToken)
                    ? new [] {accessToken}
                    : new[] {accessKey, accessSerect};

                var utility = Assembly.LoadFrom(dll).CreateInstance(cn,false,BindingFlags.CreateInstance,null,args,null,null) as ICloudDiskAccessUtility;
                if (utility != null)
                {
                    _mCloudDisks.Add(cloudDiskInstance.Name,utility);
                }

            }
        }

        private static CloudDiskManager _cloudDiskManager;

        public static CloudDiskManager Instance
        {
            get
            {
                if (_cloudDiskManager == null)
                {
                    _cloudDiskManager = new CloudDiskManager();
                    
                }
                return _cloudDiskManager;
            }
        }

        /// <summary>
        /// 按名称获取网盘操作接口实例
        /// </summary>
        /// <param name="name">网盘名称</param>
        /// <returns></returns>
        public ICloudDiskAccessUtility GetCloudDisk(string name)
        {
            return _mCloudDisks[name];
        }
        
        /// <summary>
        /// 按策略获取网盘操作接口实例
        /// </summary>
        /// <param name="diskSelectingStrategy">选取网盘策略</param>
        /// <returns></returns>
        public ICloudDiskAccessUtility GetCloudDisk(Func<IEnumerable<ICloudDiskAccessUtility>, ICloudDiskAccessUtility> diskSelectingStrategy)
        {
            return diskSelectingStrategy(_mCloudDisks.Values);
        }
    }
}
