using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using System.IO;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils.Data;
using System.Data;
using System.Data.SqlClient;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Config;
using System.Xml;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	/// <summary>
	/// 买车必看
	/// </summary>
	public class WatchMustHtmlBuilder : BaseBuilder
	{
		private string _mobileXmlSaveFormat;
		private Dictionary<int, int[]> _noContainsCateDic;
		/// <summary>
		/// 不包含分类列表
		/// </summary>
		private Dictionary<int, int[]> NoContainsCateDic
		{
			get
			{
				if (_noContainsCateDic == null)
				{
					//新闻中不包含的内容
					_noContainsCateDic = new Dictionary<int, int[]>(2);
					_noContainsCateDic.Add(1, new int[] { 152, 34, 148, 146, 198, 149, 123, 127, 13, 98, 214, 220, 14, 211, 213, 218, 15, 216, 212, 217, 24, 25, 26, 28, 197, 347, 5, 381, 179 });
					_noContainsCateDic.Add(2, new int[] { 152, 34, 148, 146, 198, 149, 123, 127, 13, 98, 214, 220, 14, 211, 213, 218, 15, 216, 212, 217, 24, 25, 26, 28, 197, 347, 381, 179, 88, 143, 142, 85, 173, 27, 187, 188, 180, 181, 185, 183, 184, 186, 29, 30, 31, 138, 136, 139, 380, 376, 137, 378, 379, 377, 66, 74, 75, 227, 233, 234, 235, 236, 237, 275 });
				}
				return _noContainsCateDic;
			}
		}
		public WatchMustHtmlBuilder()
		{
			//carNewsTypeDic = CommonData.CarNewsTypeSettings.CarNewsTypeList;
			//savePathFormat = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\pingjia\html\Serial_All_News_{0}.html");
			//_mobileXmlSaveFormat = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialNews\pingjia\mobilehtml\{0}.html");
		}

		public override void BuilderDataOrHtml(int objId)
		{
			//Log.WriteLog("start watch must news! id:" + objId.ToString());

			//if (!CommonData.SerialDic.ContainsKey(objId))
			//{
			//	Log.WriteLog("not found serial! id:" + objId.ToString());
			//	return;
			//}

			//string serialSpell = CommonData.SerialDic[objId].AllSpell.ToLower();
			//Dictionary<int, List<NewsEntity>> dataDic = GetNewsList(objId);

			//List<NewsEntity> newsList = new List<NewsEntity>();
			//List<int> exitslist = new List<int>();
			//string path = string.Empty;
			//int[] newsType = new int[] { 1, 2, 3 };
			//if (dataDic != null && dataDic.Count > 0)
			//{
			//	//得到新闻列表
			//	foreach (int entity in newsType)
			//	{
			//		if (dataDic.ContainsKey(entity))
			//		{
			//			int num = 1;
			//			if (entity == 1) num = 3;

			//			List<NewsEntity> tempnewslist = new List<NewsEntity>();
			//			List<NewsEntity> dataList = dataDic[entity];
			//			foreach (NewsEntity newsObject in dataList)
			//			{
			//				if (num <= 0) break;
			//				if (!CommonData.CategoryPathDic.ContainsKey(newsObject.CategoryId))
			//					continue;
			//				if (IsNoContainsNews(2, CommonData.CategoryPathDic[newsObject.CategoryId])) continue;
			//				if (exitslist.Contains(newsObject.NewsId)) continue;
			//				exitslist.Add(newsObject.NewsId);
			//				tempnewslist.Add(newsObject);
			//				num--;
			//			}

			//			if (tempnewslist != null && tempnewslist.Count > 0)
			//				newsList.AddRange(tempnewslist);
			//		}
			//	}

			//	//补充新闻列表
			//	if (newsList.Count > 0 && newsList.Count < 5)
			//	{
			//		foreach (int entity in newsType)
			//		{
			//			if (dataDic.ContainsKey(entity))
			//			{
			//				int num = 5 - newsList.Count;
			//				if (num < 1) break;
			//				List<NewsEntity> tempnewslist = new List<NewsEntity>();
			//				List<NewsEntity> dataList = dataDic[entity];
			//				foreach (NewsEntity newsObject in dataList)
			//				{
			//					if (num <= 0) break;
			//					if (!CommonData.CategoryPathDic.ContainsKey(newsObject.CategoryId))
			//						continue;
			//					//如果是导购分类，并且分类是精华贴的内容
			//					if (entity == 1 && IsNoContainsNews(2, CommonData.CategoryPathDic[newsObject.CategoryId])) continue;
			//					//如果此新闻已经被获取
			//					if (exitslist.Contains(newsObject.NewsId)) continue;
			//					exitslist.Add(newsObject.NewsId);
			//					tempnewslist.Add(newsObject);
			//					num--;
			//				}

			//				if (tempnewslist != null && tempnewslist.Count > 0)
			//					newsList.AddRange(tempnewslist);
			//			}
			//		}
			//	}
			//}
			////开始保存数据
			//SerialInfo csInfo = CommonData.SerialDic[objId];

			//StringBuilder htmlCode = new StringBuilder();
			//StringBuilder mobilehtmlCode = new StringBuilder();//车型移动站买车必看
			//string codeTitle = csInfo.SeoName + "买车必看";

			//htmlCode.Append("<h3><span>" + codeTitle + "</span></h3>");
			//htmlCode.Append("<div class=\"mainlist_box reco\">");

			//if (newsList != null && newsList.Count > 0)
			//{
			//	//车型移动站买车必看
			//	mobilehtmlCode.AppendFormat("<section><div class=\"m-tabs-box\"><ul class=\"m-tabs\"><li>{0} 买车必看</li></ul></div><div class=\"m-index-news-list m-index-news-list-no-border\"><ul>"
			//		, csInfo.SeoName);

			//	string baseUrl = string.Format("/{0}/{1}/", serialSpell, "{0}");

			//	htmlCode.Append("<ul class=\"list_date\">");
			//	int counter = 0;
			//	foreach (NewsEntity entity in newsList)
			//	{
			//		string cateUrl = null;
			//		if (entity.NewsCategoryShowName.CategoryUrl == NewsCategoryConfig.QitaCategoryKey)
			//			cateUrl = "#";
			//		else
			//			cateUrl = string.Format(baseUrl, entity.NewsCategoryShowName.CategoryUrl);
			//		string newsCategory = entity.NewsCategoryShowName.CategoryShowName;

			//		// modified by chengl Jun.14.2012
			//		htmlCode.Append("<li><span><a rel=\"nofollow\" href=\"" + cateUrl + "\" class=\"fl\" >" + newsCategory + "</a>| </span>");
			//		htmlCode.Append("<a href=\"" + entity.PageUrl + "\" title=\"" + entity.Title + "\" >" + entity.Title + "</a><small>" + entity.PublishTime.ToString("MM-dd") + "</small></li>");

			//		// htmlCode.Append("<li><a href=\"" + cateUrl + "\" class=\"fl\" >[" + newsCategory + "]</a>");
			//		// htmlCode.Append("<a href=\"" + entity.PageUrl + "\" title=\"" + entity.Title + "\" >" + entity.Title + "</a><small>" + entity.PublishTime.ToString("MM-dd") + "</small></li>");
			//		/*
			//		string author=entity.Author;
			//		if(!string.IsNullOrEmpty(author))
			//			author = author+" / ";
			//		mobilehtmlCode.AppendFormat("<li><h3><a href=\"{3}\">{0}</a></h3><p class=\"m-post-author\">{1}{2}</p></li>"
			//			, entity.Title
			//			, author
			//			, entity.PublishTime.ToString("yyyy-MM-dd")
			//			, entity.PageUrl.Replace("news.bitauto.com", "news.m.yiche.com"));
			//		*/
			//		mobilehtmlCode.AppendFormat("<li><a href=\"{1}\"><h4>{0}</h4></a></li>"
			//			, entity.Title
			//			 , entity.PageUrl.Replace("news.bitauto.com", "news.m.yiche.com"));
			//		counter++;
			//		if (counter >= 5)
			//			break;
			//	}
			//	htmlCode.Append("</ul>");
			//	mobilehtmlCode.Append("</ul></div></section>");
			//}
			//else
			//{
			//	// modified by chengl Jun.2.2011
			//	// 无数据时显示 暂无数据
			//	htmlCode.AppendLine("<div class=\"car_nonedata\">暂无文章</div>");
			//}
			//htmlCode.Append("<div class=\"clear\"></div>");
			////购车手册
			//string csNewGouCheShouChe = this.GetCsRainbowURL(csInfo.Id, 42);
			////销量数据
			//string csNewXiaoShouShuJu = "/" + csInfo.AllSpell.ToLower() + "/xiaoliang/";
			////口碑报告
			//string rptUrl = String.Empty;
			//if (CommonData.SerialKoubeiReport.Contains(csInfo.Id))
			//	rptUrl = String.Format("/{0}/koubei/baogao/", csInfo.AllSpell);
			////更多链接
			//StringBuilder moreUrl = new StringBuilder();
			//// modified by chengl May.21.2012 
			//// 增加 购车流程 link:http://www.bitauto.com/zhuanti/daogou/gsqgl/
			//moreUrl.Append("<a rel=\"nofollow\" href='http://www.bitauto.com/zhuanti/daogou/gsqgl/' target='_blank'>购车流程</a>");

			//if (csNewGouCheShouChe.Length > 0)
			//{
			//	moreUrl.AppendFormat("|<a href='{0}'>购车手册</a>", csNewGouCheShouChe);
			//}
			////if (rptUrl.Length > 0)
			////{
			////    moreUrl.AppendFormat("|<a href='{0}'>口碑报告</a>", rptUrl);
			////}
			//moreUrl.Append("|<a rel=\"nofollow\" href='http://car.bitauto.com/" + csInfo.AllSpell + "/koubei/' target='_blank'>口碑</a>");

			//if (CommonData.HasSaleDataList.Contains(csInfo.Id))
			//{
			//	moreUrl.AppendFormat("|<a rel=\"nofollow\" href='{0}'>销量</a>", csNewXiaoShouShuJu);
			//}
			////得到更多链接
			//if (moreUrl.Length > 0)
			//{
			//	string endMore = moreUrl.ToString();
			//	if (endMore.Length > 0 && endMore.IndexOf('|') == 0)
			//	{
			//		endMore = endMore.Remove(0, 1);
			//	}

			//	if (endMore.Length > 0)
			//	{
			//		endMore = endMore.Replace("|", " | ");
			//		htmlCode.AppendFormat("<div class='more'>{0}</div>", endMore);
			//	}
			//}

			//htmlCode.Append("</div>");
			//string filePath = string.Format(savePathFormat, objId);
			//string pathDirection = Path.GetDirectoryName(filePath);
			//if (!Directory.Exists(pathDirection))
			//	Directory.CreateDirectory(pathDirection);
			////添加买车必看文件
			//File.WriteAllText(filePath, htmlCode.ToString());

			//Log.WriteLog("end watch must news! id:" + objId.ToString());

			//#region 移动版买车必看
			//Log.WriteLog("start mobile news xml! id:" + objId.ToString());
			//string mobileFilePath = string.Format(_mobileXmlSaveFormat, objId);
			//string mobilePathDirection = Path.GetDirectoryName(mobileFilePath);
			//if (!Directory.Exists(mobilePathDirection))
			//	Directory.CreateDirectory(mobilePathDirection);
			////添加买车必看文件
			//File.WriteAllText(mobileFilePath, mobilehtmlCode.ToString(), Encoding.UTF8);
			//Log.WriteLog("end mobile news xml! id:" + objId.ToString());
			//#endregion

			////车型移动站-子品牌综述页-买车必看新闻源数据xml
			////MobileNewsXml(newsList, objId);
		}
		/// <summary>
		/// 获取新闻对象列表
		/// key固定值:1:导购，2:评测，3:试驾
		/// </summary>
		private Dictionary<int, List<NewsEntity>> GetNewsList(int serialId)
		{
			if (serialId < 1)
				return null;

			Dictionary<int, List<NewsEntity>> newsList = new Dictionary<int, List<NewsEntity>>(3);

			try
			{
				DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
					string.Format(@"
select a.*,n.Author from (
	SELECT CarNewsId, CarNewsTypeId,Title, FilePath, CategoryId, PublishTime, CmsNewsId FROM SerialNews WHERE CarNewsTypeId in ({0},{1},{2}) AND SerialId=@SerialId
) as a
INNER JOIN News n ON n.ID=a.CarNewsId 
ORDER BY a.PublishTime DESC", (int)CarNewsTypes.daogou, (int)CarNewsTypes.pingce, (int)CarNewsTypes.shijia), new SqlParameter("@SerialId", serialId));
				if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
					return null;
				int cateId, carNewsTypeId;
				DataRowCollection rows = ds.Tables[0].Rows;
				foreach (DataRow row in rows)
				{
					carNewsTypeId = ConvertHelper.GetInteger(row["CarNewsTypeId"]);
					cateId = ConvertHelper.GetInteger(row["CategoryId"]);
					//导购
					if (carNewsTypeId == (int)CarNewsTypes.daogou)
					{
						if (newsList.ContainsKey(1))
							newsList[1].Add(base.GetNewsObjectByXmlNode(row));
						else
							newsList[1] = new List<NewsEntity>(10) { base.GetNewsObjectByXmlNode(row) };
						continue;
					}
					//评测
					if (carNewsTypeId == (int)CarNewsTypes.pingce)
					{
						if (newsList.ContainsKey(2))
							newsList[2].Add(base.GetNewsObjectByXmlNode(row));
						else
							newsList[2] = new List<NewsEntity>(10) { base.GetNewsObjectByXmlNode(row) };
						continue;
					}
					//试驾
					if (carNewsTypeId == (int)CarNewsTypes.shijia)
					{
						if (newsList.ContainsKey(3))
							newsList[3].Add(base.GetNewsObjectByXmlNode(row));
						else
							newsList[3] = new List<NewsEntity>(10) { base.GetNewsObjectByXmlNode(row) };
						continue;
					}
				}
				return newsList;
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(exp);
				return null;
			}
		}
		/// <summary>
		/// 不包含的新闻列表
		/// </summary>
		private bool IsNoContainsNews(int order, List<int> cateIds)
		{
			bool isNoContains = true;
			if (!NoContainsCateDic.ContainsKey(order))
				return isNoContains;

			foreach (int tempCateId in cateIds)
			{
				foreach (int entity in NoContainsCateDic[order])
				{
					if (entity == tempCateId)
					{
						isNoContains = false;
						break;
					}
				}
				if (!isNoContains)
					break;
			}
			return isNoContains;
		}

		///// <summary>
		///// 车型移动站-子品牌综述页-买车必看新闻源数据xml
		///// </summary>
		//private void MobileNewsXml(List<NewsEntity> newsList, int objId)
		//{
		//    Log.WriteLog("start mobile news xml! id:" + objId.ToString());
		//    string filePath = string.Format(_mobileXmlSaveFormat, objId);
		//    string pathDirection = Path.GetDirectoryName(filePath);
		//    if (!Directory.Exists(pathDirection))
		//        Directory.CreateDirectory(pathDirection);

		//    XmlDocument mobileXml = new XmlDocument();
		//    XmlDeclaration mobileDeclaration = mobileXml.CreateXmlDeclaration("1.0", "utf-8", null);
		//    mobileXml.AppendChild(mobileDeclaration);
		//    XmlElement mobileRoot = mobileXml.CreateElement("root");
		//    mobileXml.AppendChild(mobileRoot);

		//    if (newsList != null && newsList.Count > 0)
		//    {
		//        for (int i = 0; i < newsList.Count && i < 5; i++)
		//        {
		//            NewsEntity news = newsList[i];
		//            XmlElement newsXml = mobileXml.CreateElement("news");
		//            newsXml.SetAttribute("id", news.NewsId.ToString());
		//            newsXml.SetAttribute("title", news.Title);
		//            newsXml.SetAttribute("publishTime", news.PublishTime.ToString());
		//            mobileRoot.AppendChild(newsXml);
		//        }
		//    }
		//    if (mobileRoot.InnerXml.Length > 0)
		//    {
		//        Log.WriteLog("save file... path:" + filePath);
		//        File.WriteAllText(filePath, mobileXml.OuterXml);
		//    }
		//    else if (File.Exists(filePath))
		//    {
		//        Log.WriteLog("delete file... path:" + filePath);
		//        File.Delete(filePath);
		//    }
		//    else
		//    {
		//        Log.WriteLog("jump... id:" + objId.ToString());
		//    }
		//    Log.WriteLog("end mobile news xml! id:" + objId.ToString());
		//}
		/// <summary>
		/// 获取彩虹条url
		/// </summary>
		/// <param name="csId"></param>
		/// <param name="rainbowitemId"></param>
		/// <returns></returns>
		private string GetCsRainbowURL(int csId, int rainbowitemId)
		{
			string url = string.Empty;
			string sql = "SELECT url FROM RainbowEdit WHERE RainbowItemID=@RainbowItemID AND csid=@csId";
			SqlParameter[] param = { 
									   new  SqlParameter("@RainbowItemID",SqlDbType.Int),
									   new  SqlParameter("@csId",SqlDbType.Int),
								   };
			param[0].Value = rainbowitemId;
			param[1].Value = csId;
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, param);
			if (ds.Tables[0].Rows.Count > 0)
			{
				url = ds.Tables[0].Rows[0]["url"].ToString();
			}
			return url;
		}
	}
}
