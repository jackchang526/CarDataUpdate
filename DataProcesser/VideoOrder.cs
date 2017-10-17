using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Data;

using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class VideoOrder
    {
        #region  del by lsf 2016-01-06
        /*
        //请求链接
        private string _RequestVideoOrder = "http://api.admin.bitauto.com/api/Statistics.aspx?s=weeklog_Url,NewsDetail_faceTitle,sum(pv),NewsDetail_Title"
                                            + ",NewsDetail_RelatedBigBrand&c=NewsDetail_type=3 and NewsDetail_RelatedBigBrand <> ''&n={0}"
                                            + "&o=sum(pv)%20desc&g=Url,NewsDetail_faceTitle,NewsDetail_picture,NewsDetail_Title,NewsDetail_RelatedBigBrand&st={1}&et={2}";
        public event LogHandler Log;
        private string _MasterBrandPath = string.Empty;
        private string _BrandPath = string.Empty;
        /// <summary>
        /// 构造函数
        /// </summary>
        public VideoOrder() 
        {
            _MasterBrandPath = Path.Combine(CommonData.CommonSettings.SavePath, "MasterBrand\\Video\\Order");
            _BrandPath = Path.Combine(CommonData.CommonSettings.SavePath, "Brand\\Video\\Order");
        }
        /// <summary>
        /// 保存视频排序的文件
        /// </summary>
        public void SaveVideoOrder()
        {
            string requestUrl = string.Format(_RequestVideoOrder, "8000"
                                            , DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 0:00:00")
                                            , DateTime.Now.ToString("yyyy-MM-dd 0:00:00"));

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                OnLog("     Start save video order......", true);
                xmlDoc.Load(requestUrl);
                OnLog("     End save video order!", true);
                if (xmlDoc == null) { OnLog("       Not get video data!", true); return; }
                Dictionary<int, List<XmlNode>> videoList = GetVideoList(xmlDoc);
                if (videoList == null || videoList.Count < 1) { OnLog("     Fail to init video order!", true); return; }
                SaveMasterBrand(videoList);
                SaveBrand(videoList);

            }
            catch(Exception ex)
            {
                OnLog(ex.Message, true);
            }
        }
        /// <summary>
        /// 保存主品牌
        /// </summary>
        private void SaveMasterBrand(Dictionary<int,List<XmlNode>> videoList)
        {
            if (videoList == null || videoList.Count < 1) { OnLog("无主品牌视频列表", true); return; }
            Dictionary<int, List<int>> masterBrandList = Common.CommonFunction.GetMasterBrandDic();
            if (masterBrandList == null || masterBrandList.Count < 1) { OnLog("未得到主品牌无素列表", true); return; }

            foreach (KeyValuePair<int, List<int>> entity in masterBrandList)
            {
                OnLog("Get master brand:" + entity.Key + " video...", false);
                string filePath = Path.Combine(_MasterBrandPath, entity.Key + ".xml");
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xElem = xmlDoc.CreateElement("root");
                int counter=0;
                foreach (int brandId in entity.Value)
                {
                    if (!videoList.ContainsKey(brandId)) continue;
                    counter++;
                    foreach (XmlNode xNode in videoList[brandId])
                    {
                        xElem.AppendChild(xmlDoc.ImportNode(xNode, true));
                    }
                }
                if (counter > 0)
                {
                    xmlDoc.AppendChild(xElem);
                    CommonFunction.SaveXMLDocument(xmlDoc, filePath);
                }
            }
        }
        /// <summary>
        /// 保存品牌
        /// </summary>
        private void SaveBrand(Dictionary<int, List<XmlNode>> videoList)
        {
            if (videoList == null || videoList.Count < 1) { OnLog("无品牌视频列表", true); return; }

            foreach (KeyValuePair<int, List<XmlNode>> entity in videoList)
            {
                OnLog("Get brand:" + entity.Key + " video...", false);
                string filePath = Path.Combine(_BrandPath, entity.Key + ".xml");
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xElem = xmlDoc.CreateElement("root");

                foreach (XmlNode xNode in entity.Value)
                {
                    xElem.AppendChild(xmlDoc.ImportNode(xNode, true));
                }
                xmlDoc.AppendChild(xElem);
                CommonFunction.SaveXMLDocument(xmlDoc, filePath);
            }            
        }
        /// <summary>
        /// 得到视频列表
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private Dictionary<int, List<XmlNode>> GetVideoList(XmlDocument xmlDoc) 
        {
            if (xmlDoc == null) { OnLog("没有取到视频排序数据", true); return null; }

            XmlNodeList xNodeList = xmlDoc.SelectNodes("NewDataSet/Table/RelatedBigBrand");
            if (xNodeList == null || xNodeList.Count < 1) { OnLog("取到视频排序数据没有品牌结点", true); return null; }
            Dictionary<int, List<XmlNode>> videoList = new Dictionary<int, List<XmlNode>>();

            foreach (XmlElement xElem in xNodeList)
            {
                if (string.IsNullOrEmpty(xElem.InnerText)) continue;
                string[] brandIdArray = xElem.InnerText.Split(',');
                foreach (string brandId in brandIdArray) 
                {
                    int id = ConvertHelper.GetInteger(brandId);
                    if (id < 1) continue;
                    if (videoList.ContainsKey(id))
                    {
                        videoList[id].Add(xElem.ParentNode);
                    }
                    else
                    {
                        List<XmlNode> xList = new List<XmlNode>();
                        xList.Add(xElem.ParentNode);
                        videoList.Add(id, xList);
                    }
                }
            }

            return videoList;
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
         * */
        #endregion
        
    }
}
