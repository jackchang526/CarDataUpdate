using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common;
using System.IO;
using System.Xml;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	/// <summary>
	/// 废弃 不再使用 modified by chengl 2015-6-2
	/// </summary>
    public class KoubeiImpressionHtmlBuilder : BaseBuilder
    {
        private XmlDocument m_doc;

        /// <summary>
        /// 包含所需数据的XML
        /// </summary>
        public XmlDocument DataXmlDocument
        {
            get { return m_doc; }
            set { m_doc = value; }
        }

        public  KoubeiImpressionHtmlBuilder()
        {
            savePathFormat = Path.Combine(CommonData.CommonSettings.SavePath, "SerialDianping\\Impression\\Html\\Impression_{0}.html");
        }

        public override void BuilderDataOrHtml(int objId)
        {
            if (m_doc == null)
            {
                m_doc = new XmlDocument();
                m_doc.Load(String.Format(Path.Combine(CommonData.CommonSettings.SavePath, "SerialDianping\\Impression\\Html\\Impression_{0}.xml"), objId));
            }

            XmlNode impressionNode = m_doc.SelectSingleNode("/Root/Serial/Impression");
            XmlNode virtuesNode = m_doc.SelectSingleNode("/Root/Serial/Virtues");
            XmlNode defectNode = m_doc.SelectSingleNode("/Root/Serial/Defect");
            string impression = impressionNode == null ? String.Empty : impressionNode.InnerText.Trim();
            string virtues = virtuesNode == null ? String.Empty : virtuesNode.InnerText.Trim();
            string defect = defectNode == null ? String.Empty : defectNode.InnerText.Trim();
            if (impression.Length == 0 && virtues.Length == 0 && defect.Length == 0)
            {
                try
                {
                    File.Delete(String.Format(savePathFormat,objId));
                }
                catch { }

            }
            else
            {
                impression = StringHelper.RemoveHtmlTag(StringHelper.SubString(impression, 100, true));
                virtues = StringHelper.RemoveHtmlTag(virtues);
                defect = StringHelper.RemoveHtmlTag(defect);

                string[] htmlList = new string[7];
                string reportUrl = String.Empty;
                if (CommonData.SerialDic.ContainsKey(objId))
                    reportUrl = "/" + CommonData.SerialDic[objId].AllSpell + "/koubei/baogao/";
                htmlList[0] = "<div class=\"line_box car_impression\"><h3><span>网友对此车的印象</span></h3>";
                htmlList[1] = "<p>" + impression + "&nbsp;&nbsp;<a rel=\"nofollow\" href=\"" + reportUrl + "\" target=\"_blank\">详细&gt;&gt;</a></p>";
                htmlList[2] = "<dl class=\"first\"><dt>优点：</dt>";
                if (StringHelper.GetRealLength(virtues) > 48)
                    htmlList[3] = "<dd title=\"" + virtues + "\">" + StringHelper.SubString(virtues, 46, true) + "</dd></dl>";
                else
                    htmlList[3] = "<dd>" + virtues + "</dd></dl>";
                htmlList[4] = "<dl class=\"second\"><dt>缺点：</dt>";
                if (StringHelper.GetRealLength(defect) > 48)
                    htmlList[5] = "<dd title=\"" + defect + "\">" + StringHelper.SubString(defect, 46, true) + "</dd>";
                else
                    htmlList[5] = "<dd>" + defect + "</dd>";
                htmlList[6] = "</dl></div>";
                File.WriteAllText(String.Format(savePathFormat, objId), String.Concat(htmlList));
            }
        }
    }
}
