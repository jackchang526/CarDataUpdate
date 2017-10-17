using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace BitAuto.CarDataUpdate.Common.Utils
{
    public static class HttpUtils
    {
        private const int BUFFER_SIZE = 1024;

        public static string DownLoadString(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("url不能为空");

            using (WebClient client = new WebClient())
            {
                string str = string.Empty;
                byte[] data = client.DownloadData(url);
                char[] buffer = new char[BUFFER_SIZE];
                using (MemoryStream stream = new MemoryStream(data))
                {
                    //StreamReader自动识别UTF-8编码的字节流，识别不了则使用gb2312编码
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("gb2312")))
                    {
                        int count;
                        while ((count = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            str += new string(buffer, 0, count);
                        }
                    }
                }
                return str;
            }
        }
    }
}
