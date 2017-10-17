using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.Tools
{
	class Program
	{
		static bool nextLine = true;
		static void Main(string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			Controller controller = new Controller();
			controller.Log += new LogHandler(getter_Log);

			// 获取所有子品牌的区域车型页的行情与促销新闻 
			// 141独立计划任务
			//args = new string[] { "UpdateSerialCityNews" };

			//直接显示帮助
			if (args != null && args.Length > 0 && (args[0].ToLower() == "/help" || args[0].ToLower() == "/showhelp"))
			{
				controller.ShowHelp();
				return;
			}
			try
			{
				getter_Log(controller, new LogArgs("公用数据初始化...", true));
				Common.CommonData.InitData();
				getter_Log(controller, new LogArgs("公用数据初始化完成...", true));

				//// 测试新闻服务
				//Common.Model.ContentMessage cm = new Common.Model.ContentMessage();
				//cm.ContentType = "News";
				//cm.From = "CMS";
				//cm.ContentId = 6083096;
				//cm.UpdateTime = DateTime.Now;
				//cm.ContentBody = new System.Xml.XmlDocument();
				//cm.ContentBody.LoadXml("<MessageBody><From>CMS</From><ContentType>News</ContentType><ContentId>6083096</ContentId><UpdateTime>2013-05-14</UpdateTime></MessageBody>");
				//BitAuto.CarDataUpdate.NewsProcesser.MessageProcesser mp
				//    = new BitAuto.CarDataUpdate.NewsProcesser.MessageProcesser();
				//mp.Processer(cm);

				controller.Execute(args);
			}
			catch (Exception ex)
			{
				getter_Log(controller, new LogArgs(ex.ToString(), true));
			}
		}
		static void getter_Log(object sender, LogArgs e)
		{
			if (nextLine != e.NextLine && !nextLine)
			{
				Console.WriteLine();
			}
			if (!nextLine)
				Console.CursorTop -= 1;

			Console.WriteLine(e.LogText);
			Log.WriteLog(e.LogText);
			nextLine = e.NextLine;
		}
	}
}
