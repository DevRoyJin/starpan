using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KuaiPanSDK;
using System.Net;
using System.IO;

namespace StarPan
{
    class KuaiPan : IDiskUtility
    {
        public KuaiPanSDK.KuaiPan sdk;
        public byte[] readFileBuff = null;
        public string readFilePath = null;

        public KuaiPan()
        {
            sdk = new KuaiPanSDK.KuaiPan("xcRYiOnHwra5Lb5o", "l5sS04iA862dm00u", "0059a7586c815f96a1f8aedd", "b5ffa76e063b4ff58e41e7843feab2ed");

        }

        public string GetFileName(string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
        }

        public string GetPathPart(string sourcePath)
        {
            return sourcePath.Substring(0, sourcePath.LastIndexOf('/')) + "/";
        }

        public static string Search_string(string s, string s1, string s2)  //截取2个字符串之间的字符串
        {
            int n1, n2;
            n1 = s.IndexOf(s1, 0) + s1.Length;   //开始位置
            n2 = s.IndexOf(s2, n1);               //结束位置
            return s.Substring(n1, n2 - n1);   //取搜索的条数，用结束的位置-开始的位置,并返回
        }

        public KuaiPanSDK.Model.MetaData getFilesList(string path)
        {

            return sdk.GetMetaData(path, null);

        }


        #region IDiskUtility Members

        public long GetQuota()
        {
            throw new NotImplementedException();
        }

        public long GetUsedSpace()
        {
            throw new NotImplementedException();
        }

        public long GetFreeSpace()
        {
            throw new NotImplementedException();
        }


        public bool DownloadFile(string path, out byte[] filebuffer)
        {
            try
            {
                if (path == readFilePath && readFileBuff != null) { Console.WriteLine("KuaiPan: Same file, return last time result"); filebuffer= readFileBuff; }
                else
                {
                    readFilePath = path;
                    Console.WriteLine("KuaiPan: another file, start read");
                }
                string downloadPath = sdk.Download(path);

                //WebClient wc = new WebClient();
                //string myCookie = @"token=0059a75882e3435a92343bd59c42802c-342602031; logindate=2013-10-30; ksc_suv=2025338563.1383101596179; relogintoken=0059a758633140bf84e328f16e342846-41119f36bbd44177872ead5a199bf55a-5875544; _et=0338322ea3d0101d517f62fc9fe13568910cac8668c473c16094f3ad61484ccbd28579e49c44241265f9; _ga=GA1.2.194446616.1376518040; _fs=b9e71c18; kphelper-inst=1; __cdl_snk=daa50229ca76ec07998e52a105b6ca7a";
                //wc.Headers.Add(HttpRequestHeader.Accept, @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                //wc.Headers.Add(HttpRequestHeader.UserAgent, @"Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36");
                //wc.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip,deflate,sdch");
                //wc.Headers.Add(HttpRequestHeader.AcceptLanguage, @"en-US,en;q=0.8");
                //wc.Headers.Add(HttpRequestHeader.Cookie, myCookie);

                Uri uri = new Uri(downloadPath);

                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                CookieContainer cc = new CookieContainer();
                request.CookieContainer = cc;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36";
                request.AllowAutoRedirect = true;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (Stream reader = response.GetResponseStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            var offset = 0;
                            var count = 0;
                            byte[] buff = new byte[32 * 1024];
                            do
                            {
                                count = reader.Read(buff, offset, buff.Length);
                                ms.Write(buff, offset, count);

                            } while (reader.CanRead && count > 0);
                            readFileBuff = ms.ToArray();
                        }
                    }

                    filebuffer = readFileBuff;
                    return true;
                    //readFileBuff = new Byte[response.ContentLength];

                    //reader.Read(readFileBuff, 0, (int)response.ContentLength);
                    //reader.Close();
                    //reader.Dispose();
                    //return readFileBuff;

                }



            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                filebuffer = null;
                return false;
            }


        }

        public bool UploadFile(string path, byte[] buffer)
        {
            var result = sdk.UpLoadFile(path, true, GetFileName(path), buffer);
            if (result != null)
            {

                return true;
            }
            return false; ;
        }

        public bool DeleteFile(string path)
        {
            return sdk.Delete(path) == null ? false : true;
        }


        public bool CreateDirectory(string path)
        {
            if (sdk.Create(path) != null) return true;
            return false;
        }

        public bool DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }


        public bool MoveFile(string fromPath, string toPath)
        {
            if(sdk.Move(fromPath, toPath)!=null)return true;
            return false;
        }

        #endregion





    }
}
