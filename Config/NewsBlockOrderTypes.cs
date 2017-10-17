using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Config
{
	/// <summary>
	/// 新闻排序类型
	/// </summary>
	public enum NewsBlockOrderTypes
	{
		/// <summary>
		/// 子品牌焦点新闻
		/// </summary>
		serialfocus = 1,
		/// <summary>
		/// 品牌焦点新闻
		/// </summary>
		brandfocus = 2,
		/// <summary>
		/// 主品牌焦点新闻
		/// </summary>
		masterfocus = 3,
		/// <summary>
		/// 子品牌买车必看
		/// </summary>
		serialpingjia = 5
	}
}
