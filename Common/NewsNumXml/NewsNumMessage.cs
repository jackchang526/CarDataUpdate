using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.Common.NewsNumXml
{
	/// <summary>
	/// 更新新闻数消息类型
	/// </summary>
	public class NewsNumMessage
	{
		/// <summary>
		/// 数据id
		/// </summary>
		public int ObjId { get; set; }
		/// <summary>
		/// 子品牌年款
		/// </summary>
		public int SerialYear { get; set; }
		/// <summary>
		/// 关联类型
		/// </summary>
		public NewsNumMsgTypes NewsNumMsgType { get; set; }
		/// <summary>
		/// 新闻类型
		/// </summary>
		public CarNewsTypes CarNewsType { get; set; }
		/// <summary>
		/// 新闻数量
		/// </summary>
		public int NewsCount { get; set; }
		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdateTime { get; set; }
		/// <summary>
		/// 最新评测文章id
		/// </summary>
		public int PingCeNewsId { get; set; }
	}
}
