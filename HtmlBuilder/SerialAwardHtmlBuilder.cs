using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public class SerialAwardHtmlBuilder : BaseBuilder
    {
        public override void BuilderDataOrHtml(int objId)
        {
            try
            {
                Console.WriteLine("开始更新子品牌ID-{0}", objId);
                var awardsIds = AwardService.GetIds(objId); //得到该车系下的distinct奖品id
                var awards = AwardService.GetList(awardsIds, objId); //根据得到的奖品Id得到集合
				/*
                var htmlResult = CreateHtml(awards);
                if (string.IsNullOrEmpty(htmlResult))
                {
                    CommonHtmlService.DeleteCommonHtml(
                        objId,
                        CommonHtmlEnum.TypeEnum.Serial,
                        CommonHtmlEnum.TagIdEnum.SerialSummary,
                        CommonHtmlEnum.BlockIdEnum.SerialAward);

                }
                else
                {
                    var commonHtmlEntity = new CommonHtmlEntity
                    {
                        BlockID = CommonHtmlEnum.BlockIdEnum.SerialAward,
                        UpdateTime = DateTime.Now,
                        ID = objId,
                        TypeID = CommonHtmlEnum.TypeEnum.Serial,
                        TagID = CommonHtmlEnum.TagIdEnum.SerialSummary
                    };
                    commonHtmlEntity.HtmlContent = htmlResult;
                    CommonHtmlService.UpdateCommonHtml(commonHtmlEntity);
                }*/

                var htmlResultNew = CreateHtmlNew(awards);
                var commonHtmlEntityNew = new CommonHtmlEntity
                {
                    BlockID = CommonHtmlEnum.BlockIdEnum.SerialAward,
                    UpdateTime = DateTime.Now,
                    ID = objId,
                    TypeID = CommonHtmlEnum.TypeEnum.Serial,
                    TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew
                };
                commonHtmlEntityNew.HtmlContent = htmlResultNew;
                CommonHtmlService.UpdateCommonHtml(commonHtmlEntityNew);

                Console.WriteLine("结束更新子品牌ID-{0}", objId);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
                Console.WriteLine("更新子品牌ID-{0}出现异常", objId);
            }
        }

        /// <summary>
        /// 车系获奖经历 1200版 lisf 2016-10-10
        /// </summary>
        /// <param name="aeards"></param>
        /// <returns></returns>
        private string CreateHtmlNew(List<Award> awards)
        {
            var sbHtml = new StringBuilder();
            sbHtml.Append("<li class=\"li4\" id=\"honor-list\" data-channelid=\"2.21.827\">");
            sbHtml.Append("<span class=\"title\">获奖记录</span>");
            if (awards == null || awards.Count == 0)
            {
                sbHtml.Append("<h4 class=\"none\">暂无</h4>");
                sbHtml.Append("</li>");
                return sbHtml.ToString();
            }
            
            var sbJson = new StringBuilder("{");

            sbHtml.Append("<ul class=\"list honor-list\">");

            foreach (var award in awards)
            {
                sbHtml.AppendFormat("<li data-id=\"{1}\"><div class=\"honor-box\"><span><a href=\"http://car.bitauto.com/jiangxiang/{1}/\" target=\"_blank\" ><img src=\"http://image.bitauto.com/{0}\"/></a></span></div></li>"
                    , award.LogoUrl
                    , award.Id);
                var sbYearJson = new StringBuilder();
                foreach (YearInfo yearInfo in award.YearInfos)
                {
                    var descSb = new StringBuilder();
                    if( yearInfo.ChildAwardInfos != null)
                        yearInfo.ChildAwardInfos.ForEach(x => { descSb.Append(x.ChildAwardName).Append("，"); });
                    sbYearJson.AppendFormat("{{\"year\":\"{0}\",\"desc\":\"{1}\"}},", yearInfo.YearName, descSb.ToString().TrimEnd('，'));
                }
                sbJson.AppendFormat("\"{0}\":{{\"name\":\"{1}\",\"logo\":\"http://image.bitauto.com/{2}\",\"yearinfo\":[{3}]}}"
                    , award.Id
                    , award.Name
                    , award.LogoUrl
                    ,sbYearJson.ToString().TrimEnd(','));
                if (awards.IndexOf(award) != awards.Count -1)
                {
                    sbJson.Append(",");
                }
            }
            sbHtml.Append("</ul>");
            sbHtml.Append("<div class=\"popup-layout-1\" style=\"visibility: hidden;\"><div class=\"inner\"></div></div>");
            sbHtml.Append("</li>");
            sbJson.Append("}");
            sbHtml.AppendFormat("<script type=\"text/javascript\">var serialAwardJson={0};</script>", sbJson.ToString());
            return sbHtml.ToString();
        }

        private string CreateHtml(List<Award> awards)
        {
            if (awards == null || awards.Count == 0)
            {
                return string.Empty;
            }
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("<div class=\"line-box\">");
            stringBuilder.Append("<div class=\"side_title\">");
            stringBuilder.Append("<h4>车型获奖经历</h4>");
            stringBuilder.Append("</div>");
            stringBuilder.Append("<div class=\"award_box\" data-channelid=\"2.21.827\">");
            stringBuilder.Append("<ul>");
            foreach (var award in awards)
            {
                stringBuilder.Append("<li>");
                stringBuilder.Append("<a class=\"\" href=\"/jiangxiang/" + award.Id + "/\" target=\"_blank\"><img width=\"60px;\" height=\"60px;\" src=\"http://image.bitauto.com/" + award.LogoUrl + "\" alt=\"\"></a>");
                if (award.Name.Length > 20)
                {
                    stringBuilder.Append("<p><a href=\"/jiangxiang/" + award.Id + "/\" target=\"_blank\" >" + award.Name.Substring(0, 20) + "</a></p>");
                }
                else
                {
                    stringBuilder.Append("<p><a href=\"/jiangxiang/" + award.Id + "/\" target=\"_blank\" >" + award.Name + "</a></p>");
                }
                stringBuilder.Append("<em>");

                int flag = 0;
                string tContent = string.Empty;
                foreach (var yearInfo in award.YearInfos)
                {
                    var titleContent = new StringBuilder();
                    if (!string.IsNullOrEmpty(yearInfo.Remarks))
                    {
                        titleContent = new StringBuilder();
                        titleContent.AppendLine(yearInfo.Remarks);
                        ++flag;
                    }
                    if (yearInfo.ChildAwardInfos != null && yearInfo.ChildAwardInfos.Count > 0)
                    {
                        foreach (var childAwardInfo in yearInfo.ChildAwardInfos)
                        {
                            if (string.IsNullOrEmpty(childAwardInfo.CarRemark))
                            {
                                titleContent.AppendLine(childAwardInfo.ChildAwardName);
                            }
                            else
                            {
                                titleContent.AppendLine(childAwardInfo.ChildAwardName + "-" + childAwardInfo.CarRemark);
                            }
                            ++flag;
                            if (flag == 5)
                            {
                                tContent = titleContent.ToString();
                            }
                        }
                    }
                    var resultContent = string.Empty;
                    if (yearInfo.ChildAwardInfos != null && yearInfo.ChildAwardInfos.Count >= 5)
                    {
                        resultContent = tContent;
                    }
                    else
                    {
                        resultContent = titleContent.ToString();
                    }
                    stringBuilder.Append("<a target=\"_blank\" href=\"/jiangxiang/" + award.Id + "/" + yearInfo.YearName + "/#y" + yearInfo.YearName + "\" title=\"" + resultContent + "\">" + yearInfo.YearName + "</a>");
                    if (award.YearInfos.IndexOf(yearInfo) != award.YearInfos.Count - 1)
                    {
                        stringBuilder.Append("、");
                    }
                }
                stringBuilder.Append("</em>");
                stringBuilder.Append("</li>");
            }
            stringBuilder.Append("</ul>");
            stringBuilder.Append("<div class=\"clear\"></div>");
            stringBuilder.Append("</div>");
            stringBuilder.Append("</div>");
            return stringBuilder.ToString();
        }
    }


}
