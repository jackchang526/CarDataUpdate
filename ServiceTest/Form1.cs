using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Messaging;

namespace ServiceTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Assembly cur = Assembly.GetExecutingAssembly();
            this.updateTime.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (txtNewsId.Text.Trim().Length == 0)
            {
                MessageBox.Show("请填写新闻ID！");
                return;
            }
            //队列名称
            string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
            //MessageQueue组件初始化
            MessageQueue queue = new MessageQueue(queuePath);

            //创建一个XMLDocument对象，用来在消息中传输
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root = xmldoc.CreateElement("MessageBody");
            xmldoc.AppendChild(root);
            //来源
            XmlElement fromEle = xmldoc.CreateElement("From");
            root.AppendChild(fromEle);
            fromEle.InnerText = "CMS";

            //消息类型
            XmlElement typeEle = xmldoc.CreateElement("ContentType");
            root.AppendChild(typeEle);
            typeEle.InnerText = "News";

            //内容ID
            XmlElement idEle = xmldoc.CreateElement("ContentId");
            root.AppendChild(idEle);
            idEle.InnerText = txtNewsId.Text;//5164152   //5160327

            //更新时间
            XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
            root.AppendChild(timeEle);
            
            DateTime updatetime;
            if (!DateTime.TryParse(this.updateTime.Text, out updatetime))
                updatetime = DateTime.Now;

            timeEle.InnerText = updatetime.ToString("yyyy-MM-dd");
            queue.DefaultPropertiesToSend.Recoverable = true;

            queue.Send(xmldoc);

            MessageBox.Show("发送消息成功！");

        }

        private void btnSaveSortInfo_Click(object sender, EventArgs e)
        {
            //NewsSortInfo nsi = new NewsSortInfo();
            //nsi.NewsId = 10000;
            //nsi.SortNumber = 1;
            //nsi.StartDate = DateTime.Now;
            //nsi.EndDate = DateTime.Now.AddDays(3);
            //string fileName = @"E:\Data\BrandNews\Brand_News_10000.xml";
            //new BrandNewsRecorder().SaveNewsSortInfoToFile(nsi, fileName);
        }

        private void btnSendMsgByFile_Click(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(txtFileName.Text);
            XmlNodeList newsNodeList = xmlDoc.SelectNodes("//listNews/newsid");

            //队列名称
            string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
			List<int> newsIdList = new List<int>(newsNodeList.Count);
            //MessageQueue组件初始化
            MessageQueue queue = new MessageQueue(queuePath);
            int counter = 0;
            foreach (XmlElement idNode in newsNodeList)
            {
				int newsId = Convert.ToInt32(idNode.InnerText);
				if (newsIdList.Contains(newsId))
					continue;

				newsIdList.Add(newsId);

                //创建一个XMLDocument对象，用来在消息中传输
                XmlDocument xmldoc = new XmlDocument();
                XmlElement root = xmldoc.CreateElement("MessageBody");
                xmldoc.AppendChild(root);
                //来源
                XmlElement fromEle = xmldoc.CreateElement("From");
                root.AppendChild(fromEle);
                fromEle.InnerText = "CMS";

                //消息类型
                XmlElement typeEle = xmldoc.CreateElement("ContentType");
                root.AppendChild(typeEle);
                typeEle.InnerText = "News";

                //内容ID
                XmlElement idEle = xmldoc.CreateElement("ContentId");
                root.AppendChild(idEle);
                idEle.InnerText = newsId.ToString();

                //更新时间
                XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
                root.AppendChild(timeEle);
                timeEle.InnerText = DateTime.Now.ToString();

                queue.Send(xmldoc);
                counter++;
            }
            MessageBox.Show("共发送了[" + counter + "]条消息！");
        }

        private void btnSendNewNews_Click(object sender, EventArgs e)
        {
            //队列名称
            string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
            //MessageQueue组件初始化
            MessageQueue queue = new MessageQueue(queuePath);


            XmlDocument newsInfoDoc = new XmlDocument();
            string fileName = Path.Combine(Application.StartupPath, "newsdata.xml");
            newsInfoDoc.Load(fileName);
            XmlNodeList newsNodeList = newsInfoDoc.SelectNodes("/Workbook/Worksheet/Table/Row");
            int counter = 0;
            foreach (XmlElement newsNode in newsNodeList)
            {
                string newsId = newsNode.ChildNodes[0].InnerText;
                string upDate = "2011-1-1";
                //创建一个XMLDocument对象，用来在消息中传输
                XmlDocument xmldoc = new XmlDocument();
                XmlElement root = xmldoc.CreateElement("MessageBody");
                xmldoc.AppendChild(root);
                //来源
                XmlElement fromEle = xmldoc.CreateElement("From");
                root.AppendChild(fromEle);
                fromEle.InnerText = "CMS";

                //消息类型
                XmlElement typeEle = xmldoc.CreateElement("ContentType");
                root.AppendChild(typeEle);
                typeEle.InnerText = "News";

                //内容ID
                XmlElement idEle = xmldoc.CreateElement("ContentId");
                root.AppendChild(idEle);
                idEle.InnerText = newsId;//5164152   //5160327

                //更新时间
                XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
                root.AppendChild(timeEle);
                timeEle.InnerText = upDate;

                queue.Send(xmldoc);
                counter++;

                if (counter % 2000 == 0)
                    MessageBox.Show("已经发送了 [" + counter + "] 条！");
            }

            MessageBox.Show("共发送了[" + counter + "]条消息！");
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (txtNewsId.Text.Trim().Length == 0)
            {
                MessageBox.Show("请填写新闻ID！");
                return;
            }

            //队列名称
            string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
            //MessageQueue组件初始化
            MessageQueue queue = new MessageQueue(queuePath);

            //创建一个XMLDocument对象，用来在消息中传输
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root = xmldoc.CreateElement("MessageBody");
            xmldoc.AppendChild(root);
            //来源
            XmlElement fromEle = xmldoc.CreateElement("From");
            root.AppendChild(fromEle);
            fromEle.InnerText = "CMS";

            //消息类型
            XmlElement typeEle = xmldoc.CreateElement("ContentType");
            root.AppendChild(typeEle);
            typeEle.InnerText = "News";

            //内容ID
            XmlElement idEle = xmldoc.CreateElement("ContentId");
            root.AppendChild(idEle);
            idEle.InnerText = txtNewsId.Text;//"5170496";//5164152   //5160327

            //更新时间
            XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
            root.AppendChild(timeEle);
            timeEle.InnerText = "2010-5-9";

            //删除节点
            XmlElement delEle = xmldoc.CreateElement("DeleteOp");
            delEle.SetAttribute("newsCategoryId", "30");
            delEle.SetAttribute("brandIds", "1765");
            delEle.SetAttribute("bigBrandIds", "");
            delEle.InnerText = "true";
            root.AppendChild(delEle);

            queue.Send(xmldoc);

            MessageBox.Show("发送消息成功！");
        }

        private void btnSendNewsComment_Click(object sender, EventArgs e)
        {
            if (txtNewsId.Text.Trim().Length == 0)
            {
                MessageBox.Show("请填写新闻IDs！");
                return;
            }
            //队列名称
            string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
            //MessageQueue组件初始化
            MessageQueue queue = new MessageQueue(queuePath);

            //创建一个XMLDocument对象，用来在消息中传输
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root = xmldoc.CreateElement("MessageBody");
            xmldoc.AppendChild(root);
            //来源
            XmlElement fromEle = xmldoc.CreateElement("From");
            root.AppendChild(fromEle);
            fromEle.InnerText = "CMS";

            //消息类型
            XmlElement typeEle = xmldoc.CreateElement("ContentType");
            root.AppendChild(typeEle);
            typeEle.InnerText = "NewsComment";

            //内容ID
            XmlElement idEle = xmldoc.CreateElement("ContentId");
            root.AppendChild(idEle);
            idEle.InnerText = "0";//5164152   //5160327

            //更新时间
            XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
            root.AppendChild(timeEle);

            DateTime updatetime;
            if (!DateTime.TryParse(this.updateTime.Text, out updatetime))
                updatetime = DateTime.Now;

            timeEle.InnerText = updatetime.ToString("yyyy-MM-dd");
            queue.DefaultPropertiesToSend.Recoverable = true;

            //新闻id列表
            XmlElement newsIdEle = xmldoc.CreateElement("NewsId");
            root.AppendChild(newsIdEle);
            newsIdEle.InnerText = txtNewsId.Text.Trim();

            queue.Send(xmldoc);

            MessageBox.Show("发送消息成功！");
        }

		private void btnSendMsgByTxtFile_Click(object sender, EventArgs e)
		{
			if (!File.Exists(txtFileName.Text))
			{
				MessageBox.Show("文件不存在！");
				return;
			}
			//队列名称
			string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
			//MessageQueue组件初始化
			MessageQueue queue = new MessageQueue(queuePath);
			int counter = 0;
			using (TextReader reader = new System.IO.StreamReader(txtFileName.Text))
			{
				string lineStr = null;
				int contentId;
				while ((lineStr= reader.ReadLine())!=null)
				{
					if(!int.TryParse(lineStr, out contentId) || contentId < 1) continue;

					//创建一个XMLDocument对象，用来在消息中传输
					XmlDocument xmldoc = new XmlDocument();
					XmlElement root = xmldoc.CreateElement("MessageBody");
					xmldoc.AppendChild(root);
					//来源
					XmlElement fromEle = xmldoc.CreateElement("From");
					root.AppendChild(fromEle);
					fromEle.InnerText = "CMS";

					//消息类型
					XmlElement typeEle = xmldoc.CreateElement("ContentType");
					root.AppendChild(typeEle);
					typeEle.InnerText = "News";

					//内容ID
					XmlElement idEle = xmldoc.CreateElement("ContentId");
					root.AppendChild(idEle);
					idEle.InnerText = contentId.ToString();

					//更新时间
					XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
					root.AppendChild(timeEle);
					timeEle.InnerText = DateTime.Now.ToString();

					queue.Send(xmldoc);
					counter++;
				}
			}
			MessageBox.Show("共发送了[" + counter + "]条消息！");

		}

		private void btnSendJiangJiaMsg_Click(object sender, EventArgs e)
		{
			if (txtNewsId.Text.Trim().Length == 0)
			{
				MessageBox.Show("请填写新闻ID！");
				return;
			}
			//队列名称
			string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
			//MessageQueue组件初始化
			MessageQueue queue = new MessageQueue(queuePath);

			//创建一个XMLDocument对象，用来在消息中传输
			XmlDocument xmldoc = new XmlDocument();
			XmlElement root = xmldoc.CreateElement("MessageBody");
			xmldoc.AppendChild(root);
			//来源
			XmlElement fromEle = xmldoc.CreateElement("From");
			root.AppendChild(fromEle);
			fromEle.InnerText = "EP";

			//消息类型
			XmlElement typeEle = xmldoc.CreateElement("ContentType");
			root.AppendChild(typeEle);
			typeEle.InnerText = "JiangJiaNews";

			//内容ID
			XmlElement idEle = xmldoc.CreateElement("ContentId");
			root.AppendChild(idEle);
			idEle.InnerText = txtNewsId.Text;//5164152   //5160327

			//更新时间
			XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
			root.AppendChild(timeEle);

			DateTime updatetime;
			if (!DateTime.TryParse(this.updateTime.Text, out updatetime))
				updatetime = DateTime.Now;

			timeEle.InnerText = updatetime.ToString("yyyy-MM-dd");
			queue.DefaultPropertiesToSend.Recoverable = true;

			queue.Send(xmldoc);

			MessageBox.Show("发送消息成功！");
		}

		private void btnDelJiangJia_Click(object sender, EventArgs e)
		{
			if (txtNewsId.Text.Trim().Length == 0)
			{
				MessageBox.Show("请填写新闻ID！");
				return;
			}

			//队列名称
			string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
			//MessageQueue组件初始化
			MessageQueue queue = new MessageQueue(queuePath);

			//创建一个XMLDocument对象，用来在消息中传输
			XmlDocument xmldoc = new XmlDocument();
			XmlElement root = xmldoc.CreateElement("MessageBody");
			xmldoc.AppendChild(root);
			//来源
			XmlElement fromEle = xmldoc.CreateElement("From");
			root.AppendChild(fromEle);
			fromEle.InnerText = "EP";

			//消息类型
			XmlElement typeEle = xmldoc.CreateElement("ContentType");
			root.AppendChild(typeEle);
			typeEle.InnerText = "JiangJiaNews";

			//内容ID
			XmlElement idEle = xmldoc.CreateElement("ContentId");
			root.AppendChild(idEle);
			idEle.InnerText = txtNewsId.Text;//"5170496";//5164152   //5160327

			//更新时间
			XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
			root.AppendChild(timeEle);
			timeEle.InnerText = "2010-5-9";

			//删除节点
			XmlElement delEle = xmldoc.CreateElement("DeleteOp");
			delEle.SetAttribute("newsCategoryId", "30");
			delEle.SetAttribute("brandIds", "1765");
			delEle.SetAttribute("bigBrandIds", "");
			delEle.InnerText = "true";
			root.AppendChild(delEle);

			queue.Send(xmldoc);

			MessageBox.Show("发送消息成功！");
		}

		private void btnSendJiangJiaMsgByFile_Click(object sender, EventArgs e)
		{
			if (!File.Exists(txtFileName.Text))
			{
				MessageBox.Show("文件不存在！");
				return;
			}
			//队列名称
			string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
			//MessageQueue组件初始化
			MessageQueue queue = new MessageQueue(queuePath);
			int counter = 0;
			using (TextReader reader = new System.IO.StreamReader(txtFileName.Text))
			{
				string lineStr = null;
				int contentId;
				while ((lineStr = reader.ReadLine()) != null)
				{
					if (!int.TryParse(lineStr, out contentId) || contentId < 1) continue;

					//创建一个XMLDocument对象，用来在消息中传输
					XmlDocument xmldoc = new XmlDocument();
					XmlElement root = xmldoc.CreateElement("MessageBody");
					xmldoc.AppendChild(root);
					//来源
					XmlElement fromEle = xmldoc.CreateElement("From");
					root.AppendChild(fromEle);
					fromEle.InnerText = "EP";

					//消息类型
					XmlElement typeEle = xmldoc.CreateElement("ContentType");
					root.AppendChild(typeEle);
					typeEle.InnerText = "JiangJiaNews";

					//内容ID
					XmlElement idEle = xmldoc.CreateElement("ContentId");
					root.AppendChild(idEle);
					idEle.InnerText = contentId.ToString();

					//更新时间
					XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
					root.AppendChild(timeEle);
					timeEle.InnerText = DateTime.Now.ToString();

					queue.Send(xmldoc);
					counter++;
				}
			}
			MessageBox.Show("共发送了[" + counter + "]条消息！");
		}
    }
}
