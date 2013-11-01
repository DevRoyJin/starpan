using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Runtime.Serialization;


using Newtonsoft.Json;
using System.Net.Sockets;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace StarPan
{
    public class FilesList
    {

        public Files[] list { get; set; }
        public ulong request_id { get; set; }

    }

    public class Files
    {
        public ulong fsid { get; set; }
        public string path { get; set; }
        public long ctime { get; set; }
        public long mtime { get; set; }
        public string md5 { get; set; }
        public ulong size { get; set; }
        public int isdir { get; set; }
        //public string[] block_list { get; set; }
        public uint ifhassubdir { get; set; }
        public int filenum { get; set; }


    }

    class BaiduPan : IDiskUtility
    {

        public static string access_token = "3.610e76fb259ee1c0cee211b548ff1a40.2592000.1385016898.1477197110-1567359";
        public static string appPath = "/apps/sjtupan/";

        #region 上传文件缓冲
        static byte[] readFileBuff = null;
        static String readFilePath = null;
        #endregion

        public long[] getQuata()
        {
            Uri uri = new Uri(string.Format(@"https://pcs.baidu.com/rest/2.0/pcs/quota?method=info&access_token={0}", access_token));

            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd();
                //StringReader str = new StringReader(result);
                Console.WriteLine("Read from PCS completed, result is " + result);
                string[] quata = result.Split(new Char[] { ':', ',' });
                Console.WriteLine("Quata is " + long.Parse(quata[1]));
                Console.WriteLine("Freespace is " + long.Parse(quata[3]));
                //Console.ReadLine();
                long[] resultnumber = { long.Parse(quata[1]), long.Parse(quata[3]) };
                reader.Dispose();
                return resultnumber;

            }

        }

        public FilesList getFilesList(string path)
        {
            FilesList deserializedList;
            //XDocument doc;
            Uri uri = new Uri(string.Format(@"https://pcs.baidu.com/rest/2.0/pcs/file?method=list&access_token={0}&path={1}", access_token, appPath+path));

            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd();
                result = result.Replace("\\", "");
                reader.Dispose();
                reader.Close();
                //Console.WriteLine(result);
                //FilesList obj = fastJSON.JSON.Instance.ToObject(result);



                //XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(json);
                //Console.WriteLine(doc);

                deserializedList = JsonConvert.DeserializeObject<FilesList>(result);
                //Console.WriteLine(deserializedList);

                //Console.ReadLine();
                //StringReader str = new StringReader(result);
                //doc = XDocument.Load(str);

            }
            return deserializedList;

        }

        public JObject getSingleFileInfo(string path)
        {
            try
            {
                Console.WriteLine("getSingleFileInfo: " + appPath + path);
                JObject deserializedList;
                Uri uri = new Uri(string.Format(@"https://pcs.baidu.com/rest/2.0/pcs/file?method=meta&access_token={0}&path={1}", access_token, appPath + path));
                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string result = reader.ReadToEnd();
                    result = result.Replace("\\", "");
                    //if (result.Contains("31066")) return null;

                    deserializedList = (JObject)JsonConvert.DeserializeObject(result);
                    return deserializedList;


                    reader.Dispose();
                    reader.Close();
                }
            }

            catch (Exception e)
            {
                return null;
            }
            return null;
        }

        public FilesList searchFile(string path)
        {
            Console.WriteLine("searchFile: " + path);
            FilesList deserializedList;
            String name = path.Substring(path.LastIndexOf('/') + 1);
            if (name == "")
            {
                name = path.Substring(0, path.Length - 1);
                name = name.Substring(name.LastIndexOf('/') + 1);
            }
            Uri uri = new Uri(string.Format(@"https://pcs.baidu.com/rest/2.0/pcs/file?method=search&access_token={0}&path={1}&wd={2}&re=1 ", access_token, path.Substring(0, path.Length - name.Length), name));
            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd();
                result = result.Replace("\\", "");
                if (result.Contains("[]")) return null;
                deserializedList = JsonConvert.DeserializeObject<FilesList>(result);
                reader.Dispose();
                reader.Close();
            }
            return deserializedList;
        }
        
        public string ConstructFileUploadPostData(byte[] buffer, string filename, string appPath, string boundary)
        {
            StringBuilder postDataBuilder = new StringBuilder();
            boundary = "--" + boundary;
            //构造上传文件post数据

            //FileName
            postDataBuilder.Append(boundary);
            postDataBuilder.Append("\r\n");
            postDataBuilder.Append("Content-Disposition: form-data; name=\"Filename\"\r\n");
            postDataBuilder.Append("\r\n\r\n");
            postDataBuilder.Append(filename);
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
                              filename));
            postDataBuilder.Append("Content-Type: application/octet-stream");
            postDataBuilder.Append("\r\n\r\n");
            //byte[] b = null;
            //using (var fs = new FileStream(filePath, FileMode.Open))
            //{
            //    using (var ms = new MemoryStream())
            //    {
            //        int count = 0;
            //        do
            //        {
            //            var buf = new byte[1024];
            //            count = fs.Read(buf, 0, 1024);
            //            ms.Write(buf, 0, count);
            //        } while (fs.CanRead && count > 0);
            //        b = ms.ToArray();
            //    }
            //}

            postDataBuilder.Append(Encoding.UTF8.GetString(buffer));
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

        public string GetFileName(string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
        }

        public string GetPathPart(string sourcePath)
        {
            return sourcePath.Substring(0, sourcePath.LastIndexOf('/')) + "/";
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

        public bool DeleteFile(string path)
        {
            try
            {

                Console.WriteLine("Delete file: " + appPath + path);

                Uri uri = new Uri(string.Format(@"https://pcs.baidu.com/rest/2.0/pcs/file?method=delete&access_token={0}", access_token));
                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                byte[] data = Encoding.UTF8.GetBytes("path=" + appPath + path);
                request.ContentType = @"application/x-www-form-urlencoded; charset=UTF-8";
                request.ContentLength = data.Length;
                request.Method = "POST";
                request.Accept = @"*/*";


                request.UserAgent = @"User-Agent: Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36";
                request.ProtocolVersion = HttpVersion.Version10;

                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(data, 0, data.Length);
                    postStream.Close();
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {

                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string result = reader.ReadToEnd();
                    // Console application output  
                    Console.WriteLine(result);
                    if (result.Contains("error_msg")) return false;

                }

                return true;
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;

        }

        public bool CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public bool DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }
        public bool UploadFile(string path, byte[] buffer)
        {
            try
            {
                //WebClient client = new WebClient(); 
                //byte[] responseBinary = client.UploadFile(url, file); 
                //string result = Encoding.UTF8.GetString(responseBinary);
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                Console.WriteLine("uploadFile: " + path);
                string filename = StarPanOperations.GetFileName(path);
                Uri uri = new Uri(string.Format(@"https://pcs.baidu.com/rest/2.0/pcs/file?method=upload&access_token={0}&path={1}&ondup=overwrite", access_token, appPath + path));
                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                String postData = System.Text.Encoding.UTF8.GetString(buffer);
                postData = ConstructFileUploadPostData(buffer, filename, appPath, boundary);
                byte[] data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "multipart/form-data; boundary=" + boundary + "\r\n";
                request.Accept = @"*/*";
                request.ContentLength = data.Length;
                request.ProtocolVersion = HttpVersion.Version10;




                // Write data  


                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(data, 0, data.Length);
                    postStream.Close();
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {

                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Console application output  
                    Console.WriteLine(reader.ReadToEnd());


                }

            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public bool DownloadFile(string path, out byte[] filebuffer)
        {
            try
            {

                if (path == readFilePath && readFileBuff != null) { Console.WriteLine("BaiduPan: Same file, return last time result"); filebuffer =readFileBuff; }
                else
                {
                    readFilePath = path;
                    Console.WriteLine("BaiduPan: another file, start read");
                }

                Uri uri = new Uri(string.Format(@"https://pcs.baidu.com/rest/2.0/pcs/file?method=download&access_token={0}&path={1}", access_token, appPath + path));
                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {

                    using (Stream reader = response.GetResponseStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            var offset = 0;
                            var count = 0;
                            byte[] buff = new byte[1024];
                            do
                            {
                                count = reader.Read(buff, offset, buff.Length);
                                ms.Write(buff, offset, count);

                            } while (reader.CanRead && count > 0);
                            filebuffer= ms.ToArray();
                        }
                    }

                    //Stream responseStream = response.GetResponseStream();
                    //readFileBuff = new byte[response.ContentLength];
                    //responseStream.Read(readFileBuff, 0, (int)response.ContentLength);
                    //responseStream.Close();
                    //responseStream.Dispose();

                }
                return true;
            }
            catch (WebException e)
            {
                filebuffer = null;
                return false;
            }
            //return readFileBuff;
        }

        #endregion

       
    }


   }


