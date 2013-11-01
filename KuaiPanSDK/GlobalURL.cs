/*
 * ========================================================================
 * Copyright(c) 2007-2012 情留メ蚊子, All Rights Reserved.
 * Welcom use the KuaiPanSDK.
 * See more information,Please goto http://www.94qing.com
 * ========================================================================
 * 
 * 作 者：情留メ蚊子
 * Q  Q: 540644769
 * 邮 箱：qlwz@qq.com
 * 网 址：http://www.94qing.com
 * ========================================================================
*/
namespace KuaiPanSDK
{
    public class GlobalURL
    {
        /// <summary>
        /// 
        /// </summary>
        public const int Version = 1;

        /// <summary>
        /// 获取未授权的Request Token
        /// </summary>
        public const string REQUESTTOKEN = "https://openapi.kuaipan.cn/open/requestToken";
        /// <summary>
        /// 请求用户授权Token
        /// </summary>
        public const string AUTHORISE = "https://www.kuaipan.cn/api.php?ac=open&op=authorise&oauth_token={0}";
        /// <summary>
        /// 获取授权过的Access Token
        /// </summary>
        public const string ACCESSTOKEN = "https://openapi.kuaipan.cn/open/accessToken";
        /// <summary>
        /// 获取用户信息
        /// </summary>
        public const string ACCOUNTINFO = "http://openapi.kuaipan.cn/{0}/account_info";
        /// <summary>
        /// 获取文件(夹)信息
        /// </summary>
        public const string METADATA = "http://openapi.kuaipan.cn/{0}/metadata/{1}/{2}";
        /// <summary>
        /// 获取文件分享链接
        /// </summary>
        public const string SHARES = "http://openapi.kuaipan.cn/{0}/shares/{1}/{2}";
        /// <summary>
        /// 创建文件(夹)
        /// </summary>
        public const string CREATEFOLDER = "http://openapi.kuaipan.cn/{0}/fileops/create_folder";
        /// <summary>
        /// 删除文件(夹)
        /// </summary>
        public const string DELETE = "http://openapi.kuaipan.cn/{0}/fileops/delete";
        /// <summary>
        /// 移动文件(夹)
        /// </summary>
        public const string MOVE = "http://openapi.kuaipan.cn/{0}/fileops/move";
        /// <summary>
        /// 复制文件(夹)
        /// </summary>
        public const string COPY = "http://openapi.kuaipan.cn/{0}/fileops/copy";
        /// <summary>
        /// 获取文件上传地址
        /// </summary>
        public const string UPLOAD_LOCATE = "http://api-content.dfs.kuaipan.cn/{0}/fileops/upload_locate";
        /// <summary>
        /// 获取文件上传地址
        /// </summary>
        public const string UPLOAD_FILE = "{1}/{0}/fileops/upload_file";
        /// <summary>
        /// 下载文件
        /// </summary>
        public const string DOWNLOAD_FILE = "http://api-content.dfs.kuaipan.cn/{0}/fileops/download_file";
    }
}
