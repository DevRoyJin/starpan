using System;
using System.IO;
using Dokan;

namespace StarPan.Structure
{
    public class PathInfo : ICloneable, IPathInfo
    {

        public DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        /// <summary>
        ///     来源网盘名称
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     文件(夹)名
        /// </summary>
        public string FileName { get; set; }

        public long Size { get; set; }

        public bool IsDir { get; set; }


        public void RecordReadTime()
        {
            this.LastAccessTime = DateTime.Now;
        }

        public void RecordWriteTime()
        {
            LastAccessTime = DateTime.Now;
            LastWriteTime = DateTime.Now;
        }

        public FileInformation ToFileInformation()
        {
            return new FileInformation
            {
                FileName = FileName,
                Attributes = IsDir ? FileAttributes.Directory : FileAttributes.Normal,
                CreationTime = CreationTime,
                LastAccessTime = LastAccessTime,
                LastWriteTime = LastWriteTime,
                Length = Size
            };
        }

        public object Clone()
        {
            return new PathInfo
            {
                CreationTime = CreationTime,
                FileName = FileName,
                IsDir = IsDir,
                LastAccessTime = LastAccessTime,
                LastWriteTime = LastWriteTime,
                Size = Size,
                Source = Source
            };
        }
    }
}