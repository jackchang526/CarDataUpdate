using System;
using System.Collections.Generic;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
	/// <summary>
	/// 新闻分类
	/// </summary>
	public class NewsCategory
	{
		private int m_categoryId;
		private int m_rootCategoryId;
		private string m_categoryPath;

		public NewsCategory(int categoryId)
		{
			m_categoryId = categoryId;
		}

		/// <summary>
		/// 分类ID
		/// </summary>
		public int CategoryId
		{
			get { return m_categoryId; }
			set { m_categoryId = value; }
		}

		/// <summary>
		/// 分类根ID
		/// </summary>
		public int RootCategoryId
		{
			get { return m_rootCategoryId; }
		}

		/// <summary>
		/// 分类全路径
		/// </summary>
		public string CategoryPath
		{
			get { return m_categoryPath; }
			set
			{
				m_categoryPath = value;
				string[] pathIdList = m_categoryPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				if (pathIdList.Length == 0)
					m_rootCategoryId = m_categoryId;
				else
					m_rootCategoryId = Convert.ToInt32(pathIdList[0]);
			}
		}
	}
}
