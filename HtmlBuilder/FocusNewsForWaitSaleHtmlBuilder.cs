using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public class FocusNewsForWaitSaleHtmlBuilder
    {
        private SerialInfo _serialInfo;

        public void BuilderDataOrHtml(int objId)
        {
            try
            {
                _serialInfo = CommonData.SerialDic[objId];

                if (_serialInfo.CsSaleState == "待销")
                {
                    //GenerateHtml(objId);

                    GenerateHtmlNew(objId); //1200版
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
        #region  1200版 lisf 2016-10-19
        /// <summary>
        /// 未上市车系 车型详解及焦点图新闻
        /// </summary>
        /// <param name="serialId"></param>
        private void GenerateHtmlNew(int serialId)
        {
            if (!CommonData.SerialDic.ContainsKey(serialId)) return;

            StringBuilder sb = new StringBuilder();
            var pingceHtml = this.GetPingCeHtmlNew(serialId);
            if (!string.IsNullOrEmpty(pingceHtml))
            {
                var focusHtml = this.BuildFocusNewsHtml(serialId, 6);
                sb.Append(pingceHtml);
                sb.Append(focusHtml);
            }
            else
            {
                var focusHtml = this.BuildFocusNewsHtml(serialId, 7);
                sb.Append(focusHtml);
            }

            bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = serialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                BlockID = CommonHtmlEnum.BlockIdEnum.FocusNewsForWaitSale,
                HtmlContent = sb.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("1200版待销子品牌综述页焦点新闻更新失败，func：GenerateHtml，serialId:" + serialId);
        }
        /// <summary>
        /// 评测块 标签 内容
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        private string GetPingCeHtmlNew(int serialId)
        {
            Dictionary<int, PingCeTagEntity> dicAllTagInfo = CarPingceInfoService.GetPingceTagsBySerialId(serialId);
            if (dicAllTagInfo.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                string[] tagNameArr = { "外观", "内饰", "空间", "动力", "操控", "配置", "总结" };
                sb.Append("<div class=\"head row\">");
                sb.Append("<div class=\"col-auto\">");
				sb.AppendFormat("<a href=\"/{0}/pingce/\" class=\"tag type-1\" target=\"_blank\">车型详解</a>", _serialInfo.AllSpell);
                sb.Append("</div>");
                sb.Append("<div class=\"col-auto\"><ul class=\"list list-gapline sm\">");

                //取标签的页码
                Dictionary<string, int> dictTagPageNumber = new Dictionary<string, int>();
                int tempPageNum = 0;
                foreach (KeyValuePair<int, PingCeTagEntity> kvp in dicAllTagInfo)
                {
                    tempPageNum++;
                    dictTagPageNumber.Add(kvp.Value.tagName, tempPageNum);
                }

                List<string> pingceTagList = new List<string>();
                foreach (string tagName in tagNameArr)
                {
                    var kvp = dicAllTagInfo.FirstOrDefault(pire => pire.Value.tagName == tagName);
                    if (kvp.Key <= 0 || kvp.Value == null)
                    {
						pingceTagList.Add(string.Format("<li><a>{0}</a></li>", tagName));
                    }
                    else
                    {
						pingceTagList.Add(string.Format("<li><a href=\"/{0}/pingce/{2}/\" class=\"current\" target=\"_blank\">{1}</a></li>",
                            _serialInfo.AllSpell,
                            kvp.Value.tagName,
                            dictTagPageNumber[kvp.Value.tagName]));
                    }
                }

                sb.Append(string.Join("", pingceTagList.ToArray()));

                sb.Append("</ul></div>");
                sb.Append("</div>");
                return sb.ToString();
            }
            return string.Empty;
        }

        private string BuildFocusNewsHtml(int serialId, int top)
        {
            //编辑设置排序，根据设置先按照设置位置生成列表，未设置位置由null代替，再由实际数据补全空位
            Dictionary<int, NewsEntity> orderNewsList = FocusNewsService.GetOrderNewsList(serialId);

            //新版总数也焦点新闻
            List<NewsEntity> focusNewsNewList = FocusNewsService.GetFocusNewsListNew(orderNewsList, serialId, top);

            StringBuilder sb = new StringBuilder();
            string serialSpell = CommonData.SerialDic[serialId].AllSpell.ToLower();
			sb.Append(" <div class=\"cont\" id=\"focusNewsContent\"><div class=\"list-txt-layout1\"><div class=\"list-txt list-txt-m list-txt-default list-txt-style2 type-1\"><ul>");
            if (focusNewsNewList == null || focusNewsNewList.Count < 1)
            {
                sb.Append("<li><div class=\"txt\"><a>暂无内容</a></div></li>");
            }
            else
            {
                string baseUrl = string.Format("/{0}/{1}/", serialSpell, "{0}");

                foreach (NewsEntity entity in focusNewsNewList)
                {
                    string cateUrl = null;

                    string newsCategory = entity.NewsCategoryShowName.CategoryShowName;
                    if (entity.NewsCategoryShowName.CategoryUrl == NewsCategoryConfig.QitaCategoryKey || entity.NewsCategoryShowName.CategoryUrl == "#" || entity.NewsCategoryShowName.CategoryKey == "huati")
                    {
                        sb.AppendFormat("<li><div class=\"txt\">");
						sb.AppendFormat("<strong><a class=\"no-link\">{0}</a>|</strong><a href=\"{1}\" target=\"_blank\">{2}</a>"
                            , newsCategory
                            , entity.PageUrl
                            , entity.Title);
                        sb.AppendFormat("</div><span>{0}</span></li>", entity.PublishTime.ToString("MM-dd"));
                    }
                    else
                    {
                        cateUrl = string.Format(baseUrl, entity.NewsCategoryShowName.CategoryUrl);
                        sb.AppendFormat("<li><div class=\"txt\">");
                        sb.AppendFormat("<strong><a href=\"{0}\" target=\"_blank\">{1}</a>|</strong><a href=\"{2}\" target=\"_blank\"{4}>{3}</a>"
                            , cateUrl
                            , newsCategory
                            , entity.PageUrl
                            , entity.Title
                            , entity.NewsCategoryShowName.CategoryUrl == "shipin" ? " class=\"video\"" : string.Empty);
                        sb.AppendFormat("</div><span>{0}</span></li>", entity.PublishTime.ToString("MM-dd"));
                    }
                }
            }
            sb.Append("</ul></div></div></div>");
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 生成块内容
        /// </summary>
        /// <param name="serialId"></param>
        private void GenerateHtml(int serialId)
        {
            if (!CommonData.SerialDic.ContainsKey(serialId)) return;
             
            StringBuilder sb = new StringBuilder();
            var pingceHtml = this.GetPingCeHtml(serialId);
            if (!string.IsNullOrEmpty(pingceHtml))
            {
                var focusHtml = this.BuildFocusNewsHtmlNew(serialId, 6, true);
                sb.Append(pingceHtml);
                sb.Append(focusHtml);
            }
            else
            {
                var focusHtml = this.BuildFocusNewsHtmlNew(serialId, 8, false);
                sb.Append(focusHtml);
            }

            bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = serialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.FocusNewsForWaitSale,
                HtmlContent = sb.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("待销子品牌综述页焦点新闻更新失败，func：GenerateHtml，serialId:" + serialId);
        }

        /// <summary>
        /// 新闻内容
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="top">取新闻多少条</param>
        /// <returns></returns>
        private string BuildFocusNewsHtmlNew(int serialId, int top, bool existPingce)
        {
            //编辑设置排序，根据设置先按照设置位置生成列表，未设置位置由null代替，再由实际数据补全空位
            Dictionary<int, NewsEntity> orderNewsList = FocusNewsService.GetOrderNewsList(serialId);

            //新版总数也焦点新闻
            List<NewsEntity> focusNewsNewList = FocusNewsService.GetFocusNewsListNew(orderNewsList, serialId, top);

            StringBuilder sb = new StringBuilder();
            string serialSpell = CommonData.SerialDic[serialId].AllSpell.ToLower();
            sb.Append("<div class=\"new-list-box list-p-sty\">");
            if (focusNewsNewList == null || focusNewsNewList.Count < 1)
            {
                sb.AppendFormat("<ul class=\"list_date {0}\">", existPingce ? "h149" : "h199");
                sb.Append("<li><span>暂无内容</span></li>");
                sb.Append("</ul>");
            }
            else
            {
                string baseUrl = string.Format("/{0}/{1}/", serialSpell, "{0}");

                sb.AppendFormat("<ul class=\"list_date {0}\">", existPingce ? "h149" : "h199");
                foreach (NewsEntity entity in focusNewsNewList)
                {
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

            }
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// 评测块 标签 内容
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        private string GetPingCeHtml(int serialId)
        {
            Dictionary<int, PingCeTagEntity> dicAllTagInfo = CarPingceInfoService.GetPingceTagsBySerialId(serialId);
            if (dicAllTagInfo.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                //固定内容输出 by sk 2014.01.10
                string[] tagNameArr = { "外观", "内饰", "空间", "动力", "操控", "配置", "总结" };
                sb.Append("<div class=\"new-tab-box\">");
                sb.Append("<span>");
                sb.AppendFormat("<a href=\"/{0}/pingce/\" target=\"_blank\">车型详解</a>", _serialInfo.AllSpell);
                sb.Append("</span>");
                sb.Append("<div class=\"link-box\">");


                //取标签的页码
                Dictionary<string, int> dictTagPageNumber = new Dictionary<string, int>();
                int tempPageNum = 0;
                foreach (KeyValuePair<int, PingCeTagEntity> kvp in dicAllTagInfo)
                {
                    tempPageNum++;
                    dictTagPageNumber.Add(kvp.Value.tagName, tempPageNum);
                }

                List<string> pingceTagList = new List<string>();
                foreach (string tagName in tagNameArr)
                {
                    var kvp = dicAllTagInfo.FirstOrDefault(pire => pire.Value.tagName == tagName);
                    if (kvp.Key <= 0 || kvp.Value == null)
                    {
                        pingceTagList.Add(string.Format("<em>{0}</em>", tagName));
                    }
                    else
                    {
                        pingceTagList.Add(string.Format("<a href=\"/{0}/pingce/{2}/\" target=\"_blank\">{1}</a>",
                            _serialInfo.AllSpell,
                            kvp.Value.tagName,
                            dictTagPageNumber[kvp.Value.tagName]));
                    }
                }

                sb.Append(string.Join("<i>|</i>", pingceTagList.ToArray()));

                sb.Append("</div>");
                sb.Append("</div>");
                return sb.ToString();
            }
            return string.Empty;
        }
    }
}
