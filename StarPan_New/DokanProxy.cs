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
        private TreeNode<FileInformation> _root;
        private IList<TreeNode<FileInformation>> _fileNodeList;


        public DokanProxy()
        {
            _fileNodeList = new List<TreeNode<FileInformation>>();
            var rootInfo = new FileInformation
            {
                FileName = "/",
                Attributes = FileAttributes.Directory,
                CreationTime = new DateTime(0),
                LastAccessTime = new DateTime(0),
                LastWriteTime = new DateTime(0),
                Length = 0

            };
            _root = new TreeNode<FileInformation>(rootInfo);
            _fileNodeList.Add(_root);
            var diskUtilitys = CloudDiskManager.Instance.GetAllCloudDisk();
            foreach (var utility in diskUtilitys)
            {
                GenerateNode(_root, utility);
            }
        }

        public TreeNode<FileInformation> Root
        {
            get
            {
                return _root;
            }
        }

        private void GenerateNode(TreeNode<FileInformation> root, ICloudDiskAccessUtility utility)
        {
            var fileList = utility.GetFileList(root.Data.FileName+"/");
            if (fileList == null)
            {
                return;
            }
            var fileInfoList = fileList.Select(f => new FileInformation()
            {
                FileName = "/" + f.Path,
                Attributes = f.IsDir ? FileAttributes.Directory : FileAttributes.Normal,
                CreationTime = new DateTime(f.CreateTime),
                LastAccessTime = new DateTime(f.ModifiyTime),
                LastWriteTime = new DateTime(f.ModifiyTime),
                Length = f.Size
            });
            foreach (var fileInfo in fileInfoList)
            {
                TreeNode<FileInformation> node = null;
                if (root.GetChildrenData().All(d => d.FileName != fileInfo.FileName))
                {
                    node = root.AddChild(fileInfo);
                    _fileNodeList.Add(node);

                }
                else
                {
                    node = root.GetChild(f => f.FileName == fileInfo.FileName);
                }
                if (fileInfo.Attributes == FileAttributes.Directory)
                {
                    GenerateNode(node, utility);
                }
            }

        }

        public void PrintFileCount()
        {
            Console.WriteLine("Files count:{0}", _fileNodeList.Count);

        }

        //public void PrintNode()
        //{
        //    foreach (var node in _fileNodeList)
        //    {
        //        StringBuilder msg = new StringBuilder();
        //        int layer = node.Layer;
        //        while (layer > 1)
        //        {
        //            msg.Append("-----");
        //            layer--;
        //        }
        //        msg.Append(node.Data.FileName);
        //        Console.WriteLine(msg);
        //    }

        //}

        public void PrintNode(TreeNode<FileInformation> node)
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

        public IList<FileInformation> FindAllChildren(string dirPath){
            Console.WriteLine("File all children {0}", dirPath);
            PrintNode(_root);
            var dirNode = _fileNodeList.FirstOrDefault(node=>node.Data.FileName==dirPath);
            if(dirNode!=null)
            {
                return dirNode.GetChildrenData();

            }
            return null;

        }

        public FileInformation GetFileInfo(string dirPath)
        {
            var dirNode = _fileNodeList.FirstOrDefault(node => node.Data.FileName == dirPath);
            if (dirNode != null)
            {
                return dirNode.Data;

            }
            return null;

        }

        
    }
}
