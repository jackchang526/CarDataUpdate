using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BitAuto.CarDataUpdate.Common
{
	public static class Log
	{
		private static log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void WriteLog(string logInfo)
		{
			try
			{
				//if (CommonData.IsWriteLog)
				//{
				//    string fileName = DateTime.Now.ToString("MM-dd") + ".log";
				//    string logPath = Path.Combine(CommonData.ApplicationPath, "Log");
				//    if (!Directory.Exists(logPath))
				//        Directory.CreateDirectory(logPath);
				//    fileName = Path.Combine(logPath, fileName);
				//    logInfo = "\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "]\r\n" + logInfo;
				//    File.AppendAllText(fileName, logInfo);
				//}
				log4.Info(logInfo);
			}
			catch (System.Exception ex)
			{
				string errInfo = "\r\nwrite log error:\r\n";
				errInfo += "logInfo:" + logInfo + "\r\nerror:" + ex.ToString();
				WriteErrorLog(errInfo);
			}
		}
		public static void WriteErrorLog(string logInfo)
		{
			try
			{
				//string fileName = DateTime.Now.ToString("Error-MM-dd") + ".log";
				//string logPath = Path.Combine(CommonData.ApplicationPath, "Log");
				//fileName = Path.Combine(logPath, fileName);
				//logInfo = "\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "]\r\n" + logInfo;
				//File.AppendAllText(fileName, logInfo);
				log4.Error(logInfo);
			}
			catch
			{

			}
		}
		public static void WriteErrorLog(Exception exp)
		{
			try
			{
				//string fileName = DateTime.Now.ToString("Error-MM-dd") + ".log";
				//string logPath = Path.Combine(CommonData.ApplicationPath, "Log");
				//fileName = Path.Combine(logPath, fileName);
				//string logInfo = "\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "]\r\nmsg:[" + exp.Message + "]\r\nStack:[" + exp.StackTrace + "]\r\n";
				//File.AppendAllText(fileName, logInfo);
				log4.Error(exp.ToString());
			}
			catch
			{

			}
		}

		public static void WriteInfoLog(string dir, string logInfo)
		{
			try
			{
				string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
				string logPath = Path.Combine(CommonData.ApplicationPath, dir);
				if (!Directory.Exists(logPath))
					Directory.CreateDirectory(logPath);
				fileName = Path.Combine(logPath, fileName);
				logInfo = "\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "]" + logInfo;
				File.AppendAllText(fileName, logInfo);
			}
			catch
			{

			}
		}

	}

	public delegate void LogHandler(object sender, LogArgs e);

	public class LogArgs : EventArgs
	{
		private string m_logText;
		private bool m_nextLine;

		public LogArgs() { }

		public LogArgs(string logText, bool nextLine)
		{
			m_logText = logText;
			m_nextLine = nextLine;
		}

		public string LogText
		{
			get { return m_logText; }
			set { m_logText = value; }
		}

		/// <summary>
		/// 是否在下一行显示
		/// </summary>
		public bool NextLine
		{
			get { return m_nextLine; }
			set { m_nextLine = value; }
		}
	}
}
