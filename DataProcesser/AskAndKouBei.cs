using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Data;

using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.news;
using BitAuto.CarDataUpdate.DataProcesser.cn.com.baa.api;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.dealer;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.imgsvr;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class AskAndKouBei
    {
        private string _KouBeiUrl = "http://koubei.bitauto.com/inc/debris/koubei/3.0/index/min_treedata_v2.xml";
		private string _AskUrl = "http://ask.bitauto.com/inc/debris/index/treedata_v2.xml";
        // private string _ImageUrl = "http://imgsvr.bitauto.com/data/autostorageall.xml";
		// modified by chengl 接口变更
		private string _ImageUrl = "http://imgsvr.bitauto.com/data/autostorage.xml";

        public event LogHandler Log;
        private string _NewsNumberPath = "NewsNumber.xml";
        /// <summary>
        /// 保存口碑和答疑
        /// </summary>
        public void SaveAskAndKouBei()
        {
            #region del by lisf 2016-01-06
            //SaveAsk();
            //SaveKouBei();
            //SaveImage();
            #endregion

            SaveKouBeiReport(0);
        }
        /// <summary>
        /// 子品牌口碑报告
        /// </summary>
        public void SaveKouBeiReport(int csId)
        {
            OnLog(string.Format("start SaveKouBeiReport. msg:[csid:{0}]", csId), true);

            List<int> csIds = null;
            if (csId > 0)
            {
                if (CommonData.SerialKoubeiReport.Contains(csId))
                {
                    csIds = new List<int>() { csId };
                }
            }
            else
            {
                csIds = new List<int>(CommonData.SerialKoubeiReport);
            }

            if (csIds == null || csIds.Count < 1)
                OnLog("该子品牌没有口碑报告！", true);
            else
            {
                string csIdStr;
                string urlFormat = CommonData.CommonSettings.SerialKouBeiReportUrl;
                string fileFormat = Path.Combine(CommonData.CommonSettings.SavePath, "KouBei\\Report\\{0}.xml");
                foreach (int tempId in csIds)
                {
                    csIdStr = tempId.ToString();
                    OnLog(string.Format("start getreport msg:[csid:{0}]", csIdStr), true);
                    CommonFunction.SaveXMLDocument(string.Format(urlFormat, csIdStr)
                        , string.Format(fileFormat, csIdStr));
                    OnLog(string.Format("end getreport msg:[csid:{0}]", csIdStr), true);
                }
            }
            OnLog(string.Format("end SaveKouBeiReport. msg:[csid:{0}]", csId), true);
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
        /// <summary>
        /// 添加新闻评论数
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void AppendNewsCommentNum(List<XmlElement> newsList)
        {
            Dictionary<int, int> numDic = new Dictionary<int, int>();		//评论数字典
            List<int> newsIdList = new List<int>();
            int counter = 0;

            try
            {
                //获取评论数
                foreach (XmlElement newsNode in newsList)
                {
                    counter++;
                    int newsId = Convert.ToInt32(newsNode.SelectSingleNode("newsid").InnerText);
                    newsIdList.Add(newsId);
                    if (newsIdList.Count > 9 || counter == newsList.Count)
                    {
                        Dictionary<int, int> tDic = GetNewsCommentNum(newsIdList.ToArray());
                        foreach (int nId in tDic.Keys)
                            numDic[nId] = tDic[nId];
                        newsIdList.Clear();
                    }
                }

                //加入新闻信息
                foreach (XmlElement newsNode in newsList)
                {
                    int newsId = Convert.ToInt32(newsNode.SelectSingleNode("newsid").InnerText);
                    if (numDic.ContainsKey(newsId))
                    {
                        XmlElement commentNumNode = newsNode.OwnerDocument.CreateElement("CommentNum");
                        newsNode.AppendChild(commentNumNode);
                        commentNumNode.InnerText = numDic[newsId].ToString();
                    }
                }
            }
            catch (System.Exception ex)
            {
                OnLog("AppendNewsCommentNum issued!", true);
                OnLog(ex.ToString(), true);
            }
        }
        /// <summary>
        /// 获取新闻评论数
        /// </summary>
        /// <param name="newsIdList"></param>
        /// <returns></returns>
        public Dictionary<int, int> GetNewsCommentNum(int[] newsIdList)
        {
            Dictionary<int, int> commentNumDic = new Dictionary<int, int>();
            NewsService ns = new NewsService();
            ns.Timeout = 2000;
            try
            {
                DataTable refs = ns.SortNewsByComments(newsIdList);
                foreach (DataRow row in refs.Rows)
                {
                    int newsID = Convert.ToInt32(row["ID"]);
                    int cCount = Convert.ToInt32(row["CommentCount"]);
                    commentNumDic[newsID] = cCount;
                }
            }
            catch (System.Exception ex)
            {
                OnLog("Get news comment num error!!! msg:" + ex.Message, true);
            }
            return commentNumDic;
        }
		///// <summary>
		///// 根据子品牌ID获取论坛话题
		///// </summary>
		///// <param name="serialId"></param>
		///// <returns></returns>
		//public DataTable GetForumSubjectBySerial(int serialId)
		//{
		//	ForumService ser = new ForumService();
		//	string forumUrl = "";
		//	// 第一个参数是固定的，第二个参数是新品牌ID,第三个参数是获取数据的数量，第五个参数是返回的品牌易车会域名。
		//	DataTable dt = ser.GetLatestTopicListByBrand("groupproduct", serialId, 5, 0, ref forumUrl);

		//	dt.Columns.Add("serialid");
		//	if (dt.Rows.Count > 0)
		//	{
		//		foreach (DataRow dr in dt.Rows)
		//		{
		//			dr["serialid"] = serialId;
		//		}
		//	}
		//	return dt;
		//}
    }
}
