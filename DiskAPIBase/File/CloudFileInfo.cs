using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiskAPIBase.File
{
    public class CloudFileInfo
    {
        /// <summary>
        /// 网盘文件或目录绝对路径
        /// </summary>
        public string Path
        {
            get; set;
        }

        /// <summary>
        ///  网盘文件或目录创建时间（timestamp）
        /// </summary>
        public long CreateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 网盘文件或目录最近更新时间（timestamp）
        /// </summary>
        public long ModifiyTime
        {
            get;
            set;
        }

        /// <summary>
        ///  网盘文件或目录大小(若为目录，则返回0)
        /// </summary>
        public long Size
        {
            get;
            set;
        }

        /// <summary>
        /// 是否为目录
        /// </summary>
        public bool IsDir
        {
            get;
            set;
        }
    }
}
