using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ServiceTest.controls
{
	public partial class msgbox : Form
	{
		public string messageText;

		public msgbox()
		{
			InitializeComponent();
		}

		private void msgbox_Load(object sender, EventArgs e)
		{
			this.richTextBox1.Text = messageText;
		}
	}
}
