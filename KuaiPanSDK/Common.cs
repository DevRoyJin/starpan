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
using System.Text;
using Newtonsoft.Json.Linq;
using PHPLibrary;

namespace KuaiPanSDK
{
    public static class Common
    {
        /// <summary>
        /// 计算文件大小函数(保留两位小数)
        /// </summary>
        /// <param name="size">字节大小</param>
        /// <returns></returns>
        public static string CountSize(long size)
        {
            string m_strSize = "";
            if (size < 1024.00)
            {
                m_strSize = size.ToString("F2") + " Byte";
            }
            else if (size >= 1024.00 && size < 1048576)
            {
                m_strSize = (size / 1024.00).ToString("F2") + " KB";
            }
            else if (size >= 1048576 && size < 1073741824)
            {
                m_strSize = (size / 1024.00 / 1024.00).ToString("F2") + " MB";
            }
            else if (size >= 1073741824)
            {
                m_strSize = (size / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " GB";
            }
            return m_strSize;
        }

        public static PHPArray JsonToPHPArray(string result)
        {
            try
            {
                PHPArray array = new PHPArray();
                JObject jsons = JObject.Parse(result);
                IEnumerable<JProperty> properties = jsons.Properties();
                foreach (var item in jsons)
                {
                    if (item.Value.ToString().StartsWith("["))
                    {
                        PHPArray temp = new PHPArray();
                        JArray jArrays = jsons.Value<JArray>(item.Key);
                        for (int j = 0; j < jArrays.Count; j++)
                        {
                            temp.Add(Common.JsonToPHPArray(jArrays[j].ToString()));
                        }
                        array.Add(item.Key, temp);
                    }
                    else
                    {
                        array.Add(item.Key, item.Value);
                    }
                }
                return array;
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="strPath">文件路径</param>
        /// <param name="msg">内容</param>
        public static void DeBug(string strPath, object msg)
        {
            System.IO.StreamWriter sw = null;
            System.IO.FileStream fs = new System.IO.FileStream(strPath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read);
            fs.Seek(0, System.IO.SeekOrigin.End);
            sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            string LineText = DateTime.Now.ToString() + ", " + msg.ToString();
            sw.WriteLine(LineText);
            sw.Close();
            sw = null;
            fs.Close();
            fs = null;
        }

        /// <summary>
        /// 对字符串进行编码
        /// </summary>
        /// <param name="value">要编码的字符串</param>
        /// <returns>编码后的字符串</returns>
        public static string UrlEncode(string value)
        {
            return UrlEncode(value, string.Empty);
        }

        public static string UrlEncode(string str, string other)
        {
            StringBuilder sb = new StringBuilder();
            string strSpecial = string.Format("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~{0}", other);
            for (int i = 0; i < str.Length; i++)
            {
                string crt = str.Substring(i, 1);
                if (strSpecial.Contains(crt))
                {
                    sb.Append(crt);
                }
                else
                {
                    byte[] bts = Encoding.UTF8.GetBytes(crt);
                    foreach (byte bt in bts)
                    {
                        sb.Append("%" + bt.ToString("X"));
                    }
                }
            }
            return sb.ToString();
        }
    }
}
