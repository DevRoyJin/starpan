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
                var diskConfig = item as CloudDiskConfiguration;
                if (diskConfig == null)
                {
                    Console.WriteLine("Error: Read cloud disk configuration failed.");
                    continue;
                }
                var dll = diskConfig.DllName;
                var cn = diskConfig.ClassName;
                object[] args = null;

                if (!string.IsNullOrEmpty(diskConfig.ConsumerKey) &&
                    !string.IsNullOrEmpty(diskConfig.ConsumerSecret) &&
                    !string.IsNullOrEmpty(diskConfig.Token) &&
                    !string.IsNullOrEmpty(diskConfig.TokenSecret))
                {
                    args = new[]
                    {
                        diskConfig.Root,diskConfig.Name,
                        diskConfig.ConsumerKey,
                        diskConfig.ConsumerSecret,
                        diskConfig.Token,
                        diskConfig.TokenSecret,
                    };
                }
                else if (!string.IsNullOrEmpty(diskConfig.AccessKey) &&
                         !string.IsNullOrEmpty(diskConfig.AccessSerect))
                {
                    args = new[] { diskConfig.Root, diskConfig.Name, diskConfig.AccessKey, diskConfig.AccessSerect };
                }

                else if (!string.IsNullOrEmpty(diskConfig.AccessToken))
                {

                    args = new[] {diskConfig.Root, diskConfig.Name, diskConfig.AccessToken};
                }
                else
                {
                    throw new Exception(string.Format("Invalid cloudDiskConfiguration: name={0}", diskConfig.Name));
                }

                var utility =
                    Assembly.LoadFrom(dll)
                        .CreateInstance(cn, false, BindingFlags.CreateInstance, null, args, null, null) as
                        ICloudDiskAccessUtility;
                if (utility != null)
                {
                    _mCloudDisks.Add(diskConfig.Name, utility);
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

        public void PrintFreeSpace()
        {
            foreach (var kv in _mCloudDisks)
            {
                Console.WriteLine("Disk :{0}-->free space:{1}",kv.Key,kv.Value.GetFreeSpace());
            }
        }

    }
}
