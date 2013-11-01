namespace DiskAPIBase
{
    public interface IDiskUtility
    {       
        long GetQuota();

        long GetUsedSpace();

        long GetFreeSpace();

        bool UploadFile(string path);

        bool DownloadFile(string pcsPath, out byte[] filebuffer);

        bool CreateDirectory(string pcsPath);

        bool DeleteDirectory(string pcsPath);

        bool DeleteFile(string pcsPath);

    }
}