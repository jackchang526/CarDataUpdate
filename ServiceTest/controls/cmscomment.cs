using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ServiceTest.controls
{
	public partial class cmscomment : UserControl
	{
        private const string msbody1 = "<?xml version=\"1.0\" ?><MessageBody><From>CMS</From><ContentType>NewsComment</ContentType><ContentId>0</ContentId><UpdateTime>{0}</UpdateTime><NewsId>{1}</NewsId></MessageBody>";

		public cmscomment()
		{
			InitializeComponent();
		}

		private void cmscomment_Load(object sender, EventArgs e)
		{
			this.textBox2.Text = DateTime.Now.ToString("yyyy-MM-dd");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(string.Format(msbody1, this.textBox2.Text, this.textBox1.Text));
			publicmethod.sendMq(doc);
			MessageBox.Show("发送消息成功！");
		}
	}
}
