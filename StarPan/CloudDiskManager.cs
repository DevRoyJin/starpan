﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DiskAPIBase;

namespace StarPan
{
    public class CloudDiskManager
    {
        private readonly IList<ICloudDiskAccessUtility> _mCloudDisks;

        private CloudDiskManager()
        {
            _mCloudDisks = new List<ICloudDiskAccessUtility>();
            //获取网盘操作接口实例 
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                var dll = ConfigurationManager.AppSettings[key];
                var utility = Assembly.LoadFrom(dll).CreateInstance(key) as ICloudDiskAccessUtility;
                if (utility != null)
                {
                    _mCloudDisks.Add(utility);
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
        /// 获取网盘操作接口实例
        /// </summary>
        /// <param name="diskSelectingStrategy">选取网盘策略</param>
        /// <returns></returns>
        public ICloudDiskAccessUtility GetClouDisk(Func<IEnumerable<ICloudDiskAccessUtility>, ICloudDiskAccessUtility> diskSelectingStrategy)
        {
            return diskSelectingStrategy(_mCloudDisks);
        }
    }
}