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
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using KuaiPanSDK.Model;
using PHPLibrary;

namespace KuaiPanSDK
{
    public class KuaiPan
    {
        #region 构造方法
        /// <summary>
        /// 第一次使用
        /// </summary>
        public KuaiPan()
        {
            NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("KuaiPanSectionGroup/KuaiPanSection");
            this.ConsumerKey = section["ConsumerKey"];
            this.ConsumerSecret = section["ConsumerSecret"];
            this.CallBackUrl = section["CallBackUrl"];
        }

        /// <summary>
        /// 知道了Token && Token_Secret的时候使用
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="token_secret">Token_Secret</param>
        public KuaiPan(string token, string token_secret)
        {
            NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("KuaiPanSectionGroup/KuaiPanSection");
            this.ConsumerKey = section["ConsumerKey"];
            this.ConsumerSecret = section["ConsumerSecret"];
            this.Token = token;
            this.TokenSecret = token_secret;
        }

        /// <summary>
        /// 无配置文件的时候使用
        /// </summary>
        /// <param name="consumerkey">ConsumerKey</param>
        /// <param name="consumersecret">ConsumerSecret</param>
        /// <param name="token">Token</param>
        /// <param name="token_secret">Token_Secret</param>
        public KuaiPan(string consumerkey, string consumersecret, string token, string token_secret)
        {
            this.ConsumerKey = consumerkey;
            this.ConsumerSecret = consumersecret;
            this.Token = token;
            this.TokenSecret = token_secret;
        }
        #endregion

        #region 常用参数
        /// <summary>
        /// ConsumerKey
        /// </summary>
        /// <returns></returns>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// ConsumerSecret
        /// </summary>
        /// <returns></returns>
        public string ConsumerSecret { get; set; }

        /// <summary>
        /// CallBackUrl
        /// </summary>
        /// <returns></returns>
        public string CallBackUrl { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// TokenSecret
        /// </summary>
        public string TokenSecret { get; set; }

        /// <summary>
        /// UserId
        /// </summary>
        public int UserId { get; private set; }

        /// <summary>
        /// 可见的根目录ID
        /// </summary>
        public string ChargedDir { get; private set; }

        /// <summary>
        /// ErrMsg
        /// </summary>
        public string ErrMsg { get; private set; }
        #endregion

        #region 获取未授权的临时token
        /// <summary>
        /// 获取未授权的临时token
        /// </summary>
        /// <returns></returns>
        private bool RequestToken()
        {
            string result = this.Http_Get(this.GetUrl(GlobalURL.REQUESTTOKEN));
            if (result == null)
            {
                return false;
            }
            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法获取Token";
                return false;
            }
            this.Token = json["oauth_token"];
            this.TokenSecret = json["oauth_token_secret"];
            return true;
        }
        #endregion

        #region 用户验证token。这个接口返回一个web页面，要求快盘用户登陆和授权。
        /// <summary>
        /// 用户验证token。这个接口返回一个web页面，要求快盘用户登陆和授权。
        /// </summary>
        /// <returns></returns>
        public string GetAuthorizationUrl()
        {
            if (this.RequestToken())
            {
                return string.Format(GlobalURL.AUTHORISE, this.Token);
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        #region 用临时token换取access token。
        /// <summary>
        /// 用临时token换取access token。
        /// </summary>
        /// <returns></returns>
        public bool GetAccessToken()
        {
            string result = this.Http_Get(this.GetUrl(GlobalURL.ACCESSTOKEN));
            if (result == null)
            {
                return false;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法获取Token";
                return false;
            }

            this.Token = json["oauth_token"];
            this.TokenSecret = json["oauth_token_secret"];
            this.UserId = json["user_id"];
            this.ChargedDir = json["charged_dir"];
            return true;
        }
        #endregion

        #region 获取用户信息
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public AccountInfo GetAccountInfo()
        {
            string url = this.GetUrl(string.Format(GlobalURL.ACCOUNTINFO, GlobalURL.Version));
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }
            AccountInfo info = new AccountInfo(json);
            if (info.UserId == 0)
            {
                return null;
            }
            return info;
        }
        #endregion

        #region 获取文件(夹)信息
        /// <summary>
        /// 获取文件(夹)信息
        /// </summary>
        /// <param name="path">相对于root的路径,包含文件名</param>
        /// <param name="array">额外参数</param>
        /// <returns></returns>
        public MetaData GetMetaData(string path, PHPArray array)
        {
            string root = "app_folder";
            string url = this.GetUrl(string.Format(GlobalURL.METADATA, GlobalURL.Version, root, Common.UrlEncode(path.Trim('/'), "/")), array);
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }

            MetaData metadata = new MetaData(json);
            return metadata;
        }
        #endregion

        #region 获取文件分享链接
        /// <summary>
        /// 获取文件分享链接
        /// </summary>
        /// <param name="path">相对于root的路径,包含文件名</param>
        /// <returns></returns>
        public Shares GetShares(string path)
        {
            string root = "app_folder";
            string url = this.GetUrl(string.Format(GlobalURL.SHARES, GlobalURL.Version, root, Common.UrlEncode(path.Trim('/'), "/")));
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }

            Shares shares = new Shares(json);
            return shares;
        }
        #endregion

        #region 创建文件(夹)
        /// <summary>
        /// 创建文件(夹)
        /// </summary>
        /// <param name="path">相对于root的路径,包含文件名</param>
        /// <returns></returns>
        public Create Create(string path)
        {
            PHPArray array = new PHPArray();
            array.Add("root", "app_folder");
            array.Add("path", path);

            string url = this.GetUrl(string.Format(GlobalURL.CREATEFOLDER, GlobalURL.Version), array);
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }

            Create create = new Create(json);
            return create;
        }
        #endregion

        #region 删除文件(夹)
        /// <summary>
        /// 删除文件(夹)
        /// </summary>
        /// <param name="path">相对于root的路径,包含文件名</param>
        /// <returns></returns>
        public Delete Delete(string path)
        {
            return this.Delete(path, true);
        }

        /// <summary>
        /// 删除文件(夹)
        /// </summary>
        /// <param name="path">相对于root的路径,包含文件名</param>
        /// <param name="to_recycle">默认True,True - 删除的文件（夹）放入回收站，空间不回收；False - 彻底删除，并释放相应空间。</param>
        /// <returns></returns>
        public Delete Delete(string path, bool to_recycle)
        {
            PHPArray array = new PHPArray();
            array.Add("root", "app_folder");
            array.Add("path", path);
            array.Add("to_recycle", to_recycle ? "True" : "False");

            string url = this.GetUrl(string.Format(GlobalURL.DELETE, GlobalURL.Version), array);
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }

            Delete delete = new Delete(json);
            return delete;
        }
        #endregion

        #region 移动文件(夹)
        /// <summary>
        /// 移动文件(夹)
        /// </summary>
        /// <param name="from_path">相对于root的旧文件路径</param>
        /// <param name="to_path">相对于root的新文件路径，包含文件名(和from_path 的文件不同名的话，则重命名)</param>
        /// <returns></returns>
        public Move Move(string from_path, string to_path)
        {
            PHPArray array = new PHPArray();
            array.Add("root", "app_folder");
            array.Add("from_path", from_path);
            array.Add("to_path", to_path);

            string url = this.GetUrl(string.Format(GlobalURL.MOVE, GlobalURL.Version), array);
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }

            Move move = new Move(json);
            return move;
        }
        #endregion

        #region 复制文件(夹)
        /// <summary>
        /// 复制文件(夹)
        /// </summary>
        /// <param name="from_path">相对于root的旧文件路径</param>
        /// <param name="to_path">相对于root的新文件夹路径，包含文件名（和from_path的文件不同名的话，则重命名）</param>
        /// <returns></returns>
        public Copy Copy(string from_path, string to_path)
        {
            PHPArray array = new PHPArray();
            array.Add("root", "app_folder");
            array.Add("from_path", from_path);
            array.Add("to_path", to_path);

            string url = this.GetUrl(string.Format(GlobalURL.COPY, GlobalURL.Version), array);
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }

            Copy copy = new Copy(json);
            return copy;
        }
        #endregion

        #region 获取文件上传地址
        /// <summary>
        /// 获取文件上传地址
        /// </summary>
        /// <returns></returns>
        private string GetUploadLocate()
        {
            PHPArray array = new PHPArray();
            array.Add("source_ip", "");

            string url = this.GetUrl(string.Format(GlobalURL.UPLOAD_LOCATE, GlobalURL.Version));
            string result = this.Http_Get(url);
            if (result == null)
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }
            return json["url"];
        }
        #endregion

        #region 上传文件
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        /// <param name="overwrite"></param>
        /// <param name="filename"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public FileData UpLoadFile(string path, bool overwrite, string filename, byte[] fileData)
        {
            PHPArray array = new PHPArray();
            array.Add("root", "app_folder");
            array.Add("path", path);
            array.Add("overwrite", overwrite ? "true" : "flase");
            string upurl = this.GetUploadLocate();
            if (upurl == null)
            {
                return null;
            }

            upurl = this.GetUrl(string.Format(GlobalURL.UPLOAD_FILE, GlobalURL.Version, upurl), array);
            MsMultiPartFormData form = new MsMultiPartFormData();
            form.AddStreamFile("file", filename, fileData);
            form.PrepareFormData();
            string result = this.Http_POST(upurl, form);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            PHPArray json = Common.JsonToPHPArray(result);
            if (json == null)
            {
                this.ErrMsg = "无法解析Json数据";
                return null;
            }
            FileData info = new FileData(json);
            return info;
        }
        #endregion

        #region 获取下载地址
        /// <summary>
        /// 获取下载地址
        /// </summary>
        /// <param name="root">app_folder或kuaipan</param>
        /// <param name="path">相对于root的路径</param>
        /// <returns></returns>
        public string Download(string path)
        {
            PHPArray array = new PHPArray();
            array.Add("root", "app_folder");
            array.Add("path", path);

            return this.GetUrl(string.Format(GlobalURL.DOWNLOAD_FILE, GlobalURL.Version), array);
        }
        #endregion

        #region 私用方法
        private string GetUrl(string url)
        {
            return this.GetUrl(url, null);
        }

        private string GetUrl(string url, PHPArray array)
        {
            OAuth o = new OAuth(this);
            o.Url = url;
            o.Array = array;
            return o.GetUrl();
        }

        private string Http_Get(string url)
        {
            HttpHelper http = new HttpHelper();
            http.Url = url;
            http.Do();
            if (http.StatusCode != HttpStatusCode.OK)
            {
                this.ErrMsg = StatusCodeMsg.GetMsg(http.StatusCode);
                this.ErrMsg = http.ErrMsg;
                return null;
            }
            return http.Html;
        }

        private string Http_POST(string url, MsMultiPartFormData form)
        {
            HttpHelper http = new HttpHelper();
            http.Url = url;
            http.Method = Method.POST;
            http.ContentType = "multipart/form-data; boundary=" + form.Boundary;
            http.PostDataByte = form.GetFormData();
            http.Do();
            return http.Html;
        }
        #endregion
    }
}
