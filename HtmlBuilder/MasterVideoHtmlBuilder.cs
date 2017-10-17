using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using System.Data;
using System.Xml;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common.NewsNumXml;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public class MasterVideoHtmlBuilder : BaseBuilder
    {

        public override void BuilderDataOrHtml(int objId)
        {
            try
            {
                List<VideoEntityV2> videoList = VideoService.GetVideoListByMasterId(objId, VideoEnum.VideoSource.All, 6);
                //主品牌视频块生成
                //MakeVideoBlockHtml(objId, videoList); 
                //主品牌视频块生成---1200改版 
                MakeVideoBlockHtmlFor1200(objId, videoList);
                //主品牌其他视频块生成
                //MakeVideoBlockHtmlOther(objId, videoList);
                //更新视频数
                UpdateVideoCount(objId);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
        /// <summary>
        /// 主品牌其他视频块生成
        /// </summary>
        /// <param name="masterId"></param>
        /// <param name="videoList"></param>
        private void MakeVideoBlockHtmlOther(int masterId, List<VideoEntity> videoList)
        {
            if (videoList.Count <= 0) return;
            StringBuilder contentString = new StringBuilder();

            string masterName = string.Empty;
            DataSet ds = MasterBrandService.GetMasterBrandDataById(masterId);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                masterName = ds.Tables[0].Rows[0]["bs_Name"].ToString();
            }
            contentString.AppendFormat("<div class=\"col-all\"><div class=\"line_box car0514_04\"><h3><span>{0}汽车最新视频</span></h3><div class=\"pic_album\"><ul class=\"list_pic\">", masterName);
            foreach (VideoEntity entity in videoList.Take(5))
            {
                string title = entity.Title;
                string facetitle = entity.ShortTitle;
                string url = entity.ShowPlayUrl;
                string pictureUrl = entity.ImageLink;
                pictureUrl = pictureUrl.Replace("/Video/", "/newsimg-165-w0/Video/");

                contentString.AppendFormat("<li><a target=\"_blank\" href=\"{0}\"><img height=\"110\" width=\"165\" alt=\"{1}\" src=\"{2}\"></a>"
                                           + "<div class=\"name\"><a target=\"_blank\" title=\"{1}\" href=\"{0}\">{3}</a></div></li>"
                                           , url
                                           , title
                                           , pictureUrl
                                           , facetitle);
            }
            contentString.Append("</ul><div class=\"clear\"></div></div></div></div>");
            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = masterId,
                TypeID = CommonHtmlEnum.TypeEnum.Master,
                TagID = CommonHtmlEnum.TagIdEnum.MasterBrandPageOther,
                BlockID = CommonHtmlEnum.BlockIdEnum.Video,
                HtmlContent = contentString.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新主品牌其他页面视频块失败：masterId:" + masterId);
        }
        /// <summary>
        /// 主品牌视频块生成  旧版
        /// </summary>
        /// <param name="masterId"></param>
        /// <param name="videoList"></param>
        //private void MakeVideoBlockHtml(int masterId, List<VideoEntity> videoList)
        //{
        //    StringBuilder htmlCode = new StringBuilder();
        //    //原创节目（id：47）
        //    List<VideoEntity> originalVideoList = VideoService.GetVideoListByMasterIdAndCategoryId(masterId, VideoEnum.CategoryTypeEnum.All, 47, 1);
        //    if (originalVideoList.Count > 0 && videoList.Find(p => p.CategoryId == 47) == null)
        //    {
        //        List<VideoEntity> resultList = videoList.Take(videoList.Count - 1).ToList();
        //        resultList.AddRange(originalVideoList);
        //        resultList.Sort((p1, p2) => DateTime.Compare(p2.Publishtime, p1.Publishtime));
        //        videoList = resultList;
        //    }
        //    if (videoList.Count > 0)
        //    {
        //        string masterName = string.Empty;
        //        DataSet ds = MasterBrandService.GetMasterBrandDataById(masterId);
        //        if (ds != null && ds.Tables[0].Rows.Count > 0)
        //        {
        //            masterName = ds.Tables[0].Rows[0]["bs_Name"].ToString();
        //        }
        //        htmlCode.AppendLine("<div class=\"line-box\">");
        //        htmlCode.AppendFormat("<div class=\"title-con\"><div class=\"title-box\"><h3><a target=\"_blank\" href=\"http://v.bitauto.com/car/master/{0}.html\">{1}-汽车视频</a></h3>",
        //            masterId,
        //            masterName);
        //        htmlCode.AppendFormat(
        //            "<div class=\"more\"><a rel=\"nofollow\"  href=\"http://v.bitauto.com/car/master/{0}.html\" target=\"_blank\">更多>></a></div></div></div>"
        //            , masterId.ToString());
        //        htmlCode.AppendLine("<div class=\"theme_list\"><UL id=\"ul_themeList\" class=\"video_list\">");

        //        foreach (VideoEntity entity in videoList)
        //        {
        //            string videoTitle = entity.Title;
        //            string faceTitle = entity.ShortTitle;
        //            string shortTitle = StringHelper.SubString(StringHelper.RemoveHtmlTag(faceTitle), 16, true);
        //            if (shortTitle.StartsWith(faceTitle) || shortTitle.Length - faceTitle.Length > 1)
        //                shortTitle = faceTitle;

        //            string imgUrl = entity.ImageLink;
        //            if (imgUrl.Trim().Length == 0)
        //                imgUrl = "http://car.bitauto.com/images/vedioImage.gif";
        //            imgUrl = imgUrl.Replace("/Video/", "/newsimg-210-w0/Video/");

        //            string filepath = entity.ShowPlayUrl;
        //            int duration = entity.Duration;
        //            htmlCode.AppendFormat(
        //                "<li><a class=\"play-link\" target=\"_blank\" href=\"{0}\"></a><a class=\"img\" href=\"{0}\" target=\"_blank\" rel=\"nofollow\"><img src=\"{1}\" alt=\"{2}\"  /></a>",
        //                filepath, imgUrl, videoTitle);
        //            htmlCode.AppendFormat("<p><a target=\"_blank\" href=\"{0}\">{1}</a></p>", filepath, videoTitle);
        //            DateTime dt = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")).AddSeconds(duration);
        //            htmlCode.AppendFormat(
        //                "<dl><dt>日期：{0}</dt></dl></li>",
        //                entity.Publishtime.ToShortDateString());
        //        }
        //        htmlCode.AppendLine("</UL></div></div>");
        //        //htmlCode.AppendFormat("<div class=\"more\"><a target=\"_blank\" href=\"http://v.bitauto.com/car/master/{0}.html\" rel=\"nofollow\">更多>></a></div>", masterId.ToString());
        //        //htmlCode.AppendLine("</div>");
        //        bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
        //        {
        //            ID = masterId,
        //            TypeID = CommonHtmlEnum.TypeEnum.Master,
        //            TagID = CommonHtmlEnum.TagIdEnum.MasterBrandPage,
        //            BlockID = CommonHtmlEnum.BlockIdEnum.Video,
        //            HtmlContent = htmlCode.ToString(),
        //            UpdateTime = DateTime.Now
        //        });
        //        if (!success) Log.WriteErrorLog("更新主品牌视频块失败：masterId:" + masterId);
        //    }
        //    else
        //    {
        //        bool success = CommonHtmlService.DeleteCommonHtml(
        //              masterId,
        //              CommonHtmlEnum.TypeEnum.Master,
        //              CommonHtmlEnum.TagIdEnum.MasterBrandPage,
        //              CommonHtmlEnum.BlockIdEnum.Video);
        //        //if (!success) Log.WriteErrorLog("删除主品牌视频块失败：masterId:" + masterId);
        //    }
        //}
        private void MakeVideoBlockHtmlFor1200(int masterId, List<VideoEntityV2> videoList)
        {
            StringBuilder htmlCode = new StringBuilder();
            //原创节目（id：47）
            List<VideoEntityV2> originalVideoList = VideoService.GetVideoListByMasterIdAndCategoryId(masterId, VideoEnum.VideoSource.All, 47, 1);
            if (originalVideoList.Count > 0 && videoList.Find(p => p.CategoryId == 47) == null)
            {
                List<VideoEntityV2> resultList = videoList.Take(videoList.Count - 1).ToList();
                resultList.AddRange(originalVideoList);
                resultList.Sort((p1, p2) => DateTime.Compare(p2.Publishtime, p1.Publishtime));
                videoList = resultList;
            }
            if (videoList.Count > 0)
            {
                string masterName = string.Empty;
                DataSet ds = MasterBrandService.GetMasterBrandDataById(masterId);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    masterName = ds.Tables[0].Rows[0]["bs_Name"].ToString();
                }
                htmlCode.AppendLine("<div class=\"row video-section margin-bottom-xlg\">");
                htmlCode.AppendFormat("<div class=\"section-header header2 mb0\"><div class=\"box\"><h2><a target=\"_blank\" href=\"http://v.bitauto.com/car/master/{0}.html\">{1}-汽车视频</a></h2></div>",
                    masterId,
                    masterName);
                htmlCode.AppendFormat(
                    "<div class=\"more\"><a rel=\"nofollow\"  href=\"http://v.bitauto.com/car/master/{0}.html\" target=\"_blank\">更多>></a></div>"
                    , masterId.ToString());
                htmlCode.AppendLine("</div>");
                htmlCode.AppendLine("<div class=\"list-box col3-240-box clearfix\">");

                foreach (VideoEntityV2 entity in videoList)
                {
                    string videoTitle = entity.Title;
                    string faceTitle = entity.ShortTitle;
                    string shortTitle = StringHelper.SubString(StringHelper.RemoveHtmlTag(faceTitle), 16, true);
                    if (shortTitle.StartsWith(faceTitle) || shortTitle.Length - faceTitle.Length > 1)
                        shortTitle = faceTitle;
                    string imgUrl = entity.ImageLink;
                    if (imgUrl.Trim().Length == 0)
                        imgUrl = "http://car.bitauto.com/images/vedioImage.gif";
                    imgUrl = imgUrl.Replace("/Video/", "/newsimg-240-w0/Video/");

                    string filepath = entity.ShowPlayUrl;
                    TimeSpan dration = new TimeSpan(0, 0, entity.Duration);
                    string drationFortter = dration.TotalMinutes > 9 ? (Math.Floor(dration.TotalMinutes) + ":" + (dration.Seconds > 9 ? dration.Seconds.ToString() : ("0" + dration.Seconds)))
                        : ("0" + dration.Minutes + ":" + (dration.Seconds > 9 ? dration.Seconds.ToString() : ("0" + dration.Seconds)));
                    htmlCode.AppendLine(string.Format("<div class=\"img-info-layout-vertical img-info-layout-video img-info-layout-vertical-240135\" data-videoid=\"{0}\">",entity.VideoId));
                    htmlCode.AppendFormat(
                        " <div class=\"img\"><a href=\"{0}\" target=\"_blank\" rel=\"nofollow\"><img src=\"{1}\" alt=\"{2}\"  /></a></div>",
                        filepath, imgUrl, videoTitle);
                    htmlCode.AppendLine("<ul class=\"p-list\">");
                    htmlCode.AppendFormat("<li class=\"video\"><a target=\"_blank\" href=\"{0}\"></a></li>", filepath);
                    htmlCode.AppendFormat("<li class=\"name\"><a target=\"_blank\" href=\"{0}\">{1}</a></li>", filepath, videoTitle);
                    htmlCode.AppendFormat("<li class=\"num\"><span class=\"play\">0</span> <span class=\"comment\">0</span> <span class=\"time\">{0}</span></li>", drationFortter);
                    htmlCode.AppendLine("</ul>");
                    htmlCode.AppendLine("</div>");
                }
                htmlCode.AppendLine("</div>");
                htmlCode.AppendLine("</div>");
                bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
                {
                    ID = masterId,
                    TypeID = CommonHtmlEnum.TypeEnum.Master,
                    TagID = CommonHtmlEnum.TagIdEnum.MasterBrandPageV2,
                    BlockID = CommonHtmlEnum.BlockIdEnum.Video,
                    HtmlContent = htmlCode.ToString(),
                    UpdateTime = DateTime.Now
                });
                if (!success) Log.WriteErrorLog("更新主品牌视频块失败：masterId:" + masterId);
            }
            else
            {
                bool success = CommonHtmlService.DeleteCommonHtml(
                      masterId,
                      CommonHtmlEnum.TypeEnum.Master,
                      CommonHtmlEnum.TagIdEnum.MasterBrandPageV2,
                      CommonHtmlEnum.BlockIdEnum.Video);
                //if (!success) Log.WriteErrorLog("删除主品牌视频块失败：masterId:" + masterId);
            }
        }
        /// <summary>
        /// 更新品牌视频数
        /// </summary>
        /// <param name="brandId"></param>
        private void UpdateVideoCount(int masterId)
        {
            try
            {
                XmlDocument newsNumDoc = NewsNumXmlDocument.GetNewsNumXmlDocument(CarTypes.MasterBrand);
                string xmlTag = CarTypes.MasterBrand.ToString();
                XmlElement xmlEle = newsNumDoc.SelectSingleNode(string.Format("/root/{0}[@ID='{1}']", xmlTag, masterId)) as XmlElement;
                int count = VideoService.GetVideoCountByMasterId(masterId, VideoEnum.VideoSource.All);
                if (xmlEle == null)
                {
                    xmlEle = newsNumDoc.CreateElement(xmlTag);
                    xmlEle.SetAttribute("ID", masterId.ToString());
                    newsNumDoc.DocumentElement.AppendChild(xmlEle);
                }
                xmlEle.SetAttribute(CarNewsTypes.video.ToString().ToLower(), count.ToString());
                NewsNumXmlDocument.SaveNewsNumXmlDocument(CarTypes.MasterBrand);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
    }
}
