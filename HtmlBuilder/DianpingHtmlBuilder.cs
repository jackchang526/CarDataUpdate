using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.Utils;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using System.Net;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	public class DianpingHtmlBuilder : BaseBuilder
	{
		private XmlDocument m_dpDoc;

		/// <summary>
		/// 子品牌的点评XML文档
		/// </summary>
		public XmlDocument DianpingXmlDocument
		{
			get { return m_dpDoc; }
			set { m_dpDoc = value; }
		}

		public DianpingHtmlBuilder()
		{
			savePathFormat = Path.Combine(CommonData.CommonSettings.SavePath, "SerialDianping\\Html\\DianpingHtml_Serial_{0}.html");
		}

		public override void BuilderDataOrHtml(int objId)
		{
			if (m_dpDoc == null || !m_dpDoc.HasChildNodes)
				return;
			if (!CommonData.SerialDic.ContainsKey(objId))
				return;
			List<string> htmlList = new List<string>();
			StringBuilder sbDianpingNew = new StringBuilder();
			string koubeiReport = string.Empty;

			SerialInfo csInfo = CommonData.SerialDic[objId];
			int count = ConvertHelper.GetInteger(m_dpDoc.DocumentElement.GetAttribute("count"));
			int dianpingCount = count;

			string moreUrl = "/" + csInfo.AllSpell + "/koubei/gengduo/";
			// add by chengl Aug.15.2012 增加h3link
			htmlList.Add("<h3><span><a href=\"" + moreUrl + "\">" + csInfo.SeoName + "-点评精选</a></span><strong><em>" + count + "条</em>");

			sbDianpingNew.Append("<div class=\"line_box  choice\">");
			sbDianpingNew.AppendFormat("<h3><span><b><a href=\"{1}\" target=\"_blank\">{0}网友口碑</a></b></span></h3>", csInfo.SeoName, moreUrl);

			//htmlList.Add("<a href=\"http://car.bitauto.com/" + csInfo.AllSpell + "/koubei/tianjia/\">我要点评</a>");
			// htmlList.Add("|<a href=\"http://i.bitauto.com/FriendMore_c0_s" + objId + "_p1_sort1_r001.html\">和车主聊聊</a>");
			// htmlList.Add("|<a href=\"http://i.bitauto.com/FriendMore_c0_s" + objId + "_p1_sort1_r010.html\">和想买的人聊聊</a>");
			if (CommonData.SerialKoubeiReport.Contains(objId))
			{
				string reportUrl = String.Empty;
				if (CommonData.SerialDic.ContainsKey(objId))
					reportUrl = "/" + CommonData.SerialDic[objId].AllSpell + "/koubei/baogao/";
				koubeiReport = string.Format("<a href=\"{0}\" target=\"_blank\">口碑报告</a> | ", reportUrl);
				htmlList.Add("|<a href=\"" + reportUrl + "\">口碑报告</a>");
			}
			htmlList.Add("</strong></h3>");
			//口碑 点评 标签 2013.07.26
			try
			{
				WebClient wc = new WebClient();
				wc.Encoding = Encoding.UTF8;
				string koubeiTagsData = wc.DownloadString(string.Format(CommonData.CommonSettings.KouBeiSerialDianpingTagsUrl, objId));
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(koubeiTagsData);
				if (xmlDoc != null && xmlDoc.HasChildNodes && xmlDoc.SelectNodes("//tag").Count > 0)
				{
					htmlList.Add(" <dl class=\"k_choice_sum\">");
					htmlList.Add("<dt>点评分类：</dt><dd>");

					sbDianpingNew.Append("<dl class=\"k_choice_sum\">");
					sbDianpingNew.Append("<dt>点评分类：</dt>");
					sbDianpingNew.Append("<dd>");
					XmlNodeList tagsNodeList = xmlDoc.SelectNodes("//tag");
					foreach (XmlNode node in tagsNodeList)
					{
						htmlList.Add(string.Format("<a target=\"_blank\" href=\"/{1}/koubei/tags/{0}/\">{0}</a>",
							node.Attributes["name"].Value,
							csInfo.AllSpell));
						sbDianpingNew.AppendFormat(" <a target=\"_blank\" href=\"/{1}/koubei/tags/{0}/\">{0}</a>",
							node.Attributes["name"].Value,
							csInfo.AllSpell);
					}
					htmlList.Add(string.Format("<a class=\"last\" target=\"_blank\" href=\"{0}\">全部&gt;&gt;</a></dd>", moreUrl));
					htmlList.Add("</dl> ");
					sbDianpingNew.AppendFormat("<a class=\"last\" target=\"_blank\" href=\"{0}\">全部&gt;&gt;</a></dd>", moreUrl);
					sbDianpingNew.Append("</dd>");
					sbDianpingNew.Append("</dl>");
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}

			StringBuilder tabCode = new StringBuilder();
			StringBuilder conCode = new StringBuilder();

			for (int i = 3; i >= 1; i--)
			{
				XmlElement dpNode = (XmlElement)m_dpDoc.SelectSingleNode("/SerialDianping/Dianping[@type=\"" + i + "\"]");
				if (dpNode == null)
					continue;
				count = ConvertHelper.GetInteger(dpNode.GetAttribute("count"));
				htmlList.Add("<div class=\"list_li\">");
				string tName = string.Empty;
				string tMoreUrl = "/" + csInfo.AllSpell + "/koubei/gengduo/{0}/";
				switch (i)
				{
					case 1:
						htmlList.Add("<h4 class=\"cha\">");
						htmlList.Add("<a rel=\"nofollow\" href=\"" + moreUrl + "\">差评</a><strong><em>(" + count + "条)</em></strong></h4>");
						sbDianpingNew.Append("  <div class=\"list_li list_li_last\">");
						sbDianpingNew.Append("    <h4 class=\"cha\"> 差评</h4>");
						sbDianpingNew.Append("    <ul>");
						tName = "差评";
						tMoreUrl = string.Format(tMoreUrl, "chaping");
						break;
					case 2:
						htmlList.Add("<h4 class=\"zhong\">");
						htmlList.Add("<a rel=\"nofollow\" href=\"" + moreUrl + "\">中评</a><strong><em>(" + count + "条)</em></strong></h4>");
						sbDianpingNew.Append("  <div class=\"list_li\">");
						sbDianpingNew.Append("    <h4 class=\"zhong\"> 中评</h4>");
						sbDianpingNew.Append("    <ul>");
						tName = "中评";
						tMoreUrl = string.Format(tMoreUrl, "zhongping");
						break;
					case 3:
						htmlList.Add("<h4 class=\"hao\">");
						htmlList.Add("<a rel=\"nofollow\" href=\"" + moreUrl + "\">好评</a><strong><em>(" + count + "条)</em></strong></h4>");
						sbDianpingNew.Append("  <div class=\"list_li\">");
						sbDianpingNew.Append("    <h4 class=\"hao\"> 好评</h4>");
						sbDianpingNew.Append("    <ul>");
						tMoreUrl = string.Format(tMoreUrl, "haoping");
						tName = "好评";
						break;
				}

				htmlList.Add("<ul>");
				int counter = 0;
				foreach (XmlElement ele in dpNode.ChildNodes)
				{
					counter++;
					string title = ele.SelectSingleNode("title").InnerText;
					string url = ele.SelectSingleNode("url").InnerText;
					string shortTitle = title;
					if (StringHelper.GetRealLength(title) > 24)
						shortTitle = StringHelper.SubString(title, 24, false);
					htmlList.Add("<li><a href=\"" + url + "\" title=\"" + title + "\">" + shortTitle + "</a></li>");
					if (counter < 7)
					{
						string strClass = string.Empty;
						if (dpNode.ChildNodes.Count == counter || counter == 6)
							strClass = "class=\"last\"";
						sbDianpingNew.AppendFormat("<li {3}><a href=\"{0}\" title=\"{1}\" target=\"_blank\">{2}</a></li>", url, title, shortTitle, strClass);
					}
					if (counter >= 7)
						break;
				}
				htmlList.Add("</ul>");
				htmlList.Add("<div class=\"more\">");
				htmlList.Add("<a rel=\"nofollow\" href=\"" + moreUrl + "\" target=\"_blank\">更多&gt;&gt;</a></div>");
				htmlList.Add("</div>");

				sbDianpingNew.Append("    </ul>");
				sbDianpingNew.AppendFormat("    <div class=\"more\"> <a rel=\"nofollow\" href=\"{0}\" target=\"_blank\">{1}条{2}&gt;&gt;</a></div>", tMoreUrl, count, tName);
				sbDianpingNew.Append("  </div>");

			}
			htmlList.Add("<div class=\"clear\"></div>");
			htmlList.Add("<div class=\"more\">");
			//htmlList.Add("<a href=\"" + moreUrl + "\">更多&gt;&gt;</a>");
			htmlList.Add("<a rel=\"nofollow\" href=\"http://car.bitauto.com/" + csInfo.AllSpell + "/koubei/tianjia/\">我要点评&gt;&gt;</a></div>");

			sbDianpingNew.Append("  <div class=\"clear\"></div>");
			sbDianpingNew.AppendFormat(" <div class=\"more\">{1}<a href=\"http://car.bitauto.com/{0}/koubei/tianjia/\" target=\"_blank\">我要点评&gt;&gt;</a></div>",
				csInfo.AllSpell,
				koubeiReport);
			sbDianpingNew.Append("</div>");

			string fileName = String.Format(savePathFormat, objId);
			if (!Directory.Exists(Path.GetDirectoryName(savePathFormat)))
				Directory.CreateDirectory(savePathFormat);
			File.WriteAllLines(fileName, htmlList.ToArray(), Encoding.UTF8);
			//新版综述页
			bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
			{
				ID = objId,
				TypeID = CommonHtmlEnum.TypeEnum.Serial,
				TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
				BlockID = CommonHtmlEnum.BlockIdEnum.KoubeiReport,
				HtmlContent = sbDianpingNew.ToString(),
				UpdateTime = DateTime.Now
			});
			if (!success) Log.WriteErrorLog("更新口碑点评失败：serialId:" + objId);
		}
	}
}
