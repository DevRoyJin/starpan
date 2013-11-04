namespace DiskAPIBase
{
    public interface ICloudDiskAccessUtility
    {   
        /// <summary>
        /// 获取网盘配额
        /// </summary>
        /// <returns>配额空间大小（字节数）</returns>
        long GetQuota();

        /// <summary>
        /// 获取网盘已使用空间大小
        /// </summary>
        /// <returns>已使用空间大小（字节数）</returns>
        long GetUsedSpace();

        /// <summary>
        /// 获取网盘可用空间大小
        /// </summary>
        /// <returns>可用空间大小（字节数）</returns>
        long GetFreeSpace();

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="path">上传网盘路径</param>
        /// <param name="fileData">文件数据流</param>
        /// <returns>true为成功，false为失败</returns>
        bool UploadFile(string path,byte[] fileData);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path">网盘下载路径</param>
        /// <param name="fileData">下载文件数据流</param>
        /// <returns>true为成功，false为失败</returns>
        bool DownloadFile(string path, out byte[] fileData);

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path">创建文件夹路径</param>
        /// <returns>true为成功，false为失败</returns>
        bool CreateDirectory(string path);

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="path">删除文件夹路径</param>
        /// <returns>true为成功，false为失败</returns>
        bool DeleteDirectory(string path);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath">删除文件夹路径</param>
        /// <returns>true为成功，false为失败</returns>
        bool DeleteFile(string filePath);

    }
}