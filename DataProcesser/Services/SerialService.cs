using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.DataProcesser.Repository;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using System.Xml;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Config;
using System.IO;

namespace BitAuto.CarDataUpdate.DataProcesser.Services
{
	public class SerialService
	{
        /*  ////del by lisf 2016-01-06
		/// <summary>
		/// 获取车款列表 根据子品牌Id
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		public static List<CarInfoForSerialSummaryEntity> GetCarInfoForSerialSummaryBySerialId(int serialId)
		{
			List<CarInfoForSerialSummaryEntity> carInfoList = new List<CarInfoForSerialSummaryEntity>();
			DataSet ds = SerialRepository.GetAllCarInfoForSerialSummary(serialId);
			if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					int carId = ConvertHelper.GetInteger(dr["car_id"]);

					Dictionary<int, string> dictCarPriceRange = CommonData.dicCarPriceRange;
					Dictionary<int, string> dictParams = CarRepository.GetCarAllParamByCarID(carId);

					string saleState = dr["Car_SaleState"].ToString().Trim();
					string carPriceRange = string.Empty;
					if (saleState == "停销")
						carPriceRange = "停售";
					else
						carPriceRange = dictCarPriceRange.ContainsKey(carId) ? dictCarPriceRange[carId] : "";

					carInfoList.Add(new CarInfoForSerialSummaryEntity()
					{
						CarID = carId,
						CarName = dr["car_name"].ToString(),
						SaleState = saleState,
						CarPriceRange = carPriceRange,
						CarPV = dr["Pv_SumNum"].ToString() == "" ? 0 : int.Parse(dr["Pv_SumNum"].ToString()),
						ReferPrice = dr["car_ReferPrice"].ToString(),
						TransmissionType = dr["UnderPan_TransmissionType"].ToString(),
						Engine_Exhaust = dr["Engine_Exhaust"].ToString(),
						CarYear = dr["Car_YearType"] == DBNull.Value ? "" : dr["Car_YearType"].ToString(),
						ProduceState = dr["Car_ProduceState"].ToString(),
						UnderPan_ForwardGearNum = dictParams.ContainsKey(724) ? dictParams[724] : "",//档位个数
						Engine_MaxPower = dictParams.ContainsKey(430) ? (int)(Convert.ToDouble(dictParams[430]) * 1.36) : 0,//最大马力
						Engine_InhaleType = dictParams.ContainsKey(425) ? dictParams[425] : "",//进气型式
						Oil_FuelType = dictParams.ContainsKey(578) ? dictParams[578] : ""//燃料类型
					});
				}
			}
			return carInfoList;
		}*/

		/// <summary>
		/// 获取在产车型所有颜色
		/// </summary>
		/// <param name="serialId"></param>
		public static Dictionary<string, int> GetProduceCarColors(int serialId)
		{
			Dictionary<string, int> dictCarsColor = new Dictionary<string, int>();
			DataSet ds = CarRepository.GetProduceCarsColorBySerialId(serialId);
			if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					string[] colors = row["CarColor"].ToString().Replace("，", ",").Split(',');
					int year = ConvertHelper.GetInteger(row["car_yeartype"]);
					foreach (string colorStr in colors)
					{
						string colorName = colorStr.Trim();
						if (!string.IsNullOrEmpty(colorName) && !dictCarsColor.ContainsKey(colorName))
						{
							dictCarsColor.Add(colorName, year);
						}
					}
				}
			}
			return dictCarsColor;
		}
		/// <summary>
		/// 在产子品牌车身颜色
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		public static List<SerialColorEntity> GetProduceSerialColors(int serialId)
		{
			Dictionary<string, int> dictCarColor = GetProduceCarColors(serialId);
			List<SerialColorEntity> serialColorList = new List<SerialColorEntity>();
			DataSet dsAllColor = SerialRepository.GetAllSerialColorRGB(0);//车型子品牌数据
			if (dsAllColor != null && dsAllColor.Tables.Count > 0 && dsAllColor.Tables[0].Rows.Count > 0)
			{
				DataRow[] drs = dsAllColor.Tables[0].Select(" cs_id='" + serialId + "' ");
				if (drs != null && drs.Length > 0)
				{
					foreach (DataRow dr in drs)
					{
						int autoId = ConvertHelper.GetInteger(dr["autoid"]);
						string colorName = dr["colorName"].ToString().Trim();
						string colorRGB = dr["colorRGB"].ToString().Trim();
						if (dictCarColor.ContainsKey(colorName))
						{
							serialColorList.Add(new SerialColorEntity()
							{
								ColorId = autoId,
								ColorName = colorName,
								ColorYear = dictCarColor[colorName],
								ColorRGB = colorRGB
							});
						}
					}
				}
			}
			return serialColorList;
		}

        /*  //del by lisf 2016-01-06
		public static Dictionary<int, Dictionary<string, string>> GetAllSerialColorNameRGB()
		{
			Dictionary<int, Dictionary<string, string>> dic = new Dictionary<int, Dictionary<string, string>>();
			try
			{
				// 取有RGB值的车身颜色
				DataSet ds = SerialRepository.GetAllSerialColorRGB(0);
				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						int csid = int.Parse(dr["cs_id"].ToString());
						string colorName = dr["colorName"].ToString().Trim();
						string colorRGB = dr["colorRGB"].ToString().Trim();
						if (dic.ContainsKey(csid))
						{
							if (!dic[csid].ContainsKey(colorName))
							{ dic[csid].Add(colorName, colorRGB); }
						}
						else
						{
							Dictionary<string, string> dicCs = new Dictionary<string, string>();
							dicCs.Add(colorName, colorRGB);
							dic.Add(csid, dicCs);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return dic;
		}*/
		/// <summary>
		/// 获取子品牌报价区间xml
		/// </summary>
		/// <returns></returns>
		public void GenerateSerialPriceRange()
		{
			try
			{
				string fileName = Path.Combine(CommonData.CommonSettings.SavePath, @"EP\cspricescope.xml");
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(CommonData.CommonSettings.PriceRangeInterface);
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
	}
}
