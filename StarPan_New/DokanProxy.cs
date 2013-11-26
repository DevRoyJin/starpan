using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiskAPIBase;
using Dokan;
using StarPan.Structure;

namespace StarPan
{
    public class DokanProxy
    {
        private TreeNode<PathNode> _root;
        private IDictionary<string, TreeNode<PathNode>> _fileNodeDictionay;


        public DokanProxy()
        {
            _fileNodeDictionay = new Dictionary<string, TreeNode<PathNode>>();
            // 初始化目录根节点
            var rootInfo = new PathNode
                {
                    FileName = "/",
                    IsDir = true,
                    CreationTime = new DateTime(0),
                    LastAccessTime = new DateTime(0),
                    LastWriteTime = new DateTime(0),
                    Size = 0,
                    Source = null
                };

            _root = new TreeNode<PathNode>(rootInfo);
            _fileNodeDictionay.Add(_root.Path, _root);
            var diskUtilitys = CloudDiskManager.Instance.GetAllCloudDisk();
            foreach (var utility in diskUtilitys)
            {
                GenerateNode(_root, utility);
            }
        }

        public TreeNode<PathNode> Root
        {
            get
            {
                return _root;
            }
        }

        public void PrintFileCount()
        {
            Console.WriteLine("Files count:{0}", _fileNodeDictionay.Count);

        }

        public void PrintNode(TreeNode<PathNode> node)
        {
            StringBuilder msg = new StringBuilder();
            int layer = node.Layer;
            while (layer > 1)
            {
                msg.Append("-----");
                layer--;
            }
            msg.Append(node.Data.FileName);
            msg.Append(string.Format("({0})", node.Path));
            Console.WriteLine(msg);
            foreach (var treeNode in node.GetChildren())
            {
                PrintNode(treeNode);
            }
        }

        public IList<PathNode> FindAllChildren(string dirPath){
            Console.WriteLine("File all children {0}", dirPath);
            PrintNode(_root);
            var dirNode = _fileNodeDictionay[dirPath];
            if(dirNode!=null)
            {
                return dirNode.GetChildrenData();

            }
            return null;

        }

        public PathNode GetFileInfo(string dirPath)
        {
            var dirNode = _fileNodeDictionay[dirPath];
            if (dirNode != null)
            {
                return dirNode.Data;

            }
            return null;

        }

        public void AddNode(FileInformation info, string path, string source)
        {
            var parentNode = _fileNodeDictionay[path];
            if (parentNode == null)
            {
                return;
            }
            if (info.Attributes == FileAttributes.Directory)
            {
                source = null;
            }

            var node = new PathNode()
                {
                    CreationTime = info.CreationTime,
                    FileName = info.FileName,
                    IsDir = info.Attributes == FileAttributes.Directory,
                    LastAccessTime = info.LastAccessTime,
                    LastWriteTime = info.LastWriteTime,
                    Size = info.Length,
                    Source = source
                };
            var treeNode = parentNode.AddChild(node);
            _fileNodeDictionay.Add(treeNode.Path, treeNode);
        }

        public void RmoveNode(string path)
        {
            var node = _fileNodeDictionay[path];
            if (node == null)
                return;

            _fileNodeDictionay.Remove(node.Path);
            node.RemoveSelf();
        }

        public void MoveNode(string curPath, string destPath)
        {
            var node = _fileNodeDictionay[curPath];
            var destNode = _fileNodeDictionay[destPath];
            if (node == null || destNode == null || !destNode.Data.IsDir)
            {
                return;
            }
            if (destNode.GetChildren().Any(n => n.Data.FileName == node.Data.FileName))
            {
                Console.WriteLine("There is file/folder with the same name {0} in {1}", node.Data.FileName,destNode.Path);
                return;
            }
            UnRegisterTreeNode(node);
            node.RemoveSelf();
            node = destNode.AddChild(node);
            RegisterTreeNdoe(node);

        }

        public void CopyNode(string curPath, string destPath)
        {
            var node = _fileNodeDictionay[curPath];
            var destNode = _fileNodeDictionay[destPath];
            if (node == null || destNode == null || !destNode.Data.IsDir)
            {
                return;
            }
            if (destNode.GetChildrenData().Any(n => n.FileName == node.Data.FileName))
            {
                Console.WriteLine("There is file/folder with the same name {0} in {1}", node.Data.FileName, destNode.Path);
                return;
            }
            var dupNode = (TreeNode<PathNode>)node.Clone();

            destNode.AddChild(dupNode);
            RegisterTreeNdoe(dupNode);
        }

        public void RenameNode(string path, string newName)
        {
            var node = _fileNodeDictionay[path];

            if (node == null || node.Data.FileName == newName)
            {
                return;
            }

            node.Rename(newName);

        }

        private void GenerateNode(TreeNode<PathNode> root, ICloudDiskAccessUtility utility)
        {
            var fileList = utility.GetFileList(root.Data.FileName + "/");
            if (fileList == null)
            {
                return;
            }
            var fileInfoList = fileList.Select(f => new PathNode()
            {
                FileName = PathHelper.GetFileName(f.Path),
                IsDir = f.IsDir,
                CreationTime = new DateTime(f.CreateTime),
                LastAccessTime = new DateTime(f.ModifiyTime),
                LastWriteTime = new DateTime(f.ModifiyTime),
                Size = f.Size,
                Source = f.IsDir ? null : utility.Name
            });
            foreach (var fileInfo in fileInfoList)
            {
                TreeNode<PathNode> node = null;
                if (root.GetChildrenData().All(d => d.FileName != fileInfo.FileName))
                {
                    node = root.AddChild(fileInfo);
                    _fileNodeDictionay.Add(node.Path, node);

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

        private void UnRegisterTreeNode(TreeNode<PathNode> node)
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

        private void RegisterTreeNdoe(TreeNode<PathNode> node)
        {
            _fileNodeDictionay.Add(node.Path,node);
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
