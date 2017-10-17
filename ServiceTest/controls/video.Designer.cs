namespace ServiceTest.controls
{
	partial class video
	{
		/// <summary> 
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region 组件设计器生成的代码

		/// <summary> 
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.label2 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbx_actiontype = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 53);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(101, 72);
			this.label2.TabIndex = 11;
			this.label2.Text = "可以输入多个id，\r\n每行一个id\r\n如：\r\n102783\r\n102782\r\n...";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(47, 200);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 10;
			this.button1.Text = "发送";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(112, 22);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(305, 140);
			this.textBox1.TabIndex = 9;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(30, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 12);
			this.label1.TabIndex = 8;
			this.label1.Text = "视频ID";
			// 
			// cbx_actiontype
			// 
			this.cbx_actiontype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbx_actiontype.FormattingEnabled = true;
			this.cbx_actiontype.Location = new System.Drawing.Point(112, 168);
			this.cbx_actiontype.Name = "cbx_actiontype";
			this.cbx_actiontype.Size = new System.Drawing.Size(121, 20);
			this.cbx_actiontype.TabIndex = 15;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 168);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(71, 12);
			this.label3.TabIndex = 16;
			this.label3.Text = "ActionType:";
			// 
			// video
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cbx_actiontype);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Name = "video";
			this.Size = new System.Drawing.Size(544, 257);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbx_actiontype;
		private System.Windows.Forms.Label label3;
	}
}
