using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data;
using BitAuto.Utils.Data;
using System.Data.SqlClient;
using BitAuto.Utils;
using System.IO;
using System.Xml;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Services;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	/// <summary>
	/// 子品牌焦点新闻 & 易车网首页帮你选车-车型资讯
	/// </summary>
	public class FocusNewsHtmlBuilder : BaseBuilder
	{


		private string homeNewsXmlPath;

		public FocusNewsHtmlBuilder()
		{
			homeNewsXmlPath = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\FocusNews\HomeXml\{0}.xml");
			carNewsTypeDic = CommonData.CarNewsTypeSettings.CarNewsTypeList;
		}
		public override void BuilderDataOrHtml(int objId)
		{
			Log.WriteLog("start serial focus news! id:" + objId.ToString());
			try
			{
				if (!CommonData.SerialDic.ContainsKey(objId))
				{
					Log.WriteLog("not found serial! id:" + objId.ToString());
					return;
				}
				string serialSpell = CommonData.SerialDic[objId].AllSpell.ToLower();

				//编辑设置排序，根据设置先按照设置位置生成列表，未设置位置由null代替，再由实际数据补全空位
				Dictionary<int, NewsEntity> orderNewsList = FocusNewsService.GetOrderNewsList(objId);

				//得到焦点新闻
				List<NewsEntity> focusNewsList = FocusNewsService.GetFocusNewsListNew(orderNewsList, objId);

				//新版总数也焦点新闻
				//List<NewsEntity> focusNewsNewList = FocusNewsService.GetFocusNewsListNew(orderNewsList, objId, 8);

                //新版总数也焦点新闻 1200版
                List<NewsEntity> focusNewsNewListFor1200 = FocusNewsService.GetFocusNewsListNew(orderNewsList, objId, 9);

				bool existPingce = false;//是否包含车型详解
				List<NewsEntity> focusNewsNewWirelessList = FocusNewsService.GetFocusNewsListNewForWireless(orderNewsList, objId, out existPingce, 6);

                //生成新版综述页焦点新闻 1200版
                BuildFocusNewsHtmlFor1200(objId, focusNewsNewListFor1200);

				//待销综述页焦点新闻
				new FocusNewsForWaitSaleHtmlBuilder().BuilderDataOrHtml(objId);

				//移动端
                BuildSerialNewsHtmlForWirelessV3(objId, focusNewsNewWirelessList, existPingce);

				//易车网首页帮你选车-车型资讯
				BuilderHomeNewsXml(objId, focusNewsList);

				//第四级V2版本文章页面生成逻辑2015-08-11
				new H5HtmlBuilder().BuildH5ArticalXml(objId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(string.Format("Msg:生成焦点新闻异常，serialId={0}\r\nStackTrace:{1}", objId, ex.ToString()));
			}
			Log.WriteLog("end serial focus news! id:" + objId.ToString());
		}

        /// <summary>
        /// 移动站综述页改版,更新焦点新闻 date:2016-6-29
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="focusNewsNewList"></param>
        /// <param name="existPingce"></param>
		private void BuildSerialNewsHtmlForWirelessV3(int serialId, List<NewsEntity> focusNewsNewList, bool existPingce)
		{
			if (!CommonData.SerialDic.ContainsKey(serialId) || focusNewsNewList == null || focusNewsNewList.Count <= 0)
			{
				bool delSuccess = CommonHtmlService.DeleteCommonHtml(
					  serialId,
					  CommonHtmlEnum.TypeEnum.Serial,
					  CommonHtmlEnum.TagIdEnum.WirelessSerialSummaryV2,
					  CommonHtmlEnum.BlockIdEnum.FocusNewsForWireless);
				return;
			}

			string serialSpell = CommonData.SerialDic[serialId].AllSpell.ToLower();
			StringBuilder sb = new StringBuilder();
			sb.Append("<div class=\"card-news\">");
			sb.Append("<ul>");
			int loop = 0;
			foreach (NewsEntity entity in focusNewsNewList)
			{
				loop++;
				string url = entity.PageUrl.Replace("news.bitauto.com", "news.m.yiche.com");
                string title = entity.Title;
                //modified by sk 22个文字 2017-03-06
                title = StringHelper.GetRealLength(title) > 44 ? StringHelper.SubString(title, 44, false) : title;
                string liClass = string.Empty;
				if (loop == 1 && existPingce)
				{
					sb.Append("<li class=\"news-xj\">");
					sb.AppendFormat("<a href=\"{0}\">{1}<span>车型详解</span><h4>{2}</h4><em><i class=\"ico-comment huifu comment_0_{3}\"></i></em></a>",
						url,
						entity.ImageLink.ToLower().IndexOf(".bitauto") == -1 ? "" : string.Format("<div class=\"img-box\"><img src=\"{0}\" alt=\"{1}\"/></div>", entity.ImageLink, entity.Title),
                        title,
						entity.NewsId);
					sb.Append("</li>");
				}
				else
				{
                    if(loop>3)
                        liClass = "display:none;";
                    if (loop == 4)
                    {
                        sb.Append("<script type=\"text/javascript\">");
                        sb.Append("showNewsInsCode('4b61d3eb-4afc-4a8d-9d72-e4898c983feb', 'f599d929-c9d6-48db-9747-4e48e5e51a24', '15bbdba0-98b1-4f40-bfb1-8ea820e38a80', '7175d79d-c43e-428d-a2f4-cff3578ff355');");
                        sb.Append("</script>");
                    }
					sb.AppendFormat("<li style=\""+liClass+"\" class=\"{5}\"><a href=\"{0}\">{1}<div class=\"con-box\"><h4>{2}</h4><em><span>{3}</span><span>{4}</span><i class=\"ico-comment huifu comment_0_{6}\"></i></em></div></a></li>",
						url,
                        entity.ImageLink.ToLower().IndexOf(".bitauto") == -1 ? "" :(string.IsNullOrEmpty(entity.ImageLink)?"":string.Format("<div class=\"img-box\"><span><img src=\"{0}\" alt=\"{1}\"/></span></div>", entity.ImageLink.Replace("-180-w0", "_180x120"), entity.Title)),
                        title,
						entity.PublishTime.ToString("yyyy-MM-dd"),
						StrCut(entity.Author, 6),
						//entity.CommentNum,
                        string.IsNullOrEmpty(entity.ImageLink) ? "news-noimg" : "",
						entity.NewsId);
				}
			}
			sb.Append("</ul>");
			sb.Append("</div>");
            if (focusNewsNewList.Count > 3)
            {
                sb.AppendFormat("<a href=\"javascript:void(0);\" id=\"btn-hot-more\" class=\"btn-more btn-add-more\"><i>加载更多</i></a>");
            }
            else
            {
                sb.AppendFormat("<a href=\"http://car.m.yiche.com/{0}/wenzhang/\"  class=\"btn-more\"><i>查看更多</i></a>",serialSpell);
            }
			bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
			{
				ID = serialId,
				TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialSummaryV2,
				BlockID = CommonHtmlEnum.BlockIdEnum.FocusNewsForWireless,
				HtmlContent = sb.ToString(),
				UpdateTime = DateTime.Now
			});
			if (!success) Log.WriteErrorLog("移动版新闻列表更新失败，func：BuildFocusNewsHtmlNew，serialId:" + serialId);
		}
        private void BuildSerialNewsHtmlForWirelessV2(int serialId, List<NewsEntity> focusNewsNewList, bool existPingce)
        {
            if (!CommonData.SerialDic.ContainsKey(serialId) || focusNewsNewList == null || focusNewsNewList.Count <= 0)
            {
                bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                      serialId,
                      CommonHtmlEnum.TypeEnum.Serial,
                      CommonHtmlEnum.TagIdEnum.WirelessSerialSummary,
                      CommonHtmlEnum.BlockIdEnum.FocusNewsForWireless);
                return;
            }

            string serialSpell = CommonData.SerialDic[serialId].AllSpell.ToLower();
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"card-news\">");
            sb.Append("<ul>");
            int loop = 0;
            foreach (NewsEntity entity in focusNewsNewList.Take(3))
            {
                loop++;
                string url = entity.PageUrl.Replace("news.bitauto.com", "news.m.yiche.com");
                if (loop == 1 && existPingce)
                {
                    sb.Append("<li class=\"news-xj\">");
                    sb.AppendFormat("<a href=\"{0}\">{1}<span>车型详解</span><h4>{2}</h4><em><i class=\"ico-comment huifu comment_0_{3}\"></i></em></a>",
                        url,
                        entity.ImageLink.ToLower().IndexOf(".bitauto") == -1 ? "" : string.Format("<div class=\"img-box\"><img src=\"{0}\" alt=\"{1}\"/></div>", entity.ImageLink, entity.Title),
                        entity.Title,
                        entity.NewsId);
                    sb.Append("</li>");
                }
                else
                {
                    sb.AppendFormat("<li><a href=\"{0}\">{1}<h4>{2}</h4><em><span>{3}</span><span>{4}</span><i class=\"ico-comment huifu comment_0_{6}\"></i></em></a></li>",
                        url,
                        entity.ImageLink.ToLower().IndexOf(".bitauto") == -1 ? "" : string.Format("<div class=\"img-box\"><img src=\"{0}\" alt=\"{1}\"/></div>", entity.ImageLink, entity.Title),
                        entity.Title,
                        entity.PublishTime.ToString("yyyy-MM-dd"),
                        StrCut(entity.Author, 6),
                        entity.CommentNum,
                        entity.NewsId);
                }
            }
            sb.Append("</ul>");
            sb.Append("</div>");
            if (focusNewsNewList.Count >= 3)
                sb.AppendFormat("<a href=\"/{0}/wenzhang/\" class=\"btn-more\"><i>查看更多文章</i></a>", serialSpell);

            bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = serialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.FocusNewsForWireless,
                HtmlContent = sb.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("移动版新闻列表更新失败，func：BuildFocusNewsHtmlNew，serialId:" + serialId);
        }

		/// <summary>
		/// 生成移动版焦点新闻块内容
		/// </summary>
		/// <param name="objId"></param>
		/// <param name="focusNewsNewList"></param>
		private void BuildSerialNewsHtmlForWireless(int objId, List<NewsEntity> focusNewsNewList)
		{
			if (!CommonData.SerialDic.ContainsKey(objId) || focusNewsNewList == null || focusNewsNewList.Count <= 0) return;

			string serialSpell = CommonData.SerialDic[objId].AllSpell.ToLower();
			StringBuilder sb = new StringBuilder();
			sb.Append("	<div id= \"carnews0\" class=\"card-news\">");
			sb.Append("<ul>");
			int i = 0;
			foreach (NewsEntity entity in focusNewsNewList)
			{
				i++;
				//去除焦点区新闻
				//if (i < 3)
				//{
				//    continue;
				//}
				string iconVideo = string.Empty;
				string url = entity.PageUrl;
				if (entity.NewsCategoryShowName.CategoryShowName == "视频")
				{
					url = url.Replace("v.bitauto.com", "v.m.yiche.com");
					iconVideo = "<div class=\"ico-video\"></div>";
				}
				else
				{
					url = url.Replace("news.bitauto.com", "news.m.yiche.com");
				}
				sb.AppendFormat("<li><a href=\"{0}\">{3}<h4>{2} {1}</h4>",
					url,
					entity.Title,
					iconVideo, entity.ImageLink.ToLower().IndexOf(".bitauto") == -1 ? "" : string.Format("<div class=\"img-box\"><img src=\"{0}\"></div>", entity.ImageLink));
				sb.AppendFormat("<span><em>{0}</em>{1}<em>{2}</em></span>", entity.PublishTime.ToString("yyyy-MM-dd"), entity.NewsCategoryShowName.CategoryShowName != "视频" ? string.Format("<em class=\"ico-comment\">{0}</em>", entity.CommentNum) : "<em class=\"ico-comment\">0</em>", StrCut(entity.Author, 6));
				sb.Append("</a></li>");
			}
			sb.Append("</ul>");

			sb.AppendFormat("<div class=\"wrap\"><a href=\"/{0}/wenzhang/\" class=\"btn-one btn-gray\">查看更多</a></div>", serialSpell);
			sb.Append("</div>");
			bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
			{
				ID = objId,
				TypeID = CommonHtmlEnum.TypeEnum.Serial,
				TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialSummary,
				BlockID = CommonHtmlEnum.BlockIdEnum.FocusNews,
				HtmlContent = sb.ToString(),
				UpdateTime = DateTime.Now
			});
			if (!success) Log.WriteErrorLog("移动版新闻列表更新失败，func：BuildFocusNewsHtmlNew，serialId:" + objId);
		}
		/// <summary>
		/// 生成综述页新增要闻
		/// </summary>
		/// <param name="objId"></param>
		/// <param name="focusNewsNewList"></param>
		private void BuildFocusSerialNewsHtmlForWireless(int objId, List<NewsEntity> focusNewsNewList)
		{
			if (!CommonData.SerialDic.ContainsKey(objId) || focusNewsNewList == null || focusNewsNewList.Count <= 0) return;

			string serialSpell = CommonData.SerialDic[objId].AllSpell.ToLower();
			StringBuilder sb = new StringBuilder();

			int index = 0;
			foreach (NewsEntity entity in focusNewsNewList)
			{
				string iconVideo = string.Empty;
				string url = entity.PageUrl;
				if (entity.NewsCategoryShowName.CategoryShowName == "视频")
				{
					url = url.Replace("v.bitauto.com", "v.m.yiche.com");
					iconVideo = "<div class=\"ico-video\"></div>";
				}
				else
				{
					url = url.Replace("news.bitauto.com", "news.m.yiche.com");
				}

				index++;
				sb.Append("<li>");
				sb.AppendFormat("<a href=\"{0}?ref=cxyw\" onclick=\"javascript:dcsMultiTrack('DCS.dcsuri', '/car/onclick/yaowen{1}.onclick','WT.ti', '要闻{1}')\">", url, index);
				sb.AppendFormat("<span>{1}{0}</span>", entity.Title, iconVideo);
				// modified by zhangll Sep.05.2014 去掉要闻评论数等
				//sb.AppendFormat("<span><em>{0}</em>{1}<em>{2}</em></span>", entity.PublishTime.ToString("yyyy-MM-dd"), entity.NewsCategoryShowName.CategoryShowName != "视频" ? string.Format("<em class=\"ico-comment\">{0}</em>", entity.CommentNum) : "<em class=\"ico-comment\">0</em>", entity.Author);
				sb.Append("<em class=\"red\">要闻</em>");
				sb.Append("<div class=\"line\"></div>");
				sb.Append("</a>");
				sb.Append("</li>");
			}
			//sb.AppendFormat("<a class=\"more-news\" href=\"/{0}/pingce/\">更多</a>", serialSpell);

			bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
			{
				ID = objId,
				TypeID = CommonHtmlEnum.TypeEnum.Serial,
				TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialFocusSummary,
				BlockID = CommonHtmlEnum.BlockIdEnum.FocusNews,
				HtmlContent = sb.ToString(),
				UpdateTime = DateTime.Now
			});
			if (!success) Log.WriteErrorLog("移动版新增要闻更新失败，func：BuildFocusSerialNewsHtmlForWireless，serialId:" + objId);
		}

		/// <summary>
		/// 生成新版总数也焦点新闻html
		/// </summary>
		/// <param name="focusNewsNewList">焦点新闻列表</param>
		private void BuildFocusNewsHtmlNew(int objId, List<NewsEntity> focusNewsNewList)
		{
			if (!CommonData.SerialDic.ContainsKey(objId)) return;
			StringBuilder sb = new StringBuilder();

			//如果焦点新闻为空,则添加'暂无文章'并且添加行情块
			string serialSpell = CommonData.SerialDic[objId].AllSpell.ToLower();
			if (focusNewsNewList == null || focusNewsNewList.Count < 1)
			{
				sb.Append("<div class=\"news_list1\">");
				sb.Append("<h2>暂无内容</h2> ");
				sb.Append("<ul class=\"list_date\">");
				//sb.Append("<li><span>暂无内容</span></li>");
				sb.Append("</ul>");
				sb.Append("</div>");
			}
			else
			{
				string baseUrl = string.Format("/{0}/{1}/", serialSpell, "{0}");
				sb.Append("<div class=\"news_list1\">");
				sb.AppendFormat("	<h2><a href=\"{0}\" target=\"_blank\">{1}</a></h2>", focusNewsNewList[0].PageUrl, focusNewsNewList[0].Title);
				sb.Append("    <ul class=\"list_date\">");
				int index = 0;
				foreach (NewsEntity entity in focusNewsNewList)
				{
					if (index == 0)
					{
						index++;
						continue;
					}
					string cateUrl = null;

					string newsCategory = entity.NewsCategoryShowName.CategoryShowName;
					if (entity.NewsCategoryShowName.CategoryUrl == NewsCategoryConfig.QitaCategoryKey || entity.NewsCategoryShowName.CategoryKey == "huati")
					{
						//sb.AppendFormat("<li><div><span class=\"c_qita\">{0}| </span><a href=\"{1}\" target=\"_blank\">{2}</a></div><small>{3}</small></li>",newsCategory, entity.PageUrl, entity.Title, entity.PublishTime.ToString("MM-dd"));
						sb.AppendFormat("<li><div><span><em>{0}</em>| </span><a href=\"{1}\" target=\"_blank\">{2}</a></div><small>{3}</small></li>",
						 newsCategory, entity.PageUrl, entity.Title, entity.PublishTime.ToString("MM-dd"));
					}
					else
					{
						cateUrl = string.Format(baseUrl, entity.NewsCategoryShowName.CategoryUrl);
						sb.AppendFormat("<li><div><span><a href=\"{0}\" class=\"fl\" target=\"_blank\">{1}</a>| </span><a href=\"{2}\" target=\"_blank\">{3}</a></div><small>{4}</small></li>",
							cateUrl, newsCategory, entity.PageUrl, entity.Title, entity.PublishTime.ToString("MM-dd"));
					}
				}
				sb.Append("</ul>");
				sb.Append("</div>");
			}
			bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
			{
				ID = objId,
				TypeID = CommonHtmlEnum.TypeEnum.Serial,
				TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
				BlockID = CommonHtmlEnum.BlockIdEnum.FocusNews,
				HtmlContent = sb.ToString(),
				UpdateTime = DateTime.Now
			});
			if (!success) Log.WriteErrorLog("新版综述页焦点新闻更新失败，func：BuildFocusNewsHtmlNew，serialId:" + objId);
		}

        /// <summary>
        /// 生成新版总数也焦点新闻html lisf 2016-10-8 1200版
        /// </summary>
        /// <param name="focusNewsNewList">焦点新闻列表</param>
        private void BuildFocusNewsHtmlFor1200(int objId, List<NewsEntity> focusNewsList)
        {
            if (!CommonData.SerialDic.ContainsKey(objId)) return;
            StringBuilder sb = new StringBuilder();

            //如果焦点新闻为空,则添加'暂无文章'并且添加行情块
            string serialSpell = CommonData.SerialDic[objId].AllSpell.ToLower();
            if (focusNewsList == null || focusNewsList.Count < 1)
            {
                sb.Append("<h3 class=\"grey-txt\" id=\"focusNewsContent\">暂无内容</h3>");
            }
            else
            {
                string baseUrl = string.Format("http://car.bitauto.com/{0}/{1}/", serialSpell, "{0}");

				sb.Append("<div class=\"col-auto list-txt-layout1 section-right\" data-channelid=\"2.21.804\"  id=\"focusNewsContent\">");
                sb.AppendFormat("<h3 class=\"no-wrap\"><a href=\"{0}\" target=\"_blank\">{1}</a></h3>"
                    , focusNewsList[0].PageUrl
                    , focusNewsList[0].Title);
                sb.Append("<div class=\"list-txt list-txt-m list-txt-default list-txt-style2 type-1\">");
                sb.Append("<ul>");
                foreach (NewsEntity entity in focusNewsList)
                {
                    if (focusNewsList.IndexOf(entity) == 0)
                    {
                        continue;
                    }
                    string cateUrl = null;

                    string newsCategory = entity.NewsCategoryShowName.CategoryShowName;
                    if (entity.NewsCategoryShowName.CategoryUrl == NewsCategoryConfig.QitaCategoryKey || entity.NewsCategoryShowName.CategoryUrl == "#" || entity.NewsCategoryShowName.CategoryKey == "huati")
                    {
                        sb.AppendFormat("<li><div class=\"txt\"><strong><a class=\"no-link\">{0}</a>|</strong><a href=\"{1}\" target=\"_blank\">{2}</a></div><span>{3}</span></li>"
                            , newsCategory
                            , entity.PageUrl
                            , entity.Title
                            , entity.PublishTime.ToString("MM-dd"));
                    }
                    else
                    {
                        cateUrl = string.Format(baseUrl, entity.NewsCategoryShowName.CategoryUrl);
                        sb.AppendFormat("<li><div class=\"txt\"><strong><a href=\"{0}\" target=\"_blank\">{1}</a>|</strong><a href=\"{2}\" target=\"_blank\"{5}>{3}</a></div><span>{4}</span></li>"
                            , cateUrl
                            , newsCategory
                            , entity.PageUrl
                            , entity.Title
                            , entity.PublishTime.ToString("MM-dd")
                            , entity.NewsCategoryShowName.CategoryUrl == "shipin" ? " class=\"video\"" : string.Empty);
                    }
                }

                sb.Append("</ul></div></div>");
            }
            bool success = Common.Services.CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = objId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                BlockID = CommonHtmlEnum.BlockIdEnum.FocusNews,
                HtmlContent = sb.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("新版综述页焦点新闻更新失败，func：BuildFocusNewsHtmlNew，serialId:" + objId);
        }

		/// <summary>
		/// 易车网首页帮你选车-车型资讯
		/// </summary>
		private void BuilderHomeNewsXml(int objId, List<NewsEntity> focusNewsList)
		{
			string homexmlPath = string.Format(homeNewsXmlPath, objId.ToString());
			string dir = Path.GetDirectoryName(homexmlPath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			if (focusNewsList == null || focusNewsList.Count < 1)
			{
				if (File.Exists(homexmlPath))
					File.Delete(homexmlPath);
			}
			else
			{
				XmlDocument homeXml = new XmlDocument();
				XmlElement homeXmlRoot = homeXml.CreateElement("root");
				homeXml.AppendChild(homeXmlRoot);
				NewsEntity newsNetity = null;
				XmlElement news = null, tempEle = null;
				for (int i = 0; i < focusNewsList.Count; i++)
				{
					newsNetity = focusNewsList[i];
					news = homeXml.CreateElement("listNews");

					tempEle = homeXml.CreateElement("title");
					tempEle.InnerText = newsNetity.Title;
					news.AppendChild(tempEle);

					tempEle = homeXml.CreateElement("facetitle");
					tempEle.InnerText = newsNetity.FaceTitle;
					news.AppendChild(tempEle);

					tempEle = homeXml.CreateElement("filepath");
					tempEle.InnerText = newsNetity.PageUrl;
					news.AppendChild(tempEle);

					homeXmlRoot.AppendChild(news);
				}
				CommonFunction.SaveXMLDocument(homeXml, homexmlPath);
			}
		}
		/// <summary>
		/// 截取指定长度字符串(按字节算)
		/// </summary>
		/// <param name="str"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		static string StrCut(string str, int length)
		{
			int len = 0;
			byte[] b;
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < str.Length; i++)
			{
				b = Encoding.Default.GetBytes(str.Substring(i, 1));
				if (b.Length > 1)
					len += 2;
				else
					len++;

				if (len > length)
				{
					sb.Append("...");
					break;
				}

				sb.Append(str[i]);
			}

			return sb.ToString();
		}
	}
}
