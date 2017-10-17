using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;
using System.Data;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	/// <summary>
	/// 综述页 视频块
	/// </summary>
	public class SerialVideoHtmlBuilder : BaseBuilder
	{
		private SerialInfo _serialInfo;

		public SerialVideoHtmlBuilder()
		{
			GetSerialHotVideoXml();
		}

		public override void BuilderDataOrHtml(int objId)
		{
			try
			{
				_serialInfo = CommonData.SerialDic[objId];

				List<VideoForSerialSummaryEntity> list = GetVideo(objId);

				MakeVideoBlockHtml(objId, list);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}

		}
        //private List<VideoForSerialSummaryEntity> GetVideo(int serialId)
        //{
        //    List<VideoForSerialSummaryEntity> list = new List<VideoForSerialSummaryEntity>();
        //    string videoNewsPath = Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"SerialNews\SerialVideo\Serial_Video_{0}.xml", serialId));
        //    int videoCount = 0;
        //    XmlDocument xmlNewVideoNews = new XmlDocument();
        //    xmlNewVideoNews.Load(videoNewsPath);
        //    if (xmlNewVideoNews != null)
        //    {
        //        XmlNodeList nodeList = xmlNewVideoNews.SelectNodes("/NewDataSet/listNews");
        //        foreach (XmlNode node in nodeList)
        //        {
        //            videoCount++;
        //            if (videoCount > 4) break;
        //            list.Add(new VideoForSerialSummaryEntity()
        //            {
        //                NewsId = ConvertHelper.GetInteger(node.SelectSingleNode("newsid").InnerText),
        //                Title = node.SelectSingleNode("title").InnerText,
        //                Facetitle = node.SelectSingleNode("facetitle").InnerText,
        //                Filepath = node.SelectSingleNode("filepath").InnerText,
        //                Picture = node.SelectSingleNode("picture").InnerText
        //            });
        //        }
        //    }
        //    List<VideoForSerialSummaryEntity> hotVideoList = new List<VideoForSerialSummaryEntity>();
        //    string serialHotVideoPath = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\hotvideo_90.xml");
        //    XmlDocument xmlHotSerialVideo = new XmlDocument();
        //    xmlHotSerialVideo.Load(serialHotVideoPath);
        //    if (xmlHotSerialVideo != null)
        //    {
        //        XmlNode serialNode = xmlHotSerialVideo.SelectSingleNode("/root/Serial[@id=" + serialId + "]");
        //        if (serialNode != null)
        //        {
        //            int loop = 0;
        //            foreach (XmlNode newsNode in serialNode.ChildNodes)
        //            {
        //                int newsId = ConvertHelper.GetInteger(newsNode.InnerText);
        //                if (list.Take(2).ToList().Find(p => p.NewsId == newsId) != null) continue;

        //                string url = CommonData.CommonSettings.NewsUrl + string.Format("?newsid={0}&showtype=3", newsNode.InnerText);

        //                DataSet ds = new DataSet();
        //                ds.ReadXml(url);
        //                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //                {
        //                    loop++;
        //                    if (loop > 2) break;
        //                    hotVideoList.Add(new VideoForSerialSummaryEntity()
        //                    {
        //                        NewsId = ConvertHelper.GetInteger(ds.Tables[0].Rows[0]["newsid"]),
        //                        Title = ds.Tables[0].Rows[0]["title"].ToString(),
        //                        Facetitle = ds.Tables[0].Rows[0]["facetitle"].ToString(),
        //                        Filepath = "http://news.bitauto.com" + ds.Tables[0].Rows[0]["filepath"].ToString(),
        //                        Picture = ds.Tables[0].Rows[0]["picture"].ToString()
        //                    });
        //                }

        //            }
        //        }
        //    }
        //    var lastList = list.Take(4 - hotVideoList.Count).ToList().Union(hotVideoList);
        //    return lastList.Take(4).ToList();
        //}
		/// <summary>
        /// date:2017-1-11
		/// desc:获取视频数据 （前两条提取最新视频，后面两条提取3个月内最热视频。最热视频不足2条，补充最新视频。）
		/// author: zf
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		private List<VideoForSerialSummaryEntity> GetVideo(int serialId)
		{
			List<VideoForSerialSummaryEntity> list = new List<VideoForSerialSummaryEntity>();
			string videoNewsPath = Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"SerialNews\SerialVideo\Serial_Video_{0}.xml", serialId));
			int videoCount = 0;
			XmlDocument xmlNewVideoNews = new XmlDocument();
			xmlNewVideoNews.Load(videoNewsPath);
			if (xmlNewVideoNews != null)
			{
				XmlNodeList nodeList = xmlNewVideoNews.SelectNodes("/NewDataSet/listNews");
				foreach (XmlNode node in nodeList)
				{
					videoCount++;
					if (videoCount > 4) break;
					list.Add(new VideoForSerialSummaryEntity()
					{
						NewsId = ConvertHelper.GetInteger(node.SelectSingleNode("newsid").InnerText),
						Title = node.SelectSingleNode("title").InnerText,
						Facetitle = node.SelectSingleNode("facetitle").InnerText,
						Filepath = node.SelectSingleNode("filepath").InnerText,
						Picture = node.SelectSingleNode("picture").InnerText
					});
				}
			}
			List<VideoForSerialSummaryEntity> hotVideoList = new List<VideoForSerialSummaryEntity>();
			string serialHotVideoPath = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\hotvideo_90.xml");
			XmlDocument xmlHotSerialVideo = new XmlDocument();
			xmlHotSerialVideo.Load(serialHotVideoPath);
			if (xmlHotSerialVideo != null)
			{
				XmlNode serialNode = xmlHotSerialVideo.SelectSingleNode("/root/Serial[@id=" + serialId + "]");
				if (serialNode != null)
				{
					int loop = 0;
					foreach (XmlNode newsNode in serialNode.ChildNodes)
					{
						int newsId = ConvertHelper.GetInteger(newsNode.InnerText);
						if (list.Take(2).ToList().Find(p => p.NewsId == newsId) != null) continue;

					    NewsEntityV2 curNewsEntity = Common.CommonFunction.GetNewsEntityFromApi(newsId);
                        if (curNewsEntity != null)
						{
							loop++;
							if (loop > 2) break;
							hotVideoList.Add(new VideoForSerialSummaryEntity()
							{
								NewsId =curNewsEntity.NewsId,
								Title = curNewsEntity.Title,
								Facetitle = curNewsEntity.ShortTitle,
								Filepath = curNewsEntity.Url,
								Picture = curNewsEntity.ImageCoverUrl
							});
						}
					}
				}
			}
			var lastList = list.Take(4 - hotVideoList.Count).ToList().Union(hotVideoList);
			return lastList.Take(4).ToList();
		}
		/// <summary>
		/// 生成块内容 入库
		/// </summary>
		/// <param name="serialId">子品牌id</param>
		/// <param name="videoList">视频数据</param>
		private void MakeVideoBlockHtml(int serialId, List<VideoForSerialSummaryEntity> videoList)
		{
			if (videoList.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("<div class=\"line_box\" id=\"car-videobox\">");
				sb.Append("	<h3>");
				sb.Append("		<span>");
				//sb.AppendFormat("<b><a href=\"{0}shipin/\">{1}视频</a></b>", baseUrl, serialSeoName);
				sb.AppendFormat("<b><a href=\"/{1}/shipin/\" target=\"_blank\">{0}视频</a></b>", _serialInfo.SeoName, _serialInfo.AllSpell);
				sb.Append("		</span>");
				sb.Append("	</h3>");
				sb.Append("	<div class=\"car-video20130802\">");
				sb.Append("		<ul>");
				int loop = 0;
				foreach (VideoForSerialSummaryEntity entity in videoList)
				{
					loop++;
					if (loop > 4) break;
					string videoTitle = entity.Title;
					videoTitle = StringHelper.RemoveHtmlTag(videoTitle);

					string shortTitle = entity.Facetitle;
					shortTitle = StringHelper.RemoveHtmlTag(shortTitle);

					string imgUrl = entity.Picture;
					if (imgUrl.Trim().Length == 0)
						imgUrl = CommonData.CommonSettings.DefaultCarPic;
					//imgUrl = imgUrl.Replace("/bitauto/", "/newsimg-242-w0-1-q70/bitauto/");
					//imgUrl = imgUrl.Replace("/autoalbum/", "/newsimg-242-w0-1-q70/autoalbum/");
					string filepath = entity.Filepath;

					sb.Append("			<li>");
					sb.AppendFormat("				<a href=\"{0}\" target=\"_blank\"><img data-original=\"{1}\" alt=\"{2}\" width=\"170\" height=\"96\"></a>", filepath, imgUrl, _serialInfo.BrandName + _serialInfo.Name + videoTitle);
					sb.AppendFormat("				<a href=\"{0}\" target=\"_blank\">{1}</a>", filepath, shortTitle != videoTitle ? shortTitle : videoTitle);
					sb.AppendFormat("				<a href=\"{0}\" target=\"_blank\" class=\"btn-play\">播放</a>", filepath);
					sb.Append("			</li>");
				}

				sb.Append("		</ul>");
				sb.Append("		<div class=\"clear\"></div>");
				sb.Append("	</div>");
				sb.Append("	<div class=\"more\">");
				sb.AppendFormat("		<a href=\"/{0}/shipin/\" target=\"_blank\">更多&gt;&gt;</a>", _serialInfo.AllSpell);
				sb.Append("	</div>");
				sb.Append("</div>");
				bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
				{
					ID = serialId,
					TypeID = CommonHtmlEnum.TypeEnum.Serial,
					TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
					BlockID = CommonHtmlEnum.BlockIdEnum.Video,
					HtmlContent = sb.ToString(),
					UpdateTime = DateTime.Now
				});
				if (!success) Log.WriteErrorLog("更新视频块失败：serialId:" + serialId);
			}
		}
		//生成最热视频新闻
		public void GetSerialHotVideoXml()
		{
			Log.WriteLog("获取最热子品牌视频新闻");
			try
			{
				string xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\hotvideo_90.xml");

				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(CommonData.CommonSettings.SerialCmsHotVideoUrl);

				CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
			}
			catch (System.Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
	}
}
