using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using System.Data;
using BitAuto.Utils.Data;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Model;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 生成子品牌车型详解数据
    /// data\SerialNews\pingce\allpingce.xml
    /// </summary>
    public class SerialPingCeData
    {
        public event LogHandler Log;
        /// <summary>
        /// 根据彩虹条，生成子品牌车型详解数据
        /// </summary>
//        public void GetSerialPingCeData(int serialId)
//        {
//            OnLog(string.Format("start GetSerialPingCeData. msg:[csid:{0}]", serialId), true);

//            string sql = string.Empty;
//            if (serialId > 0)
//            {
//                sql = string.Format("select top 1 [csid], [url], [tagid] from CarPingceInfo where csid={0} order by tagid asc", serialId);
//            }
//            else
//            {
//                sql = @"SELECT cs.[csid],cs.[url],cs.[tagid] 
//FROM [CarPingceInfo] as cs
//inner join (select csid,MIN(tagid) tagid from [CarPingceInfo] group by csid) as tag
// on cs.csid=tag.csid and cs.tagid=tag.tagid";
//            }

//            bool isReSave = false;
//            string path = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\pingce\allpingce.xml");

//            //Dictionary<int, PingCeTag> dicPingce = CommonFunction.IntiPingCeTagInfo();

//            XmlDocument doc = GetXml(path);

//            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql);

//            if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
//            {
//                OnLog("该子品牌设置评测数据！", true);
//                if (serialId > 0)
//                {
//                    isReSave = DeleteSerialData(doc, serialId);
//                }
//            }
//            else
//            {
//                foreach (DataRow row in ds.Tables[0].Rows)
//                {
//                    int csId = ConvertHelper.GetInteger(row["csid"].ToString());
//                    int tagId = ConvertHelper.GetInteger(row["tagid"].ToString());
//                    string url = row["url"].ToString().Trim();
//                    if (csId < 1 || tagId < 1 || string.IsNullOrEmpty(url))
//                    {
//                        if (csId > 0)
//                        {
//                            isReSave = DeleteSerialData(doc, csId, isReSave);
//                        }
//                        continue;
//                    }

//                    OnLog(string.Format("start csid:{0},tagid:{1},url:{2}", csId, tagId, url), true);

//                    //if (!dicPingce.ContainsKey(tagId))
//                    //{
//                    //    OnLog("tagid 不存在.", true);
//                    //    continue;
//                    //}
//                    //PingCeTag pingceTag = dicPingce[tagId];

//                    int newsId = CommonFunction.GetPingCeNewsId(url);
//                    if (newsId < 1)
//                    {
//                        OnLog("newsid 小于零.", true);
//                        isReSave = DeleteSerialData(doc, csId, isReSave);
//                        continue;
//                    }
//                    OnLog(string.Format("start 获取到评测数据. newsid:{0}", newsId), true);
//                    DataSet pingceDs = CommonFunction.GetPingCeNewsDataSet(newsId);
//                    if (pingceDs == null || pingceDs.Tables.Count < 1 || pingceDs.Tables[0].Rows.Count < 1)
//                    {
//                        OnLog(string.Format("未获取到评测数据. newsid:{0}", newsId), true);
//                        isReSave = DeleteSerialData(doc, csId, isReSave);
//                        continue;
//                    }

//                    string pingCeTitle = string.Empty;
//                    if (pingceDs.Tables[0].Columns.Contains("title"))
//                        pingCeTitle = pingceDs.Tables[0].Rows[0]["title"].ToString();
//                    else
//                        pingCeTitle = pingceDs.Tables[0].Rows[0]["facetitle"].ToString();

//                    string pingCeFaceTitle = pingceDs.Tables[0].Rows[0]["facetitle"].ToString();

//                    XmlElement newsEle = doc.SelectSingleNode(string.Format("PingCeList/News[@CsId='{0}']", csId)) as XmlElement;
//                    if (newsEle == null)
//                    {
//                        newsEle = doc.CreateElement("News");
//                        doc.DocumentElement.AppendChild(newsEle);
//                    }

//                    newsEle.SetAttribute("CsId", csId.ToString());
//                    newsEle.SetAttribute("NewsId", newsId.ToString());
//                    newsEle.SetAttribute("TagId", tagId.ToString());
//                    newsEle.SetAttribute("Title", pingCeTitle);
//                    newsEle.SetAttribute("FaceTitle", pingCeFaceTitle);
//                    newsEle.SetAttribute("Url", url);

//                    OnLog(string.Format("end csid:{0},tagid:{1},url:{2}", csId, tagId, url), true);

//                    if (!isReSave)
//                        isReSave = true;
//                }
//            }
//            if (isReSave)
//            {
//                OnLog(string.Format("更新xml文件.msg:[path:{0}]", path), true);
//                CommonFunction.SaveXMLDocument(doc, path);
//            }
//            else
//            {
//                OnLog("文件未修改.", true);
//            }
//            OnLog(string.Format("end GetSerialPingCeData. msg:[csid:{0}]", serialId), true);
//        }
        /// <summary>
        /// date:2017-1-10  
        /// desc:在上面的基础方法上进行修改，更改评测新闻数据的获取方式，从周新锋提供的新接口出，接口地址对应配置文件NewsUrl字段
        /// author:zf
        /// </summary>
        /// <param name="serialId"></param>
        public void GetSerialPingCeData(int serialId)
        {
            OnLog(string.Format("start GetSerialPingCeData. msg:[csid:{0}]", serialId), true);

            string sql = string.Empty;
            if (serialId > 0)
            {
                sql = string.Format("select top 1 [csid], [url], [tagid] from CarPingceInfo where csid={0} order by tagid asc", serialId);
            }
            else
            {
                sql = @"SELECT cs.[csid],cs.[url],cs.[tagid] 
FROM [CarPingceInfo] as cs
inner join (select csid,MIN(tagid) tagid from [CarPingceInfo] group by csid) as tag
 on cs.csid=tag.csid and cs.tagid=tag.tagid";
            }

            bool isReSave = false;
            string path = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\pingce\allpingce.xml");

            //Dictionary<int, PingCeTag> dicPingce = CommonFunction.IntiPingCeTagInfo();

            XmlDocument doc = GetXml(path);

            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql);

            if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
            {
                OnLog("该子品牌设置评测数据！", true);
                if (serialId > 0)
                {
                    isReSave = DeleteSerialData(doc, serialId);
                }
            }
            else
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int csId = ConvertHelper.GetInteger(row["csid"].ToString());
                    int tagId = ConvertHelper.GetInteger(row["tagid"].ToString());
                    string url = row["url"].ToString().Trim();
                    if (csId < 1 || tagId < 1 || string.IsNullOrEmpty(url))
                    {
                        if (csId > 0)
                        {
                            isReSave = DeleteSerialData(doc, csId, isReSave);
                        }
                        continue;
                    }

                    OnLog(string.Format("start csid:{0},tagid:{1},url:{2}", csId, tagId, url), true);

                    int newsId = CommonFunction.GetPingCeNewsId(url);
                    if (newsId < 1)
                    {
                        OnLog("newsid 小于零.", true);
                        isReSave = DeleteSerialData(doc, csId, isReSave);
                        continue;
                    }
                    OnLog(string.Format("start 获取到评测数据. newsid:{0}", newsId), true);

                    NewsEntityV2 pingceNews = CommonFunction.GetNewsEntityFromApi(newsId);
                    if (pingceNews == null)
                    {
                        OnLog(string.Format("未获取到评测数据. newsid:{0}", newsId), true);
                        isReSave = DeleteSerialData(doc, csId, isReSave);
                        continue;
                    }
                    string pingCeTitle = string.IsNullOrEmpty(pingceNews.Title)? pingceNews.ShortTitle: pingceNews.Title;
                    string pingCeFaceTitle = pingceNews.ShortTitle;

                    XmlElement newsEle = doc.SelectSingleNode(string.Format("PingCeList/News[@CsId='{0}']", csId)) as XmlElement;
                    if (newsEle == null)
                    {
                        newsEle = doc.CreateElement("News");
                        doc.DocumentElement.AppendChild(newsEle);
                    }

                    newsEle.SetAttribute("CsId", csId.ToString());
                    newsEle.SetAttribute("NewsId", newsId.ToString());
                    newsEle.SetAttribute("TagId", tagId.ToString());
                    newsEle.SetAttribute("Title", pingCeTitle);
                    newsEle.SetAttribute("FaceTitle", pingCeFaceTitle);
                    newsEle.SetAttribute("Url", url);

                    OnLog(string.Format("end csid:{0},tagid:{1},url:{2}", csId, tagId, url), true);

                    if (!isReSave)
                        isReSave = true;
                }
            }
            if (isReSave)
            {
                OnLog(string.Format("更新xml文件.msg:[path:{0}]", path), true);
                CommonFunction.SaveXMLDocument(doc, path);
            }
            else
            {
                OnLog("文件未修改.", true);
            }
            OnLog(string.Format("end GetSerialPingCeData. msg:[csid:{0}]", serialId), true);
        }
        private bool DeleteSerialData(XmlDocument doc, int csId, bool sourceIsReSave)
        {
            if (sourceIsReSave)
                DeleteSerialData(doc, csId);
            else
                return DeleteSerialData(doc, csId);
            return sourceIsReSave;
        }
        private bool DeleteSerialData(XmlDocument doc, int csId)
        {
            XmlNode node = doc.SelectSingleNode(string.Format("PingCeList/News[@CsId='{0}']", csId));
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
                OnLog(string.Format("删除子品牌信息.msg:[csi:{0}]", csId), true);
                return true;
            }
            return false;
        }
        private XmlDocument GetXml(string path)
        {
            XmlDocument doc = CommonFunction.GetLocalXmlDocument(path);
            if (doc == null)
            {
                doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
                doc.AppendChild(doc.CreateElement("PingCeList"));
            }
            return doc;
        }

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="logText"></param>
        public void OnLog(string logText, bool nextLine)
        {
            if (Log != null)
                Log(this, new LogArgs(logText, nextLine));
        }
    }
}
