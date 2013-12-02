using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiskAPIBase;
using DiskAPIBase.File;
using Dokan;
using StarPan.Structure;

namespace StarPan
{
    public class DokanOperationProxy
    {
        private readonly IDictionary<string, FileTreeNode<PathInfo>> _fileNodeDictionay;
        private readonly FileTreeNode<PathInfo> _root;

        public DokanOperationProxy()
        {
            _fileNodeDictionay = new Dictionary<string, FileTreeNode<PathInfo>>();
            // 初始化目录根节点
            var rootInfo = new PathInfo
            {
                FileName = "/",
                IsDir = true,
                CreationTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                LastWriteTime = DateTime.Now,
                Size = 0,
                Source = null
            };

            _root = new FileTreeNode<PathInfo>(rootInfo);
            //_fileNodeDictionay.Add(_root.Path, _root);
            IList<ICloudDiskAccessUtility> diskUtilitys = CloudDiskManager.Instance.GetAllCloudDisk();
            foreach (ICloudDiskAccessUtility utility in diskUtilitys)
            {
                GenerateNode(_root, utility);
            }
            RegisterTreeNdoe(_root);
        }

        public FileTreeNode<PathInfo> Root
        {
            get { return _root; }
        }

        public void PrintFileCount()
        {
            Console.WriteLine("Files count:{0}", _fileNodeDictionay.Count);
        }

        public void PrintNode(FileTreeNode<PathInfo> node)
        {
            var msg = new StringBuilder();
            int layer = node.Layer;
            while (layer > 1)
            {
                msg.Append("-----");
                layer--;
            }
            msg.Append(node.FileInfo.FileName);
            msg.Append(string.Format("({0})", node.Path));
            Console.WriteLine(msg);
            foreach (var treeNode in node.GetChildren())
            {
                PrintNode(treeNode);
            }
        }


        public FileTreeNode<PathInfo> GetNode(string path)
        {
            path = path.ToLower();
            return _fileNodeDictionay[path];
        }

        public IList<PathInfo> FindAllChildren(string dirPath)
        {
            Console.WriteLine("File all children {0}", dirPath);
            PrintNode(_root);
            FileTreeNode<PathInfo> dirNode = _fileNodeDictionay[dirPath];
            if (dirNode != null)
            {
                return dirNode.GetChildrenData();
            }
            return null;
        }

        public PathInfo GetFileInfo(string dirPath)
        {
            FileTreeNode<PathInfo> dirNode = _fileNodeDictionay[dirPath];
            if (dirNode != null)
            {
                return dirNode.FileInfo;
            }
            return null;
        }


        /// <summary>
        ///     创建文件/文件夹节点
        /// </summary>
        /// <param name="info">文件信息</param>
        /// <param name="path">所在文件夹路径</param>
        public void AddNode(FileInformation info, string path)
        {
            string source = "";
            FileTreeNode<PathInfo> parentNode = _fileNodeDictionay[path];
            //父目录不存在
            if (parentNode == null)
            {
                return;
            }
            //重名
            if (parentNode.GetChildrenData().Any(d => d.FileName == info.FileName))
            {
                return;
            }

            //文件夹不指定网盘
            if (info.Attributes == FileAttributes.Directory)
            {
                source = null;
            }

            var node = new PathInfo
            {
                CreationTime = info.CreationTime,
                FileName = info.FileName,
                IsDir = info.Attributes == FileAttributes.Directory,
                LastAccessTime = info.LastAccessTime,
                LastWriteTime = info.LastWriteTime,
                Size = info.Length,
                Source = source
            };
            FileTreeNode<PathInfo> fileTreeNode = parentNode.AddChild(node);
            RegisterTreeNdoe(fileTreeNode);

            //执行网盘操作
            if (node.IsDir)//文件夹
            {
                foreach (var utility in CloudDiskManager.Instance.GetAllCloudDisk())
                {
                    utility.CreateDirectory(path);
                }
            }
            else //文件
            {
                //获取剩余空间最大网盘
                var utility = CloudDiskManager.Instance.GetCloudDisk(list => list.OrderBy(disk => disk.GetFreeSpace()).Last());
                utility.UploadFile(path, new byte[]{});

            }

        }

        public void RemoveNode(string path)
        {
            FileTreeNode<PathInfo> node = _fileNodeDictionay[path];
            if (node == null)
                return;

            UnRegisterTreeNode(node);
            node.RemoveSelf();

            //执行网盘操作
            if (string.IsNullOrEmpty(node.FileInfo.Source))//文件夹
            {
                foreach (var utility in CloudDiskManager.Instance.GetAllCloudDisk())
                {
                    if (utility.GetFileInfo(path) != null)
                    {
                        utility.DeleteDirectory(path);
                    }
                }
            }
            else //文件
            {
                var utility = CloudDiskManager.Instance.GetCloudDisk(node.FileInfo.Source);
                utility.DeleteFile(path);

            }
            
        }

        public void MoveNode(string curPath, string destPath)
        {
            FileTreeNode<PathInfo> node = _fileNodeDictionay[curPath];
            var sourceFolder = PathHelper.GetParentDirectory(curPath);
            var sourceFileName = PathHelper.GetFileName(curPath);
            var destFolder = PathHelper.GetParentDirectory(destPath);
            var destFileName = PathHelper.GetFileName(destPath);

            FileTreeNode<PathInfo> destFolderNode = _fileNodeDictionay[destFolder];
            if (node == null || destFolderNode == null || !destFolderNode.FileInfo.IsDir)
            {
                return;
            }
            if (destFolderNode.GetChildren().Any(n => n.FileInfo.FileName == destFileName))
            {
                Console.WriteLine("There is file/folder with the same name {0} in {1}", node.FileInfo.FileName,
                    destFolderNode.Path);
                return;
            }
            UnRegisterTreeNode(node);
            //移动
            if (sourceFolder != destFolder)
            {
                node.RemoveSelf();
                node = destFolderNode.AddChild(node);
            }
            //更名
            if (sourceFileName != destFileName)
            {
                node.Rename(destFileName);
            }
            RegisterTreeNdoe(node);

            //执行网盘操作
            if (string.IsNullOrEmpty(node.FileInfo.Source))//文件夹
            {
                foreach (var utility in CloudDiskManager.Instance.GetAllCloudDisk())
                {
                    if (utility.GetFileInfo(curPath) != null)
                    {
                        utility.Move(curPath, destPath);
                    }
                }
            }
            else //文件
            {
                var utility = CloudDiskManager.Instance.GetCloudDisk(node.FileInfo.Source);
                utility.Move(curPath, destPath);
                
            }

        }

        public void CopyNode(string curPath, string destPath)
        {
            FileTreeNode<PathInfo> node = _fileNodeDictionay[curPath];
            FileTreeNode<PathInfo> destNode = _fileNodeDictionay[destPath];
            if (node == null || destNode == null || !destNode.FileInfo.IsDir)
            {
                return;
            }
            if (destNode.GetChildrenData().Any(n => n.FileName == node.FileInfo.FileName))
            {
                Console.WriteLine("There is file/folder with the same name {0} in {1}", node.FileInfo.FileName,
                    destNode.Path);
                return;
            }
            var dupNode = (FileTreeNode<PathInfo>) node.Clone();

            destNode.AddChild(dupNode);
            RegisterTreeNdoe(dupNode);
        }

        public void RenameNode(string path, string newName)
        {
            FileTreeNode<PathInfo> node = _fileNodeDictionay[path];

            if (node == null || node.FileInfo.FileName == newName)
            {
                return;
            }
            UnRegisterTreeNode(node);
            node.Rename(newName);
            RegisterTreeNdoe(node);
        }

        public bool ExistsNode(string key)
        {
            return _fileNodeDictionay.ContainsKey(key.ToLower());
        }


        private void GenerateNode(FileTreeNode<PathInfo> root, ICloudDiskAccessUtility utility)
        {
            IList<CloudFileInfo> fileList = utility.GetFileList(root.FileInfo.FileName + "/");
            if (fileList == null)
            {
                return;
            }
            IEnumerable<PathInfo> fileInfoList = fileList.Select(f => new PathInfo
            {
                FileName = PathHelper.GetFileName(f.Path).ToLower(),
                IsDir = f.IsDir,
                CreationTime = new DateTime(f.CreateTime),
                LastAccessTime = new DateTime(f.ModifiyTime),
                LastWriteTime = new DateTime(f.ModifiyTime),
                Size = f.Size,
                Source = f.IsDir ? null : utility.Name
            });
            foreach (PathInfo fileInfo in fileInfoList)
            {
                FileTreeNode<PathInfo> node;
                if (root.GetChildrenData().All(d => d.FileName != fileInfo.FileName))
                {
                    node = root.AddChild(fileInfo);
                }
                else
                {
                    node = root.GetChild(f => f.FileName == fileInfo.FileName);
                }
                if (fileInfo.IsDir)
                {
                    GenerateNode(node, utility);
                }
            }
        }

        private void UnRegisterTreeNode(FileTreeNode<PathInfo> node)
        {
            _fileNodeDictionay.Remove(node.Path);
            foreach (var treeNode in node.GetChildren())
            {
                if (treeNode.HasChild)
                {
                    UnRegisterTreeNode(treeNode);
                }
                else
                {
                    _fileNodeDictionay.Remove(treeNode.Path);
                }
            }
        }

        private void RegisterTreeNdoe(FileTreeNode<PathInfo> node)
        {
            _fileNodeDictionay.Add(node.Path, node);
            foreach (var treeNode in node.GetChildren())
            {
                if (treeNode.HasChild)
                {
                    RegisterTreeNdoe(treeNode);
                }
                else
                {
                    _fileNodeDictionay.Add(treeNode.Path, treeNode);
                }
            }
        }
    }
}