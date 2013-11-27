using System;
using System.Collections;
using System.IO;
using Dokan;

namespace StarPan
{
    public class DokanCloudDiskOperation : Dokan.DokanOperations
    {

        public int Cleanup(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int CreateFile(string filename, FileAccess access, FileShare share, FileMode mode, FileOptions options,
            DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            //// Get parent-folder 
            //// (where this new directory should be created)
            //string parentFolderPath = filename.GetPathPart();
            //MemoryFolder parentFolder = _root.GetFolderByPath(parentFolderPath);

            //// return an error if the parent-path doesn't exist
            //if (!parentFolder.Exists())
            //    return -DokanNet.ERROR_PATH_NOT_FOUND;

            //// Make sure the new directory has a valid filename
            //string newName = filename.GetFilenamePart();
            //if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
            //    return -DokanNet.ERROR_INVALID_NAME;
            //if (string.IsNullOrEmpty(newName))
            //    return -DokanNet.ERROR_INVALID_NAME;

            //// check if already exists;
            //MemoryFolder testFolder = _root.GetFolderByPath(filename);
            //if (testFolder.Exists())
            //    return -DokanNet.ERROR_ALREADY_EXISTS;

            //// make a folder :)
            //MemoryFolder newFolder = new MemoryFolder(parentFolder, newName);

            //// Inform Dokan
            //return newFolder.Exists() ? DokanNet.DOKAN_SUCCESS : DokanNet.DOKAN_ERROR;

            throw new NotImplementedException();
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int GetFileInformation(string filename, FileInformation fileinfo, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int FindFiles(string filename, ArrayList files, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }


        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }



        public int SetFileAttributes(
            string filename,
            FileAttributes attr,
            DokanFileInfo info)
        {
            return -DokanNet.DOKAN_ERROR;
        }

        public int SetFileTime(
            string filename,
            DateTime ctime,
            DateTime atime,
            DateTime mtime,
            DokanFileInfo info)
        {
            return -DokanNet.DOKAN_ERROR;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int Unmount(DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

    }
}
