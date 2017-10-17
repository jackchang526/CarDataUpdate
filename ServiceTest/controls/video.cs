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
	public partial class video : UserControl
	{
		private const string msbody1 = "<?xml version=\"1.0\" ?><MessageBody><From>CMS</From><ContentType>Video</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime><DeleteOp>{2}</DeleteOp></MessageBody>";

		public video()
		{
			InitializeComponent();

			this.cbx_actiontype.Items.AddRange(new string[] { "Update", "Delete" });
			this.cbx_actiontype.SelectedIndex = 0;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			int counter = 0;
			string[] ids = this.textBox1.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string id in ids)
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(string.Format(msbody1,
					id.Trim(),
					DateTime.Now.ToString("yyyy-MM-dd"),
					this.cbx_actiontype.Text == "Update" ? "false" : "true"));
				publicmethod.sendMq(doc);
				counter++;
			}
			MessageBox.Show("共发送了[" + counter + "]条消息！");
		}
	}
}
