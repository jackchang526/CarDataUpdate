using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BitAuto.CarDataUpdate.Service
{
    public class DelayMessage
    {
        /// <summary>
        /// 记录id
        /// </summary>
        public int Id;
        /// <summary>
        /// 新闻id
        /// </summary>
        public int ContentId;
        /// <summary>
        /// 文章的更新时间，到此时间后需要发布文章
        /// </summary>
        public DateTime UpdateDate;
        /// <summary>
        /// 消息类型
        /// </summary>
        public string ContentType;
        /// <summary>
        /// 消息体
        /// </summary>
        public XmlDocument ContentBody;
        ///// <summary>
        ///// 状态
        ///// </summary>
        //public bool State;
    }
}
