using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.HtmlBuilder.com.bitauto.baa.api;
using BitAuto.CarDataUpdate.HtmlBuilder.com.bitauto.baa.api1;
using BitAuto.CarDataUpdate.HtmlBuilder.com.bitauto.baa.api2;
using BitAuto.CarDataUpdate.HtmlBuilder.com.bitauto.baa.api5;
using BitAuto.Utils;
using System.Xml.Linq;
using System.Net;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public class SerialKoubeiHtmlBuilder : BaseBuilder
    {
        private int SerialId = 0;
        private SerialInfo _serialInfo;
        //private XmlDocument xmlKoubei;

        //private carService carService = null;
        private topicService topicService = null;
        private impressionService impressionService = null;
        private reportService reportService = null;
        private XmlDocument xmlYoudian;
        private XmlDocument xmlQuedian;
        private XmlNodeList youdianNodes;
        private XmlNodeList quedianNodes;
        private XmlDocument xmlKouBeiReport;
        private XmlDocument xmlRating;
		private DataRow drTuiJianKoubei = null;
		private string KoubeiDetailRatingUrl = CommonData.CommonSettings.KoubeiDetailRatingUrl;//口碑评分明细接口
		//private readonly Dictionary<int, DataRow> _koubeiTuijianDic = new Dictionary<int, DataRow>();
        public readonly Dictionary<int, Dictionary<string, string>> _koubeiRatingDic = new Dictionary<int,Dictionary<string,string>>();


        public SerialKoubeiHtmlBuilder()
        {
			//carService = new carService()
			//{
			//	ApiSoapHeaderValue = new com.bitauto.baa.api1.ApiSoapHeader()
			//	{
			//		AppKey = "100049",
			//		AppPwd = "EAB934E5-6021-48DF-BA69-B063FCFEA72B"
			//	}
			//};
            topicService = new topicService()
            {
                ApiSoapHeaderValue = new com.bitauto.baa.api.ApiSoapHeader()
                {
                    AppKey = "100049",
                    AppPwd = "EAB934E5-6021-48DF-BA69-B063FCFEA72B"
                }
            };
            impressionService = new impressionService()
            {
                ApiSoapHeaderValue = new com.bitauto.baa.api2.ApiSoapHeader()
                {
                    AppKey = "100049",
                    AppPwd = "EAB934E5-6021-48DF-BA69-B063FCFEA72B"
                }
            };
            reportService = new reportService()
            {
                ApiSoapHeaderValue = new com.bitauto.baa.api5.ApiSoapHeader()
                {
                    AppKey = "100049",
                    AppPwd = "EAB934E5-6021-48DF-BA69-B063FCFEA72B"
                }
            };

            //InitKoubeiTuijianDic();


			InitKoubeiRating();
        }

		private void InitKoubeiRating()
		{
			List<int> serialList = CommonFunction.GetSerialList();
			List<Tuple<int, string, string>> koubeiKeyTuple = new List<Tuple<int, string, string>>()
			{ 
				new Tuple<int,string,string>(1,"WaiGuan","WaiGuan"),
				new Tuple<int,string,string>(2,"NeiShi","NeiShi"),
				new Tuple<int,string,string>(3,"KongJian","KongJian"),
				new Tuple<int,string,string>(4,"DongLi","DongLi"),
				new Tuple<int,string,string>(5,"CaoKong","CaoKong"),
				new Tuple<int,string,string>(6,"PeiZhi","PeiZhi"),
				new Tuple<int,string,string>(7,"XingJiaBi","XingJiaBi"),
				new Tuple<int,string,string>(8,"ShuShiDu","ShuShiDu"),
				new Tuple<int,string,string>(10,"YouHao","YouHao"),
				new Tuple<int,string,string>(0,"Ratings","Rating"),
				new Tuple<int,string,string>(-1,"TopicCount","TotalCount"),
				new Tuple<int,string,string>(-2,"MinFuel","MinFuelValue"),
				new Tuple<int,string,string>(-3,"MaxFuel","MaxFuelValue"),
				new Tuple<int,string,string>(-4,"Ranker","LevelRatingRanker")
			};
			foreach (int csID in serialList)
			{
				try
				{
					string koubeiUrl = KoubeiDetailRatingUrl.Replace("{csid}", csID.ToString());
					XmlDocument xmlRating = CommonFunction.GetXmlDocument(koubeiUrl);
					//double AverageRating = 0;
					//int koubeiCount = 0;
					Console.WriteLine(csID);
					if (xmlRating != null)
					{
						XmlNode node = xmlRating.SelectSingleNode("Serial/Item/RatingItem");
						if (node == null) continue;
						Dictionary<string, string> ratingDic = new Dictionary<string, string>();
						foreach (Tuple<int, string, string> tuple in koubeiKeyTuple)
						{
							if (tuple.Item1 > 0)
							{
								XmlNode ratingNode = node.SelectSingleNode("Item[Id=" + tuple.Item1 + "]");
								if(ratingNode != null)
								{
									XmlNode detailNode = ratingNode.SelectSingleNode("ItemRating");
									string rating = detailNode == null ? "0" : detailNode.InnerText;
									ratingDic.Add(tuple.Item2, rating);
								}
							}
							else
							{
								XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/" + tuple.Item3);
								string rating = ratingNode == null ? "0" : ratingNode.InnerText;
								ratingDic.Add(tuple.Item2, rating);
							}
						}
						_koubeiRatingDic.Add(csID, ratingDic);
					}
				}
				catch (Exception ex)
				{
					Log.WriteErrorLog(string.Format("竞品口碑排行异常，serialId={0},\r\n{1}", csID, ex.ToString()));
				}
			}
		}

		private DataRow GetKoubeiTuijianBySerialId(int serialId)
		{
			string sqlStr = @"select NewsId,Title,ShortTitle,url FilePath,Summary
								from car_newsv2
								where newsid in(
								select top 1 newsId
								from car_serialnewsv2
								where categoryid=588 and serialid=@serialId and CopyRight = 0
								order by publishTime desc
								)";
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString
				, CommandType.Text, sqlStr, new SqlParameter[] { new SqlParameter("@serialId", serialId) });
			if (ds == null || ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
				return null;
			return ds.Tables[0].Rows[0];

		}
		/*
        /// <summary>
        /// 初始化推荐的口碑文章
        /// </summary>
        private void InitKoubeiTuijianDic()
        {
            var sqlStr = @"
SELECT t.SerialId,t.CarNewsId,t.CarNewsTypeId,t.Title,t.FilePath,t.PublishTime,n.Summary,n.Content FROM (
SELECT T1.* FROM SerialNews T1 WHERE T1.Id IN 
(SELECT TOP 1 T2.id FROM dbo.SerialNews T2  where  T2.SerialId =T1.SerialId
and CarNewsTypeId=17 ORDER BY T1.PublishTime DESC)) AS t LEFT JOIN dbo.News n ON t.CmsNewsId = n.CmsNewsId
";
            var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString
                , CommandType.Text, sqlStr);
            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    var csId = Int32.Parse(dataRow["SerialId"].ToString());
                    if (!_koubeiTuijianDic.ContainsKey(csId))
                    {
                        _koubeiTuijianDic.Add(csId, dataRow);
                    }

                }
            }
        }
		*/
        public override void BuilderDataOrHtml(int objId)
        {
            SerialId = objId;
            try
            {
                _serialInfo = CommonData.SerialDic[objId];
                if (_serialInfo == null) return;
                //xmlKoubei = this.GetKoubeiXml();
				drTuiJianKoubei = GetKoubeiTuijianBySerialId(SerialId);

                //xmlRating = null;
                xmlKouBeiReport = null;
                xmlYoudian = null;
                xmlQuedian = null;
                youdianNodes = null;
                quedianNodes = null;

                //xmlRating = this.GetTrimRating(objId);
                xmlKouBeiReport = this.GetTopicList(objId);
                xmlYoudian = this.GetImpressionList(2, objId);
                xmlQuedian = this.GetImpressionList(0, objId);
                if (xmlYoudian != null)
                {
                    youdianNodes = xmlYoudian.SelectNodes("ImpressionList/Item");
                }
                if (xmlQuedian != null)
                {
                    quedianNodes = xmlQuedian.SelectNodes("ImpressionList/Item");
                }

                // 更新口碑基础数据到数据库 用于排序 [AutoCarChannel].[dbo].[Car_CsKouBeiBaseInfo],在方法UpdateKoubeiRaingDetail里统一更新
                //UpdateCsKouBeiBaseInfo();
				
				//更新口碑印象块
				//RenderKoubeiImpressionHtml();
				//更新口碑内容块
				//RenderKoubeiHtml();
				//更新H5口碑内容快
				RenderFourthStageKoubeiHtml();
				//更新移动站口碑块内容
				//RenderWirelessKoubeiHtml();
				//更新移动站口碑块内容-增加一篇口碑
				RenderWirelessKoubeiHtmlV2(_serialInfo.AllSpell);

				//H5 V3
				H5KoubeiV3();

				//更新车型详解页面推荐口碑报告块
				//RenderKoubeiTuijianHtml();//弃用 2017-01-04 
				RenderKoubeiTuijianHtmlV2();

				//口碑排行块
				//RenderKoubeiRatingHtml();//弃用 2017-01-04 
				RenderKoubeiRatingHtmlV2();

				//口碑对比--网友点评
				RenderKoubeiDuiBiHtml();
				//口碑报告
				RendSerialKoubeiRatingHtml();
				//网友口碑
				RenderKoubeiHtmlNew();
				//车型详解页面，竞品车型
				SerialCompetitiveKoubeiHtmlBuilder koubei = new SerialCompetitiveKoubeiHtmlBuilder(_koubeiRatingDic);
				koubei.BuilderDataOrHtml(_serialInfo.Id);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("生成口碑块异常，serialId=" + SerialId + "," + ex.ToString());
            }
        }

        /*
        /// <summary>
        /// add by chengl Oct.21.2015
        /// 更新口碑基础数据到数据库 用于排序 [AutoCarChannel].[dbo].[Car_CsKouBeiBaseInfo]
        /// 车型综合评分\同级别车型评分\同级别高低偏移值，正数为高于，负数为低于\车型口碑总数
        /// </summary>
        private void UpdateCsKouBeiBaseInfo()
        {
            try
            {
                decimal Rating = 0;
                decimal LevelRating = 0;
                decimal RatingVariance = 0;
                int TotalCount = 0;
                if (xmlRating != null)
                {
                    Rating = ConvertHelper.GetDecimal(xmlRating.SelectSingleNode("Serial/Item/Rating").InnerText);
                    LevelRating = ConvertHelper.GetDecimal(xmlRating.SelectSingleNode("Serial/Item/LevelRating").InnerText);
                    RatingVariance = ConvertHelper.GetDecimal(xmlRating.SelectSingleNode("Serial/Item/RatingVariance").InnerText);
                    TotalCount = ConvertHelper.GetInteger(xmlRating.SelectSingleNode("Serial/Item/TotalCount").InnerText);
                }
                if (SerialId > 0)
                {
                    string sqlProcedure = "SP_UpdateCsKouBeiBaseInfo";
                    SqlParameter[] param = new SqlParameter[] { 
                    new SqlParameter("@CsID", SqlDbType.Int),
                    new SqlParameter("@Rating", SqlDbType.Decimal),
                    new SqlParameter("@LevelRating", SqlDbType.Decimal),
                    new SqlParameter("@RatingVariance", SqlDbType.Decimal),
                    new SqlParameter("@TotalCount", SqlDbType.Int)
                };
                    param[0].Value = SerialId;
                    param[1].Value = Rating;
                    param[2].Value = LevelRating;
                    param[3].Value = RatingVariance;
                    param[4].Value = TotalCount;
                    int res = BitAuto.Utils.Data.SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString
                        , CommandType.StoredProcedure, sqlProcedure, param);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }*/

        /// <summary>
        /// 生成移动站口碑块内容
        /// </summary>
        private void RenderWirelessKoubeiHtml()
        {
            try
            {
                if ((youdianNodes != null && youdianNodes.Count >= 0) ||
                    (quedianNodes != null && quedianNodes.Count >= 0))
                {
                    double AverageRating = 0;
                    if (xmlRating != null)
                    {
                        XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
                        var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
                        var rating = Double.Parse(ratingStr);
                        AverageRating = Math.Round(rating, 1);
                    }
                    double sum = (ConvertHelper.GetDouble(AverageRating)/5)*100;

                    XmlNode koubeiReportRoot = null;
                    int TopicCount = 0;
                    if (xmlKouBeiReport != null)
                    {
                        koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                        TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<div class=\"kb-box\" id='m_kb_box' data-channelid=\"27.23.731\">");
                    sb.Append("<div class=\"kb-rank\">");
                    sb.AppendFormat("<span class=\"big-star\"><em style=\"width:{0}%\"></em></span>", sum);
                    sb.AppendFormat("<strong>{0}分</strong>", AverageRating);
                    sb.Append("</div>");
                    sb.AppendFormat("<div class=\"kb-ranknum\">共{0}条口碑</div>", TopicCount);
                    sb.Append("<div class=\"clear\"></div>");
                    sb.Append("<div class=\"kb-scroll\">");
                    sb.Append("<span>优点：</span>");
                    sb.Append("<ul>");
                    if (youdianNodes != null)
                    {
                        foreach (XmlNode node in youdianNodes)
                        {
                            var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                            var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                            var Url = node.SelectSingleNode("./mUrl").InnerText;
                            sb.AppendFormat("<li><a href=\"{0}\"><em>{1}({2})</em></a></li>",
                                Url,
                                KeyWord,
                                Vote,
                                System.Web.HttpUtility.UrlEncode(KeyWord));
                        }
                    }
                    sb.Append("</ul>");
                    sb.Append("</div>");
                    sb.Append("<div class=\"kb-scroll kb-scroll-bad\">");
                    sb.Append("<span>缺点：</span>");
                    sb.Append("<ul>");
                    if (quedianNodes != null)
                    {
                        foreach (XmlNode node in quedianNodes)
                        {
                            var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                            var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                            var Url = node.SelectSingleNode("./mUrl").InnerText;
                            sb.AppendFormat("<li><a href=\"{0}\"><em>{1}({2})</em></a></li>",
                                Url,
                                KeyWord,
                                Vote,
                                System.Web.HttpUtility.UrlEncode(KeyWord));
                        }
                    }
                    sb.Append("</ul>");
                    sb.Append("</div>");
                    sb.Append("</div>");

                    bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
                    {
                        ID = SerialId,
                        TypeID = CommonHtmlEnum.TypeEnum.Serial,
                        TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialSummary,
                        BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReportNew,
                        HtmlContent = sb.ToString(),
                        UpdateTime = DateTime.Now
                    });
                    if (!success) Log.WriteErrorLog("更新Wireless口碑印象失败：serialId:" + SerialId);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        ///  生成移动站口碑块内容V2---原口碑优缺点+添加一篇口碑贴
        /// </summary>
        //private void RenderWirelessKoubeiHtmlV2(string curSerialAllSpell)
        //{
        //    try
        //    {
        //        if ((youdianNodes != null && youdianNodes.Count >= 0) || (quedianNodes != null && quedianNodes.Count >= 0))
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            sb.Append("<div class=\"tt-first tt-first-no-bd\" id=\"koubei\" data-channelid=\"27.23.730\">");
        //            sb.Append("<h3>车主口碑</h3>");
        //            sb.Append(string.Format("<div class=\"opt-more\"><a href=\"http://car.m.yiche.com/{0}/koubei/\">更多</a></div>", curSerialAllSpell));
        //            sb.Append("</div>");

        //            double AverageRating = 0;
        //            if (xmlRating != null)
        //            {
        //                XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
        //                var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
        //                var rating = Double.Parse(ratingStr);
        //                AverageRating = Math.Round(rating, 1);
        //            }
        //            double sum = (ConvertHelper.GetDouble(AverageRating) / 5) * 100;

        //            XmlNode koubeiReportRoot = null;
        //            int TopicCount = 0;
        //            if (xmlKouBeiReport != null)
        //            {
        //                koubeiReportRoot = xmlKouBeiReport.DocumentElement;
        //                TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
        //            }

        //            sb.Append("<div class=\"kb-box\" id='m_kb_box' data-channelid=\"27.23.731\">");
        //            sb.Append("<div class=\"kb-rank\">");
        //            sb.AppendFormat("<span class=\"big-star\"><em style=\"width:{0}%\"></em></span>", sum);
        //            sb.AppendFormat("<strong>{0}分</strong>", AverageRating);
        //            sb.Append("</div>");
        //            sb.AppendFormat("<div class=\"kb-ranknum\">共{0}条口碑</div>", TopicCount);
        //            sb.Append("<div class=\"clear\"></div>");
        //            sb.Append("<div class=\"kb-scroll\">");
        //            sb.Append("<ul>");
        //            if (youdianNodes != null)
        //            {
        //                foreach (XmlNode node in youdianNodes)
        //                {
        //                    var Vote = node.SelectSingleNode("./VoteCount").InnerText;
        //                    var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
        //                    var Url = node.SelectSingleNode("./mUrl").InnerText;
        //                    sb.AppendFormat("<li><a href=\"{0}\"><em>{1}({2})</em></a></li>",
        //                        Url,
        //                        KeyWord,
        //                        Vote,
        //                        System.Web.HttpUtility.UrlEncode(KeyWord));
        //                }
        //            }
        //            else
        //            {
        //                sb.Append("<li><em>暂无优点</em></li>");
        //            }
        //            sb.Append("</ul>");
        //            sb.Append("</div>");
        //            sb.Append("<div class=\"kb-scroll kb-scroll-bad\">");
        //            sb.Append("<ul>");
        //            if (quedianNodes != null)
        //            {
        //                foreach (XmlNode node in quedianNodes)
        //                {
        //                    var Vote = node.SelectSingleNode("./VoteCount").InnerText;
        //                    var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
        //                    var Url = node.SelectSingleNode("./mUrl").InnerText;
        //                    sb.AppendFormat("<li><a href=\"{0}\"><em>{1}({2})</em></a></li>",
        //                        Url,
        //                        KeyWord,
        //                        Vote,
        //                        System.Web.HttpUtility.UrlEncode(KeyWord));
        //                }
        //            }
        //            else
        //            {
        //                sb.Append("<li><em>暂无缺点</em></li>");
        //            }
        //            sb.Append("</ul>");
        //            sb.Append("</div>");
        //            sb.Append("</div>");

        //            var str = GetSummaryFirstKoubeiHtmlForWap();
        //            var koubeiResult = sb.ToString() + str.ToString();
        //            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
        //            {
        //                ID = SerialId,
        //                TypeID = CommonHtmlEnum.TypeEnum.Serial,
        //                TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialSummaryV2,
        //                BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReportNew,
        //                HtmlContent = koubeiResult,
        //                UpdateTime = DateTime.Now
        //            });
        //            if (!success) Log.WriteErrorLog("更新Wireless口碑印象失败：serialId:" + SerialId);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteErrorLog(ex.ToString());
        //    }
        //}
        //修改口碑优缺点
        private void RenderWirelessKoubeiHtmlV2(string curSerialAllSpell)
        {
            try
            {
                if ((youdianNodes != null && youdianNodes.Count >= 0) ||
                    (quedianNodes != null && quedianNodes.Count >= 0))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<div class=\"tt-first\" id=\"koubei\" data-channelid=\"27.23.730\">");
                    sb.Append("<h3>车主口碑</h3>");
                    sb.Append(
                        string.Format(
                            "<div class=\"opt-more\"><a href=\"http://car.m.yiche.com/{0}/koubei/\">更多</a></div>",
                            curSerialAllSpell));
                    sb.Append("</div>");

					double AverageRating = 0; //
					int TopicCount = 0;
					if(_koubeiRatingDic.ContainsKey(SerialId))
					{
						AverageRating = Math.Round(ConvertHelper.GetDouble(_koubeiRatingDic[SerialId]["Ratings"]), 1);
						TopicCount = ConvertHelper.GetInteger(_koubeiRatingDic[SerialId]["TopicCount"]);
					}
					//if (xmlRating != null)
					//{
					//	var ratingStr = xmlRating.SelectSingleNode("Serial/Item/Rating").InnerText;
					//	var rating = Double.Parse(ratingStr);
					//	AverageRating = Math.Round(rating, 1);
					//}
                    double sum = (ConvertHelper.GetDouble(AverageRating)/5)*100;

					//XmlNode koubeiReportRoot = null;
					//int TopicCount = 0;
					//if (xmlKouBeiReport != null)
					//{
					//	koubeiReportRoot = xmlKouBeiReport.DocumentElement;
					//	TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
					//}

                    sb.Append("<div class=\"kb-ball\" id='m_kb_box' data-channelid=\"27.23.731\">");
                    sb.Append(string.Format(
                        "<a href=\"http://car.m.yiche.com/{0}/koubei/\"><div class=\"circle-ball\">", curSerialAllSpell));
                    sb.Append("<h5>车主口碑</h5>");
                    sb.Append(string.Format("<strong>{0}分</strong>", AverageRating));
                    sb.Append(string.Format("<span class=\"big-star\"><em style=\"width:{0}%\"></em></span>", sum));
                    sb.Append("</div></a>");

                    int[] randomArr = GetRandomArray(8, 1, 8);
                    if (youdianNodes != null)
                    {
                        int youdianAcc = 0;
                        foreach (XmlNode node in youdianNodes)
                        {
                            if (youdianAcc < 4)
                            {
                                var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                                var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                                var Url = node.SelectSingleNode("./mUrl").InnerText;
                                sb.AppendFormat("<a href=\"{0}\"><div class=\"ball-item ball{1}\">{2}</div></a>",
                                    Url,
                                    randomArr[youdianAcc],
                                    KeyWord);
                            }
                            youdianAcc++;
                        }
                    }
                    if (quedianNodes != null)
                    {
                        int quedianAcc = 4;
                        foreach (XmlNode node in quedianNodes)
                        {
                            if (quedianAcc < 8)
                            {
                                var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                                var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                                var Url = node.SelectSingleNode("./mUrl").InnerText;
                                sb.AppendFormat("<a href=\"{0}\"><div class=\"ball-item ball{1}\">{2}</div></a>",
                                    Url,
                                    randomArr[quedianAcc],
                                    KeyWord);
                            }
                            quedianAcc++;
                        }
                    }
                    sb.Append("</div>");
                    var str = GetSummaryFirstKoubeiHtmlForWap();
                    var koubeiResult = sb.ToString() + str.ToString();
                    bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
                    {
                        ID = SerialId,
                        TypeID = CommonHtmlEnum.TypeEnum.Serial,
                        TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialSummaryV2,
                        BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReportNew,
                        HtmlContent = koubeiResult,
                        UpdateTime = DateTime.Now
                    });
                    if (!success) Log.WriteErrorLog("更新Wireless口碑印象失败：serialId:" + SerialId);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        /// 产生一组指定范围类不重复的随机数
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="minNum"></param>
        /// <param name="maxNum"></param>
        /// <returns></returns>
        public int[] GetRandomArray(int Number, int minNum, int maxNum)
        {
            int j;
            int[] b = new int[Number];
            Random r = new Random();
            for (j = 0; j < Number; j++)
            {
                int i = r.Next(minNum, maxNum + 1);
                int num = 0;
                for (int k = 0; k < j; k++)
                {
                    if (b[k] == i)
                    {
                        num = num + 1;
                    }
                }
                if (num == 0)
                {
                    b[j] = i;
                }
                else
                {
                    j = j - 1;
                }
            }
            return b;
        }

        private XmlDocument GetKoubeiXml()
        {
            string filePath = string.Format(CommonData.CommonSettings.SerialKouBeiReportUrlNew, SerialId);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            return xmlDoc;
        }

        public XmlDocument GetCsReport(int serialId)
        {
            string result = reportService.GetReportBySerialId(serialId);
            if (string.IsNullOrEmpty(result)) return null;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            return xmlDoc;
        }

		//private XmlDocument GetTrimRating(int serialId)
		//{
		//	string result = carService.GetTrimRating(serialId);
		//	if (string.IsNullOrEmpty(result)) return null;
		//	XmlDocument xmlDoc = new XmlDocument();
		//	xmlDoc.LoadXml(result);
		//	return xmlDoc;
		//}

        private XmlDocument GetTopicList(int serialId)
        {
            long count;
            var result = topicService.GetTopicList(serialId, 0, 1, 5, out count);
            if (string.IsNullOrEmpty(result)) return null;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            return xmlDoc;
        }

        private XmlDocument GetImpressionList(int yuYi, int serialId)
        {
            long count;
            var result = impressionService.GetImpressionListBySerialId(serialId, yuYi, 1, 6, out count);
            if (serialId == 4447)
            {
                Log.WriteLog("serialid=4447,yuyi=" + yuYi + ",result=" + result);
            }
            if (string.IsNullOrEmpty(result)) return null;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            return xmlDoc;
        }

        /// <summary>
        /// 子品牌综述页网友口碑 1200版 lisf 2016-09-27
        /// </summary>
        private void RenderKoubeiHtmlNew()
        {
            StringBuilder sbHtml = new StringBuilder();
            var TopicCount = 0;
            XmlNode koubeiReportRoot = null;
            if (xmlKouBeiReport != null)
            {
                koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
            }

            if (xmlKouBeiReport == null || koubeiReportRoot == null || TopicCount <= 0)
            {
                bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                    CommonHtmlEnum.BlockIdEnum.KoubeiReport);
                return;
            }
            double AverageRating = 0;
			if (_koubeiRatingDic.ContainsKey(SerialId))
			{
				AverageRating = Math.Round(ConvertHelper.GetDouble(_koubeiRatingDic[SerialId]["Ratings"]), 1);
			}
			//if (xmlRating != null)
			//{
			//	XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
			//	var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
			//	var rating = Double.Parse(ratingStr);
			//	AverageRating = Math.Round(rating, 1);
			//}
            sbHtml.Append("<div class=\"layout-2 koubei-section\">");

            #region 头部

			sbHtml.Append("<div class=\"section-header header2 mb0\"  data-channelid=\"2.21.817\">");
            sbHtml.Append("<div class=\"box\">");
			sbHtml.AppendFormat("<h2><a href=\"http://car.bitauto.com/{1}/koubei/gengduo/\" target=\"_blank\">{0}网友口碑</a></h2>", _serialInfo.ShowName,_serialInfo.AllSpell);
            sbHtml.Append("</div>");
            sbHtml.Append("<div class=\"more\">");
			sbHtml.Append("<a href=\"http://koubei.bitauto.com/subject/s1.html\" target=\"_blank\">口碑小妹免费送大奖</a>");
            sbHtml.AppendFormat(
				"<a href=\"http://car.bitauto.com/{0}/koubei/提车/\" target=\"_blank\">提车</a><a href=\"http://car.bitauto.com/{0}/koubei/保养/\" target=\"_blank\">保养</a>",
                _serialInfo.AllSpell);
            sbHtml.AppendFormat(
				"<a href=\"http://car.bitauto.com/{0}/koubei/维修/\" target=\"_blank\">维修</a><a href=\"http://car.bitauto.com/{0}/koubei/改装/\" target=\"_blank\">改装</a>",
                _serialInfo.AllSpell);
            sbHtml.AppendFormat(
                "<a href=\"http://car.bitauto.com/{0}/koubei/事故/\" target=\"_blank\">事故</a><a href=\"http://car.bitauto.com/{0}/koubei/\" target=\"_blank\">全部口碑&gt;&gt;</a>",
                _serialInfo.AllSpell);
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");

            sbHtml.Append("<div class=\"special-layout-9\">");
            sbHtml.Append("<div class=\"left\">");
            sbHtml.AppendFormat("<div class=\"special-layout-16\" id=\"circleProgress-report\" value=\"{0}\">",
                Utils.ConvertHelper.GetDouble(AverageRating/5));
            sbHtml.Append("<div class=\"center-info\">");
            sbHtml.AppendFormat("<span>综合评分</span><p>{0}</p>", AverageRating);
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");

            #endregion

            #region 优缺点

            sbHtml.Append("<div class=\"main\" data-channelid=\"2.21.708\">");
            sbHtml.AppendFormat("<div class=\"title\">基于{0}篇网友口碑</div>", TopicCount);

            sbHtml.Append("<dl class=\"row\">");

            sbHtml.Append("<dt class=\"col-auto\">优点：</dt>");
            sbHtml.Append("<dd class=\"col-auto\">");
            sbHtml.Append("<ul class=\"list\">");
            if (youdianNodes != null && youdianNodes.Count >= 0)
            {
                int count = 0;
                foreach (XmlNode node in youdianNodes)
                {
                    if (++count > 5) break;
                    var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                    sbHtml.AppendFormat(
                        "<li><a target=\"_blank\" href=\"http://car.bitauto.com/{0}/koubei/word/{1}/\">{2}</a></li>"
                        , _serialInfo.AllSpell
                        , System.Web.HttpUtility.UrlEncode(KeyWord)
                        , KeyWord
                        );
                }
            }
            else
            {
                sbHtml.AppendFormat("<li>暂无</li>");
            }
            sbHtml.Append("</ul></dd></dl>");
            sbHtml.Append("<dl class=\"row defect\">");
            sbHtml.Append("<dt class=\"col-auto\">缺点：</dt>");
            sbHtml.Append("<dd class=\"col-auto\">");
            sbHtml.Append("<ul class=\"list\">");
            if (quedianNodes != null && quedianNodes.Count >= 0)
            {
                int count = 0;
                foreach (XmlNode node in quedianNodes)
                {
                    if (++count > 5) break;
                    var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                    sbHtml.AppendFormat(
                        "<li><a target=\"_blank\" href=\"http://car.bitauto.com/{0}/koubei/word/{1}/\">{2}</a></li>"
                        , _serialInfo.AllSpell
                        , System.Web.HttpUtility.UrlEncode(KeyWord)
                        , KeyWord
                        );
                }
            }
            else
            {
                sbHtml.AppendFormat("<li>暂无</li>");
            }
            sbHtml.Append("</ul></dd></dl>");

            sbHtml.Append("</div>");
            sbHtml.Append("<div class=\"right1\">");
			sbHtml.AppendFormat("<a href=\"http://car.bitauto.com/{0}/koubei/posttopic.html\" class=\"btn btn-primary\" target=\"_blank\" data-channelid=\"2.21.1528\">发布口碑</a>", _serialInfo.AllSpell);
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");

            #endregion

            #region 网友点评

            List<string> topicIdList = new List<string>();
            XmlNodeList nodeTopic = koubeiReportRoot.SelectNodes("./Item");
            int topicLoop = 0;
            int lastKoubeiId = 0;
            sbHtml.Append("<div class=\"row\">");
            foreach (XmlNode node in nodeTopic)
            {
                topicLoop++;
                if (topicLoop > 3) break;
                var ID = node.SelectSingleNode("./TopicId").InnerText;
                var Title = node.SelectSingleNode("./Title").InnerText;
                var Contents = node.SelectSingleNode("./Content").InnerText;
                var CreateTime = ConvertHelper.GetDateTime(node.SelectSingleNode("./CreateTime").InnerText);
                var UserId = ConvertHelper.GetInteger(node.SelectSingleNode("./UserId").InnerText);
                var UserType = ConvertHelper.GetInteger(node.SelectSingleNode("./UserType").InnerText);
                var UserName = node.SelectSingleNode("./UserName").InnerText;
                var PurchaseDate = node.SelectSingleNode("./PurchaseDate").InnerText; //购车时间
                var PurchasePrice = node.SelectSingleNode("./PurchasePrice").InnerText; //裸车价格
                var ProvinceName = node.SelectSingleNode("./ProvinceName").InnerText;
                var CityName = node.SelectSingleNode("./CityName").InnerText;
                var Fuel = ConvertHelper.GetDouble(node.SelectSingleNode("./Fuel").InnerText);
				var carName = node.SelectSingleNode("./TrimName").InnerText;
                if (Fuel > 0) Fuel = Math.Round(Fuel, 2);
                var DealerName = node.SelectSingleNode("./DealerName").InnerText;
                topicIdList.Add(ID);
				lastKoubeiId = ConvertHelper.GetInteger(ID);
				string koubeiDetailUrl = string.Format("http://car.bitauto.com/{0}/koubei/{1}/", _serialInfo.AllSpell, ID);
                string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                string userUrl = "#";
                if (UserType == 0 && UserId > 0)
                {
                    userImage = GetUserImage(UserId, out UserName);
                    userUrl = "http://i.qichetong.com/u" + UserId + "/";
                }

                sbHtml.Append(" <div class=\"special-layout-10\" data-channelid=\"2.21.818\">");
                sbHtml.Append("     <div class=\"left-box\">");
                if (UserType == 0 && UserId > 0)
                {
                    sbHtml.AppendFormat("<a class=\"figure\" href=\"{0}\" target=\"_blank\">", userUrl);
                    sbHtml.Append("<span class=\"img\">");
                    sbHtml.AppendFormat("<img src=\"{0}\">", userImage);
                    sbHtml.Append("</span>");
                    sbHtml.Append("</a>");
                }
                else
                {
                    UserName = "易车网友";
                    sbHtml.Append("<a class=\"figure\" href=\"javascript:;\">");
                    sbHtml.Append("<span class=\"img\">");
                    sbHtml.AppendFormat("<img src=\"{0}\" alt=\"\">", userImage);
                    sbHtml.Append("</span>");
                    sbHtml.Append("</a>");
                }
                sbHtml.Append("</div>");
                sbHtml.Append("<div class=\"right-box\">");
                DateTime purchaseDateTemp = ConvertHelper.GetDateTime(PurchaseDate);
				sbHtml.AppendFormat("<h5 class=\"title\">{0}<span>{1} {5}{2}{3} {4}&nbsp;&nbsp;{6}</span></h5>"
                    , UserName
                    ,
                    purchaseDateTemp > DateTime.Parse("1900-01-01")
                        ? purchaseDateTemp.ToString("yyyy年MM月")
                        : string.Empty
                    , ProvinceName
                    , CityName
                    , DealerName
					, string.IsNullOrWhiteSpace(ProvinceName) || string.IsNullOrWhiteSpace(CityName) || string.IsNullOrWhiteSpace(DealerName) ? string.Empty : "购于"
					, carName);
                sbHtml.AppendFormat(
					"<p class=\"details\"><a href=\"{1}\" target=\"_blank\">{0}</a><a href=\"{1}\" target=\"_blank\" class=\"more\">详细>></a></p>"
                    ,
                    StringHelper.GetRealLength(Contents) > 200 ? StringHelper.SubString(Contents, 200, true) : Contents
                    , koubeiDetailUrl);
                sbHtml.Append("<div class=\"info\">");
                sbHtml.AppendFormat("<div class=\"price\">裸车价：{0}</div>",
                    ConvertHelper.GetDouble(PurchasePrice) > 0 ? (string.Format("<a href=\"{0}\" class=\"icolor\" target=\"_blank\">{1}</a>", koubeiDetailUrl, PurchasePrice + "万")) : "暂无");
                sbHtml.AppendFormat("<div class=\"oil\">油 耗：{0}</div>",
                    ConvertHelper.GetDouble(Fuel) > 0 ? (string.Format("<a href=\"{0}\" class=\"icolor\" target=\"_blank\">{1}</a>", koubeiDetailUrl,Fuel + "L")) : "暂无");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
            }
            if (TopicCount > 3)
            {
                sbHtml.AppendFormat(
                    "<div class=\"btn-box1\"><a class=\"btn btn-default\" target=\"_blank\" href=\"http://car.bitauto.com/{0}/koubei/gengduo/{1}\" data-channelid=\"2.21.1529\"><span class=\"more\">更多口碑</span></a></div>"
                    , _serialInfo.AllSpell, lastKoubeiId > 0 ? "#" + lastKoubeiId : "");
            }
            sbHtml.Append("</div>");

            #endregion

            sbHtml.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReport,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新新版口碑印象失败：serialId:" + SerialId);

            //更新购车服务 口碑块内容
            GenerateKoubeiXml();
        }

        private void RenderKoubeiHtml()
        {
            StringBuilder sbHtml = new StringBuilder();
            var TopicCount = 0;
            XmlNode koubeiReportRoot = null;
            if (xmlKouBeiReport != null)
            {
                koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
            }

            if (xmlKouBeiReport == null || koubeiReportRoot == null || TopicCount <= 0)
            {
                bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialSummary,
                    CommonHtmlEnum.BlockIdEnum.KoubeiReportNew);
                return;
            }
            double AverageRating = 0;
            if (xmlRating != null)
            {
                XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
                var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
                var rating = Double.Parse(ratingStr);
                AverageRating = Math.Round(rating, 1);
            }
            //XmlNode nodeImpressions = root.SelectSingleNode("./Impressions");
            //var VoteCount = nodeImpressions.Attributes["VoteCount"].Value;
            //XmlNodeList nodeGood = nodeImpressions.SelectNodes("./Good/Impression");
            //XmlNodeList nodeBad = nodeImpressions.SelectNodes("./Bad/Impression");

            sbHtml.Append("<div class=\"line-box  choice\">");
            sbHtml.Append("<div class=\"title-box\">");
            sbHtml.AppendFormat("<h3><a href=\"/{0}/koubei/gengduo/\" target=\"_blank\">{1}网友口碑</a></h3>",
                _serialInfo.AllSpell, _serialInfo.SeoName);
            sbHtml.AppendFormat(
                "<div class=\"more\"><a href=\"http://koubei.bitauto.com/subject/s1.html\" target=\"_blank\">“易”口碑活动招募&gt;&gt;</a> | <a href=\"/{1}/koubei/gengduo/\" target=\"_blank\">共{0}篇口碑&gt;&gt;</a> </div>",
                TopicCount, _serialInfo.AllSpell);
            sbHtml.AppendFormat(
                "<div class=\"more\"><a href=\"/{1}/koubei/gengduo/\" target=\"_blank\">共{0}篇口碑&gt;&gt;</a> </div>",
                TopicCount, _serialInfo.AllSpell);
            sbHtml.Append("</div>");

            sbHtml.Append("<div class=\"card-head-box zs_koubei_card\" data-channelid=\"2.21.708\">");
            //sbHtml.Append("<div class=\"koubei_grade\">");
            //sbHtml.Append("<div class=\"star\">");
            double sum = (ConvertHelper.GetDouble(AverageRating)/5)*118;
            sbHtml.AppendFormat(
                "<div class=\"num_box\"><div class=\"big_start_box\"><span class=\"big_start\"><em style=\"width:{0}px\"></em></span><strong>{1}分</strong><p>（口碑总数{2}）</p></div></div>",
                sum, AverageRating, TopicCount);
            //sbHtml.AppendFormat("<div class=\"star_num\">{0}分</div>", AverageRating);
            //sbHtml.Append("</div>");
            //sbHtml.AppendFormat("<strong>综合评分<em>（基于{0}篇口碑）</em></strong>", TopicCount);
            //sbHtml.Append("</div>");

            // modified by chengl 2015.5.29 只要有1条就显示 白伟&侯姐确认
            // if (youdianNodes != null && youdianNodes.Count >= 6 && quedianNodes != null && quedianNodes.Count >= 6)
            if ((youdianNodes != null && youdianNodes.Count >= 0) || (quedianNodes != null && quedianNodes.Count >= 0))
            {
                sbHtml.Append("<div class=\"txt-box kb-card\">");
                sbHtml.Append("<ul><li><span class=\"tit-span tit-yd\">优点：</span><div class=\"kb-nr-box\">");
                if (youdianNodes != null && youdianNodes.Count >= 0)
                {
                    foreach (XmlNode node in youdianNodes)
                    {
                        var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                        var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                        sbHtml.AppendFormat(
                            "<div class=\"kb-xx\"><a target=\"_blank\" href=\"http://car.bitauto.com/{3}/koubei/word/{2}/\"><p class=\"p1\">{1}</p><p class=\"p2\">{0}</p></a></div>"
                            , Vote
                            , KeyWord
                            , System.Web.HttpUtility.UrlEncode(KeyWord)
                            , _serialInfo.AllSpell);
                    }
                }
                sbHtml.Append("</div></li>");
                sbHtml.Append("<li><span class=\"tit-span\">缺点：</span><div class=\"kb-nr-box\">");
                if (quedianNodes != null && quedianNodes.Count >= 0)
                {
                    foreach (XmlNode node in quedianNodes)
                    {
                        var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                        var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                        sbHtml.AppendFormat(
                            "<div class=\"kb-xx kb-qd-sty\"><a target=\"_blank\" href=\"http://car.bitauto.com/{3}/koubei/word/{2}/\"><p class=\"p1\">{1}</p><p class=\"p2\">{0}</p></a></div>"
                            , Vote
                            , KeyWord
                            , System.Web.HttpUtility.UrlEncode(KeyWord)
                            , _serialInfo.AllSpell);
                    }
                }
                sbHtml.Append("</div></li>");
                sbHtml.Append("</ul></div>");
            }
            else
            {
                XmlDocument xmlImpression = new XmlDocument();
                string filePath =
                    String.Format(
                        Path.Combine(CommonData.CommonSettings.SavePath,
                            @"SerialDianping\ImpressionNew\Xml\Impression_{0}.xml"), SerialId);
                if (File.Exists(filePath))
                {
                    xmlImpression.Load(filePath);
                    string virtues = string.Empty;
                    string defect = string.Empty;
                    var items = xmlImpression.SelectNodes("/ReportList/Item/Detail/Item");
                    if (items != null)
                    {
                        foreach (XmlNode itemNode in items)
                        {
                            var usNameNode = itemNode.SelectSingleNode("UsName");
                            if (usNameNode != null)
                            {
                                var usName = usNameNode.InnerText.Trim();
                                if (usName == "strengths")
                                {
                                    var contentNode = itemNode.SelectSingleNode("Content");
                                    if (contentNode != null)
                                    {
                                        virtues = contentNode.InnerText.Trim();
                                    }
                                }
                                if (usName == "weaknesses")
                                {
                                    var contentNode = itemNode.SelectSingleNode("Content");
                                    if (contentNode != null)
                                    {
                                        defect = contentNode.InnerText.Trim();
                                    }
                                }
                            }
                        }
                    }

                    virtues = StringHelper.RemoveHtmlTag(virtues);
                    defect = StringHelper.RemoveHtmlTag(defect);

                    sbHtml.Append("<div class=\"txt-box kb-card\">");
                    sbHtml.Append("<ul>");
                    sbHtml.AppendFormat(
                        "<li><span class=\"tit-span tit-yd\">优点：</span><div class=\"kb-nr-box\"><p title=\"{1}\">{0}</p></div></li>",
                        StringHelper.GetRealLength(virtues) > 68 ? StringHelper.SubString(virtues, 68, true) : virtues,
                        virtues);
                    sbHtml.AppendFormat(
                        "<li><span class=\"tit-span\">缺点：</span><div class=\"kb-nr-box\"><p title=\"{1}\">{0}</p></div></li>",
                        StringHelper.GetRealLength(defect) > 68 ? StringHelper.SubString(defect, 68, true) : defect,
                        defect);
                    sbHtml.Append("</ul>");
                    sbHtml.Append("</div>");
                }
            }
            sbHtml.Append("<div class=\"clear\"></div>");
            sbHtml.Append("</div>");
            //推荐口碑
            var str = GetSummaryKoubeiTuijianHtml();
            sbHtml.Append(str);
            //推荐口碑
            sbHtml.Append("<div class=\"line-box\">");
            sbHtml.Append("<div class=\"title-con\">");
            sbHtml.Append("<div class=\"title-box title-box2\" data-channelid=\"2.21.817\">");
            sbHtml.AppendFormat("<h4><a href=\"/{0}/koubei/gengduo/\" target=\"_blank\">网友口碑</a></h4>",
                _serialInfo.AllSpell);
            sbHtml.Append("<ul class=\"title-tab\">");
            sbHtml.AppendFormat(
                "<li class=\"current\"><a href=\"/{0}/koubei/gengduo/\" target=\"_blank\">全部</a><em>|</em></li>",
                _serialInfo.AllSpell);
            sbHtml.AppendFormat("<li><a href=\"/{0}/koubei/tags/外观/\" target=\"_blank\">外观</a><em>|</em></li>",
                _serialInfo.AllSpell);
            sbHtml.AppendFormat(
                "<li class=\"\"><a href=\"/{0}/koubei/tags/内饰/\" target=\"_blank\">内饰</a><em>|</em></li>",
                _serialInfo.AllSpell);
            sbHtml.AppendFormat("<li><a href=\"/{0}/koubei/tags/空间/\" target=\"_blank\">空间</a><em>|</em></li>",
                _serialInfo.AllSpell);
            sbHtml.AppendFormat(
                "<li class=\"\"><a href=\"/{0}/koubei/tags/操控/\" target=\"_blank\">操控</a><em>|</em></li>",
                _serialInfo.AllSpell);
            sbHtml.AppendFormat("<li class=\"\"><a href=\"/{0}/koubei/tags/动力/\" target=\"_blank\">动力</a></li>",
                _serialInfo.AllSpell);
            sbHtml.Append("</ul>");
            sbHtml.AppendFormat(
                "<div class=\"more\"><a target=\"_blank\" href=\"/{0}/koubei/posttopic.html\">发布口碑&gt;&gt;</a></div>",
                _serialInfo.AllSpell);
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");

            List<string> topicIdList = new List<string>();

            XmlNodeList nodeTopic = koubeiReportRoot.SelectNodes("./Item");
            int topicLoop = 0;
            int lastKoubeiId = 0;
            sbHtml.Append("<ul class=\"koubei_block\" data-channelid=\"2.21.818\">");
            foreach (XmlNode node in nodeTopic)
            {
                topicLoop++;
                if (topicLoop > 3) break;
                var ID = node.SelectSingleNode("./TopicId").InnerText;
                var Title = node.SelectSingleNode("./Title").InnerText;
                var Contents = node.SelectSingleNode("./Content").InnerText;
                var CreateTime = ConvertHelper.GetDateTime(node.SelectSingleNode("./CreateTime").InnerText);
                var UserId = ConvertHelper.GetInteger(node.SelectSingleNode("./UserId").InnerText);
                var UserType = ConvertHelper.GetInteger(node.SelectSingleNode("./UserType").InnerText);
                var UserName = node.SelectSingleNode("./UserName").InnerText;
                //var Positive_Votes = node.SelectSingleNode("./Positive_Votes").InnerText;
                //var Negative_Votes = node.SelectSingleNode("./Negative_Votes").InnerText;
                topicIdList.Add(ID);
                lastKoubeiId = ConvertHelper.GetInteger(ID);
                string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                string userUrl = "#";
                if (UserType == 0 && UserId > 0)
                {
                    userImage = GetUserImage(UserId, out UserName);
                    userUrl = "http://i.qichetong.com/u" + UserId + "/";
                }
                //sbHtml.AppendFormat("<div class=\"koubei_item {0}\" kb-topicid=\"{1}\">",
                //    topicLoop == 3 ? "koubei_item_last" : "",
                //    ID);
                //sbHtml.Append("<div class=\"portrait\">");
                //if (UserType == 0 && UserId > 0)
                //    sbHtml.AppendFormat("<a href=\"{2}\" target=\"_blank\"><img src=\"{1}\">{0}</a>", UserName, userImage, userUrl);
                //else
                //    sbHtml.AppendFormat("<a href=\"javascript:;\"><img src=\"{1}\">{0}</a>", "易车网友", userImage);
                //sbHtml.Append("</div>");
                //sbHtml.Append("<div class=\"con\">");
                //sbHtml.AppendFormat("<div class=\"tt\"><a href=\"http://car.bitauto.com/{2}/koubei/{3}/\" target=\"_blank\">{0}</a><span>{1}</span></div>",
                //    Title, CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), _serialInfo.AllSpell, ID);
                //sbHtml.Append("<dl>");
                ////sbHtml.Append("<dt>优点：</dt>");
                //sbHtml.AppendFormat("<dd>{0}</dd>", StringHelper.GetRealLength(Contents) > 200 ? StringHelper.SubString(Contents, 200, true) : Contents);
                ////sbHtml.Append("<dt>缺点：</dt>");
                ////sbHtml.Append("<dd>目前看还算比较顺眼的，不知道再过几年会不会也会审美疲劳。前排放东西的地方太少，这么大一中控，居然就这么点放东西的地方……</dd>");
                //sbHtml.Append("</dl>");
                //sbHtml.Append("<div class=\"clear\"></div>");
                //sbHtml.Append("<div class=\"button_box\">");
                ////sbHtml.Append("<div class=\"button_gray button_koubei\">");
                ////sbHtml.AppendFormat("<cite tag=\"good\" datatag=\"topic\" dataid=\"{0}\"><a href=\"javascript:;\">顶 <s>[{1}]</s></a></cite>", ID, Positive_Votes);
                ////sbHtml.Append("<div class=\"koubei_popup\" style=\"display:none;\">投票成功，谢谢您的参与！</div>");
                ////sbHtml.Append("</div>");
                ////sbHtml.Append("<div class=\"button_gray button_koubei\">");
                ////sbHtml.AppendFormat("<cite tag=\"bad\" datatag=\"topic\" dataid=\"{0}\"><a href=\"javascript:;\">踩 <s>[{1}]</s></a></cite>", ID, Negative_Votes);
                ////sbHtml.Append("<div class=\"koubei_popup\" style=\"display:none;\">投票成功，谢谢您的参与！</div>");
                ////sbHtml.Append("</div>");
                ////sbHtml.Append("<div class=\"button_gray button_koubei\">");
                ////sbHtml.AppendFormat("<cite tag=\"comment\" datatag=\"topic\" dataid=\"{0}\"><a href=\"javascript:;\">评论 <s></s></a></cite>", ID);
                ////sbHtml.Append("</div>");
                //sbHtml.AppendFormat("<a href=\"http://car.bitauto.com/{0}/koubei/{1}/\" target=\"_blank\" class=\"all_koubei\">查看完整口碑&gt;&gt;</a>", _serialInfo.AllSpell, ID);
                //sbHtml.Append("</div>");
                //sbHtml.Append("</div>");
                ////sbHtml.Append("<div class=\"zuiyoudaoli\">最有道理</div>");
                //sbHtml.Append("</div>");

                sbHtml.Append("<li><dl>");
                sbHtml.Append("<dt>");

                if (UserType == 0 && UserId > 0)
                    sbHtml.AppendFormat(
                        "<div class=\"head\"><a target=\"_blank\" href=\"{2}\"><img src=\"{1}\"/></a></div><p class=\"user_name\"><a target=\"_blank\" href=\"{2}\">{0}</a></p>",
                        UserName, userImage, userUrl);
                else
                    sbHtml.AppendFormat(
                        "<div class=\"head\"><a target=\"_blank\" href=\"{2}\"><img src=\"{1}\"/></a></div><p class=\"user_name\"><a target=\"_blank\" href=\"{2}\">{0}</a></p>",
                        "易车网友", userImage, "javascript:;");

                sbHtml.Append("</dt>");
                sbHtml.Append("<dd>");
                sbHtml.AppendFormat(
                    "<h6 class=\"tit\"><a href=\"http://car.bitauto.com/{1}/koubei/{2}/\" target=\"_blank\">{0}</a></h6>",
                    Title, _serialInfo.AllSpell, ID);
                sbHtml.AppendFormat("<span class=\"more_date\">{0}</span>", CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sbHtml.AppendFormat("<p>{0}</p>",
                    StringHelper.GetRealLength(Contents) > 200 ? StringHelper.SubString(Contents, 200, true) : Contents);
                sbHtml.AppendFormat(
                    "<a href=\"http://car.bitauto.com/{0}/koubei/{1}/\" target=\"_blank\" class=\"more\">查看完整口碑&gt;&gt;</a>",
                    _serialInfo.AllSpell, ID);
                sbHtml.Append("</dd>");
                sbHtml.Append("</dl></li>");

            }
            sbHtml.Append("</ul>");
            if (TopicCount > 3)
            {
                sbHtml.AppendFormat(
                    "<a href=\"/{0}/koubei/gengduo/{1}\" target=\"_blank\" class=\"btn-tool-show btn-tool-show-140\"><span>查看更多口碑</span></a>",
                    _serialInfo.AllSpell, lastKoubeiId > 0 ? "#t" + lastKoubeiId : "");
            }
            //sbHtml.Append("<div class=\"button_gray button_728_42\">");
            //sbHtml.AppendFormat("<a href=\"/{0}/koubei/gengduo/{1}\" target=\"_blank\">查看更多口碑&gt;&gt;</a>", _serialInfo.AllSpell, lastKoubeiId > 0 ? "#t" + lastKoubeiId : "");
            //sbHtml.Append("</div>");
            sbHtml.Append("<div class=\"clear\"></div>");
            //sbHtml.AppendFormat("<div class=\"more\"><a target=\"_blank\" href=\"/{0}/koubei/gengduo/\">共{1}篇口碑&gt;&gt;</a></div>", _serialInfo.AllSpell, TopicCount);
            sbHtml.Append("</div>");

            //sbHtml.Append("<script type=\"text/javascript\" language=\"javascript\">");
            //sbHtml.Append("var review_init_cfg = { get_topics_api: 'http://koubei.bitauto.com/api/gettopicbyids.ashx?idList=" + string.Join(",", topicIdList.ToArray()) + "' };");
            //sbHtml.Append("review_init_cfg.init_topics_vote=true;");
            //sbHtml.Append("review_init_cfg.vote_method='jsonp';");
            //sbHtml.Append("</script>");

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReportNew,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新口碑印象失败：serialId:" + SerialId);

            //更新购车服务 口碑块内容
            GenerateKoubeiXml();

        }

        /// <summary>
        /// 车系综述页口碑推荐
        /// </summary>
        /// <returns></returns>
        private string GetSummaryKoubeiTuijianHtml()
        {
			var dr = drTuiJianKoubei;
			if (dr == null)
            {
                return string.Empty;
            }
            
            var strSummary = new StringBuilder(); //综述页  推荐口碑
            strSummary.Append("<div class=\"tj-kb-box\" data-channelid=\"2.21.709\">");
            strSummary.Append("<div class=\"h-tit\"><em>荐</em>");
            strSummary.AppendFormat("<h5><a href=\"{1}\" target=\"_blank\">{0}</a></h5>", dr["Title"], dr["FilePath"]);
            strSummary.Append("</div><div class=\"tj-con\"><i class=\"t-i\"></i><i class=\"b-i\"></i>");
            var content = (dr["Summary"] != null && dr["Summary"].ToString().Length > 0) ? dr["Summary"].ToString() : "";
            if (content.Length > 60)
            {
                content = content.Substring(0, 60) + "...";
            }
            strSummary.AppendFormat("<p>{0}<a href=\"{1}\" target=\"_blank\">查看全部&gt;&gt;</a></p>", content,
                dr["FilePath"]);
            strSummary.Append("</div></div>");
            return strSummary.ToString();
        }

		/// <summary>
        /// 车系综述页口碑第一条---For移动站
        /// </summary>
        /// <returns></returns>
        private string GetSummaryFirstKoubeiHtmlForWap()
        {
            var TopicCount = 0;
            XmlNode koubeiReportRoot = null;
            if (xmlKouBeiReport != null)
            {
                koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
            }

            if (xmlKouBeiReport == null || koubeiReportRoot == null || TopicCount <= 0)
            {
                bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.WirelessSerialSummaryV2,
                    CommonHtmlEnum.BlockIdEnum.KoubeiReportNew);
                return "";
            }
            if (xmlKouBeiReport != null)
            {
                koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
            }
            StringBuilder sbHtml = new StringBuilder();
            sbHtml.Append("<div class=\"kb-list b-shadow\">");
            sbHtml.Append("<ul data-channelid=\"27.23.1334\">");

            XmlNodeList nodeTopic = koubeiReportRoot.SelectNodes("./Item");
            int topicLoop = 0;
            foreach (XmlNode node in nodeTopic)
            {
                topicLoop++;
                if (topicLoop > 1) break;
                var ID = node.SelectSingleNode("./TopicId").InnerText;
                var Title = node.SelectSingleNode("./Title").InnerText;
                var Contents = node.SelectSingleNode("./Content").InnerText;
                var CreateTime = ConvertHelper.GetDateTime(node.SelectSingleNode("./CreateTime").InnerText);
                var UserId = ConvertHelper.GetInteger(node.SelectSingleNode("./UserId").InnerText);
                var UserType = ConvertHelper.GetInteger(node.SelectSingleNode("./UserType").InnerText);
                var UserName = node.SelectSingleNode("./UserName").InnerText;
                var Rating = node.SelectSingleNode("./Rating").InnerText;
				var dataType = ConvertHelper.GetInteger(node.SelectSingleNode("./DataType").InnerText);//口碑类型，1：为口碑，0：正常口碑
                double curRating = 0;
                if (!string.IsNullOrEmpty(Rating))
                {
                    curRating = Convert.ToDouble(Rating);
                }
                int percent = Convert.ToInt32(curRating/5*100);
                //var Positive_Votes = node.SelectSingleNode("./Positive_Votes").InnerText;
                //var Negative_Votes = node.SelectSingleNode("./Negative_Votes").InnerText;
                string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                string userUrl = "#";
                if (UserType == 0 && UserId > 0)
                {
                    userImage = GetUserImage(UserId, out UserName);
                    userUrl = "http://i.qichetong.com/u" + UserId + "/";
                }
                sbHtml.Append("<li>");
                sbHtml.AppendFormat("<a href=\"http://car.m.yiche.com/{0}/koubei/{1}/\">", _serialInfo.AllSpell, ID);
                sbHtml.AppendFormat("<h5>{0}</h5>", Title);
				sbHtml.Append("<div class=\"kb-info\">");
				if (UserType == 0 && UserId > 0)
					sbHtml.AppendFormat("<img src=\"{1}\"/><b>{0}</b>", UserName, userImage);
				else
					sbHtml.AppendFormat("<img src=\"{1}\"/><b>{0}</b>", "易车网友", userImage);
				if(dataType ==0)
					sbHtml.AppendFormat("<div class=\"kb-rank kb-rank-small\"><span class=\"big-star\"><em style=\"width:{1}%\"></em></span><strong>{0}分</strong></div>"
						, Math.Round(curRating, 2), percent);
				sbHtml.Append("</div>");
				sbHtml.AppendFormat("<p>{0}</p>",
                    StringHelper.GetRealLength(Contents) > 200 ? StringHelper.SubString(Contents, 200, true) : Contents);
                sbHtml.Append("</a>");
                sbHtml.Append("</li>");
            }
			

            sbHtml.Append("</ul>");
            sbHtml.AppendFormat(
                "<a href=\"http://car.bitauto.com/{0}/koubei/\" class=\"btn-more\" data-channelid=\"27.23.1343\"><i>查看全部{1}条网友口碑</i></a>",
                _serialInfo.AllSpell, TopicCount);
            sbHtml.Append("</div>");
            return sbHtml.ToString();
        }

        /// <summary>
        /// 车型详解页 口碑推荐
        /// </summary>
        /// <param name="dr"></param>
        private void RenderKoubeiTuijianHtml()
        {
			var dr = drTuiJianKoubei;
			if (dr == null)
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialPingCe,
                    CommonHtmlEnum.BlockIdEnum.KouBeiTuiJian);
                return;
            }
            //var dr = _koubeiTuijianDic[SerialId];
            var strXiangJie = new StringBuilder(); //详解页 推荐口碑
            strXiangJie.Append("<div class=\"tj-kb-box\">");
            strXiangJie.Append("<div class=\"title-con\">");
            strXiangJie.Append("<div class=\"title-box title-box2\">");
            strXiangJie.AppendFormat("<h4><a href=\"{0}\" target=\"_blank\">{1}官方口碑</a></h4>", dr["FilePath"],
                _serialInfo.ShowName);
            strXiangJie.Append("</div></div>");
            strXiangJie.Append("<div class=\"h-tit\"><em>荐</em>");
            strXiangJie.AppendFormat("<h5><a href=\"{1}\" target=\"_blank\">{0}</a></h5>", dr["Title"], dr["FilePath"]);
            strXiangJie.Append("</div><div class=\"tj-con\"><i class=\"t-i\"></i><i class=\"b-i\"></i>");
            var content = (dr["Summary"] != null && dr["Summary"].ToString().Length > 0) ? dr["Summary"].ToString() : "";
            if (content.Length > 60)
            {
                content = content.Substring(0, 60) + "...";
            }
            strXiangJie.AppendFormat("<p>{0}<a href=\"{1}\" target=\"_blank\">查看全部&gt;&gt;</a></p>", content,
                dr["FilePath"]);
            strXiangJie.Append("</div></div>");
            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialPingCe,
                BlockID = CommonHtmlEnum.BlockIdEnum.KouBeiTuiJian,
                HtmlContent = strXiangJie.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新口碑推荐失败：serialId:" + SerialId);
        }

        /// <summary>
        /// 车型详解页 口碑推荐
        /// </summary>
        private void RenderKoubeiTuijianHtmlV2()
        {
			var dr = drTuiJianKoubei;
			if (dr == null)
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialPingCe,
                    CommonHtmlEnum.BlockIdEnum.KouBeiTuiJianV2);
                return;
            }
            var strXiangJie = new StringBuilder(); //详解页 推荐口碑
            var content = (dr["Summary"] != null && dr["Summary"].ToString().Length > 0) ? dr["Summary"].ToString() : "";
            if (content.Length > 60)
            {
                content = content.Substring(0, 60) + "...";
            }
            strXiangJie.Append("<div class=\"koubei-section\">");
            strXiangJie.Append("    <div class=\"section-header header2 mbl\">");
            strXiangJie.Append("        <div class=\"box\">");
            strXiangJie.AppendFormat("            <h2>{0}口碑报告</h2>", _serialInfo.ShowName);
            strXiangJie.Append("        </div>");
            strXiangJie.Append("    </div>");
            strXiangJie.Append("    <div class=\"special-layout-2\">");
            strXiangJie.Append("        <h3 class=\"title\">");
            strXiangJie.AppendFormat("      <a href=\"{0}\" target=\"_blank\"><i>荐</i>", dr["FilePath"]);
            strXiangJie.Append(dr["Title"]);
            strXiangJie.Append("            </a>");
            strXiangJie.Append("        </h3>");
            strXiangJie.Append("        <p class=\"details\">");
            strXiangJie.Append(content);
            strXiangJie.AppendFormat("    <a href=\"{0}\" target=\"_blank\" class=\"more\">查看全部</a>", dr["FilePath"]);
            strXiangJie.Append("        </p>");
            strXiangJie.Append("    </div>");
            strXiangJie.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialPingCe,
                BlockID = CommonHtmlEnum.BlockIdEnum.KouBeiTuiJianV2,
                HtmlContent = strXiangJie.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新口碑推荐失败：serialId:" + SerialId);
        }

        /// <summary>
        /// 车型详解页，口碑排行V2 2016-08-29
        /// </summary>
        private void RenderKoubeiRatingHtmlV2()
        {
            if (_koubeiRatingDic == null) return;
            if (!_koubeiRatingDic.ContainsKey(SerialId))
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialPingCe,
                    CommonHtmlEnum.BlockIdEnum.KouBeiRatingV2);
                return;
            }
            var sbHtml = new StringBuilder();
			double rating = 0;
			var totalCount = 0;
			Dictionary<string,string> ratingDic = null;
			if (_koubeiRatingDic.ContainsKey(SerialId))
			{
				ratingDic = _koubeiRatingDic[SerialId];
				rating = Math.Round(ConvertHelper.GetDouble(ratingDic["Ratings"]), 1);
				totalCount = ConvertHelper.GetInteger(ratingDic["TopicCount"]);
			}
			//var ratingDic = _koubeiRatingDic[SerialId];
			//if (xmlRating == null) return;
			//XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
			//var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
			//var rating = Double.Parse(ratingStr);
			//rating = Math.Round(rating, 1);
			//XmlNode totalNode = xmlRating.SelectSingleNode("Serial/Item/TotalCount");
			//var totalCount = totalNode == null ? "0" : totalNode.InnerText;
            //var totalCount = xmlRating.SelectSingleNode("Serial/Item/TotalCount").InnerText;
            if (ConvertHelper.GetInteger(totalCount) <= 0)
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialPingCe,
                    CommonHtmlEnum.BlockIdEnum.KouBeiRatingV2);
                return;
            }

            var minFuel = ratingDic["MinFuel"];
            var maxFuel = ratingDic["MaxFuel"];
            var fuelStr = minFuel.Equals(maxFuel)
                ? ratingDic["MinFuel"] + "L"
                : ratingDic["MinFuel"] + "-" + ratingDic["MaxFuel"] + "L";
            if (minFuel.Equals(maxFuel) && "0".Equals(minFuel))
            {
                fuelStr = "暂无";
            }
            var levelStr = CommonData.SerialLevelDic.ContainsKey(SerialId) ? CommonData.SerialLevelDic[SerialId] : "";

            sbHtml.Append("<div class=\"pf-box clearfix\">");
            sbHtml.AppendFormat("    <div class=\"f-num\">{0}</div>", rating);
            sbHtml.Append("    <div class=\"x-box\">");
            sbHtml.Append("        <div class=\"big-start-box\">");
            sbHtml.AppendFormat("            <span class=\"big-start\"><em style=\"width: {0}%\"></em></span>",
                (ConvertHelper.GetDouble(rating)/5)*100);
            sbHtml.Append("        </div>");
            sbHtml.AppendFormat("        <p><em>综合评分</em>(基于{0}人平均值)", totalCount);
            sbHtml.Append("        </p>");
            sbHtml.Append("    </div>");
            sbHtml.Append("</div>");
            sbHtml.Append("<div class=\"msg-box\">");
            sbHtml.Append("    <div class=\"lef-b\">");
            sbHtml.AppendFormat("        油耗：<em>{0}</em>", fuelStr);
            sbHtml.Append("    </div>");
            sbHtml.Append("    <div class=\"rig-b\">");
            sbHtml.AppendFormat("        {0}排名：<em>{1}</em>", levelStr, ratingDic["Ranker"]);
            sbHtml.Append("    </div>");
            sbHtml.Append("</div>");
            sbHtml.Append("<ul class=\"lb-list\">");
            sbHtml.Append("    <li>");
            sbHtml.AppendFormat("        <div class=\"lb-box l\">油耗：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["YouHao"]), 1));
            sbHtml.AppendFormat("        <div class=\"lb-box\">空间：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["KongJian"]), 1));
            sbHtml.Append("    </li>");
            sbHtml.Append("    <li>");
            sbHtml.AppendFormat("        <div class=\"lb-box l\">动力：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["DongLi"]), 1));
            sbHtml.AppendFormat("        <div class=\"lb-box\">内饰：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["NeiShi"]), 1));
            sbHtml.Append("    </li>");
            sbHtml.Append("    <li>");
            sbHtml.AppendFormat("        <div class=\"lb-box l\">配置：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["PeiZhi"]), 1));
            sbHtml.AppendFormat("        <div class=\"lb-box\">性价比：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["XingJiaBi"]), 1));
            sbHtml.Append("    </li>");
            sbHtml.Append("    <li>");
            sbHtml.AppendFormat("        <div class=\"lb-box l\">操控：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["CaoKong"]), 1));
            sbHtml.AppendFormat("        <div class=\"lb-box\">舒适度：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["ShuShiDu"]), 1));
            sbHtml.Append("    </li>");
            sbHtml.Append("    <li class=\"last\">");
            sbHtml.AppendFormat("        <div class=\"lb-box l\">外观：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["WaiGuan"]), 1));
            sbHtml.Append("    </li>");
            sbHtml.Append("</ul>");
            sbHtml.Append("<div class=\"btn-sty\">");
            sbHtml.AppendFormat("    <a class=\"btn btn-primary\" href=\"{0}\" target=\"_blank\">查看更多口碑</a>",
                string.Format("http://car.bitauto.com/{0}/koubei/", _serialInfo.AllSpell));
            sbHtml.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialPingCe,
                BlockID = CommonHtmlEnum.BlockIdEnum.KouBeiRatingV2,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新口碑评分失败：serialId:" + SerialId);
        }

		/// <summary>
		/// 子品牌综述页 右侧口碑报告 1200版 lisf 2016-10-09
		/// </summary>
		private void RendSerialKoubeiRatingHtml()
		{
			if (!_koubeiRatingDic.ContainsKey(SerialId) ||
			   ConvertHelper.GetInteger(_koubeiRatingDic[SerialId]["TopicCount"]) == 0)
			{
				CommonHtmlService.DeleteCommonHtml(
					SerialId,
					CommonHtmlEnum.TypeEnum.Serial,
					CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
					CommonHtmlEnum.BlockIdEnum.KoubeiReportNew
					);
				return;
			}
			StringBuilder sbHtml = new StringBuilder();
			double rating  = Math.Round(ConvertHelper.GetDouble(_koubeiRatingDic[SerialId]["Ratings"]), 2);
			double intRating = Math.Floor(rating);
			bool isHasKoubei = false;
			string title = string.Empty;
			string url = string.Empty;
			DataRow drKoubei = drTuiJianKoubei;
			if (drKoubei != null)
			{
				isHasKoubei = true;
				title = drKoubei["Title"].ToString();
				url = drKoubei["FilePath"].ToString();
			}

			sbHtml.Append("<div class=\"special-layout-2 layout-1\">");
			if (isHasKoubei)
			{
				sbHtml.AppendFormat("<h3><a href=\"{0}\" target=\"_blank\">口碑报告</a></h3>", url);
			}
			else
			{
				sbHtml.Append("<h3>口碑报告</h3>");
			}
			sbHtml.Append(
				"<a href=\"http://koubei.bitauto.com/subject/s1.html\" target=\"_blank\" class=\"link\">口碑小妹免费送大奖></a>");
			sbHtml.Append("<div class=\"star-box\">");
			sbHtml.Append("    <div class=\"star\">");
			for (int i = 0; i < intRating; i++)
			{
				sbHtml.Append("        <i class=\"yes\"></i>");
			}
			if (intRating < 5)
			{
				sbHtml.AppendFormat("        <i class=\"no\"><i class=\"yes\" style=\"width: {0}%;\"></i></i>",
					(rating - intRating) * 100);
			}
			for (int i = 0; i < 5 - intRating - 1; i++)
			{
				sbHtml.Append("        <i class=\"no\"></i>");
			}
			sbHtml.Append("    </div>");
			sbHtml.AppendFormat(
				"    <h2><a href=\"http://car.bitauto.com/{1}/koubei/\" data-channelid=\"2.21.1531\" target=\"_blank\">{0}分</a></h2>",
				 string.Format("{0:f2}", rating), _serialInfo.AllSpell);
			if (isHasKoubei)
			{
				sbHtml.AppendFormat("<h5><a href=\"{1}\" target=\"_blank\" data-channelid=\"2.21.1532\">{0}</a></h5>",
					  title,url);
			}
			else
			{
				sbHtml.AppendFormat("    <h5>共{0}篇网友口碑</h5>",
				  _koubeiRatingDic.ContainsKey(SerialId) ? _koubeiRatingDic[SerialId]["TopicCount"] : "0");
			}
			sbHtml.Append("<div class=\"action-box\">");
			if (isHasKoubei)
			{
				sbHtml.AppendFormat("<a class=\"btn btn-primary2\" target=\"_blank\" href=\"{0}\" data-channelid=\"2.21.1532\">查看报告</a>", url);
			}
			sbHtml.AppendFormat("<a class=\"btn btn-secondary2\" href=\"http://car.bitauto.com/{0}/koubei/gengduo/\" data-channelid=\"2.21.1647\" target=\"_blank\">网友口碑</a>", _serialInfo.AllSpell);
			sbHtml.Append("</div></div>");
			//竞品口碑
			List<SerialCompetitiveInfo> competitiveList = GetCompetitiveSerialList(SerialId);
			sbHtml.Append("<div class=\"compete-car\" data-channelid=\"2.21.1646\"><h6>用户口碑对比排行</h6><ul class=\"compete-list\">");
			foreach (SerialCompetitiveInfo competitive in competitiveList)
			{
				sbHtml.AppendFormat("<li{1}><span class=\"num\">{0}.</span>"
					, competitiveList.IndexOf(competitive) + 1
					,competitive.Id == SerialId ? "  class=\"current\"":"");
				sbHtml.AppendFormat("<a href=\"/{0}/\" target=\"_blank\" class=\"name\">{1}</a>", competitive.AllSpell, competitive.ShowName);
				sbHtml.AppendFormat("<span class=\"data\">{0}分</span>", string.Format("{0:f2}", competitive.Rating));
				sbHtml.AppendFormat("<span class=\"tip\">{0}</span></li>", competitive.KoubeiDetailRating[0].RatingDesc);
			}
			sbHtml.Append("</ul></div>");
			sbHtml.Append(" <div class=\"adv-box\" data-channelid=\"2.21.1530\">");
			sbHtml.AppendFormat(
				"     <a href=\"http://koubei.bitauto.com/juhe/{0}/\" class=\"koubei-imgadv\" target=\"_blank\"></a>",
				SerialId);
			sbHtml.Append(" </div>");
			sbHtml.Append("</div>");

			bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
			{
				ID = SerialId,
				TypeID = CommonHtmlEnum.TypeEnum.Serial,
				TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
				BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReportNew,
				HtmlContent = sbHtml.ToString(),
				UpdateTime = DateTime.Now
			});
			if (!success) Log.WriteErrorLog("更新子品牌综述页口碑报告失败：serialId:" + SerialId);
		}

		private List<SerialCompetitiveInfo> GetCompetitiveSerialList(int serialId)
		{
			var compareList = this.GetCompareSerial(serialId);
			compareList.Insert(0, new SerialInfo() { 
				 Id = serialId,
				 ShowName = _serialInfo.ShowName,
				 AllSpell = _serialInfo.AllSpell
			});
			var serialCompetitiveList = new List<SerialCompetitiveInfo>();
			List<Tuple<string, int, string>> listTuple = new List<Tuple<string, int, string>>() {
				new Tuple<string, int, string>("YouHao", 1, "油耗省"),
				new Tuple<string, int, string>("CaoKong", 2, "操控好"),
				new Tuple<string, int, string>("XingJiaBi", 3, "性价比高"),
				new Tuple<string, int, string>("DongLi", 4, "动力强"),
				new Tuple<string, int, string>("PeiZhi", 5, "配置高"),
				new Tuple<string, int, string>("ShuShiDu", 6, "舒适度高"),
				new Tuple<string, int, string>("KongJian", 7, "空间大"),
				new Tuple<string, int, string>("WaiGuan", 8, "外观好"),
				new Tuple<string, int, string>("NeiShi", 9, "内饰好")
			};
			foreach (SerialInfo serialInfo in compareList)
			{
				if (compareList.IndexOf(serialInfo) > 3) break;
				if (_koubeiRatingDic.ContainsKey(serialInfo.Id)
					&& ConvertHelper.GetInteger(_koubeiRatingDic[serialInfo.Id]["TopicCount"]) > 0
					&& ConvertHelper.GetDouble(_koubeiRatingDic[serialInfo.Id]["Ratings"]) > 0)
				{
					SerialCompetitiveInfo competitive = new SerialCompetitiveInfo();
					competitive.Id = serialInfo.Id;
					competitive.Rating = Math.Round(ConvertHelper.GetDouble(_koubeiRatingDic[serialInfo.Id]["Ratings"]), 2);
					competitive.ShowName = serialInfo.ShowName;
					competitive.AllSpell = serialInfo.AllSpell;
					competitive.KoubeiDetailRating = new List<KoubeiDetailRating>();
					foreach (Tuple<string, int, string> tuple in listTuple)
					{
						KoubeiDetailRating detailRating = new KoubeiDetailRating();
						detailRating.Rating = ConvertHelper.GetDouble(_koubeiRatingDic[serialInfo.Id][tuple.Item1]);
						detailRating.Sort = tuple.Item2;
						detailRating.RatingDesc = tuple.Item3;
						competitive.KoubeiDetailRating.Add(detailRating);
					}
					competitive.KoubeiDetailRating.Sort(KoubeiDetailRating.Compare);
					serialCompetitiveList.Add(competitive);
				}
			}

			serialCompetitiveList.Sort((p1, p2) =>
			{
				if (p1.Rating < p2.Rating)
					return 1;
				else if (p1.Rating > p2.Rating)
					return -1;
				else
					return 0;
			});
			return serialCompetitiveList;
		}

		/*
        /// <summary>
        /// 子品牌综述页 口碑报告 1200版 lisf 2016-10-09
        /// </summary>
        private void RendSerialKoubeiRatingHtml()
        {
            if (!_koubeiRatingDic.ContainsKey(SerialId) ||
                ConvertHelper.GetInteger(_koubeiRatingDic[SerialId]["TopicCount"]) == 0)
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                    CommonHtmlEnum.BlockIdEnum.KoubeiReportNew
                    );
                return;
            }
            Dictionary<string, string> categaryDic = new Dictionary<string, string>()
            {
                {"空间", "KongJian"},
                {"动力", "DongLi"},
                {"操控", "CaoKong"},
                {"舒适度", "ShuShiDu"},
                {"性价比", "XingJiaBi"}
            };
            StringBuilder sbHtml = new StringBuilder();
            double rating = rating = Math.Round(ConvertHelper.GetDouble(_koubeiRatingDic[SerialId]["Ratings"]), 1);
            double intRating = intRating = Math.Floor(rating);

            sbHtml.Append("<div class=\"special-layout-2 layout-1\">");
			if (_koubeiTuijianDic.ContainsKey(SerialId))
			{
				sbHtml.AppendFormat("<h3><a href=\"{0}\" target=\"_blank\">口碑报告</a></h3>", _koubeiTuijianDic[SerialId]["FilePath"]);
			}
			else
			{
				sbHtml.Append("<h3>口碑报告</h3>");
			}
            sbHtml.Append(
				"<a href=\"http://koubei.bitauto.com/subject/s1.html\" target=\"_blank\" class=\"link\">口碑小妹免费送大奖></a>");
            sbHtml.Append("<div class=\"star-box\">");
            sbHtml.Append("    <div class=\"star\">");
            for (int i = 0; i < intRating; i++)
            {
                sbHtml.Append("        <i class=\"yes\"></i>");
            }
            if (intRating < 5)
            {
                sbHtml.AppendFormat("        <i class=\"no\"><i class=\"yes\" style=\"width: {0}%;\"></i></i>",
                    (rating - intRating)*100);
            }
            for (int i = 0; i < 5 - intRating - 1; i++)
            {
                sbHtml.Append("        <i class=\"no\"></i>");
            }
            sbHtml.Append("    </div>");
            sbHtml.AppendFormat(
                "    <h2><a href=\"http://car.bitauto.com/{1}/koubei/\" data-channelid=\"2.21.1531\" target=\"_blank\">{0}分</a></h2>",
                rating, _serialInfo.AllSpell);
            if (_koubeiTuijianDic.ContainsKey(SerialId))
            {
                sbHtml.AppendFormat(
                    "    <h5><a href=\"{1}\" target=\"_blank\" data-channelid=\"2.21.1532\">{0}</a></h5>",
                    _koubeiTuijianDic[SerialId]["Title"], _koubeiTuijianDic[SerialId]["FilePath"]);
                sbHtml.AppendFormat(
                    "    <a class=\"btn btn-secondary2\" href=\"{0}\" target=\"_blank\" data-channelid=\"2.21.1532\">查看报告</a>",
                    _koubeiTuijianDic[SerialId]["FilePath"]);
            }
            else
            {
                sbHtml.AppendFormat("    <h5>共{0}篇网友口碑</h5>",
                    _koubeiRatingDic.ContainsKey(SerialId) ? _koubeiRatingDic[SerialId]["TopicCount"] : "0");
                sbHtml.AppendFormat(
                    "    <a class=\"btn btn-secondary2\" href=\"http://car.bitauto.com/{0}/koubei/gengduo/\" target=\"_blank\" data-channelid=\"2.21.1532\">查看全部</a>",
                    _serialInfo.AllSpell);
            }
            sbHtml.Append("</div>");
            sbHtml.Append("<div class=\"chart-box\"><div class=\"chart\"><ul class=\"list\">");
            foreach (KeyValuePair<string, string> kv in categaryDic)
            {
                double value = _koubeiRatingDic.ContainsKey(SerialId)
                    ? Math.Round(ConvertHelper.GetDouble(_koubeiRatingDic[SerialId][kv.Value]), 1)
                    : 0;
                double percent = value == 0 ? 1 : (value*100/5);
                sbHtml.Append("<li>");
                sbHtml.AppendFormat("    <span class=\"title\">{0}</span>", kv.Key);
                sbHtml.AppendFormat("    <span class=\"data\" style=\"height: {0}%;\"></span>", percent);
                sbHtml.AppendFormat("    <span class=\"score\" style=\"bottom: {0}%\">{1}分</span>", percent, value);
                sbHtml.Append("</li>");
            }

            sbHtml.Append(" </ul></div></div>");
            sbHtml.Append(" <div class=\"adv-box\" data-channelid=\"2.21.1530\">");
            sbHtml.AppendFormat(
                "     <a href=\"http://koubei.bitauto.com/juhe/{0}/\" class=\"koubei-imgadv\" target=\"_blank\"></a>",
                SerialId);
            sbHtml.Append(" </div>");
            sbHtml.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReportNew,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新子品牌综述页口碑报告失败：serialId:" + SerialId);
        }
		*/
        /// <summary>
        /// 车型详解页，口碑排行
        /// </summary>
        private void RenderKoubeiRatingHtml()
        {
            if (_koubeiRatingDic == null) return;
            if (!_koubeiRatingDic.ContainsKey(SerialId))
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialPingCe,
                    CommonHtmlEnum.BlockIdEnum.KouBeiRating);
                return;
            }
            var sbHtml = new StringBuilder();
            var ratingDic = _koubeiRatingDic[SerialId];
            if (xmlRating == null) return;
            XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
            var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
            var rating = Double.Parse(ratingStr);
            rating = Math.Round(rating, 1);
            XmlNode totalNode = xmlRating.SelectSingleNode("Serial/Item/TotalCount");
            var totalCount = totalNode == null ? "0" : totalNode.InnerText;
            //var totalCount = xmlRating.SelectSingleNode("Serial/Item/TotalCount").InnerText;
            if (ConvertHelper.GetInteger(totalCount) <= 0)
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialPingCe,
                    CommonHtmlEnum.BlockIdEnum.KouBeiRating);
                return;
            }
            sbHtml.Append("<div class=\"line-box\">");
            sbHtml.Append("<div class=\"side_title\">");
            sbHtml.AppendFormat("<h4><a href=\"/{0}/koubei/\" target=\"_blank\">网友对此车的印象</a></h4>", _serialInfo.AllSpell);
            sbHtml.Append("</div>");
            sbHtml.Append("<div class=\"kb-x-box\"><div class=\"pf-box\">");
            sbHtml.AppendFormat("<div class=\"f-num\">{0}</div>", rating);
            sbHtml.Append("<div class=\"x-box\"><div class=\"mid_start_box\">");
            sbHtml.AppendFormat("<span class=\"mid_start\"><em style=\"width:{0}%\"></em></span>",
                (ConvertHelper.GetDouble(rating)/5)*100);
            sbHtml.AppendFormat("<p><em>综合评分</em>（基于{0}篇口碑）</p>", totalCount);
            sbHtml.Append("</div></div></div>");
            sbHtml.Append("<div class=\"msg-box\"><div class=\"lef-b\">");
            var minFuel = ratingDic["MinFuel"];
            var maxFuel = ratingDic["MaxFuel"];
            var fuelStr = minFuel.Equals(maxFuel)
                ? ratingDic["MinFuel"] + "L"
                : ratingDic["MinFuel"] + "-" + ratingDic["MaxFuel"] + "L";
            if (minFuel.Equals(maxFuel) && "0".Equals(minFuel))
            {
                fuelStr = "暂无";
            }
            sbHtml.AppendFormat(
                "油耗：<a href=\"http://car.bitauto.com/{1}/youhao/\" target=\"_blank\"><em>{0}</em></a></div>", fuelStr,
                _serialInfo.AllSpell);
            sbHtml.Append("<div class=\"rig-b\">");
            var levelStr = CommonData.SerialLevelDic.ContainsKey(SerialId) ? CommonData.SerialLevelDic[SerialId] : "";
            sbHtml.AppendFormat("{0}排名：<em>{1}</em></div>", levelStr, ratingDic["Ranker"]);
            sbHtml.Append("</div>");
            sbHtml.Append("<ul class=\"lb-list\">");
            sbHtml.Append("<li>");
            sbHtml.AppendFormat("<div class=\"lb-box\">油耗：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["YouHao"]), 1));
            sbHtml.AppendFormat("<div class=\"lb-box\">空间：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["KongJian"]), 1));
            sbHtml.Append("</li>");
            sbHtml.Append("<li>");
            sbHtml.AppendFormat("<div class=\"lb-box\">动力：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["DongLi"]), 1));
            sbHtml.AppendFormat("<div class=\"lb-box\">内饰：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["NeiShi"]), 1));
            sbHtml.Append("</li>");
            sbHtml.Append("<li>");
            sbHtml.AppendFormat("<div class=\"lb-box\">配置：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["PeiZhi"]), 1));
            sbHtml.AppendFormat("<div class=\"lb-box\">性价比：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["XingJiaBi"]), 1));
            sbHtml.Append("</li>");
            sbHtml.Append("<li>");
            sbHtml.AppendFormat("<div class=\"lb-box\">操控：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["CaoKong"]), 1));
            sbHtml.AppendFormat("<div class=\"lb-box\">舒适度：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["ShuShiDu"]), 1));
            sbHtml.Append("</li>");
            sbHtml.Append("<li class=\"last\">");
            sbHtml.AppendFormat("<div class=\"lb-box\">外观：<span>{0}分</span></div>",
                Math.Round(float.Parse(ratingDic["WaiGuan"]), 1));
            sbHtml.Append("</li>");
            sbHtml.Append("</ul>");
            sbHtml.Append("<div class=\"more-btn button_orange\">");
            sbHtml.AppendFormat("<a href=\"{0}\" target=\"_blank\">查看更多口碑</a>",
                string.Format("http://car.bitauto.com/{0}/koubei/", _serialInfo.AllSpell));
            sbHtml.Append("</div></div></div>");
            bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialPingCe,
                BlockID = CommonHtmlEnum.BlockIdEnum.KouBeiRating,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新口碑评分失败：serialId:" + SerialId);
        }

        private void RenderKoubeiImpressionHtml()
        {
            StringBuilder sbHtml = new StringBuilder();

            //if (xmlKoubei == null) return;

            if (xmlRating == null) return;
            XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
            var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
            //var ratingStr = xmlRating.SelectSingleNode("Serial/Item/Rating").InnerText;
            var rating = Double.Parse(ratingStr);
            rating = Math.Round(rating, 1);
            XmlNode totalNode = xmlRating.SelectSingleNode("Serial/Item/TotalCount");
            var totalCount = totalNode == null ? "0" : totalNode.InnerText;
            //var totalCount = xmlRating.SelectSingleNode("Serial/Item/TotalCount").InnerText;

            //XmlNode root = xmlKoubei.DocumentElement;
            //var AverageRating = root.Attributes["AverageRating"].Value;
            //var TopicCount = ConvertHelper.GetInteger(root.Attributes["TopicCount"].Value);
            //XmlNode nodeImpressions = root.SelectSingleNode("./Impressions");
            //var VoteCount = nodeImpressions.Attributes["VoteCount"].Value;
            //XmlNodeList nodeGood = nodeImpressions.SelectNodes("./Good/Impression");
            //XmlNodeList nodeBad = nodeImpressions.SelectNodes("./Bad/Impression");
            //modified by sk 2015.06.01 去掉印象条数限制 王凯
            //if (youdianNodes != null && youdianNodes.Count >= 6 && quedianNodes != null && quedianNodes.Count >= 6)
            if ((youdianNodes != null && youdianNodes.Count >= 0) || (quedianNodes != null && quedianNodes.Count >= 0))
            {
                sbHtml.Append("<div class=\"line-box\">");

                sbHtml.Append("<div class=\"side_title\">");
                sbHtml.AppendFormat("<h4><a href=\"/{0}/koubei/\" target=\"_blank\">网友对此车的印象</a></h4>",
                    _serialInfo.AllSpell);
                sbHtml.Append("</div>");

                double sum = (ConvertHelper.GetDouble(rating)/5)*118;

                sbHtml.Append("<div class=\"big_start_box mt15\">");
                sbHtml.AppendFormat(
                    "<span class=\"big_start\"><em style=\"width:{0}px\"></em></span><strong>{1}分</strong>", sum, rating);
                sbHtml.Append("</div>");
                sbHtml.AppendFormat("<p class=\"zh_pinfen\">综合评分<span>（基于{0}篇口碑）</span></p>", totalCount);

                sbHtml.Append("<div class=\"youque_box\" data-channelid=\"2.21.828\">");
                sbHtml.Append("<h6>优点：</h6>");
                sbHtml.Append("<p>");

                int goodNum = 0;
                if (youdianNodes != null)
                {
                    foreach (XmlNode node in youdianNodes)
                    {
                        goodNum++;
                        if (goodNum > 6) break;
                        var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                        var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                        var Url = node.SelectSingleNode("./Url").InnerText;
                        sbHtml.AppendFormat(
                            "<a target=\"_blank\" href=\"{2}\"><span title=\"{0}位网友评价\">{1}</span></a>", Vote,
                            KeyWord.Length > 6 ? KeyWord.Substring(0, 6) : KeyWord, Url);
                    }
                }
                sbHtml.Append("</p>");
                sbHtml.Append("</div>");


                sbHtml.Append("<div class=\"youque_box quedian\" data-channelid=\"2.21.828\">");
                sbHtml.Append("<h6>缺点：</h6>");
                sbHtml.Append("<p>");
                int badNum = 0;
                if (quedianNodes != null)
                {
                    foreach (XmlNode node in quedianNodes)
                    {
                        badNum++;
                        if (badNum > 6) break;
                        var Vote = node.SelectSingleNode("./VoteCount").InnerText;
                        var KeyWord = node.SelectSingleNode("./Keyword").InnerText;
                        var Url = node.SelectSingleNode("./Url").InnerText;
                        sbHtml.AppendFormat(
                            "<a target=\"_blank\" href=\"{2}\"><span title=\"{0}位网友评价\">{1}</span></a>", Vote,
                            KeyWord.Length > 6 ? KeyWord.Substring(0, 6) : KeyWord, Url);
                    }
                }
                sbHtml.Append("</p>");
                sbHtml.Append("</div>");



                sbHtml.Append("<div class=\"btn_box\">");
                sbHtml.AppendFormat(
                    "<span class=\"button_orange\"><a href=\"/{0}/koubei/posttopic.html\" data-channelid=\"2.21.829\" target=\"_blank\">发布口碑</a></span>",
                    _serialInfo.AllSpell);
                sbHtml.AppendFormat(
                    "<span class=\"button_gray\"><a href=\"/{0}/koubei/wposttopic.html\" data-channelid=\"2.21.830\" target=\"_blank\">发布微口碑</a></span>",
                    _serialInfo.AllSpell);
                sbHtml.Append("</div>");

                sbHtml.Append("<div class=\"clear\"></div>");
                sbHtml.Append("</div>");

                bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
                {
                    ID = SerialId,
                    TypeID = CommonHtmlEnum.TypeEnum.Serial,
                    TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                    BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiImpression,
                    HtmlContent = sbHtml.ToString(),
                    UpdateTime = DateTime.Now
                });
                if (!success) Log.WriteErrorLog("更新口碑印象失败：serialId:" + SerialId);
            }
            else
            {
                bool success = CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialSummary,
                    CommonHtmlEnum.BlockIdEnum.KoubeiImpression);
            }
        }

        private void GenerateKoubeiXml()
        {
            try
            {
                string filePath = Path.Combine(CommonData.CommonSettings.BuyCarServiceSavePath,
                    string.Format(@"Koubei\{0}.xml", SerialId));

                var TopicCount = 0;
                XmlNode koubeiReportRoot = null;
                if (xmlKouBeiReport != null)
                {
                    koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                    TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
                }

                if (xmlKouBeiReport == null || koubeiReportRoot == null || TopicCount <= 0)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sb.Append("<Root>");
                XmlNodeList nodeList = koubeiReportRoot.SelectNodes("./Item");
                foreach (XmlNode node in nodeList)
                {
                    var ID = node.SelectSingleNode("./TopicId").InnerText;
                    var Title = node.SelectSingleNode("./Title").InnerText;
                    var Contents = node.SelectSingleNode("./Content").InnerText;

                    var UserId = ConvertHelper.GetInteger(node.SelectSingleNode("./UserId").InnerText);
                    var UserType = ConvertHelper.GetInteger(node.SelectSingleNode("./UserType").InnerText);
                    var UserName = node.SelectSingleNode("./UserName").InnerText;

                    var rating = node.SelectSingleNode("./Rating").InnerText;

                    string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                    string userUrl = "#";
                    if (UserType == 0 && UserId > 0)
                    {
                        userImage = GetUserImage(UserId, out UserName);
                        userUrl = "http://i.qichetong.com/u" + UserId + "/";
                    }
                    sb.Append("<Item>");
                    sb.AppendFormat("<Title><![CDATA[{0}]]></Title>", Title);
                    sb.AppendFormat("<Content><![CDATA[{0}]]></Content>", Contents);
                    sb.AppendFormat("<UserName><![CDATA[{0}]]></UserName>",
                        (UserType == 0 && UserId > 0) ? UserName : "易车网友");
                    sb.AppendFormat("<UserImage>{0}</UserImage>", userImage);
                    sb.AppendFormat("<UserUrl>{0}</UserUrl>", userUrl);
                    sb.AppendFormat("<Url>http://car.bitauto.com/{0}/koubei/{1}/</Url>", _serialInfo.AllSpell, ID);
                    sb.AppendFormat("<Rating>{0}</Rating>", rating);
                    sb.Append("</Item>");
                }
                sb.Append("</Root>");
                CommonFunction.SaveFileContent(sb.ToString(), filePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string GetUserImage(int userId, out string showName)
        {
            string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
            try
            {
                using (DataCenter.WCF.DataProvideClient client = new DataCenter.WCF.DataProvideClient())
                {
                    var userFieldInfo = client.GetUserData(new int[] {userId}, new string[] {"Avatar60", "ShowName"});
                    userImage = userFieldInfo[userId]["avatar60"].ToString();
                    showName = userFieldInfo[userId]["showname"].ToString();
                }
            }
            catch (Exception ex)
            {
                showName = string.Empty;
                Log.WriteErrorLog(ex.ToString());
            }
            return userImage;
        }

        /// <summary>
        /// 第四级 精选网友口碑
        /// </summary>
        private void RenderFourthStageKoubeiHtml()
        {
            StringBuilder sbHtml = new StringBuilder();
            var TopicCount = 0;
            XmlNode koubeiReportRoot = null;
            if (xmlKouBeiReport != null)
            {
                koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
            }

            if (xmlKouBeiReport == null || koubeiReportRoot == null || TopicCount <= 0)
            {
                bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.H5SerialSummary,
                    CommonHtmlEnum.BlockIdEnum.KoubeiReportNew);
                return;
            }
            List<string> topicIdList = new List<string>();

            XmlNodeList nodeTopic = koubeiReportRoot.SelectNodes("./Item");
            int topicLoop = 0;
            int lastKoubeiId = 0;
            sbHtml.Append("<ul>");
            foreach (XmlNode node in nodeTopic)
            {
                topicLoop++;
                if (topicLoop > 3) break;
                var ID = node.SelectSingleNode("./TopicId").InnerText;
                var Title = node.SelectSingleNode("./Title").InnerText;
                var Contents = node.SelectSingleNode("./Content").InnerText;
                var CreateTime = ConvertHelper.GetDateTime(node.SelectSingleNode("./CreateTime").InnerText);
                var UserId = ConvertHelper.GetInteger(node.SelectSingleNode("./UserId").InnerText);
                var UserType = ConvertHelper.GetInteger(node.SelectSingleNode("./UserType").InnerText);
                var UserName = node.SelectSingleNode("./UserName").InnerText;

                topicIdList.Add(ID);
                lastKoubeiId = ConvertHelper.GetInteger(ID);
                string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                string userUrl = "#";
                if (UserType == 0 && UserId > 0)
                {
                    userImage = GetUserImage(UserId, out UserName);
                    userUrl = "http://i.qichetong.com/u" + UserId + "/";
                }

                sbHtml.Append("<li>");
                var koubeiUrl = string.Format("http://car.m.yiche.com/{0}/koubei/{1}/", _serialInfo.AllSpell, ID);
                var showContent = string.Empty;
                showContent = StringHelper.GetRealLength(Contents) > 50
                    ? StringHelper.SubString(Contents, 50, true)
                    : Contents;
                if (string.IsNullOrEmpty(showContent))
                {
                    showContent = StringHelper.GetRealLength(Title) > 50
                        ? StringHelper.SubString(Title, 50, true)
                        : Title;
                }
                if (UserType == 0 && UserId > 0)
                {
                    sbHtml.AppendFormat(
                        "<a href=\"{2}\"><div class=\"koubei_img\"><img src=\"{1}\"/></div><div class=\"koubei_name\">{0}</div><p>{3}</p></a>",
                        UserName, userImage, koubeiUrl, showContent);
                }
                else
                {
                    sbHtml.AppendFormat(
                        "<a href=\"{2}\"><div class=\"koubei_img\"><img src=\"{1}\"/></div><div class=\"koubei_name\">{0}</div><p>{3}</p></a>",
                        "易车网友", userImage, koubeiUrl, showContent);
                }
                sbHtml.Append("</li>");

            }
            sbHtml.Append("</ul>");

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.H5SerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReportNew,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新H5口碑印象失败：serialId:" + SerialId);
        }

        private string GetNodeValue(XmlNode node, string defaultStr)
        {
            return node != null && !string.IsNullOrEmpty(node.InnerText.Trim()) ? node.InnerText.Trim() : defaultStr;
        }

        /// <summary>
        /// 第四级V2 油耗、印象、网友口碑块 2015-08-14
        /// </summary>
        public void H5KoubeiV3()
        {
            try
            {
                //var savePath = @"\\192.168.0.174\Data\Koubei\SerialKouBei\";
                var savePath = CommonData.CommonSettings.SavePath + @"\Koubei\SerialKouBei\";
                var path = Path.Combine(savePath, Path.GetFileName(string.Format("{0}.xml", SerialId)));

                var root = new XElement("root");

                #region 编辑点评

                var editorCommentorys = new XElement("EditorCommentorys");

                #region 循环

                Dictionary<int, CarInfoEntity> dictCar = CarRepository.GetNewYearCarData(SerialId);
                XmlDocument doc = new XmlDocument();
                doc.Load(string.Format(CommonData.CommonSettings.EditorCommentUrlNew, SerialId));
                XmlNodeList nodeList = doc.SelectNodes("//CarEstimate");
                var editorCommentoryCount = 0;
                if (nodeList != null)
                {
					XmlDocument eidtorUserXml = CommonFunction.GetXmlDocument(CommonData.CommonSettings.EidtorUserUrl);
                    foreach (XmlNode node in nodeList)
                    {
                        if (editorCommentoryCount >= 2)
                            break;
                        int userId =
                            ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CommentUserId"), string.Empty));
                        int carId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CarId"), string.Empty));
						XmlNode ele = null;
						if (eidtorUserXml != null)
						{
							ele = eidtorUserXml.SelectSingleNode(string.Format("//User[UserId='{0}']", userId.ToString()));
						}
						else
						{
							eidtorUserXml = CommonFunction.GetXmlDocument(CommonData.CommonSettings.EidtorUserUrl);
							if (eidtorUserXml != null)
							{
								ele = eidtorUserXml.SelectSingleNode(string.Format("//User[UserId='{0}']", userId.ToString()));
							}
						}

                        if (carId <= 0 || ele == null)
                        {
                            continue;
                        }
                        if (!dictCar.ContainsKey(carId)) continue;
                        var comment = GetNodeValue(node.SelectSingleNode("Comment"), string.Empty);
                        var userName = GetNodeValue(node.SelectSingleNode("CommentUserName"), string.Empty);
                        var commentTime =
                            ConvertHelper.GetDateTime(GetNodeValue(node.SelectSingleNode("CommentTime"), string.Empty));
                        var userImg = GetNodeValue(ele.SelectSingleNode("UserPhotoPath"), string.Empty);
                        var squarePhoto = GetNodeValue(ele.SelectSingleNode("SquarePhoto"),
                            "http://img1.bitautoimg.com/images/not.gif");
                        var userBlogUrl = GetNodeValue(ele.SelectSingleNode("UserBlogUrl"), string.Empty);
                        var carEntity = dictCar[carId];


                        var editorCommentory = new XElement("EditorCommentory"
                            , new XElement("Name", new XCData(userName))
                            , new XElement("SquarePhoto", new XCData(squarePhoto))
                            , new XElement("Cars"
                                , new XElement("YearType", new XCData(carEntity.YearType.ToString()))
                                , new XElement("CarName", new XCData(carEntity.CarName))
                                )
                            , new XElement("Comment", new XCData(comment))
                            );

                        editorCommentorys.Add(editorCommentory);
                        editorCommentoryCount++;
                    }
                }

                #endregion

                root.Add(editorCommentorys);

                #endregion

                #region 网友评分

                var ratingStr = "0";
                if (_koubeiRatingDic.ContainsKey(SerialId))
                {
					ratingStr = _koubeiRatingDic[SerialId]["Ratings"];
					//XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
					//ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
                    //ratingStr = xmlRating.SelectSingleNode("Serial/Item/Rating").InnerText;
                }
                var scoreEle = new XElement("Score"
                    , new XElement("Rating", new XCData(ratingStr))
                    );
                root.Add(scoreEle);

                #endregion

                #region 网友油耗

                var fuleValue = "暂无";
                if (_koubeiRatingDic.ContainsKey(SerialId))
                {
					var dic = _koubeiRatingDic[SerialId];
                    var minFuel = dic["MinFuel"];
                    var maxFuel = dic["MaxFuel"];
                    fuleValue = minFuel.Equals(maxFuel) ? minFuel + "L" : minFuel + "-" + maxFuel + "L";
                    if (minFuel.Equals(maxFuel) && "0".Equals(minFuel))
                    {
                        fuleValue = "暂无";
                    }
                }
                var fuleEle = new XElement("FuleValue"
                    , new XCData(fuleValue));
                root.Add(fuleEle);

                #endregion

                #region 口碑印象

                var koubeiImpressionEle = new XElement("KoubeiImpression");
                var goodEle = new XElement("Good");

                #region 循环

                if (youdianNodes != null)
                {
                    var goodsCount = 0;
                    foreach (XmlNode node in youdianNodes)
                    {
                        if (goodsCount >= 10)
                            break;
                        var vote = node.SelectSingleNode("./VoteCount").InnerText;
                        var keyWord = node.SelectSingleNode("./Keyword").InnerText;
                        var categoryName = node.SelectSingleNode("./CategoryName").InnerText;

                        var goodItem = new XElement("Item"
                            , new XElement("VoteCount", new XCData(vote))
                            , new XElement("Keyword", new XCData(keyWord))
                            , new XElement("CategoryName", new XCData(categoryName)));
                        goodEle.Add(goodItem);
                        goodsCount++;
                    }
                }

                #endregion

                koubeiImpressionEle.Add(goodEle);

                var badEle = new XElement("Bad");

                #region 循环

                if (quedianNodes != null)
                {
                    var badCount = 0;
                    foreach (XmlNode node in quedianNodes)
                    {
                        if (badCount >= 10)
                            break;
                        var vote = node.SelectSingleNode("./VoteCount").InnerText;
                        var keyWord = node.SelectSingleNode("./Keyword").InnerText;
                        var categoryName = node.SelectSingleNode("./CategoryName").InnerText;

                        var badItem = new XElement("Item"
                            , new XElement("VoteCount", new XCData(vote))
                            , new XElement("Keyword", new XCData(keyWord))
                            , new XElement("CategoryName", new XCData(categoryName)));
                        badEle.Add(badItem);
                        badCount++;
                    }
                }

                #endregion

                koubeiImpressionEle.Add(badEle);

                root.Add(koubeiImpressionEle);

                #endregion

                #region 评论文章

                var topicCount = 0;
                var articsEle = new XElement("CommentArtics", new XAttribute("TopicCount", topicCount));

                #region 循环

                if (xmlKouBeiReport != null)
                {
                    XmlNode koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                    if (koubeiReportRoot != null)
                    {
                        if (koubeiReportRoot.Attributes != null)
                        {
                            topicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
                            articsEle.SetAttributeValue("TopicCount", topicCount);
                        }

                        var nodeTopic = koubeiReportRoot.SelectNodes("./Item");
                        if (nodeTopic != null)
                        {
                            var loopCount = 0;
                            foreach (XmlNode node in nodeTopic)
                            {
                                if (loopCount >= 3)
                                {
                                    break;
                                }
                                var trimYear = node.SelectSingleNode("./TrimYear").InnerText;
                                var trimName = node.SelectSingleNode("./TrimName").InnerText;
                                var fuel = node.SelectSingleNode("./Fuel").InnerText;
                                var mileage = node.SelectSingleNode("./Mileage").InnerText;
                                var topicId = node.SelectSingleNode("./TopicId").InnerText;
                                var title = node.SelectSingleNode("./Title").InnerText;
                                var contents = node.SelectSingleNode("./Content").InnerText;
                                var createTime = node.SelectSingleNode("./CreateTime").InnerText;
                                var userId = node.SelectSingleNode("./UserId").InnerText;
                                var userType = node.SelectSingleNode("./UserType").InnerText;
                                var userName = node.SelectSingleNode("./UserName").InnerText;
                                var userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                                var userUrl = "#";
                                if (ConvertHelper.GetInteger(userType) == 0 && ConvertHelper.GetInteger(userId) > 0)
                                {
                                    userImage = GetUserImage(ConvertHelper.GetInteger(userId), out userName);
                                    userUrl = "http://i.qichetong.com/u" + userId + "/";
                                }
                                var koubeiUrl = string.Format("http://car.m.yiche.com/{0}/koubei/{1}/",
                                    _serialInfo.AllSpell,
                                    SerialId);
                                var articEle = new XElement("Artic"
                                    , new XElement("TrimYear", new XCData(trimYear))
                                    , new XElement("TrimName", new XCData(trimName))
                                    , new XElement("Fuel", new XCData(fuel))
                                    , new XElement("Mileage", new XCData(mileage))
                                    , new XElement("TopicId", new XCData(topicId))
                                    , new XElement("Title", new XCData(title))
                                    , new XElement("Contents", new XCData(contents))
                                    , new XElement("CreateTime", new XCData(createTime))
                                    , new XElement("UserId", new XCData(userId))
                                    , new XElement("UserType", new XCData(userType))
                                    , new XElement("UserName", new XCData(userName))
                                    , new XElement("UserImage", new XCData(userImage))
                                    , new XElement("UserUrl", new XCData(userUrl))
                                    , new XElement("KoubeiUrl", new XCData(koubeiUrl))
                                    );
                                articsEle.Add(articEle);
                                loopCount++;
                            }
                        }
                    }
                }

                #endregion

                root.Add(articsEle);

                #endregion

                var directoryName = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);
                    root.Save(path);
                    Log.WriteLog("更新H5 V3编辑点评、口碑印象、网友评分和网友油耗和点评文章的XML数据源生成成功：serialId:" + SerialId);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("错了：serialId:" + SerialId + ",msg:" + ex.ToString());
            }
        }

        /// <summary>
        /// 生成PC端口碑对比--网友点评块
        /// </summary>
        private void RenderKoubeiDuiBiHtml()
        {
            //CommonFunction.GetCarList();
            StringBuilder sbHtml = new StringBuilder();
            var TopicCount = 0;
            XmlNode koubeiReportRoot = null;
            if (xmlKouBeiReport != null)
            {
                koubeiReportRoot = xmlKouBeiReport.DocumentElement;
                TopicCount = ConvertHelper.GetInteger(koubeiReportRoot.Attributes["TotalCount"].Value);
            }

            if (xmlKouBeiReport == null || koubeiReportRoot == null || TopicCount <= 0)
            {
                bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.KouBeiDuiBi,
                    CommonHtmlEnum.BlockIdEnum.WangYouDianPing);
                return;
            }
            double AverageRating = 0;
			if (_koubeiRatingDic.ContainsKey(SerialId))
			{
				AverageRating = Math.Round(ConvertHelper.GetDouble(_koubeiRatingDic[SerialId]["Ratings"]), 1);
			}
			//if (xmlRating != null)
			//{
			//	XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
			//	var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
			//	var rating = Double.Parse(ratingStr);
			//	AverageRating = Math.Round(rating, 1);
			//}
            double sum = (ConvertHelper.GetDouble(AverageRating)/5)*100;
            List<string> topicIdList = new List<string>();
            XmlNodeList nodeTopic = koubeiReportRoot.SelectNodes("./Item");
            int topicLoop = 0;
            int lastKoubeiId = 0;


            if (nodeTopic != null && nodeTopic.Count > 0)
            {
                #region

                foreach (XmlNode node in nodeTopic)
                {
                    topicLoop++;
                    if (topicLoop > 2) break;
                    var ID = node.SelectSingleNode("./TopicId").InnerText;
                    var Title = node.SelectSingleNode("./Title").InnerText;
                    var Contents = node.SelectSingleNode("./Content").InnerText;
                    var CreateTime = ConvertHelper.GetDateTime(node.SelectSingleNode("./CreateTime").InnerText);
                    var UserId = ConvertHelper.GetInteger(node.SelectSingleNode("./UserId").InnerText);
                    var UserType = ConvertHelper.GetInteger(node.SelectSingleNode("./UserType").InnerText);
                    var UserName = node.SelectSingleNode("./UserName").InnerText;
                    topicIdList.Add(ID);
                    lastKoubeiId = ConvertHelper.GetInteger(ID);
                    string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                    string userUrl = "#";
                    if (UserType == 0 && UserId > 0)
                    {
                        userImage = GetUserImage(UserId, out UserName);
                        userUrl = "http://i.qichetong.com/u" + UserId + "/";
                    }

                    sbHtml.Append("<div class=\"list-box\">");
                    sbHtml.Append("	<div class=\"head\">");
                    sbHtml.Append("	            <div class=\"head-pic\">");
                    sbHtml.AppendFormat("		    <a href=\"{0}\" target=\"_blank\"><img src=\"{1}\"></a>", userUrl,
                        userImage);
                    sbHtml.Append("	            </div>");
                    sbHtml.AppendFormat("		<p class=\"user-name\"><a href=\"{0}\" target=\"_blank\">{1}</a></p>",
                        userUrl, UserName);
                    sbHtml.Append("		<div class=\"start2-box inline-b\">");
                    sbHtml.AppendFormat("			<div class=\"start\"><em style=\"width: {0}%;\"></em></div>", sum);
                    sbHtml.Append("		</div>");
                    sbHtml.Append("	</div>");
                    sbHtml.AppendFormat(
                        "	<p class=\"txt\">{0}&nbsp;&nbsp;<a href=\"{1}\" target=\"_blank\">查看更多</a></p>",
                        StringHelper.SubString(Contents, 120, true),
                        "http://car.bitauto.com/" + _serialInfo.AllSpell + "/koubei/" + ID + "/");
                    sbHtml.Append("</div>");
                }

                #endregion

                if (TopicCount >= 2)
                {
                    sbHtml.AppendFormat("<p class=\"more\"><a href=\"{0}\" target=\"_blank\">查看更多点评</a></p>",
                        "http://car.bitauto.com/" + _serialInfo.AllSpell + "/koubei/gengduo/", TopicCount);
                }
            }
            else
            {
                sbHtml.Append("<span class=\"gray\">暂无点评</span>");
            }

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.KouBeiDuiBi,
                BlockID = CommonHtmlEnum.BlockIdEnum.WangYouDianPing,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新PC口碑对比：serialId:" + SerialId);

        }

    }

	/// <summary>
	/// 竞品口碑
	/// </summary>
	class SerialCompetitiveInfo : SerialInfo 
	{
		public double Rating;

		public List<KoubeiDetailRating> KoubeiDetailRating;
	}

	/// <summary>
	/// 口碑详细评分
	/// </summary>
	class KoubeiDetailRating
	{
		public double Rating;
		public int Sort;
		public string RatingDesc;

		public static int Compare(KoubeiDetailRating k1, KoubeiDetailRating k2)
		{
			if (k1.Rating == k2.Rating)
			{
				return k1.Sort-k2.Sort;
			}
			return k1.Rating > k2.Rating ? -1 : 1;
		}
	}
}
