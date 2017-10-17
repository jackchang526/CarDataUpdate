using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.DataProcesser.Repository;
using System.IO;
using System.Xml;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser.Services
{
	public class CarService
	{
		/// <summary>
		/// 获取车款所有参数
		/// </summary>
		/// <param name="carID">车款id</param>
		/// <returns></returns>
		public static Dictionary<int, string> GetCarAllParamByCarID(int carID)
		{
			return CarRepository.GetCarAllParamByCarID(carID);
		}

		public void GenerateCarPriceRange()
		{
			try
			{
				string fileName = Path.Combine(CommonData.CommonSettings.SavePath, @"EP\carpricescope.xml");
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(CommonData.CommonSettings.AllCarPriceNoZone);
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
 	}
}
