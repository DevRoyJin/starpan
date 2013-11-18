﻿using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DiskAPIBase.File;

namespace DiskAPIBase
{
    public interface ICloudDiskAccessUtility
    {   
        /// <summary>
        /// 网盘实例名
        /// </summary>
        string Name { get;  }

        /// <summary>
        /// 网盘根目录
        /// </summary>
        string Root { get;  }

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

        /// <summary>
        /// 获取路径下文件或目录列表
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <returns>文件或目录列表</returns>
        IList<CloudFileInfo> GetFileList(string dirPath);

        /// <summary>
        /// 获取某一文件/目录信息
        /// </summary>
        /// <param name="path">文件或目录路径</param>
        /// <returns>文件或目录信息</returns>
        CloudFileInfo GetFileInfo(string path);


        /// <summary>
        /// 移动文件夹或文件（当）
        /// </summary>
        /// <param name="path">原文件夹/文件路径</param>
        /// <param name="newName">目标文件夹/文件路径</param>
        /// <returns>true为成功，false为失败</returns>
        bool Move(string path, string newName);


    }
}