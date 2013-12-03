using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiskAPIBase;

namespace StarPan.Structure
{
    public class FileTreeNode<T> : ICloneable where T : ICloneable, IPathInfo
    {
        private readonly IList<FileTreeNode<T>> _children;

        private readonly object _locker = new object();

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
            IsDataModified = false;
        }

        #region Properties

        /// <summary>
        /// 文件信息
        /// </summary>
        public T FileInfo { get; private set; }

        /// <summary>
        /// 标志文件数据已被缓存
        /// </summary>
        public bool IsFileDateCached
        {
            get
            {
                return _fileDataStream != null;
            }
        }

        /// <summary>
        /// 标志文件数据发生改变
        /// </summary>
        public bool IsDataModified
        {
            get; private set;
        }

        /// <summary>
        /// 父节点，若本身为根节点，则为空
        /// </summary>
        public FileTreeNode<T> Parent { get; private set; }

        /// <summary>
        /// 所在目录路径
        /// </summary>
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

        /// <summary>
        /// 路径
        /// </summary>
        public string Path
        {
            get { return ParentPath + FileInfo.FileName; }
        }

        /// <summary>
        /// 所在层
        /// </summary>
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

        /// <summary>
        /// 所占空间大小
        /// </summary>
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

        /// <summary>
        /// 是否包含子节点
        /// </summary>
        public bool HasChild
        {
            get { return _children.Any(); }
        }
        #endregion

        #region Public Methods

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

        /// <summary>
        /// 请求文件数据
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public uint ReadFileData(long offset, byte[] buffer)
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
                    Console.WriteLine("Request file data-->>file:{0} buffer:{1} offset:{2}", FileInfo.FileName,
                        buffer.Length, offset);
                    Console.WriteLine("stream length:{0}  stream position:{1}", _fileDataStream.Length,
                        _fileDataStream.Position);
                    if (_fileDataStream.Position != offset)
                    {
                        _fileDataStream.Seek(offset, SeekOrigin.Begin);
                    }
                    int iResult = _fileDataStream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine("Read bytes:{0}", iResult);
                    return (uint) iResult;
                }
            }
            return 0;

        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public uint WriteFileData(long offset, byte[] buffer)
        {
            if (FileInfo.IsDir)
            {
                return 0;
            }
            lock (_locker)
            {
                _fileDataStream = new MemoryStream();

                Console.WriteLine("Write file data-->>file:{0} buffer:{1} offset:{2}", FileInfo.FileName,
                    buffer.Length, offset);
                Console.WriteLine("stream length:{0}  stream position:{1}", _fileDataStream.Length,
                    _fileDataStream.Position);
                if (_fileDataStream.Position != offset)
                {
                    _fileDataStream.Seek(offset, SeekOrigin.Begin);
                }
                _fileDataStream.Write(buffer, 0, buffer.Length);

                Console.WriteLine("Write bytes:{0}", buffer.Length);
                Console.WriteLine("stream length:{0}  stream position:{1}", _fileDataStream.Length,
                    _fileDataStream.Position);
                //标志文件数据发生改变
                if (!IsDataModified)
                {
                    IsDataModified = true;
                }

                return (uint) buffer.Length;

            }
            return 0;

        }

        /// <summary>
        /// 释放文件数据
        /// </summary>
        public void ReleaseFileData()
        {
            lock (_locker)
            {
                if (_fileDataStream != null)
                {
                    if (IsDataModified)
                    {
                        //更改文件大小
                        FileInfo.Size = _fileDataStream.Length;
                        ICloudDiskAccessUtility utility = null;
                        bool isNew = false;
                        //若为新文件，需要先指定网盘
                        if (string.IsNullOrEmpty(FileInfo.Source))
                        {
                            isNew = true;
                            //获取剩余空间最大网盘
                            utility = CloudDiskManager.Instance.GetCloudDisk(list=>list.OrderBy(disk=>disk.GetFreeSpace()).Last());
                            FileInfo.Source = utility != null ? utility.Name : "";
                        }
                        else
                        {
                            utility = CloudDiskManager.Instance.GetCloudDisk(FileInfo.Source);
                        }

                        if (utility != null)
                        {
                            //上传文件
                            if (!isNew)
                            {
                                if (utility.GetFileInfo(Path) != null)
                                {
                                    utility.DeleteFile(Path);
                                }                                
                            }
                            utility.UploadFile(Path, _fileDataStream.ToArray());

                        }
                    }
                    //文件修改标志位复位
                    IsDataModified = false;
                    //清空文件缓存
                    _fileDataStream.Dispose();
                    _fileDataStream = null;
                }
            }

        }
        #endregion
    }
}