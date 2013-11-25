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
                    CurrentPath = "/",
                    ParentPath = null,
                    Source = null
                };

            _root = new TreeNode<PathNode>(rootInfo);
            _fileNodeDictionay.Add(rootInfo.CurrentPath, _root);
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

        private void GenerateNode(TreeNode<PathNode> root, ICloudDiskAccessUtility utility)
        {
            var fileList = utility.GetFileList(root.Data.FileName+"/");
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
                CurrentPath = "/" + f.Path,
                ParentPath = "/" + PathHelper.GetParentDirectory(f.Path),
                Source = f.IsDir ? null : utility.Name
            });
            foreach (var fileInfo in fileInfoList)
            {
                TreeNode<PathNode> node = null;
                if (root.GetChildrenData().All(d => d.CurrentPath != fileInfo.CurrentPath))
                {
                    node = root.AddChild(fileInfo);
                    _fileNodeDictionay.Add(node.Data.CurrentPath,node);

                }
                else
                {
                    node = root.GetChild(f => f.CurrentPath == fileInfo.CurrentPath);
                }
                if (fileInfo.IsDir)
                {
                    GenerateNode(node, utility);
                }
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
                    CurrentPath = "/" + PathHelper.CombineWebPath(path, info.FileName),
                    ParentPath = path,
                    FileName = info.FileName,
                    IsDir = info.Attributes == FileAttributes.Directory,
                    LastAccessTime = info.LastAccessTime,
                    LastWriteTime = info.LastWriteTime,
                    Size = info.Length,
                    Source = source
                };
            var treeNode = parentNode.AddChild(node);
            _fileNodeDictionay.Add(node.CurrentPath, treeNode);
        }

        public void RmoveNode(string path)
        {
            var node = _fileNodeDictionay[path];
            if (node == null)
                return;

            node.RemoveSelf();
        }

        public void MoveNode(string curPath, string destinationPath)
        {
            var node = _fileNodeDictionay[curPath];
            var destNode = _fileNodeDictionay[destinationPath];
            if (node == null || destNode == null || !destNode.Data.IsDir)
            {
                return;
            }
            if (destNode.GetChildren().Any(n => n.Data.FileName == node.Data.FileName))
            {
                Console.WriteLine("There is file/folder with the same name {0} in {1}",node.Data.FileName,destNode.Data.CurrentPath);
                return;
            }
            node.RemoveSelf();
            destNode.AddChild(node);

        }

        public void CopyNode(string curPath, string destinationPath)
        {
            var node = _fileNodeDictionay[curPath];
            var destNode = _fileNodeDictionay[destinationPath];
            if (node == null || destNode == null || !destNode.Data.IsDir)
            {
                return;
            }
            if (destNode.GetChildren().Any(n => n.Data.FileName == node.Data.FileName))
            {
                Console.WriteLine("There is file/folder with the same name {0} in {1}", node.Data.FileName, destNode.Data.CurrentPath);
                return;
            }
            var dupNode = (TreeNode<PathNode>)node.Clone();
            destNode.AddChild(dupNode);

            //TODO:处理路径变更
            //
        }
        
    }
}
