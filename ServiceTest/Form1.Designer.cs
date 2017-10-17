namespace ServiceTest
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnSendNewsComment = new System.Windows.Forms.Button();
			this.updateTime = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtNewsId = new System.Windows.Forms.TextBox();
			this.btnDel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.btnSendMessage = new System.Windows.Forms.Button();
			this.btnSendNewNews = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnSendMsgByTxtFile = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.btnSendMsgByFile = new System.Windows.Forms.Button();
			this.txtFileName = new System.Windows.Forms.TextBox();
			this.btnSaveSortInfo = new System.Windows.Forms.Button();
			this.btnSendJiangJiaMsg = new System.Windows.Forms.Button();
			this.btnDelJiangJia = new System.Windows.Forms.Button();
			this.btnSendJiangJiaMsgByFile = new System.Windows.Forms.Button();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.btnDelJiangJia);
			this.groupBox2.Controls.Add(this.btnSendJiangJiaMsg);
			this.groupBox2.Controls.Add(this.btnSendNewsComment);
			this.groupBox2.Controls.Add(this.updateTime);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.txtNewsId);
			this.groupBox2.Controls.Add(this.btnDel);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.btnSendMessage);
			this.groupBox2.Location = new System.Drawing.Point(12, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(628, 82);
			this.groupBox2.TabIndex = 9;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "单条消息";
			// 
			// btnSendNewsComment
			// 
			this.btnSendNewsComment.Location = new System.Drawing.Point(396, 25);
			this.btnSendNewsComment.Name = "btnSendNewsComment";
			this.btnSendNewsComment.Size = new System.Drawing.Size(126, 23);
			this.btnSendNewsComment.TabIndex = 7;
			this.btnSendNewsComment.Text = "发送新闻评论";
			this.btnSendNewsComment.UseVisualStyleBackColor = true;
			this.btnSendNewsComment.Click += new System.EventHandler(this.btnSendNewsComment_Click);
			// 
			// updateTime
			// 
			this.updateTime.Location = new System.Drawing.Point(56, 57);
			this.updateTime.Name = "updateTime";
			this.updateTime.Size = new System.Drawing.Size(100, 21);
			this.updateTime.TabIndex = 6;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 60);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 12);
			this.label3.TabIndex = 5;
			this.label3.Text = "时间：";
			// 
			// txtNewsId
			// 
			this.txtNewsId.Location = new System.Drawing.Point(56, 27);
			this.txtNewsId.Name = "txtNewsId";
			this.txtNewsId.Size = new System.Drawing.Size(100, 21);
			this.txtNewsId.TabIndex = 1;
			// 
			// btnDel
			// 
			this.btnDel.Location = new System.Drawing.Point(295, 25);
			this.btnDel.Name = "btnDel";
			this.btnDel.Size = new System.Drawing.Size(75, 23);
			this.btnDel.TabIndex = 4;
			this.btnDel.Text = "删除新闻";
			this.btnDel.UseVisualStyleBackColor = true;
			this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 30);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 0;
			this.label2.Text = "消息ID：";
			// 
			// btnSendMessage
			// 
			this.btnSendMessage.Location = new System.Drawing.Point(162, 25);
			this.btnSendMessage.Name = "btnSendMessage";
			this.btnSendMessage.Size = new System.Drawing.Size(113, 23);
			this.btnSendMessage.TabIndex = 0;
			this.btnSendMessage.Text = "发送一条消息";
			this.btnSendMessage.UseVisualStyleBackColor = true;
			this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
			// 
			// btnSendNewNews
			// 
			this.btnSendNewNews.Location = new System.Drawing.Point(131, 226);
			this.btnSendNewNews.Name = "btnSendNewNews";
			this.btnSendNewNews.Size = new System.Drawing.Size(111, 23);
			this.btnSendNewNews.TabIndex = 8;
			this.btnSendNewNews.Text = "发送最新消息";
			this.btnSendNewNews.UseVisualStyleBackColor = true;
			this.btnSendNewNews.Click += new System.EventHandler(this.btnSendNewNews_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.btnSendMsgByTxtFile);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.btnSendJiangJiaMsgByFile);
			this.groupBox1.Controls.Add(this.btnSendMsgByFile);
			this.groupBox1.Controls.Add(this.txtFileName);
			this.groupBox1.Location = new System.Drawing.Point(12, 114);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(642, 89);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "按文件发送消息";
			// 
			// btnSendMsgByTxtFile
			// 
			this.btnSendMsgByTxtFile.Location = new System.Drawing.Point(355, 55);
			this.btnSendMsgByTxtFile.Name = "btnSendMsgByTxtFile";
			this.btnSendMsgByTxtFile.Size = new System.Drawing.Size(75, 23);
			this.btnSendMsgByTxtFile.TabIndex = 3;
			this.btnSendMsgByTxtFile.Text = "文本消息发送";
			this.btnSendMsgByTxtFile.UseVisualStyleBackColor = true;
			this.btnSendMsgByTxtFile.Click += new System.EventHandler(this.btnSendMsgByTxtFile_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 60);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(221, 12);
			this.label1.TabIndex = 2;
			this.label1.Text = "说明：将文件中的新闻发送一遍用以测试";
			// 
			// btnSendMsgByFile
			// 
			this.btnSendMsgByFile.Location = new System.Drawing.Point(256, 55);
			this.btnSendMsgByFile.Name = "btnSendMsgByFile";
			this.btnSendMsgByFile.Size = new System.Drawing.Size(93, 23);
			this.btnSendMsgByFile.TabIndex = 1;
			this.btnSendMsgByFile.Text = "发送消息";
			this.btnSendMsgByFile.UseVisualStyleBackColor = true;
			this.btnSendMsgByFile.Click += new System.EventHandler(this.btnSendMsgByFile_Click);
			// 
			// txtFileName
			// 
			this.txtFileName.Location = new System.Drawing.Point(7, 21);
			this.txtFileName.Name = "txtFileName";
			this.txtFileName.Size = new System.Drawing.Size(515, 21);
			this.txtFileName.TabIndex = 0;
			// 
			// btnSaveSortInfo
			// 
			this.btnSaveSortInfo.Location = new System.Drawing.Point(12, 226);
			this.btnSaveSortInfo.Name = "btnSaveSortInfo";
			this.btnSaveSortInfo.Size = new System.Drawing.Size(113, 23);
			this.btnSaveSortInfo.TabIndex = 6;
			this.btnSaveSortInfo.Text = "保存排序信息";
			this.btnSaveSortInfo.UseVisualStyleBackColor = true;
			this.btnSaveSortInfo.Click += new System.EventHandler(this.btnSaveSortInfo_Click);
			// 
			// btnSendJiangJiaMsg
			// 
			this.btnSendJiangJiaMsg.Location = new System.Drawing.Point(162, 55);
			this.btnSendJiangJiaMsg.Name = "btnSendJiangJiaMsg";
			this.btnSendJiangJiaMsg.Size = new System.Drawing.Size(113, 23);
			this.btnSendJiangJiaMsg.TabIndex = 8;
			this.btnSendJiangJiaMsg.Text = "发送一条降价消息";
			this.btnSendJiangJiaMsg.UseVisualStyleBackColor = true;
			this.btnSendJiangJiaMsg.Click += new System.EventHandler(this.btnSendJiangJiaMsg_Click);
			// 
			// btnDelJiangJia
			// 
			this.btnDelJiangJia.Location = new System.Drawing.Point(295, 53);
			this.btnDelJiangJia.Name = "btnDelJiangJia";
			this.btnDelJiangJia.Size = new System.Drawing.Size(93, 23);
			this.btnDelJiangJia.TabIndex = 9;
			this.btnDelJiangJia.Text = "删除降价新闻";
			this.btnDelJiangJia.UseVisualStyleBackColor = true;
			this.btnDelJiangJia.Click += new System.EventHandler(this.btnDelJiangJia_Click);
			// 
			// btnSendJiangJiaMsgByFile
			// 
			this.btnSendJiangJiaMsgByFile.Location = new System.Drawing.Point(436, 55);
			this.btnSendJiangJiaMsgByFile.Name = "btnSendJiangJiaMsgByFile";
			this.btnSendJiangJiaMsgByFile.Size = new System.Drawing.Size(117, 23);
			this.btnSendJiangJiaMsgByFile.TabIndex = 1;
			this.btnSendJiangJiaMsgByFile.Text = "发送文本降价消息";
			this.btnSendJiangJiaMsgByFile.UseVisualStyleBackColor = true;
			this.btnSendJiangJiaMsgByFile.Click += new System.EventHandler(this.btnSendJiangJiaMsgByFile_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(723, 354);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.btnSendNewNews);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnSaveSortInfo);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtNewsId;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.Button btnSendNewNews;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSendMsgByFile;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Button btnSaveSortInfo;
        private System.Windows.Forms.TextBox updateTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSendNewsComment;
		private System.Windows.Forms.Button btnSendMsgByTxtFile;
		private System.Windows.Forms.Button btnSendJiangJiaMsg;
		private System.Windows.Forms.Button btnDelJiangJia;
		private System.Windows.Forms.Button btnSendJiangJiaMsgByFile;
    }
}

