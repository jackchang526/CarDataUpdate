using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using System.Data;
using BitAuto.Services.Cache;
using BitAuto.CarDataUpdate.DataProcesser;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.CarInfo
{
	public class Car : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			int carId = msg.ContentId;
			try
			{
				Log.WriteLog(string.Format("开始更新车型参数memcache，车型ID:{0}", carId));
				RewriteMemCache.RewriteCarCompareMemCache(carId);
				Log.WriteLog(string.Format("开始更新选车工具车型数据，车型ID:{0}", carId));
				//UpdateSelectCarData(carId);//注释掉 不用了
				//更新高级选车工具数据
				UpdateSelectCarDataV2(carId);

				
				//更新购车服务选车表
				//UpdateBuyCarServiceSelectCarData(carId); //临时注释掉
				
				Log.WriteLog(string.Format("开始更新互联互通导航，车型ID:{0}", carId));
				CommonNavigation nav = new CommonNavigation();
				CarEntity car = CommonData.GetCarDataById(carId);
				if (car != null && car.CsId > 0)
				{
					Log.WriteLog(string.Format("更新所属子品牌互联互通导航，子品牌ID:{0}", car.CsId));
					//nav.GenerateSerialNavigation(car.CsId);
					nav.GenerateSerialNavigationV2(car.CsId);
					//nav.GenerateSerialBarInfo(car.CsId);
					nav.GenerateSerialNavigationM(car.CsId);
				}
				nav.GenerateCarNavigationV2(carId);
				
				//nav.GenerateCarNavigation(carId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("更新车款Id异常，carId=" + carId + ",\r\n" + ex.ToString());
			}
		}
		//更新选车工具车型数据
		private void UpdateSelectCarData(int carId)
		{
			try
			{
				CarInfoForSelecting.UpdateCarDataByCarId(carId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
		//更新选车工具车型数据
		private void UpdateSelectCarDataV2(int carId)
		{
			try
			{
				CarInfoForSelecting.UpdateCarDataByCarIdV2(carId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
		/// <summary>
		/// 更新购车服务选车工具表数据
		/// </summary>
		/// <param name="carId"></param>
		private void UpdateBuyCarServiceSelectCarData(int carId)
		{
			try
			{
				CarInfoForSelecting.UpdateBuyCarServiceSelectCar(0, carId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

		/*
		//车型详细参数更新memcache
		public static void RewriteCarCompareMemCache(int carId)
		{
			string memCacheKeyTemp = "Car_Dictionary_CarCompareData_{0}";
			if (carId <= 0) return;
			#region 多种数据源
			DataSet dsAllCarBaseInfo = CommonData.GetAllCarBaseInfo(carId);
			//// 子品牌论坛地址
			//Dictionary<int, string> dicCsBBS = CommonData.GetAllSerialBBSUrl();
			//// 车型网友油耗
			//Dictionary<int, string> dicCarFuel = CommonData.GetAllCarNetfriendsFuel();
			//// 车型报价区间
			//Dictionary<int, string> dicCarPriceRange = CommonData.GetAllCarPriceRange();
			//// 车型封面图
			//Dictionary<int, string> dicCarDefaultPhoto = CommonData.GetCarDefaultPhoto(2);
			//// 子品牌封面
			//Dictionary<int, string> dicSerialPhoto = CommonData.GetAllSerialPicURL(true);
			//// 每个车型的行情价
			//Dictionary<int, string> dicCarHangQingPrice = CommonData.GetAllCarHangQingPrice();
			#endregion

			Dictionary<string, string> dicCar = new Dictionary<string, string>();
			if (dsAllCarBaseInfo != null && dsAllCarBaseInfo.Tables.Count > 0 && dsAllCarBaseInfo.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dr in dsAllCarBaseInfo.Tables[0].Rows)
				{
					#region 基本信息

					int carid = int.Parse(dr["car_id"].ToString());
					int csid = int.Parse(dr["cs_id"].ToString());
					string carName = dr["car_name"].ToString().Trim();
					string csName = dr["cs_name"].ToString().Trim();
					string csShowName = dr["Cs_ShowName"].ToString().Trim();
					string csAllSpell = dr["AllSpell"].ToString().Trim().ToLower();
					string carProduceState = dr["Car_ProduceState"].ToString().Trim();
					string carSaleState = dr["Car_SaleState"].ToString().Trim();
					string carReferPrice = dr["car_ReferPrice"].ToString().Trim() == "" ? "无" : (decimal.Parse(dr["car_ReferPrice"].ToString().Trim())).ToString("F2") + "万";
					string carYearType = dr["Car_YearType"].ToString().Trim() == "0" ? "" : dr["Car_YearType"].ToString().Trim();
					string bbsURL = "http://baa.bitauto.com/";
					string cs_CarLevel = dr["cs_CarLevel"].ToString().Trim();
					string carHangQingPrice = "";
					string carJiangJiaPrice = "";
					if (CommonData.dicCsBBS.ContainsKey(csid))
					{ bbsURL = CommonData.dicCsBBS[csid]; }
					// 车型网友油耗
					string userFuel = "无";
					if (CommonData.dicCarFuel.ContainsKey(carid))
					{ userFuel = CommonData.dicCarFuel[carid]; }
					userFuel = (userFuel == "无" ? "" : userFuel);
					// 车型报价区间
					string carPriceRange = "无";
					if (CommonData.dicCarPriceRange.ContainsKey(carid))
					{ carPriceRange = CommonData.dicCarPriceRange[carid]; }
					carPriceRange = carPriceRange == "" ? "无" : carPriceRange;
					// 车型图片 先检查车型是否有封面，再检查子品牌封面
					string carPic = CommonData.CommonSettings.DefaultCarPic;
					if (CommonData.dicCarDefaultPhoto.ContainsKey(carid))
					{ carPic = CommonData.dicCarDefaultPhoto[carid]; }
					else if (CommonData.dicSerialPhoto.ContainsKey(csid))
					{ carPic = CommonData.dicSerialPhoto[csid]; }
					else
					{ carPic = CommonData.CommonSettings.DefaultCarPic; }
					// 车型行情价
					if (CommonData.dicCarHangQingPrice.ContainsKey(carid))
					{ carHangQingPrice = CommonData.dicCarHangQingPrice[carid]; }
					// add by chengl Jan.23.2013
					if (CommonData.dicCarJiangJiaPrice.ContainsKey(carid))
					{ carJiangJiaPrice = CommonData.dicCarJiangJiaPrice[carid]; }

					dicCar.Add("Car_ID", carid.ToString());
					dicCar.Add("Car_Name", carName);
					dicCar.Add("CarImg", carPic);
					dicCar.Add("Cs_ID", csid.ToString());
					dicCar.Add("Cs_Name", csName);
					dicCar.Add("Cs_ShowName", csShowName);
					dicCar.Add("Cs_AllSpell", csAllSpell);
					dicCar.Add("Car_YearType", carYearType);
					dicCar.Add("Car_ProduceState", carProduceState);
					dicCar.Add("Car_SaleState", carSaleState);
					dicCar.Add("CarReferPrice", carReferPrice);
					dicCar.Add("AveragePrice", carPriceRange);
					dicCar.Add("Car_UserFuel", userFuel);
					dicCar.Add("Cs_BBSUrl", bbsURL);
					dicCar.Add("Cs_CarLevel", cs_CarLevel);
					dicCar.Add("Car_HangQingPrice", carHangQingPrice);
					dicCar.Add("Car_JiangJiaPrice", carJiangJiaPrice);
					#endregion

					#region 扩展参数
					// 车型扩展参数
					DataSet dsCarParam = CommonData.GetCarParamByCarID(carid);
					if (dsCarParam != null && dsCarParam.Tables.Count > 0 && dsCarParam.Tables[0].Rows.Count > 0)
					{
						foreach (DataRow drCarParam in dsCarParam.Tables[0].Rows)
						{
							string aliasName = drCarParam["AliasName"].ToString().Trim();
							string pvalue = drCarParam["Pvalue"].ToString().Trim();
							if (pvalue == "")
							{ continue; }
							if (!dicCar.ContainsKey(aliasName))
							{
								dicCar.Add(aliasName, pvalue);
							}
						}
					}
					dsCarParam.Clear();
					#endregion

					#region 数据存入memcache
					if (dicCar != null && dicCar.Count > 0)
					{
						// 默认7天
						DistCacheWrapper.Insert(string.Format(memCacheKeyTemp, carid), dicCar, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
						// bool result = mc.Set(string.Format(memCacheKeyTemp, carid), dicCar);
						// LogCommon.WriteOperateLog("车型对比数据跟新memcache 车型ID:" + carid.ToString() + " result:" + result.ToString());
					}
					#endregion

					#region 清理数据
					dicCar.Clear();
					#endregion
				}
			}

			#region 清理数据
			dsAllCarBaseInfo.Clear();
			dicCar.Clear();
			//dicCsBBS.Clear();
			//dicCarFuel.Clear();
			//dicCarPriceRange.Clear();
			//dicCarDefaultPhoto.Clear();
			//dicSerialPhoto.Clear();
			#endregion
		}
		*/
	}
}
