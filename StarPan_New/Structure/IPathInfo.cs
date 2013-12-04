using System;

namespace StarPan.Structure
{
    public interface IPathInfo
    {
        /// <summary>
        ///     文件(夹)名
        /// </summary>
        string FileName { get; set; }

        bool IsDir { get; set; }

        long Size { get; set; }

        string Source { get; set; }

        void RecordReadTime();

        void RecordWriteTime();
    }
}