using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using System.Xml;
using System.IO;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;
using System.Data;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	public class SerialVideoHtmlNewBuilder : BaseBuilder
	{
		private SerialInfo _serialInfo;
		private string serialHotVideoPath;

		public SerialVideoHtmlNewBuilder()
		{
			serialHotVideoPath = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\hotvideonew_90.xml");
			GetSerialHotVideoXml();
		}

		public override void BuilderDataOrHtml(int objId)
		{
			try
			{
				_serialInfo = CommonData.SerialDic[objId];
				List<VideoEntityV2> videoList = GetVideoNewList(objId);
				////老版综述页视频块
				//MakeOldVideoBlockHtml(objId, videoList);
				//新版综述页视频块
				//MakeVideoBlockHtml(objId, videoList);
                //1200版 综述页视频块
                MakeVideoBlockHtmlNew(objId, videoList);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
		#region 去除老版视频块
		/*
 		private void MakeOldVideoBlockHtml(int serialId, List<VideoEntity> videoList)
		{
			////取数据
			//List<VideoEntity> videoList = VideoService.GetVideoList(serialId, VideoEnum.CategoryTypeEnum.Serial, 4);
			if (videoList.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("<div class=\"line_box\" id=\"car-videobox\">");
				sb.Append("	<h3>");
				sb.Append("		<span>");
				sb.AppendFormat("			<a href=\"/{0}/shipin/\">{1}视频</a>", _serialInfo.AllSpell, _serialInfo.SeoName);
				sb.Append("		</span>");
				sb.Append("	</h3>");
				sb.Append("	<div class=\"car-video20130802\">");
				sb.Append("		<ul>");
				int loop = 0;
				foreach (VideoEntity entity in videoList)
				{
					loop++;
					if (loop > 4) break;
					string videoTitle = entity.Title;
					videoTitle = StringHelper.RemoveHtmlTag(videoTitle);

					string shortTitle = entity.ShortTitle;
					shortTitle = StringHelper.RemoveHtmlTag(shortTitle);

					string imgUrl = entity.ImageLink;
					if (imgUrl.Trim().Length == 0)
						imgUrl = CommonData.CommonSettings.DefaultCarPic;
					string filepath = entity.ShowPlayUrl;

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
				sb.AppendFormat("		<a href=\"/{0}/shipin/\">更多&gt;&gt;</a>", _serialInfo.AllSpell);
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
				if (!success) Log.WriteErrorLog("更新旧版综述页视频块失败：serialId:" + serialId);
			}
		}
		 */
		#endregion
		/// <summary>
		/// 生成块内容 入库
		/// </summary>
		/// <param name="serialId">子品牌id</param>
		/// <param name="videoList">视频数据</param>
		private void MakeVideoBlockHtml(int serialId, List<VideoEntity> videoList)
		{
			//List<VideoEntity> videoList = GetVideoNewList(serialId);
			if (videoList.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
                sb.Append("<div class=\"line_box\" id=\"car-videobox\" data-channelid=\"2.21.812\">");

				sb.Append("<div class=\"title-box\">");
				sb.AppendFormat("<h3><a href=\"/{1}/shipin/\" target=\"_blank\">{0}视频</a></h3>", _serialInfo.SeoName, _serialInfo.AllSpell);
				sb.Append("	<div class=\"more\">");
				sb.AppendFormat("		<a href=\"/{0}/shipin/\" target=\"_blank\">更多&gt;&gt;</a>", _serialInfo.AllSpell);
				sb.Append("	</div>");
				sb.Append("</div>");

				sb.Append("	<div class=\"theme_list_box\"><div class=\"theme_list\">");
				sb.Append("		<ul class=\"video_list\">");
				int loop = 0;
				foreach (VideoEntity entity in videoList)
				{
					loop++;
					if (loop > 3) break;
					//string videoTitle = entity.Title;
					//videoTitle = StringHelper.RemoveHtmlTag(videoTitle);

					string shortTitle = StringHelper.RemoveHtmlTag(entity.ShortTitle);
					if (StringHelper.GetRealLength(shortTitle) > 24)
						shortTitle = StringHelper.SubString(shortTitle, 24, true);
					string imgUrl = entity.ImageLink;
					if (imgUrl.Trim().Length == 0)
						imgUrl = CommonData.CommonSettings.DefaultCarPic;
					//imgUrl = imgUrl.Replace("/bitauto/", "/newsimg-242-w0-1-q70/bitauto/");
					//imgUrl = imgUrl.Replace("/autoalbum/", "/newsimg-242-w0-1-q70/autoalbum/");
					imgUrl = imgUrl.Replace("/Video/", "/newsimg-210-w0/Video/");
					string filepath = entity.ShowPlayUrl;

					sb.Append("			<li>");
					sb.AppendFormat("				<a href=\"{0}\" target=\"_blank\" class=\"play-link\">播放</a>", filepath);
					sb.AppendFormat("				<a class=\"img\" href=\"{0}\" target=\"_blank\"><img data-original=\"{1}\" alt=\"{2}\"></a>", filepath, imgUrl, _serialInfo.BrandName + _serialInfo.Name + shortTitle);
					sb.AppendFormat("				<p><a href=\"{0}\" target=\"_blank\">{1}</a></p>", filepath, shortTitle);
					//sb.AppendFormat("<dl><dt>日期：{0}</dt></dl>", entity.Publishtime.ToString("yyyy-MM-dd"));
					sb.Append("			</li>");
				}
 				sb.Append("		</ul>");
				sb.Append("		<div class=\"clear\"></div>");
				sb.Append("</div></div>");
				
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
			else
			{
				bool success = CommonHtmlService.DeleteCommonHtml(
					  serialId,
					  CommonHtmlEnum.TypeEnum.Serial,
					  CommonHtmlEnum.TagIdEnum.SerialSummary,
					  CommonHtmlEnum.BlockIdEnum.Video);
			}
		}

        /// <summary>
		/// 生成块内容 入库  1200版 2016-09-28 lisf
		/// </summary>
		/// <param name="serialId">子品牌id</param>
		/// <param name="videoList">视频数据</param>
        private void MakeVideoBlockHtmlNew(int serialId, List<VideoEntityV2> videoList)
        {
            if (videoList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<div class=\"layout-2 video-section\" id=\"car-videobox\" data-channelid=\"2.21.812\">");

                sb.Append("<div class=\"section-header header2 mbl\">");
                sb.AppendFormat("<div class=\"box\"><h3><a href=\"/{1}/shipin/\" target=\"_blank\">{0}视频</a></h3></div>", _serialInfo.ShowName, _serialInfo.AllSpell);
                sb.Append("	<div class=\"more\">");
                sb.AppendFormat("<a href=\"http://car.bitauto.com/{0}/shipin/\" target=\"_blank\">全部视频&gt;&gt;</a>", _serialInfo.AllSpell);
                sb.Append("	</div>");
                sb.Append("</div>");

                sb.Append("	<div class=\"row col3-240-box\">");
                int loop = 0;
                //List<long> vedioIdList = new List<long>();
                foreach (VideoEntityV2 entity in videoList)
                {
                    loop++;
                    if (loop > 3) break;
                    //vedioIdList.Add(entity.VideoId);

                    string shortTitle = StringHelper.RemoveHtmlTag(entity.ShortTitle);
                    string imgUrl = entity.ImageLink;
                    if (imgUrl.Trim().Length == 0)
                        imgUrl = CommonData.CommonSettings.DefaultCarPic;
                    imgUrl = imgUrl.Replace("/Video/", "/newsimg-240-w0/Video/");
                    string filepath = entity.ShowPlayUrl;
                    TimeSpan dration = new TimeSpan(0, 0, entity.Duration);
                    string drationFortter = dration.TotalMinutes > 9 ? (Math.Floor(dration.TotalMinutes) + ":" + (dration.Seconds > 9 ? dration.Seconds.ToString() : ("0" + dration.Seconds)))
                        : ("0" + dration.Minutes + ":" + (dration.Seconds > 9 ? dration.Seconds.ToString() : ("0" + dration.Seconds)));

                    sb.AppendFormat("<div class=\"img-info-layout-vertical img-info-layout-video img-info-layout-vertical-240135\" data-type=\"{1}\" data-id=\"{0}\" data-channelid=\"2.21.812\">", entity.VideoId,entity.Source == 1 ? "vf":"v");
                    sb.Append("       <div class=\"img\">");
					sb.AppendFormat("           <a href=\"{0}\" target=\"_blank\"><img data-original=\"{1}\"></a>", filepath, imgUrl);
                    sb.Append("       </div>");
                    sb.Append("       <ul class=\"p-list\">");
                    sb.AppendFormat("           <li class=\"video\"><a href=\"{0}\" target=\"_blank\"></a></li>", filepath);
                    sb.AppendFormat("           <li class=\"name\"><a href=\"{0}\" target=\"_blank\">{1}</a></li>", filepath, shortTitle);
                    sb.AppendFormat("           <li class=\"num\"><span class=\"play\">0</span> <span class=\"comment\">0</span> <span class=\"time\">{0}</span></li>", drationFortter);
                    sb.Append("       </ul>");
                    sb.Append("   </div>");
                }
                sb.Append("</div></div>");
                //sb.AppendFormat("<script type=\"text/javascript\">var vedioIds = \"{0}\";</script>",string.Join(",", vedioIdList));
                bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
                {
                    ID = serialId,
                    TypeID = CommonHtmlEnum.TypeEnum.Serial,
                    TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                    BlockID = CommonHtmlEnum.BlockIdEnum.Video,
                    HtmlContent = sb.ToString(),
                    UpdateTime = DateTime.Now
                });
                if (!success) Log.WriteErrorLog("更新视频块失败：serialId:" + serialId);
            }
            else
            {
                bool success = CommonHtmlService.DeleteCommonHtml(
                      serialId,
                     CommonHtmlEnum.TypeEnum.Serial,
                     CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                     CommonHtmlEnum.BlockIdEnum.Video);
            }
        }
		/// <summary>
		/// 获取最新视频 及 最热视频2条
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		private List<VideoEntityV2> GetVideoNewList(int serialId)
		{
			List<VideoEntityV2> videoList = VideoService.GetVideoList(serialId, VideoEnum.VideoSource.All, 4);

			List<VideoEntityV2> hotVideoList = new List<VideoEntityV2>();
			XmlDocument xmlHotSerialVideo = new XmlDocument();
			xmlHotSerialVideo.Load(serialHotVideoPath);
			if (xmlHotSerialVideo != null)
			{
				XmlNode serialNode = xmlHotSerialVideo.SelectSingleNode("/root/Serial[@id=" + serialId + "]");
				if (serialNode != null)
				{
					int loop = 0;
					foreach (XmlNode node in serialNode.ChildNodes)
					{
						int videoId = ConvertHelper.GetInteger(node.InnerText);
						if (videoList.Take(2).ToList().Find(p => p.VideoId == videoId) != null) continue;
						VideoEntityV2 entity = VideoService.GetVideoByVideoId(videoId,0);//热门视频只出视频库的视频，李东确认
						if (entity != null)
						{
							loop++;
							if (loop > 2) break;
							hotVideoList.Add(entity);
						}
					}
				}
			}
			var lastList = videoList.Take(4 - hotVideoList.Count).ToList().Union(hotVideoList);
			return lastList.Take(4).ToList();
		}
		/// <summary>
		/// 获取90天子品牌最热视频
		/// </summary>
		private void GetSerialHotVideoXml()
		{
			Log.WriteLog("获取最热子品牌视频新闻");
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(string.Format(CommonData.CommonSettings.SerialCmsHotVideoNewUrl, 4, 90));
				CommonFunction.SaveXMLDocument(xmlDoc, serialHotVideoPath);
			}
			catch (System.Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
	}
}
