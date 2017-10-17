using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.WebServiceDAL;
using BitAuto.CarDataUpdate.WebServiceModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
    public class NewsBLL
    {
        private NewsDAL newsDal;

        private readonly string _oldPubMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MessageBody><From>CMS</From><ContentType>news</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime><DeleteOp newsCategoryId=\"\" brandIds=\"\" bigBrandIds=\"\">{2}</DeleteOp></MessageBody>";

        private readonly string _oldDelMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MessageBody><From>CMS</From><ContentType>news</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime><DeleteOp>{2}</DeleteOp></MessageBody>";
        private readonly string _queueName = CommonData.CommonSettings.QueueName;
        public NewsBLL()
        {
            newsDal = new NewsDAL();
        }

        public void Update(XElement bodyElement)
        {
            try
            {
                CommonFunction.InsertMessageDbLog(bodyElement, "News", "CMS", false);
                Guid guid = Guid.Empty;
                string entityId = bodyElement.Element("EntityId").Value;
                string updateTime = bodyElement.Element("PublishTime").Value;
                if (!string.IsNullOrEmpty(entityId) && Guid.TryParse(entityId, out guid))
                {
                    ProcessGuid(entityId,updateTime);
                }
                else { Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " :更新新闻消息. cmsentityid=" + entityId); }
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":" + ex.ToString());
            }
        }

        public void Delete(XElement bodyElement)
        {
            try
            {
                CommonFunction.InsertMessageDbLog(bodyElement, "News", "CMS", false);
                Guid guid = Guid.Empty;
                string entityId = bodyElement.Element("EntityId").Value;
                if (!string.IsNullOrEmpty(entityId) && Guid.TryParse(entityId, out guid))
                {
                    //ProcessGuid(entityId, "delete");
                    newsDal.Delete(guid);
                    //发送车型频道消息队列，以下过程为将ugc消息发往消息队列处理
                    int newsId = ConvertHelper.GetInteger(bodyElement.Element("NewsId").Value);
                    string updateTime = DateTime.Now.GetDateTimeFormats('s')[0].ToString();
                    if (newsId > 0)
                    {
                        string msg = string.Format(_oldDelMessage, newsId, updateTime, "true");
                        MessageService.SendMessage(_queueName, msg);
                    }
                }
                else { Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " :更新新闻消息. cmsentityid=" + entityId); }
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":" + ex.ToString());
            }
        }

        public void ProcessGuid(string curSingleGuid,string updateTime)
        {
            string paramdata = "entityId=" + curSingleGuid;  //"entityId=DDBF8EAC-37FE-4B98-B5EF-5A7D278DD206";  
            string url = "http://api.admin.bitauto.com/news3/v1/news/show?" + paramdata;
            string singleNewObject = GetResponseFromUrl(url);
            if (!string.IsNullOrEmpty(singleNewObject))
            {
                //json转换
                NewsEntity curNewJson = JsonConvert.DeserializeObject<NewsEntity>(singleNewObject);
                //处理返回的json对象
                newsDal.Update(curNewJson);
                //发送车型频道消息队列
                if (curNewJson.NewsId > 0)
                {
                    string msg = string.Format(_oldPubMessage, curNewJson.NewsId, updateTime, "false");
                    MessageService.SendMessage(_queueName, msg);
                }
            }
            else
            {
                Log.WriteErrorLog("无法获取服务器返回新闻信息:guid=" + curSingleGuid);
            }
        }
        public string GetResponseFromUrl(string url, int interval = 60000)
        {
            string result = string.Empty;
            HttpWebRequest req = null;
            HttpWebResponse response = null;
            Stream responseStream = null;
            try
            {
                req = (HttpWebRequest) WebRequest.Create(url);
                req.Timeout = interval;
                using (response = req.GetResponse() as HttpWebResponse)
                {
                    using (responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException webException)
            {
                var responseText = string.Empty;
                var resEx = webException.Response as HttpWebResponse;
                if (resEx == null)
                {
                    Log.WriteErrorLog(string.Format("请求新闻Url:{0},webException异常信息:null", url));
                    throw webException;
                }

                try
                {
                    using (StreamReader sr = new StreamReader(resEx.GetResponseStream(), Encoding.UTF8))
                    {
                        responseText = sr.ReadToEnd();
                    }
                    resEx.Close();
                    resEx = null;
                    Log.WriteErrorLog(string.Format("status={0} {1},请求新闻Url:{2},webException异常信息:{3}",
                        webException.Status, webException.Message, url, responseText));
                }
                catch (Exception subException)
                {
                    Log.WriteErrorLog(string.Format("webException异常信息:{0}", subException.ToString()));
                    throw subException;
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(string.Format("请求新闻Url:{1},GetResponseStream异常信息:{0}", ex.ToString(), url));
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
            return result;
        }

        //public static string GetContentByUrl(string url)
        //{
        //    string returnString = string.Empty;
        //    if (string.IsNullOrEmpty(url))
        //    {
        //        return returnString;
        //    }
        //    try
        //    {
        //        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        //        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //        using (Stream receiveStream = httpWebResponse.GetResponseStream())
        //        {
        //            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
        //            using (StreamReader readStream = new StreamReader(receiveStream, encode))
        //            {
        //                StringBuilder tempStr = new StringBuilder();
        //                while (!readStream.EndOfStream)
        //                {
        //                    tempStr.Append(readStream.ReadLine());
        //                }
        //                returnString = tempStr.ToString();
        //            }
        //        }
        //        httpWebResponse.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        returnString = "";
        //        Log.WriteErrorLog(string.Format("请求新闻Url:{0},异常信息:{1}", url,ex.Message));
        //    }
        //    return returnString;
        //}
        #region 老消息队列 add by sk 2016-08-29

        //public void UpdateQueue(XElement bodyElement)
        //{

        //    int newsId = ConvertHelper.GetInteger(bodyElement.Element("NewsId").Value);
        //    string updateTime = bodyElement.Element("PublishTime").Value;
        //    if (newsId > 0)
        //    {
        //        string msg = string.Format(_oldPubMessage, newsId, updateTime, "false");
        //        MessageService.SendMessage(_queueName, msg);
        //    }
        //    else
        //    {
        //        Log.WriteErrorLog("老新闻消息异常 NewsId<=0," + bodyElement.ToString());
        //    }
        //}
        //public void DeleteQueue(XElement bodyElement)
        //{
        //    int newsId = ConvertHelper.GetInteger(bodyElement.Element("NewsId").Value);
        //    string updateTime = bodyElement.Element("PublishTime").Value;
        //    if (newsId > 0)
        //    {
        //        string msg = string.Format(_oldDelMessage, newsId, updateTime, "true");
        //        MessageService.SendMessage(_queueName, msg);
        //    }
        //    else
        //    {
        //        Log.WriteErrorLog("老新闻消息异常 NewsId<=0," + bodyElement.ToString());
        //    }
        //}

        #endregion
    }
}
