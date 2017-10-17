using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ServiceTest.cs;

namespace ServiceTest.controls
{
	public partial class carinfoforcar : UserControl
	{
        private const string msbody1 = "<?xml version=\"1.0\" ?><MessageBody><From>Car</From><ContentType>{2}</ContentType><ContentId>{0}</ContentId><UpdateTime>{1}</UpdateTime><ActionType>{3}</ActionType></MessageBody>";
		public carinfoforcar()
		{
			InitializeComponent();

			this.comboBox1.Items.Add(new mstype() { name = "车型", control = "Car" });
			this.comboBox1.Items.Add(new mstype() { name = "子品牌", control = "Serial" });
			this.comboBox1.Items.Add(new mstype() { name = "品牌", control = "Brand" });
			this.comboBox1.Items.Add(new mstype() { name = "主品牌", control = "MasterBrand" });
			this.comboBox1.Items.Add(new mstype() { name = "厂商", control = "Producer" });
            this.comboBox1.SelectedIndex = 1;

            this.cbx_actiontype.Items.AddRange(new string[] { "Update", "Insert", "Delete" });
            this.cbx_actiontype.SelectedIndex = 0;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			mstype t = (mstype)this.comboBox1.SelectedItem;
			int counter = 0;
			string[] ids = this.textBox1.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string id in ids)
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(string.Format(msbody1, 
					id.Trim(), 
					DateTime.Now.ToString("yyyy-MM-dd"),
					t.control,
                    this.cbx_actiontype.Text
					));
				publicmethod.sendMq(doc);
				counter++;
			}
			MessageBox.Show("共发送了[" + counter + "]条消息！");
		}
	}
}
