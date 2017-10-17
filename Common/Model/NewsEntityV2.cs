using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    /// <summary>
    /// date:2017-1-10
    /// desc:注意将该类与本目录下NewsEntity.cs文件中的类(旧版本)加以区分，该类对应周新锋提供的最新json接口转换所需用到的改版后的新闻类
    /// author:zf
    /// </summary>
    /// <summary>
    /// 新闻信息类
    /// </summary>
    public class NewsEntityV2
    {
        public List<NewsPageEntity> Pages { get; set; }
        public string TemplatePath { get; set; }
        public string Author { get; set; }
        public int CategoryId { get; set; }
        public short CopyRight { get; set; }
        public string ImageCoverUrl { get; set; }
        public int NewsId { get; set; }
        public DateTime? PublishTime { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public Guid UniqueId { get; set; }
        public string Url { get; set; }
        public string LinkUrl { get; set; }
        public string RelatedBrand { get; set; }
        public string KeyWords { get; set; }
        public string Summary { get; set; }
        public string Tags { get; set; }
        public string VendorId { get; set; }
        public Source Source { get; set; }
        public Editor Editor { get; set; }
        public int CommentCount { get; set; }
        public List<string> MoreImages { get; set; }
    }

    /// <summary>
    /// 新闻分页类
    /// </summary>
    public class NewsPageEntity
    {
        public int? Carid { get; set; }
        public string Content { get; set; }
        public string SerialId { get; set; }
        public string Title { get; set; }
        //public bool IsEstimate { get; set; }
        public string PageUrl { get; set; }
    }
    public class Source
    {
        public string Url { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
    /// <summary>
    /// 新闻作者信息类
    /// </summary>
    public class Editor
    {
        public string Url { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
