using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace ServiceTest.controls
{
	public partial class cmsnews : UserControl
	{
		private const string msbody1 = "<?xml version=\"1.0\" ?><MessageBody><From>CMS</From><ContentType>News</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime></MessageBody>";
		private const string msbody2 = "<?xml version=\"1.0\" ?><MessageBody><From>CMS</From><ContentType>News</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime><DeleteOp>{2}</DeleteOp></MessageBody>";

		public cmsnews()
		{
			InitializeComponent();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox3.Text = this.openFileDialog1.FileName;
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(string.Format(msbody1, this.textBox1.Text, this.textBox2.Text));
			publicmethod.sendMq(doc);
			MessageBox.Show("发送消息成功！");
		}

		private void cmsnews_Load(object sender, EventArgs e)
		{
			this.textBox2.Text = DateTime.Now.ToString("yyyy-MM-dd");
		}

		private void button2_Click(object sender, EventArgs e)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(string.Format(msbody2, this.textBox1.Text, this.textBox2.Text, "true"));
			publicmethod.sendMq(doc);
			MessageBox.Show("发送消息成功！");
		}

		private void button4_Click(object sender, EventArgs e)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(this.textBox3.Text);
			XmlNodeList newsNodeList = xmlDoc.SelectNodes("//listNews/newsid");

			List<int> newsIdList = new List<int>(newsNodeList.Count);
			int counter = 0;
			foreach (XmlElement idNode in newsNodeList)
			{
				int newsId = Convert.ToInt32(idNode.InnerText);
				if (newsIdList.Contains(newsId))
					continue;

				newsIdList.Add(newsId);

				XmlDocument doc = new XmlDocument();
				doc.LoadXml(string.Format(msbody1, newsId, this.textBox2.Text));
				publicmethod.sendMq(doc);

				counter++;
			}
			MessageBox.Show("共发送了[" + counter + "]条消息！");
		}

		private void button5_Click(object sender, EventArgs e)
		{
			int counter = 0;
			using (TextReader reader = new System.IO.StreamReader(this.textBox3.Text))
			{
				string lineStr = null;
				int contentId;
				while ((lineStr = reader.ReadLine()) != null)
				{
					if (!int.TryParse(lineStr, out contentId) || contentId < 1) continue;

					XmlDocument doc = new XmlDocument();
					doc.LoadXml(string.Format(msbody1, contentId, this.textBox2.Text));
					publicmethod.sendMq(doc);

					counter++;
				}
			}
			MessageBox.Show("共发送了[" + counter + "]条消息！");
		}
	}
}
