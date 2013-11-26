using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarPan.Structure
{
    public interface IPathNode
    {
        /// <summary>
        /// 文件(夹)名
        /// </summary>
        string FileName { get; set; }

        bool IsDir { get; set; }
    }
}
