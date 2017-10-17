using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using System.Data;
using System.IO;
using System.Xml;

namespace BitAuto.CarDataUpdate.Common.Interface
{
    public abstract class BaseBuilder
    {
        /// <summary>
        /// 文件保存路径，格式化串
        /// </summary>
        public string savePathFormat;
        protected Dictionary<int, CarNewsTypeItem> carNewsTypeDic;

		#region /*之前分类名称的定义*/
		/*
		private Dictionary<string, string> _kindCateNameDic;
		/// <summary>
		/// 分类名称字典
		/// </summary>
		protected Dictionary<string, string> KindCateNameDic
		{
			get
			{
				if (_kindCateNameDic == null)
				{
					//分类名称包含
					_kindCateNameDic = new Dictionary<string, string>(8);
					_kindCateNameDic.Add("xinwen", "新闻");
					_kindCateNameDic.Add("hangqing", "行情");
					_kindCateNameDic.Add("daogou", "导购");
					_kindCateNameDic.Add("shipin", "视频");
					_kindCateNameDic.Add("yongche", "用车");
					_kindCateNameDic.Add("shijia", "试驾");
					_kindCateNameDic.Add("pingce", "评测");
					_kindCateNameDic.Add("gaizhuang", "改装");
				}
				return _kindCateNameDic;
			}
		}
		//private Dictionary<string, int[]> _kindCateDic;
		/// <summary>
		/// 分类字典
		/// </summary>
		protected Dictionary<string, int[]> KindCateDic
		{
			get
			{
				if (_kindCateDic == null)
				{
					//新闻分类包含
					_kindCateDic = new Dictionary<string, int[]>(8);
					_kindCateDic["xinwen"] = new int[] { 152, 34, 148, 146, 198, 149, 123, 127, 13, 98, 214, 220, 14, 211, 213, 218, 15, 216, 212, 217 };
					_kindCateDic["daogou"] = new int[] { 4, 179 };
					_kindCateDic["shijia"] = new int[] { 29, 30 };
					_kindCateDic["hangqing"] = new int[] { 3, 16, 215 };
					_kindCateDic["yongche"] = new int[] { 87, 88, 143, 142, 86, 85, 173, 56, 54, 53, 55, 201 };
					_kindCateDic["shipin"] = new int[] { 74, 70, 348, 67 };
					_kindCateDic["pingce"] = new int[] { 31, 221 };
					_kindCateDic["gaizhuang"] = new int[] { 87 };
				}
				return _kindCateDic;
			}
		} */
		#endregion

        /// <summary>
        /// 派生类要实现的方法
        /// </summary>
        /// <param name="objId">可能对象ID，包括主品牌，品牌，子品牌等</param>
        public abstract void BuilderDataOrHtml(int objId);

        /// <summary>
        /// 得到新闻对象
        /// </summary>
        /// <param name="xNode"></param>
        /// <returns></returns>
        protected NewsEntity GetNewsObjectByXmlNode(DataRow row)
        {
            NewsEntity newsObject = new NewsEntity();
            newsObject.NewsId = ConvertHelper.GetInteger(row["cmsnewsid"]);
            newsObject.Title = row["title"].ToString();
			newsObject.FaceTitle = (row.Table != null && row.Table.Columns.Contains("facetitle")) ? row["facetitle"].ToString() : string.Empty;
            newsObject.PageUrl = row["filepath"].ToString();
            newsObject.PublishTime = ConvertHelper.GetDateTime(row["publishTime"]);
            newsObject.CategoryId = ConvertHelper.GetInteger(row["CategoryId"]);
			newsObject.NewsCategoryShowName = GetNewsCategory(CommonData.CategoryPathDic[newsObject.CategoryId]);
			newsObject.Author = row.Table.Columns.Contains("Author") ? row["Author"].ToString() : string.Empty;
            return newsObject;
        }
		/// <summary>
		/// 得到新闻分类的名称
		/// </summary>
		/// <returns></returns>
		public NewsCategoryShowName GetNewsCategory(List<int> cateIds)
		{
			NewsCategoryConfig categoryConfig = CommonData.NewsCategoryConfig;
			foreach (int tempCateId in cateIds)
			{
				foreach (KeyValuePair<string, NewsCategoryShowName> kindCate in categoryConfig.NewsCategoryShowNames)
				{
					if (kindCate.Key != NewsCategoryConfig.QitaCategoryKey 
						&& kindCate.Value.CategoryIds.Contains(tempCateId))
						return kindCate.Value;
				}
			}
			return categoryConfig.NewsCategoryShowNames.ContainsKey(NewsCategoryConfig.QitaCategoryKey)
				? categoryConfig.NewsCategoryShowNames[NewsCategoryConfig.QitaCategoryKey] : null;
		}
        /// <summary>
        /// 得到彩虹条信息
        /// </summary>
        /// <param name="csId"></param>
        /// <param name="rainbowEditId"></param>
        /// <returns></returns>
        protected string GetCsRainbowAndURLInfo(int csId, int rainbowEditId)
        {
            string url = "";

            DataRow[] rows = CommonData.RainbowData.Tables[0].Select(" csId='" + csId + "' and RainbowitemId='" + rainbowEditId + "' ");
            if (rows != null && rows.Length > 0)
            {
                url = rows[0]["url"].ToString().Trim().ToLower();
            }
            return url;
        }
        /// <summary>
        /// 获取对比车型
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        protected List<SerialInfo> GetCompareSerial(int serialId)
        {
            List<SerialInfo> list = new List<SerialInfo>();
            XmlDocument xmlDoc = CommonFunction.GetLocalXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"SerialCityCompare\{0}_CityCompare.xml", serialId)));
            if (xmlDoc != null)
            {
                XmlNodeList nodeList = xmlDoc.SelectNodes("CityCompare/City[@ID=0]/Serial");
                foreach (XmlNode node in nodeList)
                {
                    int tempSerialId = ConvertHelper.GetInteger(node.Attributes["ID"].Value);
                    string serialName = node.Attributes["ShowName"].Value;
                    string allspell = node.Attributes["AllSpell"].Value;
                    list.Add(new SerialInfo()
                    {
                        Id = tempSerialId,
                        ShowName = serialName,
                        AllSpell = allspell
                    });
                }
            }
            return list;
        }
    }
}
