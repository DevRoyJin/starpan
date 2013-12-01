using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StarPan.Structure
{
    public class FileTreeNode<T> : ICloneable where T : ICloneable, IPathInfo
    {
        private readonly IList<FileTreeNode<T>> _children;

        private readonly object _locker =new object();

        //缓存下载文件数据
        private MemoryStream _fileDataStream;

        public FileTreeNode()
        {
            _children = new List<FileTreeNode<T>>();
        }

        public FileTreeNode(T fileInfo)
            : this()
        {
            FileInfo = fileInfo;
        }

        public T FileInfo { get; private set; }


        public FileTreeNode<T> Parent { get; private set; }

        public string ParentPath
        {
            get
            {
                if (Parent == null)
                {
                    return "";
                }

                var stack = new Stack<string>();

                FileTreeNode<T> parent = Parent;
                while (parent != null)
                {
                    stack.Push(parent.FileInfo.FileName);
                    parent = parent.Parent;
                }

                var strBuilder = new StringBuilder();
                while (stack.Count > 0)
                {
                    string nodeName = stack.Pop();
                    if (nodeName != "/")
                    {
                        strBuilder.Append(nodeName);
                    }
                    strBuilder.Append("/");
                }
                return strBuilder.ToString();
            }
        }

        public string Path
        {
            get { return ParentPath + FileInfo.FileName; }
        }

        public int Layer
        {
            get
            {
                int i = 1;
                FileTreeNode<T> parent = Parent;
                while (parent != null)
                {
                    i++;
                    parent = parent.Parent;
                }
                return i;
            }
        }

        public long Size
        {
            get
            {
                if (FileInfo.IsDir)
                {
                    if (_children.Any())
                    {
                        return _children.Sum(c => c.Size);
                    }

                    return 0;
                }

                return FileInfo.Size;
            }
            set
            {
                if (FileInfo.IsDir)
                {
                    return;
                }

                FileInfo.Size = value;
            }
        }

        public bool HasChild
        {
            get { return _children.Any(); }
        }

        public object Clone()
        {
            var data = (T) FileInfo.Clone();
            var node = new FileTreeNode<T>(data);
            foreach (var treeNode in _children)
            {
                node.AddChild((FileTreeNode<T>) treeNode.Clone());
            }
            return node;
        }

        public FileTreeNode<T> AddChild(T data)
        {
            if (!FileInfo.IsDir)
            {
                return null;
            }
            var node = new FileTreeNode<T>(data);
            _children.Add(node);
            node.Parent = this;
            return node;
        }

        public FileTreeNode<T> AddChild(FileTreeNode<T> node)
        {
            if (!FileInfo.IsDir)
            {
                return null;
            }
            _children.Add(node);
            node.Parent = this;
            return node;
        }

        public void RemoveChild(FileTreeNode<T> child)
        {
            child.Parent = null;
            _children.Remove(child);
        }

        public void RemoveSelf()
        {
            if (Parent != null)
            {
                Parent.RemoveChild(this);
                Parent = null;
            }
        }

        public FileTreeNode<T> GetChild(Func<T, bool> condition)
        {
            return _children.FirstOrDefault(node => condition(node.FileInfo));
        }

        public T[] GetChildrenData()
        {
            return _children.Select(c => c.FileInfo).ToArray();
        }

        public FileTreeNode<T>[] GetChildren()
        {
            return _children.ToArray();
        }

        public void Rename(string newName)
        {
            FileInfo.FileName = newName;
        }

        public uint RequestFileData(long offset, byte[] buffer)
        {
            if (FileInfo.IsDir)
            {
                return 0;
            }
            lock (_locker)
            {
                if (_fileDataStream == null)
                {
                    //从网盘读取数据
                    var utility = CloudDiskManager.Instance.GetCloudDisk(FileInfo.Source);
                    if (utility != null)
                    {
                        byte[] fileData;

                        if (utility.DownloadFile(Path, out fileData))
                        {
                            _fileDataStream = new MemoryStream(fileData, false);
                        }
                    }
                }
                if (_fileDataStream != null)
                {
                    int iResult = _fileDataStream.Read(buffer, (int)offset, buffer.Length);

                    if (iResult < buffer.Length)
                    {
                        //读取完毕,释放内存
                        _fileDataStream.Dispose();
                        _fileDataStream = null;
                    }
                    return (uint)iResult;
                }
            }
            return 0;
        }
    }
}