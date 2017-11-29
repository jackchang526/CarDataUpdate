using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.Utils;
using System.Data;
using System.Xml;
using BitAuto.CarDataUpdate.Common.NewsNumXml;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public class BrandVideoHtmlBuilder : BaseBuilder
    {

        public override void BuilderDataOrHtml(int objId)
        {
            try
            {
                List<VideoEntityV2> videoList = VideoService.GetVideoListByBrandId(objId, VideoEnum.VideoSource.All, 6);
                //生成品牌视频块  
                //MakeVideoBlockHtml(objId, videoList);
                //生成品牌视频块 For 1200
                MakeVideoBlockHtmlFor1200(objId, videoList);
                //生成品牌其他视频
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
        /// 生成品牌其他视频
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="videoList"></param>
        private void MakeVideoBlockHtmlOther(int brandId, List<VideoEntity> videoList)
        {
            if (videoList.Count <= 0) return;
            StringBuilder contentString = new StringBuilder();
            string brandName = string.Empty;
            DataSet ds = BrandService.GetBrandDataById(brandId);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                brandName = ds.Tables[0].Rows[0]["cb_Name"].ToString();
            }
            contentString.AppendFormat("<div class=\"col-all\"><div class=\"line_box car0514_04\"><h3><span>{0}汽车最新视频</span></h3><div class=\"pic_album\"><ul class=\"list_pic\">", brandName);
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
                ID = brandId,
                TypeID = CommonHtmlEnum.TypeEnum.Brand,
                TagID = CommonHtmlEnum.TagIdEnum.BrandPageOther,
                BlockID = CommonHtmlEnum.BlockIdEnum.Video,
                HtmlContent = contentString.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新品牌视频块失败：brandId:" + brandId);
        }
        /// <summary>
        /// 生成品牌视频块  旧版
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="videoList"></param>
        //private void MakeVideoBlockHtml(int brandId, List<VideoEntity> videoList)
        //{
        //    StringBuilder htmlCode = new StringBuilder();

        //    //原创节目（id：47）
        //    List<VideoEntity> originalVideoList = VideoService.GetVideoListByBrandIdAndCategoryId(brandId, VideoEnum.CategoryTypeEnum.All, 47, 1);
        //    if (originalVideoList.Count > 0 && videoList.Find(p => p.CategoryId == 47) == null)
        //    {
        //        List<VideoEntity> resultList = videoList.Take(videoList.Count - 1).ToList();
        //        resultList.AddRange(originalVideoList);
        //        resultList.Sort((p1, p2) => DateTime.Compare(p2.Publishtime, p1.Publishtime));
        //        videoList = resultList;
        //    }
        //    if (videoList.Count > 0)
        //    {
        //        string brandName = string.Empty;
        //        DataSet ds = BrandService.GetBrandDataById(brandId);
        //        if (ds != null && ds.Tables[0].Rows.Count > 0)
        //        {
        //            brandName = ds.Tables[0].Rows[0]["cb_Name"].ToString();
        //        }
        //        htmlCode.AppendLine("<div class=\"title-con\"><div class=\"title-box\">");
        //        htmlCode.AppendFormat("<h3><a target=\"_blank\" href=\"http://v.bitauto.com/car/brand/{0}.html\">{1}-视频</a></h3>",
        //            brandId.ToString(),
        //            brandName.Replace("·", "&bull;"));
        //        htmlCode.AppendFormat(
        //           "<div class=\"more\"><a rel=\"nofollow\"  href=\"http://v.bitauto.com/car/brand/{0}.html\" target=\"_blank\">更多>></a></div></div></div>"
        //           , brandId);
        //        htmlCode.AppendLine("<div class=\"theme_list\"><UL id=\"ul_themeList\" class=\"video_list\">");

        //        foreach (VideoEntity entity in videoList)
        //        {
        //            string videoTitle = entity.Title;
        //            string shortTitle = entity.ShortTitle;
        //            shortTitle = StringHelper.SubString(StringHelper.RemoveHtmlTag(shortTitle), 16, true);

        //            string imgUrl = entity.ImageLink;
        //            if (imgUrl.Trim().Length == 0)
        //                imgUrl = "http://car.bitauto.com/images/vedioImage.gif";
        //            imgUrl = imgUrl.Replace("/Video/", "/newsimg-210-w0/Video/");
        //            string filepath = entity.ShowPlayUrl;

        //            int duration = entity.Duration;
        //            DateTime dt = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")).AddSeconds(duration);

        //            //htmlCode.AppendFormat(
        //            //    "<li><a href=\"{0}\" target=\"_blank\" class=\"v_bg\" alt=\"视频播放\" rel=\"nofollow\"></a><a href=\"{0}\" target=\"_blank\" rel=\"nofollow\"><img src=\"{1}\" alt=\"{2}\" width=\"120\" height=\"80\" /></a>",
        //            //    filepath, imgUrl, videoTitle);

        //            //htmlCode.AppendFormat(
        //            //    "<a class=\"title\" href=\"{0}\" title=\"{1}\" target=\"_blank\">{2}</a><div>时长：{3}</div></li>",
        //            //    filepath, videoTitle, shortTitle, dt.ToString("HH:mm:ss"));

        //            htmlCode.AppendFormat(
        //                "<li><a class=\"play-link\" target=\"_blank\" href=\"{0}\"></a><a class=\"img\" href=\"{0}\" target=\"_blank\" rel=\"nofollow\"><img src=\"{1}\" alt=\"{2}\"  /></a>",
        //                filepath, imgUrl, videoTitle);
        //            htmlCode.AppendFormat("<p><a target=\"_blank\" href=\"{0}\">{1}</a></p>", filepath, videoTitle);
        //            htmlCode.AppendFormat(
        //                "<dl><dt>日期：{0}</dt></dl></li>",
        //                entity.Publishtime.ToShortDateString());

        //        }
        //        htmlCode.AppendLine("</UL>");
        //        htmlCode.AppendLine("</div>");
        //        bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
        //        {
        //            ID = brandId,
        //            TypeID = CommonHtmlEnum.TypeEnum.Brand,
        //            TagID = CommonHtmlEnum.TagIdEnum.BrandPage,
        //            BlockID = CommonHtmlEnum.BlockIdEnum.Video,
        //            HtmlContent = htmlCode.ToString(),
        //            UpdateTime = DateTime.Now
        //        });
        //        if (!success) Log.WriteErrorLog("更新品牌视频块失败：brandId:" + brandId);
        //    }
        //    else
        //    {
        //        bool success = CommonHtmlService.DeleteCommonHtml(
        //              brandId,
        //              CommonHtmlEnum.TypeEnum.Brand,
        //              CommonHtmlEnum.TagIdEnum.BrandPage,
        //              CommonHtmlEnum.BlockIdEnum.Video);
        //        //if (!success) Log.WriteErrorLog("删除品牌视频块失败：brandId:" + brandId);
        //    }
        //}
        private void MakeVideoBlockHtmlFor1200(int brandId, List<VideoEntityV2> videoList)
        {
            StringBuilder htmlCode = new StringBuilder();

            //原创节目（id：47）
            List<VideoEntityV2> originalVideoList = VideoService.GetVideoListByBrandIdAndCategoryId(brandId, VideoEnum.VideoSource.All, 47, 1);
            if (originalVideoList.Count > 0 && videoList.Find(p => p.CategoryId == 47) == null)
            {
                List<VideoEntityV2> resultList = videoList.Take(videoList.Count - 1).ToList();
                resultList.AddRange(originalVideoList);
                resultList.Sort((p1, p2) => DateTime.Compare(p2.Publishtime, p1.Publishtime));
                videoList = resultList;
            }
            if (videoList.Count > 0)
            {
                string brandName = string.Empty;
                DataSet ds = BrandService.GetBrandDataById(brandId);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    brandName = ds.Tables[0].Rows[0]["cb_Name"].ToString();
                }
                htmlCode.AppendLine("<div class=\"row video-section margin-bottom-xlg\">");
                htmlCode.AppendFormat("<div class=\"section-header header2 mb0\"><div class=\"box\"><h2><a target=\"_blank\" href=\"http://v.bitauto.com/car/brand/{0}.html\">{1}-视频</a></h2></div>",
                    brandId.ToString(),
                    brandName.Replace("·", "&bull;"));
                htmlCode.AppendFormat(
                    "<div class=\"more\"><a rel=\"nofollow\"  href=\"http://v.bitauto.com/car/brand/{0}.html\" target=\"_blank\">更多>></a></div>"
                    , brandId.ToString());
                htmlCode.AppendLine("</div>");
                htmlCode.AppendLine("<div class=\"list-box col3-240-box clearfix\">");

                foreach (VideoEntityV2 entity in videoList)
                {
                    string videoTitle = entity.Title;
                    string shortTitle = entity.ShortTitle;
                    shortTitle = StringHelper.SubString(StringHelper.RemoveHtmlTag(shortTitle), 16, true);

                    string imgUrl = entity.ImageLink;
                    if (imgUrl.Trim().Length == 0)
                        imgUrl = "http://car.bitauto.com/images/vedioImage.gif";
                    imgUrl = imgUrl.Replace("/Video/", "/newsimg-240-w0/Video/");  // "/newsimg-210-w0/Video/
                    string filepath = entity.ShowPlayUrl;
                    TimeSpan dration = new TimeSpan(0, 0, entity.Duration);
                    string drationFortter = dration.TotalMinutes > 9 ? (Math.Floor(dration.TotalMinutes) + ":" + (dration.Seconds > 9 ? dration.Seconds.ToString() : ("0" + dration.Seconds)))
                        : ("0" + dration.Minutes + ":" + (dration.Seconds > 9 ? dration.Seconds.ToString() : ("0" + dration.Seconds)));
                    htmlCode.AppendLine(string.Format("<div class=\"img-info-layout-vertical img-info-layout-video img-info-layout-vertical-240135\" data-type=\"{1}\" data-videoid=\"{0}\">",entity.VideoId,entity.Source == 1 ? "vf":"v"));
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
                    ID = brandId,
                    TypeID = CommonHtmlEnum.TypeEnum.Brand,
                    TagID = CommonHtmlEnum.TagIdEnum.BrandPageV2,
                    BlockID = CommonHtmlEnum.BlockIdEnum.Video,
                    HtmlContent = htmlCode.ToString(),
                    UpdateTime = DateTime.Now
                });
                if (!success) Log.WriteErrorLog("更新品牌视频块失败：brandId:" + brandId);
            }
            else
            {
                bool success = CommonHtmlService.DeleteCommonHtml(
                      brandId,
                      CommonHtmlEnum.TypeEnum.Brand,
                      CommonHtmlEnum.TagIdEnum.BrandPageV2,
                      CommonHtmlEnum.BlockIdEnum.Video);
                //if (!success) Log.WriteErrorLog("删除品牌视频块失败：brandId:" + brandId);
            }
        }
        /// <summary>
        /// 更新品牌视频数
        /// </summary>
        /// <param name="brandId"></param>
        private void UpdateVideoCount(int brandId)
        {
            try
            {
                XmlDocument newsNumDoc = NewsNumXmlDocument.GetNewsNumXmlDocument(CarTypes.Brand);
                string xmlTag = CarTypes.Brand.ToString();
                XmlElement xmlEle = newsNumDoc.SelectSingleNode(string.Format("/root/{0}[@ID='{1}']", xmlTag, brandId)) as XmlElement;
                int count = VideoService.GetVideoCountByBrandId(brandId, VideoEnum.CategoryTypeEnum.All);
                if (xmlEle == null)
                {
                    xmlEle = newsNumDoc.CreateElement(xmlTag);
                    xmlEle.SetAttribute("ID", brandId.ToString());
                    newsNumDoc.DocumentElement.AppendChild(xmlEle);
                }
                xmlEle.SetAttribute(CarNewsTypes.video.ToString().ToLower(), count.ToString());
                NewsNumXmlDocument.SaveNewsNumXmlDocument(CarTypes.Brand);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
    }
}
