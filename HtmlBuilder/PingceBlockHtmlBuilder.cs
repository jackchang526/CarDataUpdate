using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using System.IO;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data;
using System.Xml;
using System.Text.RegularExpressions;
using BitAuto.Utils;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public class PingceBlockHtmlBuilder : BaseBuilder
    {
        PhotoImageConfig photoConfig;
        private string m_seriaPingceData;
        private Dictionary<int, Dictionary<string, int>> m_serialNewsNumDic;
        /// <summary>
        /// 各子品牌文章数量字典
        /// </summary>
        private Dictionary<int, Dictionary<string, int>> SerialNewsNumDic
        {
            get
            {
                if (m_serialNewsNumDic == null)
                    InitAllSerialNewsNumDic();
                return m_serialNewsNumDic;
            }
        }
        private SerialInfo _serialInfo;


        public PingceBlockHtmlBuilder()
        {
            savePathFormat = Path.Combine(CommonData.CommonSettings.SavePath, @"serialnews\pingce\Html\Serial_All_News_{0}.html");
            carNewsTypeDic = CommonData.CarNewsTypeSettings.CarNewsTypeList;
            photoConfig = (PhotoImageConfig)ConfigurationManager.GetSection("PhotoImageConfig");
        }

		public override void BuilderDataOrHtml(int serialId)
		{
			Log.WriteLog("start PingceBlockHtmlBuilder ! id:" + serialId.ToString());

			if (!CommonData.SerialDic.ContainsKey(serialId))
			{
				Log.WriteLog("not found serial! id:" + serialId.ToString());
				return;
			}
			SerialInfo csInfo = CommonData.SerialDic[serialId];
			_serialInfo = csInfo;

			string sqlGetCsPingCeNewsIDFromRainbow = "select csid,url,tagid from CarPingceInfo where csid={0}";
			DataSet dsCsPingCeNew = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString
				, CommandType.Text
				, string.Format(sqlGetCsPingCeNewsIDFromRainbow, serialId));
			List<int> listExistTagId = new List<int>();
			if (dsCsPingCeNew != null && dsCsPingCeNew.Tables.Count > 0 && dsCsPingCeNew.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dr in dsCsPingCeNew.Tables[0].Rows)
				{
					int tagId = ConvertHelper.GetInteger(dr["tagid"]);
					listExistTagId.Add(tagId);
				}
			}
			Dictionary<int, PingCeTag> dicAllTagInfo = CommonFunction.IntiPingCeTagInfo();
			BuilderPingceBlockHtmlNew(csInfo, dicAllTagInfo, listExistTagId);
		}
		
        public void BuilderDataOrHtmlOld(int serialId)
        { 
            Log.WriteLog("start PingceBlockHtmlBuilder ! id:" + serialId.ToString());

            if (!CommonData.SerialDic.ContainsKey(serialId))
            {
                Log.WriteLog("not found serial! id:" + serialId.ToString());
                return;
            }
            SerialInfo csInfo = CommonData.SerialDic[serialId];
            _serialInfo = csInfo;
            // modified by chengl Jan.4.2012
            Dictionary<int, PingCeTag> dicAllTagInfo = CommonFunction.IntiPingCeTagInfo();
            // string[] tagList = { "导语", "外观", "内饰", "空间", "视野", "灯光", "动力", "操控", "舒适性", "油耗", "配置与安全", "总结" };
            int newsId = 0;
            //if(this.DataFileDictionary.ContainsKey("dataFile"))
            //{
            //    string dataFile = String.Format(this.DataFileDictionary["dataFile"], serialId);
            //    if(File.Exists(dataFile))
            //    {
            //        //得到要显示的新闻ID
            //        DataSet pingceNewsData = new DataSet();
            //        pingceNewsData.ReadXml(dataFile);
            //        if (pingceNewsData != null && pingceNewsData.Tables.Contains("listNews")
            //            && pingceNewsData.Tables["listNews"] != null && pingceNewsData.Tables["listNews"].Rows.Count > 0)
            //        {
            //            newsId = ConvertHelper.GetInteger(pingceNewsData.Tables["listNews"].Rows[0]["newsid"]);
            //        }
            //    }
            //}
            #region 彩虹条 60 车型详解，新规则调整 by sk 2013.01.09
            List<string> htmlList = new List<string>();
            htmlList.Add("<div style=\"z-index:1\" class=\"line_box line_box_noneBg\">");
            string sqlGetCsPingCeNewsIDFromRainbow = "select csid,url,tagid from CarPingceInfo where csid={0}";
            DataSet dsCsPingCeNew = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString
                , CommandType.Text
                , string.Format(sqlGetCsPingCeNewsIDFromRainbow, serialId));
            string baseUrl = "/" + csInfo.AllSpell + "/";
            List<int> listExistTagId = new List<int>();
            if (dsCsPingCeNew != null && dsCsPingCeNew.Tables.Count > 0 && dsCsPingCeNew.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsCsPingCeNew.Tables[0].Rows)
                {
                    int tagId = ConvertHelper.GetInteger(dr["tagid"]);
                    listExistTagId.Add(tagId);
                }
            }
            if (listExistTagId.Count > 0)
            { htmlList.Add("<div class=\"car_sub_tt\"><a rel=\"nofollow\" href=\"" + baseUrl + "pingce/\" >车型详解</a></div>"); }
            else
            { htmlList.Add("<div class=\"car_sub_tt\">车型详解</div>"); }

            htmlList.Add("<ul class=\"carDetail carDetail2\">");

            int pageNum = 0;
            foreach (KeyValuePair<int, PingCeTag> kvp in dicAllTagInfo)
            {
                if (listExistTagId.Contains(kvp.Key))
                {
                    pageNum++;
                    htmlList.Add("<li><a href=\"" + baseUrl + "pingce/" + pageNum + "/\">" + kvp.Value.tagName + "</a></li>");
                }
                else
                {
                    htmlList.Add("<li class=\"noDetail\">" + kvp.Value.tagName + "</li>");
                }
            }
            //BuilderPingceBlockHtml(csInfo, dicAllTagInfo, listExistTagId);
            //1200版
            BuilderPingceBlockHtmlNew(csInfo, dicAllTagInfo, listExistTagId);
            //// modified by chengl Jan.18.2012
            //// 评测块不采用最新的，采用彩虹条 60 车型详解
            //string sqlGetCsPingCeNewsIDFromRainbow = "select csid,RainbowItemID,url from dbo.RainbowEdit where csid={0} and RainbowItemID=60";
            //DataSet dsCsPingCeNew = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString
            //    , CommandType.Text
            //    , string.Format(sqlGetCsPingCeNewsIDFromRainbow, serialId));
            //if (dsCsPingCeNew != null && dsCsPingCeNew.Tables.Count > 0 && dsCsPingCeNew.Tables[0].Rows.Count > 0)
            //{
            //    string url = dsCsPingCeNew.Tables[0].Rows[0]["url"].ToString().Trim();
            //    if (url != "")
            //    {
            //        string[] arrTempURL = url.Split('/');
            //        if (int.TryParse(arrTempURL[arrTempURL.Length - 1].ToString().Substring(3, 7), out newsId))
            //        { }
            //    }
            //}

            //List<string> titleList = new List<string>();
            //if (newsId > 0)
            //{
            //    DataSet pccDs = GetPingCeNewsFormatContent(newsId);
            //    if (pccDs != null && pccDs.Tables.Count > 0 && pccDs.Tables[0].Rows.Count > 0 && pccDs.Tables[0].Columns.Contains("content"))
            //    {
            //        DataRow row = pccDs.Tables[0].Rows[0];
            //        string newsContent = row["content"].ToString();
            //        string RegexString = "<div(?:[^<]*)?id=\"bt_pagebreak\"[^>]*>([^<]*)</div>";
            //        Regex r = new Regex(RegexString);
            //        string[] newsGroup = r.Split(newsContent);
            //        Regex rex = new Regex(@"\$\$(?<title>.+)\$\$");
            //        foreach (string pageStr in newsGroup)
            //        {
            //            Match m = rex.Match(pageStr);
            //            if (m.Success)
            //            {
            //                string pageTitle = m.Result("${title}");
            //                if (pageTitle.Length == 0)
            //                    continue;
            //                else
            //                    titleList.Add(pageTitle);
            //            }
            //        }
            //    }
            //}
            //string baseUrl = "/" + csInfo.AllSpell + "/";
            ///*
            //string csNewGouCheShouChe = this.GetCsRainbowAndURLInfo(serialId, 42);
            //string csNewKeJi = String.Empty;//this.GetCsRainbowAndURLInfo(serialId, 41);
            //string csNewShangShi = this.GetCsRainbowAndURLInfo(serialId, 37);
            //string csNewWeiXiuBaoYang = this.GetCsRainbowAndURLInfo(serialId, 40);
            //string csNewYiCheCheShi = this.GetCsRainbowAndURLInfo(serialId, 43);
            //string csNewAnQuan = String.Empty;//this.GetCsRainbowAndURLInfo(serialId, 44);
            //if(SerialNewsNumDic.ContainsKey(serialId))
            //{
            //    if(SerialNewsNumDic[serialId]["anquan"] > 0)
            //        csNewAnQuan = "http://car.bitauto.com/tree_anquan/sb_" + serialId + "/";
            //    if(SerialNewsNumDic[serialId]["keji"] > 0)
            //        csNewKeJi = "http://car.bitauto.com/tree_keji/sb_" + serialId + "/";
            //}

            //string csNewMaiCheCheShi = String.Empty; //this.GetCsRainbowAndURLInfo(serialId, 39);
            //if (HtmlBuilderData.BitautoTestDic.ContainsKey(serialId))
            //    csNewMaiCheCheShi = HtmlBuilderData.BitautoTestDic[serialId];

            //string csNewXiaoShouShuJu = "http://car.bitauto.com/" + csInfo.AllSpell + "/xiaoliang/";
            ////string csNewYouHao = "";
            //*/

            //List<string> htmlList = new List<string>();
            //htmlList.Add("<div style=\"z-index:1\" class=\"line_box line_box_noneBg\">");

            //#region 2011-07-11 wangzt 移除买车测试和口碑报告，车型综述页改版
            ///*string linkStr = "";
            //if (csNewMaiCheCheShi.Length > 0)
            //{
            //    linkStr = "<a href=\"" + csNewMaiCheCheShi + "\" >买车测试</a>";
            //}
            //string rptUrl = String.Empty;
            //if (HtmlBuilderData.SerialKoubeiReport.Contains(serialId))
            //    rptUrl = String.Format("http://car.bitauto.com/{0}/koubei/baogao/", csInfo.AllSpell);
            //if (rptUrl.Length > 0)
            //{
            //    if (linkStr.Length > 0)
            //        linkStr += " | ";
            //    linkStr += "<a href=\"" + rptUrl + "\" >口碑报告</a>";
            //}
            //if (linkStr.Length > 0)
            //{
            //    htmlList.Add("<div class=\"more\">" + linkStr + "</div>");
            //}*/
            //#endregion

            //if (newsId > 0)
            //{ htmlList.Add("<div class=\"car_sub_tt\"><a href=\"" + baseUrl + "pingce/\" >车型详解</a></div>"); }
            //else
            //{ htmlList.Add("<div class=\"car_sub_tt\">车型详解</div>"); }

            //htmlList.Add("<ul class=\"carDetail\">");

            //foreach (KeyValuePair<int, PingCeTag> kvp in dicAllTagInfo)
            //{
            //    if (kvp.Value.tagName == "导语")
            //    {
            //        if (newsId > 0)
            //            htmlList.Add("<li><a href=\"" + baseUrl + "pingce/1/\">导语</a></li>");
            //        else
            //            htmlList.Add("<li class=\"noDetail\">导语</li>");
            //        continue;
            //    }

            //    //先匹配关键词
            //    int pageNum = 0;
            //    Regex r = new Regex(kvp.Value.tagRegularExpressions);
            //    for (int i = 0; i < titleList.Count; i++)
            //    {
            //        string tmpTitle = titleList[i];
            //        if (r.IsMatch(tmpTitle))
            //        {
            //            pageNum = i + 2;
            //            break;
            //        }
            //    }

            //    if (pageNum > 0)
            //        htmlList.Add("<li><a href=\"" + baseUrl + "pingce/" + pageNum + "/\">" + kvp.Value.tagName + "</a></li>");
            //    else
            //        htmlList.Add("<li class=\"noDetail\">" + kvp.Value.tagName + "</li>");
            //}
            //// delete by chengl Jan.4.2012
            ////foreach (string tagName in tagList)
            ////{
            ////    if (tagName == "导语")
            ////    {
            ////        if (newsId > 0)
            ////            htmlList.Add("<li><a href=\"" + baseUrl + "pingce/1/\">导语</a></li>");
            ////        else
            ////            htmlList.Add("<li class=\"noDetail\">导语</li>");
            ////        continue;
            ////    }

            ////    //先匹配关键词
            ////    int pageNum = 0;
            ////    for (int i = 0; i < titleList.Count; i++)
            ////    {
            ////        string tmpTitle = titleList[i];
            ////        if (tmpTitle.IndexOf(tagName) > -1)
            ////        {
            ////            pageNum = i + 2;
            ////            break;
            ////        }
            ////    }

            ////    if (pageNum > 0)
            ////        htmlList.Add("<li><a href=\"" + baseUrl + "pingce/" + pageNum + "/\">" + tagName + "</a></li>");
            ////    else
            ////        htmlList.Add("<li class=\"noDetail\">" + tagName + "</li>");
            ////}
            #endregion
            htmlList.Add("</ul>");
            #region 碰撞测试
            bool isHasCNCAP = GetCNCAPHtml(serialId, htmlList);

            // modified by chengl May.14.2012
            if (!isHasCNCAP)
            {
                htmlList.Add("<div class=\"more cncap\">");
                if (SerialNewsNumDic.ContainsKey(serialId)
                    && SerialNewsNumDic[serialId].ContainsKey("anquan")
                    && SerialNewsNumDic[serialId]["anquan"] > 0)
                {
                    //htmlList.Add("<a href=\"/tree_anquan/sb_" + serialId + "/\" target=\"_blank\">安全测试&gt;&gt;</a>");
                    //改为新的子品牌安全文章页面
                    htmlList.Add("<a href=\"/" + csInfo.AllSpell + "/anquan/\" target=\"_blank\">安全测试&gt;&gt;</a>");
                }
                htmlList.Add("</div>");
            }

            #endregion
            htmlList.Add("<div class=\"clear\"></div>");

            htmlList.Add("<div class=\"carTxt\">");
            #region 不要了
            /*
			if (csNewShangShi.Length == 0)
				htmlList.Add("上市报道&nbsp;&nbsp;&nbsp;");
			else
				htmlList.Add("<a href=\"" + csNewShangShi + "\">上市报道</a>&nbsp;&nbsp;&nbsp;");
			if (csNewGouCheShouChe.Length == 0)
				htmlList.Add("<span class=\"noContent\">购车手册</span>");
			else
				htmlList.Add("<span><a href=\"" + csNewGouCheShouChe + "\">购车手册</a></span>");
			if (HtmlBuilderData.HasSaleDataList.Contains(serialId))
				htmlList.Add("<span><a href=\"" + csNewXiaoShouShuJu + "\">销量</a></span>");
			else
				htmlList.Add("<span class=\"noContent\">销量</a></span>");
			if (csNewKeJi.Length == 0)
				htmlList.Add("|&nbsp;&nbsp;&nbsp;技术&nbsp;&nbsp;&nbsp;");
			else
				htmlList.Add("|&nbsp;&nbsp;&nbsp;<a href=\"" + csNewKeJi + "\">技术</a>&nbsp;&nbsp;&nbsp;");
			if (csNewAnQuan.Length == 0)
				htmlList.Add("|&nbsp;&nbsp;&nbsp;安全&nbsp;&nbsp;&nbsp;");
			else
				htmlList.Add("|&nbsp;&nbsp;&nbsp;<a href=\"" + csNewAnQuan + "\">安全</a>&nbsp;&nbsp;&nbsp;");
			if (HtmlBuilderData.HasMaintanceDataList.Contains(serialId))
				htmlList.Add("|&nbsp;&nbsp;&nbsp;<a href=\"http://car.bitauto.com/" + csInfo.AllSpell + "/baoyang/\">维修养护</a>");
			else
				htmlList.Add("|&nbsp;&nbsp;&nbsp;维修养护");
			 */
            #endregion

            if (SerialNewsNumDic.ContainsKey(serialId) && SerialNewsNumDic[serialId].ContainsKey("imageCount") && SerialNewsNumDic[serialId]["imageCount"] > 0)
                htmlList.Add("<span><a rel=\"nofollow\" href=\"" + baseUrl + "tupian/\" target=\"_blank\">图片<em>(" + SerialNewsNumDic[serialId]["imageCount"] + ")</em></a></span> <b>|</b>");
            else
                htmlList.Add("<span>图片<em>(0)</em></span> <b>|</b>");

            if (SerialNewsNumDic.ContainsKey(serialId) && SerialNewsNumDic[serialId].ContainsKey("shipinCount") && SerialNewsNumDic[serialId]["shipinCount"] > 0)
                htmlList.Add("<span><a rel=\"nofollow\" href=\"" + baseUrl + "shipin/\" target=\"_blank\">视频<em>(" + SerialNewsNumDic[serialId]["shipinCount"] + ")</em></a></span> <b>|</b>");
            else
                htmlList.Add("<span>视频<em>(0)</em></span> <b>|</b>");

            if (SerialNewsNumDic.ContainsKey(serialId) && SerialNewsNumDic[serialId].ContainsKey("koubeiCount") && SerialNewsNumDic[serialId]["koubeiCount"] > 0)
                htmlList.Add("<span><a rel=\"nofollow\" href=\"" + baseUrl + "koubei/\" target=\"_blank\">口碑<em>(" + SerialNewsNumDic[serialId]["koubeiCount"] + ")</em></a></span> <b>|</b>");
            else
                htmlList.Add("<span>口碑<em>(0)</em></span> <b>|</b>");

            if (SerialNewsNumDic.ContainsKey(serialId) && SerialNewsNumDic[serialId].ContainsKey("askCount") && SerialNewsNumDic[serialId]["askCount"] > 0)
                htmlList.Add("<span><a rel=\"nofollow\" href=\"http://ask.bitauto.com/browse/" + serialId + "/\" target=\"_blank\">问答<em>(" + SerialNewsNumDic[serialId]["askCount"] + ")</em></a></span>");
            else
                htmlList.Add("<span>问答<em>(0)</em></span>");

            htmlList.Add("</div>");
            htmlList.Add("</div>");

            //检查目录
            string dir = Path.GetDirectoryName(this.savePathFormat);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(String.Format(this.savePathFormat, serialId), String.Concat(htmlList.ToArray()), Encoding.UTF8);

            Log.WriteLog("end PingceBlockHtmlBuilder! id:" + serialId.ToString());
        }

        /// <summary>
        /// 获取CNCAP部分的HTML
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="htmlList"></param>
        private bool GetCNCAPHtml(int serialId, List<string> htmlList)
        {
            bool hasCNCAP = false;
            //检查是否有星级数据
            if (CommonData.CNCAPData != null)
            {
                try
                {
                    DataRow[] rows = CommonData.CNCAPData.Tables[0].Select("Cs_Id='" + serialId + "'");
                    if (rows.Length > 0)
                    {
                        string cncapLevel = rows[rows.Length - 1]["Pvalue"].ToString().Trim();
                        if (cncapLevel.Length > 0)
                        {
                            int levelNum = 0;
                            switch (cncapLevel)
                            {
                                case "一星":
                                    levelNum = 1;
                                    break;
                                case "二星":
                                    levelNum = 2;
                                    break;
                                case "三星":
                                    levelNum = 3;
                                    break;
                                case "四星":
                                    levelNum = 4;
                                    break;
                                case "五星":
                                    levelNum = 5;
                                    break;
                            }
                            if (levelNum != 0)
                            {
                                htmlList.Add("<div class=\"more cncap\">");
                                if (SerialNewsNumDic[serialId].ContainsKey("anquan") && SerialNewsNumDic[serialId]["anquan"] > 0)
                                {
                                    SerialInfo csInfo = CommonData.SerialDic[serialId];
                                    //htmlList.Add("<a href=\"/tree_anquan/sb_" + serialId + "/\" target=\"_blank\">CNCAP" + cncapLevel + "&gt;&gt;</a>");
                                    //到子品牌的安全文章页
                                    htmlList.Add("<a href=\"/" + csInfo.AllSpell + "/anquan/\" target=\"_blank\">CNCAP" + cncapLevel + "&gt;&gt;</a>");
                                }
                                else
                                {
                                    htmlList.Add("<span>CNCAP" + cncapLevel + "</span>");
                                }

                                htmlList.Add("<div class=\"star" + levelNum + "\"></div>");
                                htmlList.Add("<b>碰撞：</b></div>");
                                hasCNCAP = true;
                            }
                        }
                    }
                }
                catch { }
            }

            if (!hasCNCAP)
            {
                // 当没有CNCAP时候检查NCAP
                if (CommonData.NCAPData != null)
                {
                    try
                    {
                        DataRow[] rows = CommonData.NCAPData.Tables[0].Select("Cs_Id='" + serialId + "'");
                        if (rows.Length > 0)
                        {
                            string cncapLevel = rows[rows.Length - 1]["Pvalue"].ToString().Trim();
                            if (cncapLevel.Length > 0)
                            {
                                int levelNum = 0;
                                switch (cncapLevel)
                                {
                                    case "一星":
                                        levelNum = 1;
                                        break;
                                    case "二星":
                                        levelNum = 2;
                                        break;
                                    case "三星":
                                        levelNum = 3;
                                        break;
                                    case "四星":
                                        levelNum = 4;
                                        break;
                                    case "五星":
                                        levelNum = 5;
                                        break;
                                }
                                if (levelNum != 0)
                                {
                                    htmlList.Add("<div class=\"more cncap\">");
                                    if (SerialNewsNumDic[serialId].ContainsKey("anquan") && SerialNewsNumDic[serialId]["anquan"] > 0)
                                    {
                                        htmlList.Add("<a href=\"/tree_anquan/sb_" + serialId + "/\" target=\"_blank\">CNCAP" + cncapLevel + "&gt;&gt;</a>");
                                    }
                                    else
                                    {
                                        htmlList.Add("<span>CNCAP" + cncapLevel + "</span>");
                                    }
                                    htmlList.Add("<div class=\"star" + levelNum + "\"></div>");
                                    htmlList.Add("<b>碰撞：</b></div>");
                                    hasCNCAP = true;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            // modified by chengl May.14.2012
            return hasCNCAP;

            //if (!hasCNCAP)
            //{
            //    htmlList.Add("<div class=\"more cncap\">");
            //    htmlList.Add("<a href=\"/tree_anquan/sb_" + serialId + "/\" target=\"_blank\">安全测试&gt;&gt;</a>");
            //    htmlList.Add("</div>");
            //}

        }
        private DataSet GetPingCeNewsFormatContent(int newsId)
        {
            if (String.IsNullOrEmpty(m_seriaPingceData))
                m_seriaPingceData = CommonData.CommonSettings.NewsUrl + "?newsid={0}&showtype=3";

            XmlReader reader = XmlReader.Create(string.Format(this.m_seriaPingceData, newsId));
            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(reader);
            }
            catch
            { }
            finally
            {
                if (reader != null && reader.ReadState != ReadState.Closed)
                    reader.Close();
            }
            return ds;
        }

        private NewsEntityV2 GetPingCeNewsFormatContentV2(int newsId)
        {
            NewsEntityV2 curPingceNew = null;
            if (String.IsNullOrEmpty(m_seriaPingceData))
            {
                curPingceNew = Common.CommonFunction.GetNewsEntityFromApi(newsId);
            }
            return curPingceNew;
        }


        /// <summary>
        /// 初始化所有子品牌的文章数量列表
        /// </summary>
        public void InitAllSerialNewsNumDic()
        {
            XmlDocument xmlDoc = CommonFunction.GetXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\newsNum.xml"));
            XmlNodeList serialNodeList = xmlDoc.SelectNodes("/SerilaList/Serial");
            m_serialNewsNumDic = new Dictionary<int, Dictionary<string, int>>();

            //图片，视频，口碑，答疑
            Dictionary<int, int> imageCountDic = CommonData.GetAllSerialImageCount();
            Dictionary<int, int> shipinCountDic = CommonData.GetAllSerialShipinCount();
            Dictionary<int, int> koubeiCountDic = CommonData.GetAllSerialKoubeiCount();
            Dictionary<int, int> askCountDic = CommonData.GetAllSerialAskCount();
            foreach (XmlElement serialNode in serialNodeList)
            {
                int serialId = ConvertHelper.GetInteger(serialNode.GetAttribute("id"));
                if (serialId == 0)
                    continue;
                m_serialNewsNumDic[serialId] = new Dictionary<string, int>();
                m_serialNewsNumDic[serialId]["anquan"] = ConvertHelper.GetInteger(serialNode.GetAttribute("anquan"));
                m_serialNewsNumDic[serialId]["keji"] = ConvertHelper.GetInteger(serialNode.GetAttribute("keji"));
                if (imageCountDic.ContainsKey(serialId))
                    m_serialNewsNumDic[serialId]["imageCount"] = imageCountDic[serialId];
                if (shipinCountDic.ContainsKey(serialId))
                    m_serialNewsNumDic[serialId]["shipinCount"] = shipinCountDic[serialId];
                if (koubeiCountDic.ContainsKey(serialId))
                    m_serialNewsNumDic[serialId]["koubeiCount"] = koubeiCountDic[serialId];
                if (askCountDic.ContainsKey(serialId))
                    m_serialNewsNumDic[serialId]["askCount"] = askCountDic[serialId];
            }
        }
        #region 1200版
        /// <summary>
        /// 1200版车型综述页 车型详解块生成
        /// </summary>
        /// <param name="csInfo"></param>
        /// <param name="dicAllTagInfo"></param>
        /// <param name="listExistTagId"></param>
        private void BuilderPingceBlockHtmlNew(SerialInfo csInfo, Dictionary<int, PingCeTag> dicAllTagInfo, List<int> listExistTagId)
        {
            StringBuilder sbPingce = new StringBuilder();
            sbPingce.Append("<div class=\"col-auto section-left\">");
            //string imgUrl = GetImgUrl(csInfo.Id);

            string pingceHtml = GetPingceHtmlNew(csInfo, dicAllTagInfo, listExistTagId);
            string goucheHtml = GetGouCheHtml(csInfo.Id);
            sbPingce.Append(pingceHtml).Append(goucheHtml);
            sbPingce.Append("</div>");
            bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = csInfo.Id,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                BlockID = CommonHtmlEnum.BlockIdEnum.Pingce,
                HtmlContent = sbPingce.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("1200版综述页车型详解更新失败：BuliderDataOrHtml,serialId:" + csInfo.Id);
        }



        /// <summary>
        /// 子品牌焦点图第一张->白底图->默认图
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
		private string GetImgUrl(int serialId, int imgSize)
        {
            string img = CommonData.CommonSettings.DefaultCarPic;
            XmlDocument doc = new XmlDocument();
            //图片
            string imgFilePath = Path.Combine(photoConfig.SavePath, string.Format(@"SerialFocusImage\SerialFocusImage_{0}.xml", serialId));
            try
            {
                if (File.Exists(imgFilePath))
                {
                    doc.Load(imgFilePath);
                    XmlNodeList xnl = doc.SelectNodes("/ImageData/ImageList/ImageInfo");
                    foreach (XmlElement imgNode in xnl)
                    {
                        int ImageId = ConvertHelper.GetInteger(imgNode.GetAttribute("ImageId"));
						string imgUrl = imgNode.GetAttribute("ImageUrl");
						if (imgUrl.ToLower().IndexOf("bitautoimg.com") == -1)
						{
							img = CommonFunction.GetPublishHashImageDomain(ImageId) + imgUrl;
						}
						img = string.Format(imgUrl, 4);
                        break;
                    }
                }
                if (img == CommonData.CommonSettings.DefaultCarPic)
                {
					Dictionary<int, string> dicSerialPhoto = CommonData.dicSerialNewPhoto;
					if (dicSerialPhoto.ContainsKey(serialId))
					{
						img = dicSerialPhoto[serialId].Replace("_2.", string.Format("_{0}.", imgSize));
					}
					/*
                    string fileName = Path.Combine(photoConfig.SavePath, "SerialCoverWithout.xml");
                    doc.Load(fileName);
                    XmlNode xnl = doc.SelectSingleNode("/SerialList/Serial[SerialId='" + serialId + "']");
                    if (xnl != null)
                    {
                        if (!string.IsNullOrEmpty(xnl.Attributes["ImageUrl2"].ToString()))
                        {
							img = string.Format(xnl.Attributes["ImageUrl2"].ToString(), imgSize);
                        }
                        else if (!string.IsNullOrEmpty(xnl.Attributes["ImageUrl"].ToString()))
                        {
							img = string.Format(xnl.Attributes["ImageUrl"].ToString(), imgSize);
                        }
                    }
					 * */
                }
            }
            catch (Exception ex)
            {
				Log.WriteErrorLog("PingceBlockHtmlBuilder.GetImgUrl,文件解析错误，文件路径：" + imgFilePath + ";" + ex.Message);
            }
            
            return img;
        }

		/// <summary>
		/// 获取车系位置图片
		/// </summary>
		/// <param name="serialId">车系id</param>
		/// <param name="positionId">位置id</param>
		/// <param name="imgSize">图片尺寸</param>
		/// <returns></returns>
		private string GetPositionImg(int serialId, int positionId,int imgSize)
		{
			string imgUrl = string.Empty;
			string FilePath = Path.Combine(photoConfig.SavePath, string.Format("SerialElevenImage\\" + serialId + ".xml"));
			if (File.Exists(FilePath))
			{
				XmlDocument xmlDoc = new XmlDocument();
				try
				{
					xmlDoc.Load(FilePath);
					XmlNode positionNode = xmlDoc.SelectSingleNode("CarImageList/CarImage[@PositionId=" + positionId + "]");
					if (positionNode != null)
					{
						imgUrl = string.Format(positionNode.Attributes["ImageUrl"].Value, imgSize);
					}
				}
				catch (Exception ex)
				{
					Log.WriteErrorLog("PingceBlockHtmlBuilder.GetPositionImg,文件解析错误，文件路径：" + FilePath + ";" + ex.Message);
				}
			}
			if (string.IsNullOrWhiteSpace(imgUrl))
			{
				imgUrl = GetImgUrl(serialId, imgSize);
			}
			return imgUrl;
		}

        /// <summary>
        /// 购车手车
        /// </summary>
        /// <returns></returns>
        private string GetGouCheHtml(int serialId)
        {
            StringBuilder sb = new StringBuilder();
            int newsId = 0;
            string daogouUrl = base.GetCsRainbowAndURLInfo(serialId, 42);
            if (!string.IsNullOrEmpty(daogouUrl))
            {
                string[] arrTempURL = daogouUrl.Split('/');
                string pageName = arrTempURL[arrTempURL.Length - 1];
                if (pageName.Length >= 10)
                {
                    if (int.TryParse(pageName.Substring(3, 7), out newsId))
                    { }
                }
            }
            bool isHaveNews = false;
            string PingCeTitle = "暂无内容";
            string PingCeFilePath = string.Empty;
			string imgUrl = string.Empty;
            if (newsId > 0)
            {
                //DataSet ds = GetPingCeNewsFormatContent(newsId);   注释旧版获取新闻的方式,用下面几行获取新闻实体
                NewsEntityV2 curPingceNewsEntity = GetPingCeNewsFormatContentV2(newsId);
                if (curPingceNewsEntity != null)
                {
                    PingCeTitle = curPingceNewsEntity.ShortTitle;
                    PingCeFilePath = curPingceNewsEntity.Url;
                    isHaveNews = true;
                }
            }
            else
            {
				
                DataSet dsDaogou = GetTopSerialNewsNew(serialId, 8, 1);
                if (dsDaogou != null && dsDaogou.Tables.Count > 0 && dsDaogou.Tables[0].Rows.Count > 0)
                {
                    DataRow row = dsDaogou.Tables[0].Rows[0];
                    PingCeTitle = row["FaceTitle"].ToString();
                    PingCeFilePath = row["filepath"].ToString();
					imgUrl = row["ImageConverUrl"].ToString();
                    string[] arrTempURL = PingCeFilePath.Split('/');
                    string pageName = arrTempURL[arrTempURL.Length - 1];
                    if (pageName.Length >= 10)
                    {
                        if (int.TryParse(pageName.Substring(3, 7), out newsId))
                        { }
                    }
                    isHaveNews = true;
                }
            }
			if (string.IsNullOrWhiteSpace(imgUrl))
			{
				imgUrl = GetPositionImg(serialId, 43, 1);
			}
            PingCeFilePath = PingCeFilePath.StartsWith("http://") ? PingCeFilePath : ("http://news.bitauto.com" + PingCeFilePath);
            sb.Append("<div class=\"row special-layout-1 type-1\"  data-channelid=\"2.21.1535\"><div class=\"col-auto left\">");
            if (isHaveNews)
                sb.AppendFormat("<a href=\"{0}\" target=\"_blank\">", PingCeFilePath);
            else
                sb.Append("<a>");
            sb.AppendFormat("<div class=\"figure\"><img src=\"{0}\"></div>", imgUrl);
            sb.Append("</a>");
            sb.Append("<div class=\"bottom\">易车出品</div></div><div class=\"col-auto right\">");
            if (isHaveNews)
            {
                sb.AppendFormat("<h3><a href=\"{0}\" target=\"_blank\">{1}</a></h3>", PingCeFilePath, PingCeTitle);
                sb.AppendFormat("<div class=\"info\"><span class=\"view\" data-vnewsid=\"{0}\"></span><span class=\"comment\" data-cnewsid=\"{0}\"></span></div>", newsId);
            }
            else
            {
                sb.AppendFormat("<h3><a>{0}</a></h3>", PingCeTitle);
            }
            sb.Append("</div></div>");
            return sb.ToString();
        }

        private string GetPingceHtmlNew(SerialInfo csInfo, Dictionary<int, PingCeTag> dicAllTagInfo, List<int> listExistTagId)
        {
            int newsId = 0;
            bool isHaveNews = false;
            string PingCeTitle = "暂无内容";
            string PingCeFaceTitle = "暂无内容";
            string PingCeFilePath = string.Empty;
            Dictionary<int, PingCeTagNew> dictPingceNews = GetPingceTagsByCsId(csInfo.Id);
            if (dictPingceNews.Count > 0)
            {
                string url = dictPingceNews.First().Value.url;
                string[] arrTempURL = url.Split('/');
                string pageName = arrTempURL[arrTempURL.Length - 1];
                if (pageName.Length >= 10)
                {
                    if (int.TryParse(pageName.Substring(3, 7), out newsId))
                    { }
                }
            }
            if (newsId > 0)
            {
                NewsEntityV2 curPingceNewsEntity = GetPingCeNewsFormatContentV2(newsId);
                if (curPingceNewsEntity != null)
                {
                    PingCeTitle = curPingceNewsEntity.Title;
                    PingCeFaceTitle = curPingceNewsEntity.ShortTitle;
                    PingCeFilePath = curPingceNewsEntity.Url;
                    isHaveNews = true;
                }
            }
			string imgUrl = GetPositionImg(csInfo.Id, 97, 1);
            PingCeFilePath = (string.IsNullOrEmpty(PingCeFilePath) || PingCeFilePath.StartsWith("http://")) ? PingCeFilePath : ("http://news.bitauto.com" + PingCeFilePath);
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"row special-layout-1\" id=\"pingce_left_top\" data-channelid=\"2.21.1534\">");
            sb.Append("<div class=\"col-auto left\">");
			//if (isHaveNews)
			//{
			sb.AppendFormat("<a href=\"http://car.bitauto.com/{0}/pingce/\" target=\"_blank\">", _serialInfo.AllSpell);
			//}
			//else
			//{
			//	sb.AppendFormat("<a>");
			//}
            sb.AppendFormat("<div class=\"figure\"><img src=\"{0}\"></div>", imgUrl);
            sb.AppendFormat("<div class=\"caption\"><span class=\"title\">YICHE VIEW</span><h4>详解{0}</h4></div>", _serialInfo.ShowName);
            sb.AppendFormat("</a>");
            sb.Append("<div class=\"bottom\">易车出品</div></div>");
            sb.Append("<div class=\"col-auto right\">");
            if (isHaveNews)
            {
                sb.AppendFormat("<h3><a href=\"{0}\" target=\"_blank\">{1}</a></h3>", PingCeFilePath, PingCeFaceTitle);
            }
            else
            {
                sb.AppendFormat("<h3><a>{0}</a></h3>", PingCeTitle);
            }
            sb.Append("<div class=\"info\">");
            if (isHaveNews)
            {
                sb.AppendFormat("<span class=\"view\" data-vnewsid=\"{0}\"></span><span class=\"comment\" data-cnewsid=\"{0}\"></span>", newsId);
            }
            sb.Append("</div>");
            //分类
            sb.Append(GetTagHtml(csInfo, dicAllTagInfo, listExistTagId));
            sb.Append("</div>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private string GetTagHtml(SerialInfo csInfo, Dictionary<int, PingCeTag> dicAllTagInfo, List<int> listExistTagId)
        {
            string baseUrl = "/" + csInfo.AllSpell + "/";
            string[] tagNameArr = { "外观", "内饰", "空间", "动力", "操控", "油耗", "配置", "总结" };
            //取标签的页码
            Dictionary<string, int> dictTagPageNumber = new Dictionary<string, int>();
            int tempPageNum = 0;
            foreach (KeyValuePair<int, PingCeTag> kvp in dicAllTagInfo)
            {
                if (listExistTagId.Contains(kvp.Key))
                {
                    tempPageNum++;
                    dictTagPageNumber.Add(kvp.Value.tagName, tempPageNum);
                }
            }
            int loop = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class=\"list list-gapline sm\">");
            foreach (string tagName in tagNameArr)
            {
                KeyValuePair<int, PingCeTag> kvp = dicAllTagInfo.FirstOrDefault(pire => pire.Value.tagName == tagName);
                string noborder = string.Empty;

                if (loop > 0 && loop % 4 == 0)
                    sb.Append("</ul><ul class=\"list list-gapline sm\">");
                if (listExistTagId.Contains(kvp.Key))
                {
                    sb.AppendFormat("<li><a href=\"{0}\" target=\"_blank\">{1}</a></li>",
                          baseUrl + "pingce/" + dictTagPageNumber[kvp.Value.tagName] + "/",
                          kvp.Value.tagName);
                }
                else
                {
                    sb.AppendFormat("<li><a class=\"no-link\"\">{0}</a></li>", kvp.Value.tagName);
                }
                loop++;
            }
            sb.Append("</ul>");
            return sb.ToString();
        }
        #endregion


        /// <summary>
        /// 新版综述页 车型详解块生成
        /// </summary>
        /// <param name="serialId">子品牌id</param>
        /// <param name="dicAllTagInfo">车型详解分类</param>
        /// <param name="listExistTagId">有数据的tagid</param>
        /// <returns></returns>
        private void BuilderPingceBlockHtml(SerialInfo csInfo, Dictionary<int, PingCeTag> dicAllTagInfo, List<int> listExistTagId)
        {
            StringBuilder sbPingceNew = new StringBuilder();

            int serialId = csInfo.Id;
            //取导购前2条
            bool isGetFirst = false;
            DataSet ds = GetTopSerialNewsNew(serialId, 8, 2);
            //买车必看 单车保养 新闻 add by sk 2016.01.26
            DataSet dsBaoyang = GetTopBaoyangNewsNew(serialId);

            string pingceHtml = GetPingceHtml(serialId);
            string daotouHtml = GetDaogouHtml(serialId, ds, out isGetFirst);

            sbPingceNew.Append("<div class=\"news_outerbox\">");
            sbPingceNew.AppendFormat("    <h2><span><a href=\"/{1}/pingce/\" target=\"_blank\">车型详解</a></span><em>{0}</em></h2>", string.IsNullOrEmpty(pingceHtml) ? "暂无内容" : pingceHtml, _serialInfo.AllSpell);
            sbPingceNew.Append("    <ul class=\"news_xiangjie\">");

            int pageNum = 0;
            int loop = 0;
            string baseUrl = "/" + csInfo.AllSpell + "/";
            //固定内容输出 by sk 2014.01.10
            string[] tagNameArr = { "外观", "内饰", "空间", "动力", "操控", "油耗", "配置", "总结" };
            //取标签的页码
            Dictionary<string, int> dictTagPageNumber = new Dictionary<string, int>();
            int tempPageNum = 0;
            foreach (KeyValuePair<int, PingCeTag> kvp in dicAllTagInfo)
            {
                if (listExistTagId.Contains(kvp.Key))
                {
                    tempPageNum++;
                    dictTagPageNumber.Add(kvp.Value.tagName, tempPageNum);
                }
            }

            foreach (string tagName in tagNameArr)
            {
                KeyValuePair<int, PingCeTag> kvp = dicAllTagInfo.FirstOrDefault(pire => pire.Value.tagName == tagName);
                string noborder = string.Empty;
                loop++;
                if (loop % 4 == 0)
                    noborder = " class=\"noborder\"";
                if (listExistTagId.Contains(kvp.Key))
                {
                    pageNum++;
                    sbPingceNew.AppendFormat("<li {2}><a href=\"{0}\"  target=\"_blank\">{1}</a></li>",
                          baseUrl + "pingce/" + dictTagPageNumber[kvp.Value.tagName] + "/",
                          kvp.Value.tagName, noborder);
                }
                else
                {
                    sbPingceNew.AppendFormat("<li {1}>{0}</li>", kvp.Value.tagName, noborder);
                }
            }
            sbPingceNew.Append("    </ul>");
            sbPingceNew.Append("</div>");
            sbPingceNew.Append(daotouHtml);

            //买车必看 单车保养分类->导购（第一条已经购车手册了，提取第二条） modified by sk 2016.01.26
            DataRow dr = null;
            string cateName = "买车必看";
            string cateUrl = "daogou";
            if (dsBaoyang != null && dsBaoyang.Tables.Count > 0 && dsBaoyang.Tables[0].Rows.Count > 0)
            {
                dr = dsBaoyang.Tables[0].Rows[0];
                cateName = "养车费用";
                cateUrl = "yongche";
            }
            else
            {
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (isGetFirst)
                    {
                        if (ds.Tables[0].Rows.Count > 1)
                        {
                            dr = ds.Tables[0].Rows[1];
                        }
                    }
                    else
                    {
                        dr = ds.Tables[0].Rows[0];
                    }
                }
            }
            if (dr != null)
            {
                string faceTitle = dr["FaceTitle"].ToString();
                string filepath = dr["filepath"].ToString();

                sbPingceNew.Append("<div class=\"news_outerbox news_maiche\">");
                sbPingceNew.AppendFormat("<h2><span><a href=\"/{0}/{4}/\" target=\"_blank\">{3}</a></span><a href=\"{1}\"  target=\"_blank\">{2}</a></h2>",
                _serialInfo.AllSpell,
                filepath,
                faceTitle,
                cateName,
                cateUrl);
                sbPingceNew.Append("</div>");
            }

            bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = serialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.Pingce,
                HtmlContent = sbPingceNew.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("新版综述页车型详解更新失败：BuliderDataOrHtml,serialId:" + serialId);
        }
        private string GetDaogouHtml(int serialId, DataSet dsDaogou, out bool isGetFirst)
        {
            isGetFirst = false;
            StringBuilder sb = new StringBuilder();
            string daogouUrl = base.GetCsRainbowAndURLInfo(serialId, 42);
            int newsId = 0;
            if (!string.IsNullOrEmpty(daogouUrl))
            {
                string[] arrTempURL = daogouUrl.Split('/');
                string pageName = arrTempURL[arrTempURL.Length - 1];
                if (pageName.Length >= 10)
                {
                    if (int.TryParse(pageName.Substring(3, 7), out newsId))
                    { }
                }
            }
            if (newsId > 0)
            {
                NewsEntityV2 curPingceNewsEntity = GetPingCeNewsFormatContentV2(newsId);
                if (curPingceNewsEntity != null)
                {
                    string PingCeTitle = string.Empty;
                    string PingCeFilePath = string.Empty;
                    PingCeTitle = curPingceNewsEntity.ShortTitle;
                    PingCeFilePath = curPingceNewsEntity.Url;
                    string summary = curPingceNewsEntity.Summary;

                    sb.Append("<div class=\"news_outerbox news_innerbox_b news_maiche\">");
                    sb.AppendFormat("    <h2><span><a href=\"/{2}/daogou/\" target=\"_blank\">购车手册</a></span><a href=\"http://news.bitauto.com{0}\"  target=\"_blank\">{1}</a></h2>", PingCeFilePath, PingCeTitle, _serialInfo.AllSpell);
                    if (string.IsNullOrEmpty(summary))
                        summary = "暂无内容";
                    sb.AppendFormat("    <p>{0}</p>", summary);
                    sb.Append("</div>");
                }
                else
                {
                    sb.Append("<div class=\"news_outerbox news_innerbox_b news_maiche\">");
                    sb.Append("    <h2><span>购车手册</span>暂无内容</h2>");
                    //sb.Append("    <p>暂无内容</p>");
                    sb.Append("</div>");
                }
            }
            else
            {
                //导购最新文章第一条
                //DataSet ds = GetTopSerialNews(serialId, 8, 1);
                if (dsDaogou != null && dsDaogou.Tables.Count > 0 && dsDaogou.Tables[0].Rows.Count > 0)
                {
                    isGetFirst = true;
                    string faceTitle = dsDaogou.Tables[0].Rows[0]["FaceTitle"].ToString();
                    string filepath = dsDaogou.Tables[0].Rows[0]["filepath"].ToString();
                    string summary = ConvertHelper.GetString(dsDaogou.Tables[0].Rows[0]["summary"]);
                    sb.Append("<div class=\"news_outerbox news_innerbox_b news_maiche\">");
                    sb.AppendFormat("    <h2><span><a href=\"/{2}/daogou/\" target=\"_blank\">购车手册</a></span><a href=\"{0}\"  target=\"_blank\">{1}</a></h2>", filepath, faceTitle, _serialInfo.AllSpell);
                    if (string.IsNullOrEmpty(summary))
                        summary = "暂无内容";
                    sb.AppendFormat("    <p>{0}</p>", summary);
                    sb.Append("</div>");
                }
                else
                {
                    sb.Append("<div class=\"news_outerbox news_innerbox_b news_maiche\">");
                    sb.Append("    <h2><span>购车手册</span>暂无内容</h2>");
                    //sb.Append("    <p>暂无内容</p>");
                    sb.Append("</div>");
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 获取子品牌新闻列表
        /// </summary>
        public static DataSet GetTopSerialNews(int serialId, int carNewsTypeId, int top)
        {
            if (serialId < 1 || carNewsTypeId < 1 || top < 1) { return null; }

            string sql = @"SELECT TOP (@Top) n.FaceTitle,sn.FilePath,n.summary,sn.PublishTime
							 FROM SerialNews sn
							 inner join dbo.News n ON n.ID = sn.CarNewsId
							 WHERE SerialId=@SerialId AND CarNewsTypeId=@CarNewsTypeId  AND sn.CreativeType=0
							 ORDER BY sn.PublishTime DESC";
            SqlParameter[] param = new SqlParameter[]{
                new SqlParameter("@Top",top),
                new SqlParameter("@SerialId",serialId),
				new SqlParameter("@CarNewsTypeId",carNewsTypeId)
            };
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, param);
            return ds;
        }

        /// <summary>
        /// 取分类新闻，新库
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static DataSet GetTopSerialNewsNew(int serialId, int carNewsTypeId, int top)
        {
            if (serialId < 1 || carNewsTypeId < 1 || top < 1) { return null; }

			string sql = @" SELECT TOP (@Top) n.ShortTitle FaceTitle, n.Url filepath, n.PublishTime,n.Id,n.ImageConverUrl
	                    FROM    dbo.Car_SerialNewsV2 sn
			                    INNER JOIN dbo.Car_NewsV2 n ON sn.CarNewsId = n.Id
	                    WHERE   SerialId = @SerialId
			                    AND sn.CategoryId in (SELECT CmsCategoryId
							                    FROM    [dbo].[CarNewsTypeDef]
							                    WHERE   CarNewsTypeId = @CategoryId)
			                    and  sn.CopyRight = 0 
	                    ORDER BY n.PublishTime DESC";
            SqlParameter[] param = new SqlParameter[]{
                 new SqlParameter("@SerialId",serialId),
                 new SqlParameter("@Top",top),
                 new SqlParameter("@CategoryId",carNewsTypeId)
            };
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, param);
            return ds;
        }

        public static DataSet GetTopBaoyangNews(int serialId)
        {
            string sql = @"SELECT TOP 1
										n.FaceTitle, n.FilePath, n.PublishTime
								FROM    [dbo].[SerialNews] sn
										INNER JOIN dbo.News n ON n.ID = sn.CarNewsId
								WHERE   SerialId = @SerialId
										AND CarNewsTypeId = 11
										AND CategoryId = 533
								ORDER BY n.PublishTime DESC";
            SqlParameter[] param = new SqlParameter[]{
                 new SqlParameter("@SerialId",serialId)
            };
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, param);
            return ds;
        }
        public static DataSet GetTopBaoyangNewsNew(int serialId)
        {
            string sql = @"SELECT TOP 1 n.ShortTitle FaceTitle,n.Url FilePath, n.PublishTime
								FROM     dbo.Car_SerialNewsV2 sn
										INNER JOIN dbo.Car_NewsV2 n ON sn.CarNewsId = n.Id
								WHERE   SerialId = @SerialId
                                        AND sn.CategoryId=533
                                        and  sn.CopyRight = 0 
								ORDER BY n.PublishTime DESC";
            SqlParameter[] param = new SqlParameter[]{
                 new SqlParameter("@SerialId",serialId)
            };
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, param);
            return ds;
        }
        private string GetPingceHtml(int serialId)
        {
            string strPingceHtml = string.Empty;
            int newsId = 0;
            Dictionary<int, PingCeTagNew> dictPingceNews = GetPingceTagsByCsId(serialId);
            if (dictPingceNews.Count > 0)//查找“导语”新闻 ;modified by 2014.02.25	调整为取第一条
            {
                string url = dictPingceNews.First().Value.url;
                string[] arrTempURL = url.Split('/');
                string pageName = arrTempURL[arrTempURL.Length - 1];
                if (pageName.Length >= 10)
                {
                    if (int.TryParse(pageName.Substring(3, 7), out newsId))
                    { }
                }
            }
            if (newsId > 0)
            {
                NewsEntityV2 curPingceNewsEntity = GetPingCeNewsFormatContentV2(newsId);
                if (curPingceNewsEntity != null)
                {
                    string PingCeTitle = string.Empty;
                    string PingCeFilePath = string.Empty;
                    PingCeTitle = curPingceNewsEntity.ShortTitle;
                    PingCeFilePath = curPingceNewsEntity.Url;
                    strPingceHtml = string.Format("<a href=\"http://news.bitauto.com{0}\" target=\"_blank\">{1}</a>", PingCeFilePath, PingCeTitle);
                }
            }
            return strPingceHtml;
        }
        private Dictionary<int, PingCeTagNew> GetPingceTagsByCsId(int csId)
        {
            Dictionary<int, PingCeTagNew> dicAllTagInfo = IntiPingCeTagInfoNew();
            Dictionary<int, PingCeTagNew> dic = new Dictionary<int, PingCeTagNew>();
            string sql = "SELECT [csid],[url],[tagid] FROM [CarPingceInfo] WHERE csid=@csid ORDER BY tagid";
            SqlParameter[] parameters = { new SqlParameter("@csid", SqlDbType.Int) };
            parameters[0].Value = csId;
            DataSet ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, parameters);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int tagId = ConvertHelper.GetInteger(dr["tagid"]);
                    string url = dr["url"].ToString();
                    PingCeTagNew pingce = new PingCeTagNew();
                    pingce.tagId = tagId;
                    pingce.tagName = dicAllTagInfo[tagId].tagName;
                    pingce.url = url;
                    if (!dic.ContainsKey(tagId))
                    {
                        dic.Add(tagId, pingce);
                    }
                }
            }
            return dic;
        }
        /// <summary>
        /// 初始化评测的各个标签 名及匹配规则
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, PingCeTagNew> IntiPingCeTagInfoNew()
        {
            Dictionary<int, PingCeTagNew> dic = new Dictionary<int, PingCeTagNew>();
            // 导语
            PingCeTagNew pct1 = new PingCeTagNew();
            pct1.tagName = "导语";
            pct1.tagRegularExpressions = "(导语：|导语:)";
            dic.Add(1, pct1);
            // 外观
            PingCeTagNew pct2 = new PingCeTagNew();
            pct2.tagName = "外观";
            pct2.tagRegularExpressions = "(外观：|外观:)";
            dic.Add(2, pct2);
            // 内饰
            PingCeTagNew pct3 = new PingCeTagNew();
            pct3.tagName = "内饰";
            pct3.tagRegularExpressions = "(内饰：|内饰:)";
            dic.Add(3, pct3);
            // 空间
            PingCeTagNew pct4 = new PingCeTagNew();
            pct4.tagName = "空间";
            pct4.tagRegularExpressions = "(空间：|空间:)";
            dic.Add(4, pct4);
            // 视野
            PingCeTagNew pct5 = new PingCeTagNew();
            pct5.tagName = "视野";
            pct5.tagRegularExpressions = "(视野：|视野:)";
            dic.Add(5, pct5);
            // 灯光
            PingCeTagNew pct6 = new PingCeTagNew();
            pct6.tagName = "灯光";
            pct6.tagRegularExpressions = "(灯光：|灯光:)";
            dic.Add(6, pct6);
            // 动力
            PingCeTagNew pct7 = new PingCeTagNew();
            pct7.tagName = "动力";
            pct7.tagRegularExpressions = "(动力：|动力:)";
            dic.Add(7, pct7);
            // 操控
            PingCeTagNew pct8 = new PingCeTagNew();
            pct8.tagName = "操控";
            pct8.tagRegularExpressions = "(操控：|操控:)";
            dic.Add(8, pct8);
            // 舒适性
            PingCeTagNew pct9 = new PingCeTagNew();
            pct9.tagName = "舒适性";
            pct9.tagRegularExpressions = "(舒适性：|舒适：|舒适性:|舒适:)";
            dic.Add(9, pct9);
            // 油耗
            PingCeTagNew pct10 = new PingCeTagNew();
            pct10.tagName = "油耗";
            pct10.tagRegularExpressions = "(油耗：|油耗:)";
            dic.Add(10, pct10);
            // 配置
            PingCeTagNew pct11 = new PingCeTagNew();
            pct11.tagName = "配置";
            pct11.tagRegularExpressions = "(配置与安全：|配置：|配置与安全:|配置:)";
            dic.Add(11, pct11);
            // 总结
            PingCeTagNew pct12 = new PingCeTagNew();
            pct12.tagName = "总结";
            pct12.tagRegularExpressions = "(总结：|总结:)";
            dic.Add(12, pct12);
            return dic;
        }
        /// <summary>
        /// 评测的标签 名及匹配规则
        /// </summary>
        public struct PingCeTagNew
        {
            public string tagName;
            public string tagRegularExpressions;
            public int tagId;
            public string url;
        }
    }
}
