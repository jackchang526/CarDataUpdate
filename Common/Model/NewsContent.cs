using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class NewsContent
    {
        /// <summary>
        /// 标识id
        /// </summary>
        public int Id;
        /// <summary>
        /// 新闻id
        /// </summary>
        public int CmsNewsId;
        /// <summary>
        /// 页号
        /// </summary>
        public int PageNum;
        /// <summary>
        /// 作者
        /// </summary>
        public string Author;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title;
        /// <summary>
        /// 标题
        /// </summary>
        public string FaceTitle;
        /// <summary>
        /// Summary
        /// </summary>
        public string Summary;
        /// <summary>
        /// 图片
        /// </summary>
        public string Picture;
        /// <summary>
        /// 内容
        /// </summary>
        public string Content;
        /// <summary>
        /// 第一张图片地址
        /// </summary>
        public string FirstPicUrl;
        /// <summary>
        /// 新闻地址
        /// </summary>
        public string FilePath;
        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublishTime;
        /// <summary>
        /// SourceName
        /// </summary>
        public string SourceName;
        /// <summary>
        /// SourceUrl
        /// </summary>
        public string SourceUrl;
        /// <summary>
        /// 编辑id
        /// </summary>
        public int EditorId;
        /// <summary>
        /// 编辑名称
        /// </summary>
        public string EditorName;
        /// <summary>
        /// 页数
        /// </summary>
        public int PageCount;
        /// <summary>
        /// 新闻分类，-1为无效值,0为原创，1为转载，2为软文，3为爬虫采入，4为没有对应上的，5为论坛引入
        /// </summary>
        public int CreativeType;
        /// <summary>
        /// 车型id
        /// </summary>
        public int CarId;
        /// <summary>
        /// 状态
        /// </summary>
        public bool IsSate;
        /// <summary>
        /// 车型年款
        /// </summary>
        public int YearType;
		/// <summary>
		/// 关联省
		/// </summary>
		public List<int> NewsProvinces;
        /// <summary>
        /// 关联城市
        /// </summary>
        public List<int> NewsCitys;
        /// <summary>
        /// 新闻页信息
        /// 子品牌id
        /// </summary>
        public Dictionary<int, NewsPageInfo> NewsPages;
        /// <summary>
        /// 新闻关联的品牌ID列表
        /// </summary>
        public List<int> BrandIdList;
        /// <summary>
        /// 新闻分类关联的车型新闻分类列表
        /// </summary>
        public Dictionary<int,int> CarNewsTypes;

        /// <summary>
        /// 新闻新闻评论数
        /// </summary>
        public int CommentNum;
		/// <summary>
		/// 视频播放时长,00:50
		/// </summary>
		public string Duration;

        /// <summary>
        /// 置换/行情类 促销：开始时间
        /// </summary>
        public DateTime? BeginDate;

        /// <summary>
        /// 置换/行情类 促销：结束时间
        /// </summary>
        public DateTime? EndDate;
        
    }
}
