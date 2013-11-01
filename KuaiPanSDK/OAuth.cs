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
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using PHPLibrary;

namespace KuaiPanSDK
{
    /// <summary>
    /// OAuth签名
    /// </summary>
    public class OAuth
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="sdk"></param>
        public OAuth(KuaiPan sdk)
        {
            this.SDK = sdk;
            this._nonce = OAuth.GetOAuthNonce();
            this._timeStamp = OAuth.GetTimeStamp();
        }

        /// <summary>
        /// SDK
        /// </summary>
        public KuaiPan SDK { get; private set; }

        /// <summary>
        /// 访问地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 其他参数
        /// </summary>
        public PHPArray Array { get; set; }

        private string _signature;
        private string _timeStamp;
        private string _nonce;

        /// <summary>
        /// 生成Url
        /// </summary>
        /// <returns></returns>
        public string GetUrl()
        {
            this._signature = OAuth.GetSignature(this.GetSignatureString(), string.Format("{0}&{1}", this.SDK.ConsumerSecret, string.IsNullOrEmpty(this.SDK.TokenSecret) ? "" : this.SDK.TokenSecret));
            string result = string.Concat(this.Url, this.ParameToString(false));
            return result;
        }

        /// <summary>
        /// 返回签名基字符串
        /// </summary>
        /// <returns></returns>
        private string GetSignatureString()
        {
            string ParameString = this.ParameToString(true).Substring(1);
            if (this.Url.IndexOf("upload_file") > 0)
            {
                return string.Format("POST&{0}&{1}", Common.UrlEncode(this.Url), Common.UrlEncode(ParameString));
            }
            else
            {
                return string.Format("GET&{0}&{1}", Common.UrlEncode(this.Url), Common.UrlEncode(ParameString));
            }
        }

        /// <summary>
        /// 返回参数字符串
        /// </summary>
        /// <param name="isEncode">是否urlencode</param>
        /// <returns></returns>
        private string ParameToString(bool isEncode)
        {
            List<Parameter> list = this.GetParameList();
            return OAuth.ParameToString(list, isEncode);
        }

        /// <summary>
        /// 生成所有参数
        /// </summary>
        /// <returns></returns>
        private List<Parameter> GetParameList()
        {
            List<Parameter> parame = new List<Parameter>();
            if (!string.IsNullOrEmpty(this.SDK.CallBackUrl))
            {
                parame.Add(new Parameter("oauth_callback", this.SDK.CallBackUrl));
            }
            parame.Add(new Parameter("oauth_consumer_key", this.SDK.ConsumerKey));
            parame.Add(new Parameter("oauth_nonce", this._nonce));
            parame.Add(new Parameter("oauth_signature_method", "HMAC-SHA1"));
            parame.Add(new Parameter("oauth_timestamp", this._timeStamp));
            parame.Add(new Parameter("oauth_version", "1.0"));
            if (!string.IsNullOrEmpty(this._signature))
            {
                parame.Add(new Parameter("oauth_signature", this._signature));
            }
            if (!string.IsNullOrEmpty(this.SDK.Token))
            {
                parame.Add(new Parameter("oauth_token", this.SDK.Token));
            }
            if (this.Array != null)
            {
                foreach (KeyValuePair<object, object> p in this.Array)
                {
                    parame.Add(new Parameter((string)p.Key, p.Value.ToString()));
                }
            }
            parame.Sort(delegate(Parameter x, Parameter y)
            {
                if (x.Name == y.Name)
                {
                    return string.Compare(x.Value, y.Value);
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            });
            return parame;
        }

        /// <summary>
        /// 得到签名字符串
        /// </summary>
        /// <param name="sigstr">要签名的字符串</param>
        /// <param name="key">签名密匙</param>
        /// <returns></returns>
        public static string GetSignature(string sigstr, string key)
        {
            HMACSHA1 hash = new HMACSHA1();
            hash.Key = Encoding.ASCII.GetBytes(key);
            byte[] dataBuffer = Encoding.ASCII.GetBytes(sigstr);
            byte[] hashBytes = hash.ComputeHash(dataBuffer);
            string oauth_signature = Common.UrlEncode(Convert.ToBase64String(hashBytes));
            return oauth_signature;
        }

        /// <summary>
        /// 返回参数字符串
        /// </summary>
        /// <param name="par"></param>
        /// <param name="isEncode"></param>
        /// <returns></returns>
        public static string ParameToString(List<Parameter> par, bool isEncode)
        {
            StringBuilder ParameString = new StringBuilder();
            for (int i = 0; i < par.Count; i++)
            {
                string formatString = i == 0 ? "?{0}={1}" : "&{0}={1}";
                if (isEncode)
                {
                    ParameString.AppendFormat(formatString, Common.UrlEncode(par[i].Name), Common.UrlEncode(par[i].Value));
                }
                else
                {
                    ParameString.AppendFormat(formatString, par[i].Name, par[i].Value);
                }
            }
            return ParameString.ToString();
        }

        /// <summary>
        /// 获取的当前时间戳整型值
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// 得到随机值
        /// </summary>
        /// <returns></returns>
        public static string GetOAuthNonce()
        {
            string result = System.Guid.NewGuid().ToString();
            result = result.Replace("-", "");
            return result.Substring(0, 8);
        }
    }
}
