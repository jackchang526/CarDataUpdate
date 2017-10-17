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
	public partial class serialyearfocusimage : UserControl
	{
		private const string msbody1 = "<?xml version=\"1.0\" ?><MessageBody><From>Photo</From><ContentType>SerialYearFocusImage</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime><Year>{2}</Year></MessageBody>";

		public serialyearfocusimage()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(string.Format(msbody1, this.textBox1.Text, DateTime.Now.ToString("yyyy-MM-dd"), this.textBox2.Text));
			publicmethod.sendMq(doc);
			MessageBox.Show("发送消息成功！");
		}
	}
}
