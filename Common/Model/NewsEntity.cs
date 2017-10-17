using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class NewsEntity
    {
        private string _Title = string.Empty;
		private string _FaceTitle = string.Empty;
        private DateTime _PublishTime = new DateTime();
        private int _RelatedMainSerialID = 0;
        private string _SerialName = string.Empty;
        private string _PageUrl = string.Empty;
        private int _NewsId = 0;

        /// <summary>
        /// 新闻ID
        /// </summary>
        public int NewsId
        {
            get { return _NewsId; }
            set { _NewsId = value; }
        }
        /// <summary>
        /// 关联的主要子品牌ID
        /// </summary>
        public int RelatedMainSerialID
        {
            get { return _RelatedMainSerialID; }
            set { _RelatedMainSerialID = value; }
        }
        /// <summary>
        /// 关联的主要子品牌名称
        /// </summary>
        public string SerialName
        {
            get { return _SerialName; }
            set { _SerialName = value; }
        }
        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublishTime
        {
            get { return _PublishTime; }
            set { _PublishTime = value; }
        }
        /// <summary>
        /// 新闻标题
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }
		/// <summary>
		/// 新闻短标题
		/// </summary>
		public string FaceTitle
		{
			get { return _FaceTitle; }
			set { _FaceTitle = value; }
		}
        /// <summary>
        /// 页面链接
        /// </summary>
        public string PageUrl
        {
            get { return _PageUrl; }
            set { _PageUrl = value; }
        }
        /// <summary>
        /// 分类id
        /// </summary>
        public int CategoryId { get; set; }
		/// <summary>
		/// 分类信息
		/// </summary>
		public NewsCategoryShowName NewsCategoryShowName { get; set; }
		/// <summary>
		/// 作者
		/// 注：不一定有值，使用时看一下数据源
		/// </summary>
		public string Author { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentNum { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImageLink { get; set; }
    }
}
