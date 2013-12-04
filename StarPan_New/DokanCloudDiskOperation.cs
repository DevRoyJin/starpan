using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using DiskAPIBase;
using Dokan;
using StarPan.Structure;

namespace StarPan
{
    public class DokanCloudDiskOperation : DokanOperations
    {
        private const string ROOT_FOLDER = "\\";

        private static object _locker = new object(); // we'll have to make this threadsafe sooner or later
        private readonly DokanOperationProxy _operationProxy = new DokanOperationProxy();

        public int Cleanup(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan Cleanup --> filename:{0}", filename);
            filename = PathHelper.ConvertToWebPath(filename);
            if (_operationProxy.ExistsNode(filename))
            {
                var node = _operationProxy.GetNode(filename);
                if (node.IsFileDateCached)
                {
                    node.ReleaseFileData();
                }
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan CloseFile --> filename:{0}", filename);
            return DokanNet.DOKAN_SUCCESS;
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan OpenDirectory --> filename:{0}", filename);
            if (filename == ROOT_FOLDER)
                return DokanNet.DOKAN_SUCCESS;
            filename = PathHelper.ConvertToWebPath(filename);
            if (!_operationProxy.ExistsNode(filename))
                return -DokanNet.ERROR_FILE_NOT_FOUND; //#46

            return DokanNet.DOKAN_SUCCESS;
        }

        public int CreateFile(string filename, FileAccess access, FileShare share, FileMode mode, FileOptions options,
            DokanFileInfo info)
        {
            Console.WriteLine("Dokan CreateFile --> filename:{0}  mode:{1}", filename,mode);
            if (filename == ROOT_FOLDER)
                return DokanNet.DOKAN_SUCCESS;

            // get parent folder where this file is to be created;
            string parentFolder = PathHelper.ConvertToWebPath(filename.GetPathPart());
            if (string.IsNullOrEmpty(parentFolder))
            {
                parentFolder = "/";
            }
            // does the parent exist?
            if (!_operationProxy.ExistsNode(parentFolder))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            // get the name of the file to be created/opened;
            string newName = filename.GetFilenamePart();

            // Make sure the new directory has a valid filename
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
                return -DokanNet.ERROR_INVALID_NAME;
            if (string.IsNullOrEmpty(newName))
                return -DokanNet.ERROR_INVALID_NAME;

            // we'll need this file later on;
            string thisFile = PathHelper.ConvertToWebPath(filename);

            FileTreeNode<PathInfo> parent = _operationProxy.GetNode(parentFolder.ToLower());
            // this is called when we should create a new file;
            // so raise an error if it's a directory;           
            if (parent.GetChildrenData().Any(d => d.FileName == filename.GetFilenamePart() && d.IsDir))
            {
                //this is a folder?
                info.IsDirectory = true;
                if (mode == FileMode.Open || mode == FileMode.OpenOrCreate)
                {
                    //info.IsDirectory = true;
                    return DokanNet.DOKAN_SUCCESS;
                }

                // you can't make a file with the same name as a folder;
                return -DokanNet.ERROR_ALREADY_EXISTS;
            }

            var newFileInfo = new FileInformation
            {
                Attributes = FileAttributes.Normal & FileAttributes.NotContentIndexed,
                CreationTime = DateTime.Now,
                FileName = newName,
                LastAccessTime = DateTime.Now,
                LastWriteTime = DateTime.Now,
                Length = 0
            };

            // there's no folder with this name, the parent exists;
            // attempt to use the file
            switch (mode)
            {
                    // Opens the file if it exists and seeks to the end of the file, 
                    // or creates a new file
                case FileMode.Append:
                    if (!_operationProxy.ExistsNode(thisFile))
                    {
                        _operationProxy.AddNode(newFileInfo, parentFolder);
                    }
                    return DokanNet.DOKAN_SUCCESS;

                    // Specifies that the operating system should create a new file. 
                    // If the file already exists, it will be overwritten. 
                case FileMode.Create:
                    //if (!thisFile.Exists())
                    _operationProxy.AddNode(newFileInfo, parentFolder);
                    //else
                    //	thisFile.Content = new Thought.Research.AweBuffer(1024); //MemoryStream();
                    return DokanNet.DOKAN_SUCCESS;

                    // Specifies that the operating system should create a new file. 
                    // If the file already exists, an IOException is thrown.
                case FileMode.CreateNew:
                    if (_operationProxy.ExistsNode(thisFile))
                        return -DokanNet.ERROR_ALREADY_EXISTS;
                    _operationProxy.AddNode(newFileInfo, parentFolder);
                    return DokanNet.DOKAN_SUCCESS;

                    // Specifies that the operating system should open an existing file. 
                    // A System.IO.FileNotFoundException is thrown if the file does not exist.
                case FileMode.Open:
                    if (!_operationProxy.ExistsNode(thisFile))
                        return -DokanNet.ERROR_FILE_NOT_FOUND;
                    return DokanNet.DOKAN_SUCCESS;

                    // Specifies that the operating system should open a file if it exists; 
                    // otherwise, a new file should be created.
                case FileMode.OpenOrCreate:
                    if (!_operationProxy.ExistsNode(thisFile))
                        _operationProxy.AddNode(newFileInfo, parentFolder);
                    return DokanNet.DOKAN_SUCCESS;

                    // Specifies that the operating system should open an existing file. 
                    // Once opened, the file should be truncated so that its size is zero bytes
                case FileMode.Truncate:
                    if (!_operationProxy.ExistsNode(thisFile))
                        _operationProxy.AddNode(newFileInfo, parentFolder);
                    //thisFile.Size = 0;
                    return DokanNet.DOKAN_SUCCESS;
            }

            return DokanNet.DOKAN_ERROR;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan CreateDirectory --> filename:{0}", filename);
            // Make sure the new directory has a valid filename
            string newName = filename.GetFilenamePart();
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
                return -DokanNet.ERROR_INVALID_NAME;
            if (string.IsNullOrEmpty(newName))
                return -DokanNet.ERROR_INVALID_NAME;

            // Get parent-folder 
            // (where this new directory should be created)
            string parentFolderPath = PathHelper.ConvertToWebPath(filename.GetPathPart());
            if (string.IsNullOrEmpty(parentFolderPath))
            {
                parentFolderPath = "/";
            }
            //// return an error if the parent-path doesn't exist
            if (!_operationProxy.ExistsNode(parentFolderPath))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            if(_operationProxy.ExistsNode(PathHelper.ConvertToWebPath(filename)))
            {
                return DokanNet.DOKAN_SUCCESS;
            }
            var fInfo = new FileInformation
            {
                Attributes = FileAttributes.Directory,
                CreationTime = DateTime.Now,
                FileName = newName,
                LastAccessTime = DateTime.Now,
                LastWriteTime = DateTime.Now,
                Length = 0
            };
            _operationProxy.AddNode(fInfo, parentFolderPath);

            //// Inform Dokan
            return _operationProxy.ExistsNode(PathHelper.ConvertToWebPath(filename))
                ? DokanNet.DOKAN_SUCCESS
                : DokanNet.DOKAN_ERROR;
        }


        public int FindFiles(string filename, ArrayList files, DokanFileInfo info)
        {
            Console.WriteLine("Dokan FindFiles --> filename:{0}", filename);
            filename = PathHelper.ConvertToWebPath(filename);
            // do we have this folder?
            if (!_operationProxy.ExistsNode(filename))
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            if (!_operationProxy.GetNode(filename).FileInfo.IsDir)
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            FileTreeNode<PathInfo> node = _operationProxy.GetNode(filename);
            // we have this folder, list all it's children;
            foreach (PathInfo item in node.GetChildrenData())
            {
                FileInformation fileinfo = item.ToFileInformation();

                //// if it's a file, then also report a size;
                //if (item is MemoryFile)
                //    fileinfo.Length = (item as MemoryFile).Size;

                files.Add(fileinfo);
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            Console.WriteLine("Dokan ReadFile --> filename:{0}", filename);
            // get parentfolder
            Dokan.DokanNet.DokanResetTimeout(1000 * 30, info);
            string parentFolder = PathHelper.ConvertToWebPath(filename.GetPathPart());
            if (string.IsNullOrEmpty(parentFolder))
            {
                parentFolder = "/";
            }
            // exists?
            if (!_operationProxy.ExistsNode(parentFolder))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            // get file
            string path = PathHelper.ConvertToWebPath(filename);
            // exists?
            if (!_operationProxy.ExistsNode(path))
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            FileTreeNode<PathInfo> node = _operationProxy.GetNode(path);
            readBytes = node.ReadFileData(offset, buffer);
            return DokanNet.DOKAN_SUCCESS;
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            Console.WriteLine("Dokan WriteFile --> filename:{0}", filename);
            // get parentfolder
            string parentFolder = PathHelper.ConvertToWebPath(filename.GetPathPart());
            if (string.IsNullOrEmpty(parentFolder))
            {
                parentFolder = "/";
            }
            // exists?
            if (!_operationProxy.ExistsNode(parentFolder))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            // get file
            string path = PathHelper.ConvertToWebPath(filename);
            // exists?
            if (!_operationProxy.ExistsNode(path))
            {
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            }

            var node = _operationProxy.GetNode(path);
            writtenBytes = node.WriteFileData(offset, buffer);


            return DokanNet.DOKAN_SUCCESS;
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan FlushFileBuffers --> filename:{0}", filename);
            return DokanNet.DOKAN_SUCCESS;
        }

        public int GetFileInformation(string filename, FileInformation fileinfo, DokanFileInfo info)
        {
            Console.WriteLine("Dokan GetFileInformation --> filename:{0}", filename);
            if (filename == ROOT_FOLDER || info.IsDirectory)
            {
                filename = PathHelper.ConvertToWebPath(filename);
                // ..about a folder?

                if (!_operationProxy.ExistsNode(filename))
                    return -DokanNet.ERROR_PATH_NOT_FOUND;
                FileTreeNode<PathInfo> node = _operationProxy.GetNode(filename);
                FileInformation folder = node.FileInfo.ToFileInformation();
                fileinfo.FileName = folder.FileName;
                fileinfo.Attributes = folder.Attributes;
                fileinfo.LastAccessTime = folder.LastAccessTime;
                fileinfo.LastWriteTime = folder.LastWriteTime;
                fileinfo.CreationTime = folder.CreationTime;
                return DokanNet.DOKAN_SUCCESS;
            }
            // ..about a file?
            string path = PathHelper.ConvertToWebPath(filename);
            string parentFolder = PathHelper.ConvertToWebPath(filename.GetPathPart());
            if (string.IsNullOrEmpty(parentFolder))
            {
                parentFolder = "/";
            }
            // at least the parent should exist;
            if (!_operationProxy.ExistsNode(parentFolder))
                return DokanNet.DOKAN_ERROR;

            // does it exist?
            //var file = parentFolder.FetchFile(name);
            if (!_operationProxy.ExistsNode(path))
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            FileInformation file = _operationProxy.GetNode(path).FileInfo.ToFileInformation();
            fileinfo.FileName = file.FileName;
            fileinfo.Attributes = file.Attributes;
            fileinfo.LastAccessTime = file.LastAccessTime;
            fileinfo.LastWriteTime = file.LastWriteTime;
            fileinfo.CreationTime = file.CreationTime;
            fileinfo.Length = file.Length;
            return DokanNet.DOKAN_SUCCESS;
        }


        public int DeleteFile(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan DeleteFile --> filename:{0}", filename);
            // fetch file;
            string path = PathHelper.ConvertToWebPath(filename);

            // exists?
            if (!_operationProxy.ExistsNode(path))
                return -DokanNet.ERROR_FILE_NOT_FOUND;

            // delete it;
            _operationProxy.RemoveNode(path);

            return DokanNet.DOKAN_SUCCESS;
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            Console.WriteLine("Dokan DeleteDirectory --> filename:{0}", filename);
            // find target;
            string path = PathHelper.ConvertToWebPath(filename);

            // exists?
            if (!_operationProxy.ExistsNode(path))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            // delete it;
            _operationProxy.RemoveNode(path);
            return DokanNet.DOKAN_SUCCESS;
        }

        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            Console.WriteLine("Dokan MoveFile --> filename:{0} newname:{1} replace:{2}", filename, newname, replace);
            _operationProxy.PrintNode(_operationProxy.Root);
            if (!_operationProxy.ExistsNode(PathHelper.ConvertToWebPath(filename)))
            {
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            }
            // find new parent 
            string newParent = PathHelper.ConvertToWebPath(newname.GetPathPart());
            if (string.IsNullOrEmpty(newParent))
            {
                newParent = "/";
            }
            // does it exist?
            if (!_operationProxy.ExistsNode(newParent))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            // Make sure we have a valid name
            string newFileName = newname.GetFilenamePart();
            if (newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
                return -DokanNet.ERROR_INVALID_NAME;
            if (string.IsNullOrEmpty(newFileName))
                return -DokanNet.ERROR_INVALID_NAME;

            // Make sure that there's not already a folder with this name;
            if (_operationProxy.ExistsNode(PathHelper.ConvertToWebPath(newname)))
            {
                FileTreeNode<PathInfo> existsNode = _operationProxy.GetNode(newname);
                if (existsNode.FileInfo.IsDir)
                {
                    return -DokanNet.ERROR_ALREADY_EXISTS;
                }
                return -DokanNet.ERROR_FILE_EXISTS;
            }
            string curPath = PathHelper.ConvertToWebPath(filename);
            string destPath = PathHelper.ConvertToWebPath(newname);
            
            _operationProxy.MoveNode(curPath, destPath);
            _operationProxy.PrintNode(_operationProxy.Root);
            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            Console.WriteLine("Dokan SetEndOfFile --> filename:{0} length:{1}", filename, length);
            // get parentfolder
            string parentFolder = PathHelper.ConvertToWebPath(filename.GetPathPart());
            if (string.IsNullOrEmpty(parentFolder))
            {
                parentFolder = "/";
            }
            // exists?
            if (!_operationProxy.ExistsNode(parentFolder))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            // get file
            string path = PathHelper.ConvertToWebPath(filename);
            // exists?
            if (!_operationProxy.ExistsNode(path))
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            FileTreeNode<PathInfo> file = _operationProxy.GetNode(path);
            file.Size = length;
            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            Console.WriteLine("Dokan SetEndOfFile --> filename:{0} length:{1}", filename, length);
            // get parentfolder
            string parentFolder = PathHelper.ConvertToWebPath(filename.GetPathPart());
            if (string.IsNullOrEmpty(parentFolder))
            {
                parentFolder = "/";
            }
            // exists?
            if (!_operationProxy.ExistsNode(parentFolder))
                return -DokanNet.ERROR_PATH_NOT_FOUND;

            // get file
            string path = PathHelper.ConvertToWebPath(filename);
            // exists?
            if (!_operationProxy.ExistsNode(path))
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            FileTreeNode<PathInfo> file = _operationProxy.GetNode(path);
            file.Size = length;
            return DokanNet.DOKAN_SUCCESS;
        }


        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes,
            DokanFileInfo info)
        {
            Console.WriteLine("Dokan GetDiskFreeSpace");
            //totalBytes = (ulong)Environment.WorkingSet;
            totalBytes = 1024*1024*1024; //TODO:Read form config

            // The total number free bytes amounts to the total, minus what's used;
            freeBytesAvailable = totalBytes - (ulong) _operationProxy.Root.Size;

            // The Dokan-interface seems to ignore this one;
            totalFreeBytes = int.MaxValue;

            return 0;
        }

        public int LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            Console.WriteLine("Dokan LockFile --> filename:{0} offset:{1}  length:{2}", filename, offset, length);
            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetFileAttributes(
            string filename,
            FileAttributes attr,
            DokanFileInfo info)
        {
            Console.WriteLine("Dokan SetFileAttributes --> filename:{0}", filename); 
            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetFileTime(
            string filename,
            DateTime ctime,
            DateTime atime,
            DateTime mtime,
            DokanFileInfo info)
        {
            Console.WriteLine("Dokan SetFileTime --> filename:{0}", filename); 
            return DokanNet.DOKAN_SUCCESS;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            Console.WriteLine("Dokan UnlockFile --> filename:{0}", filename); 
            return DokanNet.DOKAN_SUCCESS;
        }

        public int Unmount(DokanFileInfo info)
        {
            Console.WriteLine("Dokan Unmount"); 
            return DokanNet.DOKAN_SUCCESS;
        }
    }
}