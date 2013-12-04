using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DiskAPIBase
{
    /// <summary>
    /// 有关HTTP请求的辅助类
    /// </summary>
    public class HttpWebResponseUtility
    {
        public const int DefaultRequestTimeout = 60000;

        public static HttpWebRequest CreateGetHttpRequest(string url, int? timeout, string userAgent, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request;
        }

        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        /// <summary>
        /// 创建GET方式的HTTP请求并获取response
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="timeout">请求的超时时间</param>
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>
        /// <returns></returns>
        public static HttpWebResponse CreateGetHttpResponse(string url, int? timeout, string userAgent, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 创建POST方式的HTTP请求
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>
        /// <param name="timeout">请求的超时时间</param>
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>
        /// <returns></returns>
        public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int? timeout, string userAgent, Encoding requestEncoding, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                throw new ArgumentNullException("requestEncoding");
            }
            HttpWebRequest request = null;
            //如果是发送HTTPS请求
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //如果需要POST数据
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                    }
                    i++;
                }
                byte[] data = requestEncoding.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 创建POST方式的HTTP请求（文件上传）
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="contentType">请求内容类型</param>
        /// <param name="postData">post数据（自定义）</param>
        /// <param name="timeout">请求的超时时间</param>
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>        
        /// <returns></returns>
        public static HttpWebResponse CreatePostHttpResponse(string url,string contentType,string postData, int? timeout, string userAgent, Encoding requestEncoding, CookieCollection cookies)
        {         
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                throw new ArgumentNullException("requestEncoding");
            }
            HttpWebRequest request = null;
            //如果是发送HTTPS请求
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = contentType;
            request.Accept = @"*/*";


            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //写入自定义post数据
            byte[] data = requestEncoding.GetBytes(postData);
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            return request.GetResponse() as HttpWebResponse;
        }




        public static string ConvertReponseToString(HttpWebResponse response)
        {
            byte[] b = null;
            using (Stream stream = response.GetResponseStream())
            {
                using (var ms = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        byte[] buf = new byte[1024];
                        count = stream.Read(buf, 0, 1024);
                        ms.Write(buf, 0, count);
                    } while (stream.CanRead && count > 0);
                    b = ms.ToArray();
                }
            }
            if (b.Length > 0)
            {
                return Encoding.Default.GetString(b);

            }
            return null;
        }

        #region [Obsoleted]
        public static string ConstructFileUploadPostData(string filePath,string appPath,string boundary)
        {       
            var postDataBuilder = new StringBuilder();
            boundary = "--" + boundary;
            //构造上传文件post数据
            if (!string.IsNullOrEmpty(filePath))
            {
                if (System.IO.File.Exists(filePath))
                {
                    //FileName
                    postDataBuilder.Append(boundary);
                    postDataBuilder.Append("\r\n");
                    postDataBuilder.Append("Content-Disposition: form-data; name=\"Filename\"\r\n");
                    postDataBuilder.Append("\r\n\r\n");
                    postDataBuilder.Append(Path.GetFileName(filePath));
                    postDataBuilder.Append("\r\n");

                    //path
                    postDataBuilder.Append(boundary);
                    postDataBuilder.Append("\r\n");
                    postDataBuilder.Append("Content-Disposition: form-data; name=\"path\"\r\n");
                    postDataBuilder.Append("\r\n\r\n");
                    postDataBuilder.Append(appPath);
                    postDataBuilder.Append("\r\n");

                    //filedata
                    postDataBuilder.Append(boundary);
                    postDataBuilder.Append("\r\n");
                    postDataBuilder.Append(
                        string.Format("Content-Disposition: form-data; name=\"Filedata\"; filename=\"{0}\"\r\n",
                                      Path.GetFileName(filePath)));
                    postDataBuilder.Append("Content-Type: application/octet-stream");
                    postDataBuilder.Append("\r\n\r\n");
                    byte[] b = null;
                    using (var fs = new FileStream(filePath, FileMode.Open))
                    {
                        using (var ms = new MemoryStream())
                        {
                            int count = 0;
                            do
                            {
                                var buf = new byte[1024];
                                count = fs.Read(buf, 0, 1024);
                                ms.Write(buf, 0, count);
                            } while (fs.CanRead && count > 0);
                            b = ms.ToArray();
                        }
                    }
                    postDataBuilder.Append(Encoding.UTF8.GetString(b));
                    postDataBuilder.Append("\r\n");

                    //upload
                    postDataBuilder.Append(boundary);
                    postDataBuilder.Append("\r\n");
                    postDataBuilder.Append("Content-Disposition: form-data; name=\"Upload\"\r\n");
                    postDataBuilder.Append("\r\n");
                    postDataBuilder.Append("Submit Query\r\n");

                    //--endline
                    postDataBuilder.Append(boundary);
                    postDataBuilder.Append("--\r\n");
                }
                return postDataBuilder.ToString();
            }
            return null;
        }
        #endregion

        public static string ConstructFileUploadPostData(string fileName, byte[] fileData, string appPath,
            string boundary)
        {
            boundary = "--" + boundary;
            if (fileData == null || fileData.Length == 0)
            {
                throw new InvalidOperationException("File Data is null.");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("File name is null or empty.");
            }
            //组装上传文件post数据
            var postDataBuilder = new StringBuilder();
            //FileName
            postDataBuilder.Append(boundary);
            postDataBuilder.Append("\r\n");
            postDataBuilder.Append("Content-Disposition: form-data; name=\"Filename\"\r\n");
            postDataBuilder.Append("\r\n\r\n");
            postDataBuilder.Append(fileName);
            postDataBuilder.Append("\r\n");

            //path
            postDataBuilder.Append(boundary);
            postDataBuilder.Append("\r\n");
            postDataBuilder.Append("Content-Disposition: form-data; name=\"path\"\r\n");
            postDataBuilder.Append("\r\n\r\n");
            postDataBuilder.Append(appPath);
            postDataBuilder.Append("\r\n");

            //filedata
            postDataBuilder.Append(boundary);
            postDataBuilder.Append("\r\n");
            postDataBuilder.Append(
                string.Format("Content-Disposition: form-data; name=\"Filedata\"; filename=\"{0}\"\r\n",
                              fileName));
            postDataBuilder.Append("Content-Type: application/octet-stream");
            postDataBuilder.Append("\r\n\r\n");
            postDataBuilder.Append(Encoding.UTF8.GetString(fileData));
            postDataBuilder.Append("\r\n");

            //upload
            postDataBuilder.Append(boundary);
            postDataBuilder.Append("\r\n");
            postDataBuilder.Append("Content-Disposition: form-data; name=\"Upload\"\r\n");
            postDataBuilder.Append("\r\n");
            postDataBuilder.Append("Submit Query\r\n");

            //--endline
            postDataBuilder.Append(boundary);
            postDataBuilder.Append("--\r\n");

            return postDataBuilder.ToString();

        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受
        }
    }
}