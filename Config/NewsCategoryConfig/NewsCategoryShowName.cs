using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Config
{
	/// <summary>
	/// 新闻分类名称
	/// </summary>
	public class NewsCategoryShowName
	{
		/// <summary>
		/// 分类key
		/// </summary>
		public string CategoryKey;
		/// <summary>
		/// 分类显示名称
		/// </summary>
		public string CategoryShowName;
		/// <summary>
		/// 对应Url关键字
		/// </summary>
		public string CategoryUrl;
		/// <summary>
		/// 关联新闻分类id
		/// </summary>
		public List<int> CategoryIds;
		public NewsCategoryShowName()
		{
			CategoryIds = new List<int>();
		}
	}
}
