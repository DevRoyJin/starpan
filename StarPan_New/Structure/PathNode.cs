using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dokan;

namespace StarPan.Structure
{
    public class PathNode:ICloneable
    {
        /// <summary>
        /// 文件(夹)名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件(夹)路径
        /// </summary>
        public string CurrentPath { get; set; }

        /// <summary>
        /// 上一级文件夹路径
        /// </summary>
        public string ParentPath { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public long Size { get; set; }

        public bool IsDir { get; set; }

        /// <summary>
        /// 来源网盘名称
        /// </summary>
        public string Source { get; set; }


        public FileInformation ToFileInformation()
        {
            return new FileInformation()
                {
                    FileName = this.FileName,
                    Attributes = this.IsDir ? FileAttributes.Directory : FileAttributes.Normal,
                    CreationTime = CreationTime,
                    LastAccessTime = LastAccessTime,
                    LastWriteTime = LastWriteTime,
                    Length = Size
                };
        }

        public object Clone()
        {
            return new PathNode()
                {
                    CreationTime = CreationTime,
                    CurrentPath = CurrentPath,
                    FileName = FileName,
                    IsDir = IsDir,
                    LastAccessTime = LastAccessTime,
                    LastWriteTime = LastWriteTime,
                    ParentPath = ParentPath,
                    Size = Size,
                    Source = Source
                };

        }
    }
}
