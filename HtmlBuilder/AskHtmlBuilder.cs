using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    /// <summary>
    ///     生成HTML问答块
    /// </summary>
    public class AskHtmlBuilder : BaseBuilder
    {
        public override void BuilderDataOrHtml(int objId)
        {
        }

        /// <summary>
        ///     生成车型首页问答HTML
        ///     author:songcl date:2014-11-27
        /// </summary>
        public void BuilderDefaultAskHtml()
        {
            string askDefault = CommonData.CommonSettings.AskDefaultUrl; //正式环境
            //const string askDefault = "http://192.168.2.83/askdata/question/getlist.xml?status=2&top=8"; //测试环境

            var commonHtmlEntity = new CommonHtmlEntity
            {
                BlockID = CommonHtmlEnum.BlockIdEnum.Ask,
                UpdateTime = DateTime.Now,
                ID = 1, //规定
                TypeID = CommonHtmlEnum.TypeEnum.Other,
                TagID = CommonHtmlEnum.TagIdEnum.CarDefault
            };
            commonHtmlEntity.HtmlContent = GetDefaultHtml(askDefault);
            UpdateHtml(commonHtmlEntity);
        }


        /// <summary>
        ///     生成主品牌问答HTML
        ///     author:songcl date:2014-11-27
        /// </summary>
        /// <param name="id"></param>
        public void BuilderMasterAskHtml(int id)
        {
            string askMasterUrl = CommonData.CommonSettings.AskMasterUrl; //正式环境
            //const string askMasterUrl = "http://192.168.2.83/askdata/question/getlist.xml?status=2&masterId={0}&top=8"; //测试环境

            Dictionary<int, string> masterDic = MasterBrandService.GetMasterDic();
            var commonHtmlEntity = new CommonHtmlEntity
            {
                BlockID = CommonHtmlEnum.BlockIdEnum.Ask,
                UpdateTime = DateTime.Now,
                TypeID = CommonHtmlEnum.TypeEnum.Master,
                TagID = CommonHtmlEnum.TagIdEnum.MasterBrandPage
            };
            if (id != 0)
            {
                commonHtmlEntity.ID = id;
                string askMaster = string.Format(askMasterUrl, id);
                commonHtmlEntity.HtmlContent = GetHtml(askMaster, id, masterDic[id], id, true);
                UpdateHtml(commonHtmlEntity);
            }
            else
            {
                foreach (var master in masterDic)
                {
                    commonHtmlEntity.ID = master.Key;
                    string askMaster = string.Format(askMasterUrl, master.Key);
                    commonHtmlEntity.HtmlContent = GetHtml(askMaster, master.Key, master.Value, master.Key, true);
                    UpdateHtml(commonHtmlEntity);
                }
            }
        }

        /// <summary>
        ///     生成车系问答HTML
        ///     author:songcl date:2014-11-27
        /// </summary>
        /// <param name="id"></param>
        public void BuilderSerialAskHtml(int id)
        {
            string askSerialUrl = CommonData.CommonSettings.AskSerialUrl; //正式环境
            //const string askSerialUrl = "http://192.168.2.83/askdata/question/getlist.xml?status=2&masterId={0}&serialid={1}&top=8"; //测试环境

            Dictionary<int, int> serialMasterBrandDic = CommonData.SerialMasterBrandDic;
            Dictionary<int, SerialInfo> serialInfoDic = CommonData.SerialDic;
            var commonHtmlEntity = new CommonHtmlEntity
            {
                BlockID = CommonHtmlEnum.BlockIdEnum.Ask,
                UpdateTime = DateTime.Now,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummary
            };

            if (id != 0)
            {
                string askSerial = string.Format(askSerialUrl, serialMasterBrandDic[id], id);
                commonHtmlEntity.ID = id;
                commonHtmlEntity.HtmlContent = GetHtml(askSerial, id, serialInfoDic[id].ShowName,
                    serialMasterBrandDic[id], false);
                UpdateHtml(commonHtmlEntity);
            }
            else
            {
                foreach (var serialInfo in serialInfoDic)
                {
                    string askSerial = string.Format(askSerialUrl, serialMasterBrandDic[serialInfo.Key], serialInfo.Key);
                    commonHtmlEntity.ID = serialInfo.Key;
                    commonHtmlEntity.HtmlContent = GetHtml(askSerial, serialInfo.Key, serialInfo.Value.ShowName,
                        serialMasterBrandDic[serialInfo.Key], false);
                    UpdateHtml(commonHtmlEntity);
                }
            }
        }

        private static void UpdateHtml(CommonHtmlEntity commonHtmlEntity)
        {
            try
            {
                bool success = CommonHtmlService.UpdateCommonHtml(commonHtmlEntity);

                string messsage = string.Format("问答块生成成功：ID:{0},TypeID:{1},TagID:{2},BlockID:{3},UpdateTime:{4}",
                    commonHtmlEntity.ID, commonHtmlEntity.TypeID, commonHtmlEntity.TagID,
                    commonHtmlEntity.BlockID, commonHtmlEntity.UpdateTime);

                if (success) Log.WriteLog(messsage);
            }
            catch (Exception)
            {
                string messsage = string.Format("问答块生成失败：ID:{0},TypeID:{1},TagID:{2},BlockID:{3},UpdateTime:{4}",
                    commonHtmlEntity.ID, commonHtmlEntity.TypeID, commonHtmlEntity.TagID,
                    commonHtmlEntity.BlockID, commonHtmlEntity.UpdateTime);
                Log.WriteErrorLog(messsage);
            }
        }

        /// <summary>
        ///     组织车型首页问答HTML
        ///     author：songcl date:2014-11-26
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetDefaultHtml(string path)
        {
            var sbLeft = new StringBuilder();
            var sbRight = new StringBuilder();

            XElement root = XElement.Load(path);

            #region 查找最长的Label

            string maxLabelLength = string.Empty; //默认值
            foreach (XElement item in root.Descendants("Question"))
            {
                XElement label = item.Element("Label");
                if (label != null)
                {
                    if (label.Value.Length > maxLabelLength.Length)
                    {
                        maxLabelLength = label.Value;
                    }
                }
            }

            #endregion

            int i = 0;
            foreach (XElement item in root.Descendants("Question"))
            {
                GetDefaultHtml(item, i, sbLeft, sbRight, maxLabelLength);
                i++;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<div class='title-box'>");
            stringBuilder.AppendLine("  <h3><a href='http://ask.bitauto.com/'>易车问答</a></h3>");
            stringBuilder.AppendLine(
                "  <span>百位汽车专家在线答疑，5分钟就有靠谱答案！<a href='http://ask.bitauto.com/tiwen/'>我要提问&gt;&gt;</a></span>");
            stringBuilder.AppendLine(
                "  <div class='more'><a href='http://ask.bitauto.com/browse/' target='_blank'>更多问题&gt;&gt;</a></div>");
            stringBuilder.AppendLine("</div>");

            stringBuilder.AppendLine("<div class='ask-part-con ask-l-set-m'>");

            stringBuilder.AppendLine("  <div class='ask-part-con-list'>");
            stringBuilder.AppendLine("      <ul>");
            stringBuilder.AppendLine(sbLeft.ToString());
            stringBuilder.AppendLine("      </ul>");
            stringBuilder.AppendLine("  </div>");

            stringBuilder.AppendLine("  <div class='ask-part-con-list ask-f-set'>");
            stringBuilder.AppendLine("      <ul>");
            stringBuilder.AppendLine(sbRight.ToString());
            stringBuilder.AppendLine("      </ul>");
            stringBuilder.AppendLine("  </div>");

            stringBuilder.AppendLine("</div>");

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     组织车型首页问答HTML
        ///     author：songcl date:2014-11-26
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        /// <param name="sbLeft"></param>
        /// <param name="sbRight"></param>
        /// <param name="maxLength"></param>
        private static void GetDefaultHtml(XContainer item, int i, StringBuilder sbLeft, StringBuilder sbRight,
            string maxLength)
        {
            XElement title = item.Element("Title");
            string titleVal = string.Empty;
            if (title != null)
            {
                titleVal = title.Value;
            }

            XElement url = item.Element("Url");
            string urlVal = string.Empty;
            if (url != null)
            {
                urlVal = url.Value;
            }

            XElement label = item.Element("Label");
            string labelVal = string.Empty;
            if (label != null)
            {
                labelVal = label.Value;
            }

            XElement labelUrl = item.Element("LabelUrl");
            string labelUrlVal = string.Empty;
            if (labelUrl != null)
            {
                labelUrlVal = labelUrl.Value;
            }
            string space;
            if (i%2 == 0)
            {
                sbLeft.AppendLine("<li>");

                sbLeft.AppendLine(string.Format("<span><a href='{0}' target='_blank'>{1}</a>{2} | </span>",
                    labelUrlVal, StandardizationString(labelVal, maxLength, out space, true),
                    space));

                sbLeft.AppendLine(string.Format("<a href='{0}' target='_blank'>{1}</a>", urlVal,
                    StandardizationString(titleVal, 18)));

                sbLeft.AppendLine("</li>");
            }
            else
            {
                sbRight.AppendLine("<li>");

                sbRight.AppendLine(string.Format("<span><a href='{0}' target='_blank'>{1}</a>{2} | </span>",
                    labelUrlVal,
                    StandardizationString(labelVal, maxLength, out space, true), space));

                sbRight.AppendLine(string.Format("<a href='{0}' target='_blank'>{1}</a>", urlVal,
                    StandardizationString(titleVal, 18)));

                sbRight.AppendLine("</li>");
            }
        }

        /// <summary>
        ///     组织主品牌和子品牌问答块HTML
        ///     author：songcl date:2014-11-27
        /// </summary>
        /// <param name="dataSoursePath"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="masterId"></param>
        /// <param name="isMaster"></param>
        /// <returns></returns>
        private static string GetHtml(string dataSoursePath, int id, string name, int masterId, bool isMaster)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<div class='line_box sum-ask' data-channelid=\"2.21.824\">");
            stringBuilder.AppendLine("  <div class='title-box'>");
            stringBuilder.AppendLine("      <h3>");
            stringBuilder.AppendLine(string.Format("<a href='http://ask.bitauto.com/browse/{0}/'>{1}问答</a>", id, name));
            stringBuilder.AppendLine("      </h3>");
            stringBuilder.AppendLine("        <span>百位汽车专家在线答疑，5分钟就有靠谱答案！");

            stringBuilder.AppendLine(isMaster
                ? string.Format(
                    "<a href='http://ask.bitauto.com/tiwen?masterid={0}'>我要提问&gt;&gt;</a>",
                    masterId)
                : string.Format(
                    "<a href='http://ask.bitauto.com/tiwen?masterid={0}&serialid={1}'>我要提问&gt;&gt;</a>",
                    masterId, id));

            stringBuilder.AppendLine("        </span>");
            stringBuilder.AppendLine("      <div class='more'>");
            stringBuilder.AppendLine(
                string.Format(
                    "<a target='_blank' href='http://ask.bitauto.com/browse/{0}/'>更多问题&gt;&gt;</a>", id));

            stringBuilder.AppendLine("      </div>");
            stringBuilder.AppendLine("  </div>");
            stringBuilder.AppendLine("  <div class='clear'></div>");
            stringBuilder.AppendLine("  <div class='sum-ask-tro'>");
            stringBuilder.AppendLine("      <div class='sum-ask-tro-list zs-ask-list'>");
            stringBuilder.AppendLine("          <ul id='data_box1_0' class='sum-ask-tro-list-con'>");

            #region 问题列表HTML

            GetQuestionHtml(dataSoursePath, stringBuilder);

            #endregion

            stringBuilder.AppendLine("          </ul>");
            stringBuilder.AppendLine("      </div>");
            stringBuilder.AppendLine("  </div>");

            #region 专家列表HTML

            GetExpertHtml(stringBuilder, masterId);

            #endregion

            stringBuilder.AppendLine("  <div class='clear'></div>");
            stringBuilder.AppendLine("</div>");

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     组织主品牌或者子品牌HTML
        ///     author：songcl date:2014-11-27
        /// </summary>
        /// <param name="dataSoursePath"></param>
        /// <param name="stringBuilder"></param>
        private static void GetQuestionHtml(string dataSoursePath, StringBuilder stringBuilder)
        {
            XElement root = XElement.Load(dataSoursePath);
            if (root.Descendants("Question").Any())
            {
                #region 查找最长的Label

                string maxLabelLength = string.Empty; //默认值
                foreach (XElement item in root.Descendants("Question"))
                {
                    XElement label = item.Element("Label");
                    if (label != null)
                    {
                        if (label.Value.Length > maxLabelLength.Length)
                        {
                            maxLabelLength = label.Value;
                        }
                    }
                }

                #endregion

                foreach (XElement item in root.Descendants("Question"))
                {
                    #region 组织数据

                    XElement title = item.Element("Title");
                    string titleVal = string.Empty;
                    if (title != null)
                    {
                        titleVal = title.Value;
                    }

                    XElement url = item.Element("Url");
                    string urlVal = string.Empty;
                    if (url != null)
                    {
                        urlVal = url.Value;
                    }

                    XElement label = item.Element("Label");
                    string labelVal = string.Empty;
                    if (label != null)
                    {
                        labelVal = label.Value;
                    }

                    XElement labelUrl = item.Element("LabelUrl");
                    string labelUrlVal = string.Empty;
                    if (labelUrl != null)
                    {
                        labelUrlVal = labelUrl.Value;
                    }

                    #endregion

                    stringBuilder.AppendLine("<li>");
                    string space;
                    stringBuilder.AppendLine(
                        string.Format("<span><a target='_blank' href='{0}'>{1}</a>{2} | </span>", labelUrlVal,
                            StandardizationString(labelVal, maxLabelLength, out space, true), space));
                    stringBuilder.AppendLine(string.Format("<a target='_blank' href='{0}'>{1}</a>", urlVal,
                        StandardizationString(titleVal, 25)));
                    stringBuilder.AppendLine("</li>");
                }
            }
            else
            {
                stringBuilder.AppendLine("              <li class='no-ask-list'>");
                stringBuilder.AppendLine("还没有人提问哦，点击'<a href='http://ask.bitauto.com/tiwen/'>\"提问\"</a>'来做第一个提问者吧！");
                stringBuilder.AppendLine("              </li>");
            }
        }

        /// <summary>
        ///     组织专家HTML
        ///     author：songcl date:2014-11-27
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="masterId">主品牌ID</param>
        private static void GetExpertHtml(StringBuilder stringBuilder, int masterId)
        {
            string askExpertUrl = CommonData.CommonSettings.AskExpertUrl; //正式环境
            //const string askExpertUrl = "http://192.168.2.83/askdata/expert/getlist.xml?masterId={0}&top=3";//本地测试

            string askExpert = string.Format(askExpertUrl, masterId);
            //var categoryList = new List<string>();
            XElement root = XElement.Load(askExpert);

            stringBuilder.AppendLine("  <div class='sum-ask-brick zs-ask-set'>");
            stringBuilder.AppendLine("      <ul>");

            foreach (XElement item in root.Descendants("Expert"))
            {
                #region 数据组织

                XElement id = item.Element("Id");
                string idVal = string.Empty;
                if (id != null)
                {
                    idVal = id.Value;
                }

                XElement name = item.Element("Name");
                string nameVal = string.Empty;
                if (name != null)
                {
                    nameVal = name.Value;
                }

                XElement spaceUrl = item.Element("SpaceUrl");
                string spaceUrlVal = string.Empty;
                if (spaceUrl != null)
                {
                    spaceUrlVal = spaceUrl.Value;
                }

                XElement tiwenUrl = item.Element("TiwenUrl");
                string tiwenUrlVal = string.Empty;
                if (tiwenUrl != null)
                {
                    tiwenUrlVal = tiwenUrl.Value;
                }


                XElement company = item.Element("Company");
                string companyVal = string.Empty;
                if (company != null)
                {
                    companyVal = company.Value;
                }

                //XElement categorys = item.Element("Categorys");
                //if (categorys != null)
                //{
                //    foreach (XElement category in categorys.Descendants("Category"))
                //    {
                //        XElement categoryName = category.Element("Name");
                //        if (categoryName != null)
                //        {
                //            string categoryNameVal = categoryName.Value;
                //            categoryList.Add(categoryNameVal);
                //        }
                //    }
                //}
                //string tempStr = categoryList.Aggregate(string.Empty, (current, str) => current + (str + ","));
                //tempStr = tempStr.Remove(tempStr.LastIndexOf(',')).Replace(',', '、');
                //categoryList.Clear();

                #endregion

                stringBuilder.AppendLine("<li>");
                stringBuilder.AppendLine(
                    string.Format(
                        "<a target='_blank' href='{0}'><img src='http://image.bitautoimg.com/ask/ExpertImage/{1}.jpg'></a>",
                        spaceUrlVal, idVal));
                stringBuilder.AppendLine("<dl>");
                stringBuilder.AppendLine(string.Format("<dt><a target='_blank' href='{0}'>{1}</a></dt>", spaceUrlVal,
                    nameVal));

                stringBuilder.AppendLine(string.Format("<dd>{0}</dd>",
                    companyVal.Length <= 12
                        ? companyVal
                        : companyVal.Substring(0, 11)));

                stringBuilder.AppendLine(string.Format("<dd><a target='_blank' href='{0}'>向TA提问&gt;&gt;</a></dd>",
                    tiwenUrlVal));
                stringBuilder.AppendLine("</dl>");
                stringBuilder.AppendLine("<div class='clear'></div>");
                stringBuilder.AppendLine("<em>专</em>");
                stringBuilder.AppendLine("</li>");
            }

            stringBuilder.AppendLine("      </ul>");
            stringBuilder.AppendLine("  </div>");
        }

        /// <summary>
        ///     按长度标准化字符串，长则截取
        ///     author：songcl date:2014-12-02
        /// </summary>
        /// <param name="sourseStr"></param>
        /// <param name="standard"></param>
        /// <returns></returns>
        private static string StandardizationString(string sourseStr, int standard)
        {
            string temp = string.Empty;

            if (sourseStr.Length > standard)
            {
                temp = sourseStr.Substring(0, standard - 1);
            }
            else
            {
                temp = sourseStr;
            }

            return temp;
        }


        /// <summary>
        ///     按长度标准化字符串，长则截取，短则空格补齐
        ///     author：songcl date:2015-04-11
        /// </summary>
        /// <param name="sourseStr"></param>
        /// <param name="standard"></param>
        /// <param name="space"></param>
        /// <param name="isFill">是否需要空格填充</param>
        /// <returns></returns>
        private static string StandardizationString(string sourseStr, string standard, out string space,
            bool isFill = false)
        {
            string temp = string.Empty;
            space = string.Empty;
            if (sourseStr.Length == standard.Length)
            {
                temp = sourseStr;
            }
            else if (sourseStr.Length > standard.Length)
            {
                temp = sourseStr.Substring(0, standard.Length - 1);
            }
            else if (sourseStr.Length < standard.Length)
            {
                if (isFill)
                {
                    try
                    {
                        string moreStr = standard.Substring(sourseStr.Length, standard.Length - sourseStr.Length);
                        int relLength = Encoding.Default.GetByteCount(moreStr);
                        for (int i = 0; i < relLength; i++)
                        {
                            space += "&ensp;";
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                temp = sourseStr;
            }
            return temp;
        }

        #region 1200改版  author:lisf date:2016-07-29
        /// <summary>
        ///     生成车型首页问答HTML
        /// </summary>
        public void BuilderDefaultAskHtmlNew()
        {
            //string askDefault = CommonData.CommonSettings.AskDefaultUrl;
			const string askDefault = "http://192.168.2.83/askdata/question/getlist.xml?status=2&top=8"; //测试环境

            var commonHtmlEntity = new CommonHtmlEntity
            {
                BlockID = CommonHtmlEnum.BlockIdEnum.AskNew,
                UpdateTime = DateTime.Now,
                ID = 1, //规定
                TypeID = CommonHtmlEnum.TypeEnum.Other,
                TagID = CommonHtmlEnum.TagIdEnum.CarDefault
            };
            commonHtmlEntity.HtmlContent = GetDefaultHtmlNew(askDefault);
            UpdateHtml(commonHtmlEntity);
        }


        /// <summary>
        ///     组织车型首页问答HTML
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetDefaultHtmlNew(string path)
        {
            //var sbLeft = new StringBuilder();
            //var sbRight = new StringBuilder();

            XElement root = XElement.Load(path);

            #region 查找最长的Label
            string maxLabelLength = string.Empty; //默认值
            foreach (XElement item in root.Descendants("Question"))
            {
                XElement label = item.Element("Label");
                if (label != null)
                {
                    if (label.Value.Length > maxLabelLength.Length)
                    {
                        maxLabelLength = label.Value;
                    }
                }
            }
            #endregion
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(" <div class=\"main-inner-section ask-answer\">");
            stringBuilder.AppendLine("    <div class=\"section-header header2\">");
            stringBuilder.AppendLine("        <div class=\"box\">");
            stringBuilder.AppendLine("            <h2>易车问答</h2>");
            stringBuilder.AppendLine("        </div>");
			stringBuilder.AppendLine("        <div class=\"more\"><a href=\"http://ask.bitauto.com/tiwen/\" target=\"_blank\">我要提问</a><a href=\"http://ask.bitauto.com/browse/\" target=\"_blank\">更多问题</a></div>");
            stringBuilder.AppendLine("    </div>");
            stringBuilder.AppendLine("    <div class=\"list-txt list-txt-m list-txt-default list-txt-style3\">");
            stringBuilder.AppendLine("        <ul class=\"clearfix\">");
            foreach (XElement item in root.Descendants("Question"))
            {
                XElement title = item.Element("Title");
                string titleVal = string.Empty;
                if (title != null)
                {
                    titleVal = title.Value;
                }

                XElement url = item.Element("Url");
                string urlVal = string.Empty;
                if (url != null)
                {
                    urlVal = url.Value;
                }

                XElement label = item.Element("Label");
                string labelVal = string.Empty;
                if (label != null)
                {
                    labelVal = label.Value;
                }

                XElement labelUrl = item.Element("LabelUrl");
                string labelUrlVal = string.Empty;
                if (labelUrl != null)
                {
                    labelUrlVal = labelUrl.Value;
                }
                string space;
				stringBuilder.AppendLine(string.Format("<li><div class=\"txt\"><strong><a href=\"{0}\" target=\"_blank\">{1}</a>{2}|</strong><a href=\"{3}\" target=\"_blank\">{4}</a></div></li>"
                    , labelUrlVal
                    , StandardizationString(labelVal, maxLabelLength, out space, true)
                    , space
                    , urlVal
                    , StandardizationString(titleVal, 18)));
            }
            stringBuilder.AppendLine("        </ul>");
            stringBuilder.AppendLine("    </div>");
            stringBuilder.AppendLine("</div>");
            return stringBuilder.ToString();
        }

        #endregion
        

    }
}