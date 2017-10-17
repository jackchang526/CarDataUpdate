using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	public class H5HtmlBuilder : BaseBuilder
	{
		private static readonly Dictionary<int, SerialInfo> SerialInfoDic = CommonData.SerialDic;

		// 车型封面图
		//private readonly Dictionary<int, string> _dicCarDefaultPhoto = CommonData.GetCarDefaultPhoto(2);

		public override void BuilderDataOrHtml(int objId)
		{
			//throw new NotImplementedException();
		}

		#region H5新闻列表XML数据生成 songcl 2015-09-11

		public void BuildH5ArticalXml(int id)
		{
			if (id != 0)
			{
				GenerateH5ArticalXml(id);
			}
			else
			{
				foreach (var serialInfo in SerialInfoDic)
				{
					GenerateH5ArticalXml(serialInfo.Key);
				}
			}
		}

		private void GenerateH5ArticalXml(int id)
		{
			try
			{
				if (!SerialInfoDic.ContainsKey(id))
				{
					Log.WriteErrorLog("func:GenerateH5ArticalXml, id不存在，id=" + id);
					return;
				}

				var serialInfo = SerialInfoDic[id];
				var existPingce = false; //是否包含车型详解
				var existDaogou = false;
				//编辑设置排序，根据设置先按照设置位置生成列表，未设置位置由null代替，再由实际数据补全空位
				var orderNewsList = FocusNewsService.GetOrderNewsList(id);
				var newsEntities = GetData(orderNewsList, id, out existPingce, out existDaogou, 20);
				if (newsEntities.Count == 0)
					return;
				var savePath = CommonData.CommonSettings.SavePath + @"\SerialNews\H5V3News\";
				var path = Path.Combine(savePath, Path.GetFileName(string.Format("{0}.xml", id)));
				var root = new XElement("root");
				foreach (var newsEntity in newsEntities)
				{
					if (newsEntity == null)
						continue;
					var ele = new XElement("Item"
                        ,new XElement("newsid", new XCData(newsEntity.NewsId.ToString()))
                        , new XElement("author", new XCData(newsEntity.Author))
						, new XElement("title", new XCData(newsEntity.Title))
						, new XElement("url", new XCData(newsEntity.PageUrl.Replace("news.bitauto.com", "news.m.yiche.com")))
						, new XElement("newscategoryshowname",
							new XCData(newsEntity.NewsCategoryShowName != null
								? newsEntity.NewsCategoryShowName.CategoryShowName
								: string.Empty))
						, new XElement("publishtime", new XCData(Convert.ToDateTime(newsEntity.PublishTime).ToString(CultureInfo.InvariantCulture)))
						, new XElement("img", new XCData(newsEntity.ImageLink)));
					root.Add(ele);
				}
				var directoryName = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directoryName))
				{
					if (!Directory.Exists(directoryName))
						Directory.CreateDirectory(directoryName);
					root.Save(path);
					Log.WriteLog(id + " 新闻数据XML 生成成功");
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("生成h5新闻xml异常：id=" + id + "\r\n" + ex.ToString());
			}
		}

		private List<NewsEntity> GetData(Dictionary<int, NewsEntity> orderNewsList, int id, out bool existPingce, out bool existDaogou,
			int top = 4)
		{
			var list = FocusNewsService.GetFocusNewsListNewForH5(orderNewsList, id, out existPingce, out existDaogou, top);

			return list;
		}

		#endregion
	}
}