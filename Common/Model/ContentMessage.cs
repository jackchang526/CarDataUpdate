using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BitAuto.CarDataUpdate.Common.Model
{
	public class ContentMessage
	{
		// add by chengl Sep.3.2013
		protected Int64 m_logID;
		protected bool m_isDelete;
		protected string m_from;
		protected string m_contentType;
		protected int m_contentId;
		protected DateTime m_updateTime;
		protected XmlDocument m_contentDoc;

		/// <summary>
		/// 消息来源
		/// </summary>
		public string From
		{
			get { return m_from; }
			set { m_from = value; }
		}

		/// <summary>
		/// 内容类型
		/// </summary>
		public string ContentType
		{
			get { return m_contentType; }
			set { m_contentType = value; }
		}

		/// <summary>
		/// 内容ID
		/// </summary>
		public int ContentId
		{
			get { return m_contentId; }
			set { m_contentId = value; }
		}

		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdateTime
		{
			get { return m_updateTime; }
			set { m_updateTime = value; }
		}

		/// <summary>
		/// 获取到的内容
		/// </summary>
		public XmlDocument ContentBody
		{
			get { return m_contentDoc; }
			set { m_contentDoc = value; }
		}

		/// <summary>
		/// 日志ID 数据库表MessageStat 自增列
		/// </summary>
		public Int64 LogID
		{
			get { return m_logID; }
			set { m_logID = value; }
		}

		/// <summary>
		/// 是否是删除消息
		/// </summary>
		public bool IsDelete
		{
			get { return m_isDelete; }
			set { m_isDelete = value; }
		}
	}
}
