using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class NewsPageInfo
    {
        private string m_pageTitle;
        private string m_pageContent;
        private string m_pageLink;
        private string m_pagePicUrl;
        private int m_serialId;
        private int m_pageIndex;
        private int m_serialYear;
        private int m_carId;
        private int m_Id;

        public NewsPageInfo()
        {
            m_pageTitle = String.Empty;
            m_pageContent = String.Empty;
            m_pageLink = String.Empty;
            m_pagePicUrl = String.Empty;
        }

        /// <summary>
        /// 页标题
        /// </summary>
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        /// <summary>
        /// 页标题
        /// </summary>
        public string PageTitle
        {
            get { return m_pageTitle; }
            set { m_pageTitle = value; }
        }

        /// <summary>
        /// 页内容
        /// </summary>
        public string PageContent
        {
            get { return m_pageContent; }
            set { m_pageContent = value; }
        }

        /// <summary>
        /// 页地址
        /// </summary>
        public string PageLink
        {
            get { return m_pageLink; }
            set { m_pageLink = value; }
        }

        /// <summary>
        /// 当前页中第一幅图片的链接
        /// </summary>
        public string FirstPicUrl
        {
            get { return m_pagePicUrl; }
            set { m_pagePicUrl = value; }
        }

        /// <summary>
        /// 关联的子品牌ID
        /// </summary>
        public int SerialId
        {
            get { return m_serialId; }
            set { m_serialId = value; }
        }

        /// <summary>
        /// 页号
        /// </summary>
        public int PageIndex
        {
            get { return m_pageIndex; }
            set { m_pageIndex = value; }
        }

        /// <summary>
        /// 所在子品牌的年款
        /// </summary>
        public int SerialYear
        {
            get { return m_serialYear; }
            set { m_serialYear = value; }
        }

        /// <summary>
        /// 关联的车型ID
        /// </summary>
        public int CarId
        {
            get { return m_carId; }
            set { m_carId = value; }
        }
		/// <summary>
		/// 是否首页
		/// </summary>
		public bool IsFirst { get; set; }
    }
}
