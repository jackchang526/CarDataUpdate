using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
    /// 车贷数据接口
	/// </summary>
	public class InsuranceAndLoan
    {
        #region   del by lsf 2016-01-06
        /*
		/// <summary>
		/// 取车贷数据接口
		/// </summary>
		public static void UpdateCarLoanPackageXml()
		{
			var url = Common.CommonData.CommonSettings.CarLoanPackageXmlUrl;
			var path = Path.Combine(Common.CommonData.CommonSettings.SavePath, "InsuranceAndLoan\\CalculateAllSerialBrands.xml");

			if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path))
			{
				Console.WriteLine("CommonSettings配置文件缺少必要的配置'CarLoanPackageXmlUrl, /help!");
			}

			string backFilePath = string.Empty;
			try
			{
				FileInfo fileInfo = new FileInfo(path);

				//存在备份上一版本
				if (fileInfo.Exists)
				{
					backFilePath = fileInfo.FullName + ".bak";
					fileInfo.CopyTo(backFilePath, true);
				}

				if (!fileInfo.Directory.Exists)
				{
					fileInfo.Directory.Create();
				}

				XDocument xdoc = XDocument.Load(url);

				xdoc.Save(path);
				Log.WriteLog("取车贷数据接口结束.");
			}
			catch (Exception exp)
			{
				//回滚
				if (File.Exists(backFilePath))
				{
					File.Copy(backFilePath, path, true);
				}
				Log.WriteLog("取车贷数据接口异常:" + exp.ToString());
			}
		}
        */
        #endregion
        
	}
}
