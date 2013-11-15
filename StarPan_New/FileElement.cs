using System;
using System.IO;

namespace StarPan
{
    class FileElement
    {
        //struct of a single file property
        public string parentPath { get; set; } //contians '/' at the end of string , does not contain app_root part
        public DateTime createTime { get; set; }
        public DateTime motifyTime { get; set; }
        public DateTime accessTime { get; set; }
        public long size { get; set; }
        public FileAttributes isdir { get; set; } //System.IO.FileAttributes.Directory and System.IO.FileAttributes.Normal;
        public string name { get; set; } //only name, does not contain any '/' symbol
        public int origin { get; set; } //0=baidu; 1=kuaipan ; 2=?
    }
}
