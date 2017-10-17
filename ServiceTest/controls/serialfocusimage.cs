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
	public partial class serialfocusimage : UserControl
	{
		private const string msbody1 = "<?xml version=\"1.0\" ?><MessageBody><From>Photo</From><ContentType>SerialFocusImage</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime></MessageBody>";

		public serialfocusimage()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			int counter = 0;
			string[] ids = this.textBox1.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string id in ids)
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(string.Format(msbody1, id.Trim(), DateTime.Now.ToString("yyyy-MM-dd")));
				publicmethod.sendMq(doc);
				counter++;
			}
			MessageBox.Show("共发送了[" + counter + "]条消息！");
		}
	}
}
