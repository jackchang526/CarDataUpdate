using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class QuestionInfo
    {
        /// <summary>
        /// 问题id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// url地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 子品牌综述页块的 前缀标签，二级分类没有取一级分类
        /// </summary>
        public string RowTagName { get; set; }
        /// <summary>
        /// 子品牌综述页块的 前缀标签url
        /// </summary>
        public string RowTagUrl { get; set; }
        /// <summary>
        /// 子品牌综述页块的title
        /// </summary>
        public string RowTitle { get; set; }
        
        private static QuestionInfo ConvertToQuestion(XmlElement questionNode)
        {
            string timeStr = questionNode.GetAttribute("time");
            DateTime time = new DateTime();
            DateTime.TryParse(timeStr, out time);
            QuestionInfo question = new QuestionInfo()
            {
                Id = questionNode.GetAttribute("id").Trim(),
                Title = questionNode.GetAttribute("title").Trim(),
                Url = questionNode.GetAttribute("url").Trim(),
                Time = time
            };
            //string tagName = questionNode.GetAttribute("tag");
            //if (string.IsNullOrEmpty(tagName))
            //{
                question.RowTagName = questionNode.GetAttribute("category");
                question.RowTagUrl = questionNode.GetAttribute("categoryurl");
            //}
            //else
            //{
            //    question.RowTagName = tagName;
            //    question.RowTagUrl = questionNode.GetAttribute("tagurl");
            //}

            question.RowTitle = question.Title;
            if (StringHelper.GetRealLength(question.RowTagName + question.Title) > 58)
            {
                question.RowTitle = StringHelper.SubString(question.RowTitle,
                    58 - StringHelper.GetRealLength(question.RowTagName), true);
            }
            return question;
        }

        public static QuestionInfo[] ConvertToQuestions(XmlDocument xmlDocument)
        {
            XmlNodeList questionNodes = xmlDocument.SelectNodes("/root/question");
            QuestionInfo[] questions = new QuestionInfo[questionNodes.Count];
            for (int i = 0; i < questionNodes.Count; i++)
            {
                QuestionInfo question = ConvertToQuestion((XmlElement)questionNodes[i]);
                questions[i] = question;
            }

            return questions;
        }
		public static QuestionInfo[] ConvertToQuestions(XmlDocument xmlDocument, string category)
		{
			XmlNodeList questionNodes = xmlDocument.SelectNodes(string.Format("/root/{0}/question",category));
			QuestionInfo[] questions = new QuestionInfo[questionNodes.Count];
			for (int i = 0; i < questionNodes.Count; i++)
			{
				QuestionInfo question = ConvertToQuestion((XmlElement)questionNodes[i]);
				questions[i] = question;
			}

			return questions;
		}
    }

    public class AskExpertInfo
    {
        /// <summary>
        /// 专家名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 专家首页
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 擅长分类
        /// </summary>
        public string Categorys { get; set; }

        /// <summary>
        /// 擅长品牌
        /// </summary>
        public string MasterBrands { get; set; }

        /// <summary>
        /// 专家头像
        /// </summary>
        public string ImageUrl { get; set; }

        private static AskExpertInfo ConvertToExpert(XmlElement expertNode)
        {
            AskExpertInfo expert = new AskExpertInfo()
            {
                UserName = expertNode.GetAttribute("username").Trim(),
                Url = expertNode.GetAttribute("url"),
                Categorys = expertNode.GetAttribute("categorys"),
                MasterBrands = expertNode.GetAttribute("masters"),
                ImageUrl = expertNode.GetAttribute("imgurl")
            };
            if (!string.IsNullOrEmpty(expert.Categorys))
            {
                string[] cates = expert.Categorys.Split(',');
                if (cates.Length > 2)
                    expert.Categorys = string.Format("{0},{1}", cates[0], cates[1]);
            }
            return expert;
        }

        public static AskExpertInfo[] ConvertToExperts(XmlDocument xmlDocument)
        {
            XmlNodeList expertNodes = xmlDocument.SelectNodes("/root/experts/expert");
            AskExpertInfo[] experts = new AskExpertInfo[expertNodes.Count];
            for (int i = 0; i < expertNodes.Count; i++)
            {
                experts[i] = ConvertToExpert((XmlElement)expertNodes[i]);
            }

            return experts;
        }
    }
}
