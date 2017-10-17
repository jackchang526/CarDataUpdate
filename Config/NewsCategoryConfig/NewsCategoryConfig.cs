using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Config
{
	public class NewsCategoryConfig
	{
		public const string QitaCategoryKey = "qita";
		/// <summary>
		/// cms新闻类型，只有以下类型才有可能被记录
		/// 数字代表：0为原创，1为转载，5为论坛引入  (2软文  3爬虫  不提取)
		/// </summary>
		public List<int> CMSCreativeTypes = null;
		/// <summary>
		/// 子品牌焦点新闻首条新闻分类
		/// </summary>
		public List<int> SerialFocusTopCategoryIds = null;
		/// <summary>
		/// 子品牌焦点新闻视频分类
		/// </summary>
		public List<int> SerialFocusVideoCategoryIds = null;
		/// <summary>
		/// 子品牌焦点新闻视频分类
		/// </summary>
		public Dictionary<string, NewsCategoryShowName> NewsCategoryShowNames = null;
		public NewsCategoryConfig()
		{
			CMSCreativeTypes = new List<int>();
			SerialFocusTopCategoryIds = new List<int>();
			SerialFocusVideoCategoryIds = new List<int>();
			NewsCategoryShowNames = new Dictionary<string, NewsCategoryShowName>();
		}
	}
}
