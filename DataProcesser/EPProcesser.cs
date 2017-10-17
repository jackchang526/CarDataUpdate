using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 所有易湃接口在此逐一获取
	/// add by chengl Apr.28.2014
	/// </summary>
	public class EPProcesser
	{
		public event LogHandler Log;
		private string _RootPath = string.Empty;

		public EPProcesser()
		{
			_RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "EP");
		}

		/// <summary>
		/// 所有易湃接口执行方法列表，有新接口需要取在此添加
		/// </summary>
		public void EPProcesserList()
		{
			SaveHuiMaiCheAllCsUrl();
		}

		/// <summary>
		/// 惠买车 新 接口所有 子品牌ID对url地址 http://www.huimaiche.com/api/apicarinfo.aspx
		/// 接口提供 caohb Apr.28.2014
		/// </summary>
		public void SaveHuiMaiCheAllCsUrl()
		{
			OnLog("惠买车新接口所有 子品牌ID对url地址", true);
			string xmlPath = "http://www.huimaiche.com/api/apicarinfo.aspx";
			try
			{
				System.Net.WebClient wc = new System.Net.WebClient();
				string xmlStr = wc.DownloadString(xmlPath);
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xmlStr);
				CommonFunction.SaveXMLDocument(doc, Path.Combine(_RootPath, "HuiMaiCheAllCsUrl.xml"));
			}
			catch (Exception ex)
			{
				OnLog(string.Format("惠买车新接口所有 子品牌ID对url地址异常 {0} ex:{1}", xmlPath, ex.ToString()), true);
			}
			OnLog("惠买车新接口所有 子品牌ID对url地址 end", true);
		}


		/// <summary>
		/// 写Log
		/// </summary>
		/// <param name="logText"></param>
		public void OnLog(string logText, bool nextLine)
		{
			if (Log != null)
				Log(this, new LogArgs(logText, nextLine));
		}

	}
}
