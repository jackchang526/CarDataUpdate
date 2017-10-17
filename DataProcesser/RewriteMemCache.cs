using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using System.Data;
using BitAuto.Services.Cache;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class RewriteMemCache
	{
		#region 更新车型详细参数memcache
		//车型详细参数更新memcache
		public static void RewriteCarCompareMemCache(int carId)
		{
			try
			{
				string memCacheKeyTemp = "Car_Dictionary_CarCompareData_{0}";
				string memCacheKeyWithOptionalTemp = "Car_Dictionary_CarCompareDataWithOptional_{0}";
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
				// 车型最大降价
				Dictionary<int, string> dicCarJiangJiaPrice = CommonData.GetAllCarJiangJia();

				Dictionary<int, Dictionary<string, string>> dicSerialColor = CommonData.dictSerialColor;
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
						//// 车型行情价
						//if (CommonData.dicCarHangQingPrice.ContainsKey(carid))
						//{ carHangQingPrice = CommonData.dicCarHangQingPrice[carid]; }
						// add by chengl Jan.23.2013
						// modified by chengl Apr.1.2013 
						// 降价有计划任务删除失效的最大降幅，所以不能在common内初始化此数据
						if (dicCarJiangJiaPrice.ContainsKey(carid))
						{ carJiangJiaPrice = dicCarJiangJiaPrice[carid]; }

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
						// 车型车身颜色中文名
						string bodyColor = string.Empty;

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
								// 如果是车身颜色
								if (aliasName == "OutStat_BodyColor")
								{ bodyColor = pvalue; }
							}
						}
						dsCarParam.Clear();

						
						#endregion

						#region 车型车身颜色RGB值

						List<string> listBodyColorRGB = new List<string>();
						if (!string.IsNullOrEmpty(bodyColor))
						{
							if (dicSerialColor.ContainsKey(csid))
							{
								// 临时车型参数颜色名
								List<string> listTemp = new List<string>();
								string[] colorNameArray = bodyColor.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
								if (colorNameArray.Length > 0)
								{
									foreach (string name in colorNameArray)
									{
										string colorName = name.Trim();
										if (colorName != "" && !listTemp.Contains(colorName))
										{
											listTemp.Add(colorName);
										}
									}
								}
								if (listTemp.Count > 0)
								{
									foreach (KeyValuePair<string, string> kvp in dicSerialColor[csid])
									{
										if (listTemp.Contains(kvp.Key))
										{
											if (listBodyColorRGB.Count > 0)
											{ listBodyColorRGB.Add("|"); }
											listBodyColorRGB.Add(kvp.Key + "," + kvp.Value);
										}
									}
								}
							}
						}

						dicCar.Add("Car_OutStat_BodyColorRGB", string.Concat(listBodyColorRGB.ToArray()));

						Dictionary<string, string> dicCarWithOptional = new Dictionary<string, string>(dicCar);//克隆一份

						//选配参数
						DataSet dsOptional = CommonData.GetCarOptionalForCompare(carid);
						if (dsOptional != null && dsOptional.Tables.Count > 0 && dsOptional.Tables[0].Rows.Count > 0)
						{
							foreach (DataRow drOptional in dsOptional.Tables[0].Rows)
							{
								//int carid = Convert.ToInt32(dr["CarId"]);
								int pid = Convert.ToInt32(drOptional["ParamId"]);
								string aliasName = drOptional["AliasName"].ToString().Trim();
								string pvalue = drOptional["Pvalue"].ToString().Trim();

								float price = Convert.ToSingle(drOptional["Price"]);

								if (pvalue == "" || price == 0)
								{ continue; }

								if (!dicCar.ContainsKey(aliasName))
								{
									dicCarWithOptional.Add(aliasName, string.Format("{0}|{1}", pvalue, price));
								}
								else
								{
									if (dicCarWithOptional[aliasName] == "选配" && pvalue == "选配")
									{
										dicCarWithOptional[aliasName] = string.Format("{0}|{1}", pvalue, price);
									}
									else
									{
										dicCarWithOptional[aliasName] = string.Format("{0},{1}|{2}", dicCarWithOptional[aliasName], pvalue, price);
									}
								}
							}
						}
						#endregion

						#region 数据存入memcache
						if (dicCar != null && dicCar.Count > 0)
						{
							//modified by sk 2015.11.24 mem 改为 2小时
							DistCacheWrapper.Insert(string.Format(memCacheKeyTemp, carid), dicCar, 1000 * 60 * 60 * 2);
						}
						if (dicCarWithOptional != null && dicCarWithOptional.Count > 0)
						{
							//modified by sk 2015.11.24 mem 改为 2小时
							DistCacheWrapper.Insert(string.Format(memCacheKeyWithOptionalTemp, carid), dicCarWithOptional, 1000 * 60 * 60 * 2);
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
			catch (Exception ex)
			{
				Log.WriteErrorLog("更新车型详细参数memcache异常：carId=" + carId + "\r\n" + ex.ToString());
			}
		}
		#endregion

		#region 更新指数memcache
		/// <summary>
		/// 子品牌级别排行 指数数据MemCache数据
		/// </summary>
		public static void ReWriteIndexSerialLevelRank()
		{
			// 取进6个月有销量的子品牌ID
			GetLastSixMonthSaleSerial();

			string memCacheKeyTemp = "Car_Dictionary_{0}";

			// 关注指数子品牌的按级别排行 取最后的月
			Dictionary<int, int> dicUVIX = new Dictionary<int, int>();
			// 购车指数子品牌的按级别排行 取最后的月
			Dictionary<int, int> dicDealerIX = new Dictionary<int, int>();
			// 销量指数子品牌的按级别排行 取最后的月
			Dictionary<int, int> dicSaleIX = new Dictionary<int, int>();

			GetAllLevelSerialRankToDic(dicUVIX, com.bitauto.index.IndexType.UV);
			GetAllLevelSerialRankToDic(dicDealerIX, com.bitauto.index.IndexType.Dealer);
			GetAllLevelSerialRankToDic(dicSaleIX, com.bitauto.index.IndexType.Sale);

			#region 数据存入memcache
			if (dicUVIX != null && dicUVIX.Count > 0)
			{
				// 默认7天
				DistCacheWrapper.Insert(string.Format(memCacheKeyTemp, "SerialLevelUVRank"), dicUVIX, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
			}
			else
			{
				Console.WriteLine("\r\nMemCache 关注指数为0...");
				Log.WriteLog("MemCache 关注指数为0...");
			}

			if (dicDealerIX != null && dicDealerIX.Count > 0)
			{
				// 默认7天
				DistCacheWrapper.Insert(string.Format(memCacheKeyTemp, "SerialLevelDealerRank"), dicDealerIX, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
			}
			else
			{
				Console.WriteLine("\r\nMemCache 购车指数为0...");
				Log.WriteLog("MemCache 购车指数为0...");
			}

			if (dicUVIX != null && dicUVIX.Count > 0)
			{
				// 默认7天
				DistCacheWrapper.Insert(string.Format(memCacheKeyTemp, "SerialLevelSaleRank"), dicSaleIX, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
			}
			else
			{
				Console.WriteLine("\r\nMemCache 销量指数为0...");
				Log.WriteLog("MemCache 销量指数为0...");
			}
			#endregion

		}

		/// <summary>
		/// 取进6个月有销量的子品牌ID
		/// </summary>
		private static void GetLastSixMonthSaleSerial()
		{
			try
			{
				com.bitauto.index.IndexData IX = new com.bitauto.index.IndexData();
				System.Net.ServicePointManager.Expect100Continue = false;
				string csListStr = IX.GetCsidListByLastSixMonth(com.bitauto.index.IndexType.Sale);
				if (!string.IsNullOrEmpty(csListStr) && csListStr.Length > 0)
				{
					string fileName = CommonData.CommonSettings.SavePath + "\\SaleCsIDList.xml";
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
					sb.AppendLine("<Root>");
					string[] csListArray = csListStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					if (csListArray.Length > 0)
					{
						foreach (string csidStr in csListArray)
						{
							int csid = 0;
							if (int.TryParse(csidStr, out csid))
							{
								if (csid > 0)
								{
									sb.AppendLine(string.Format("<Cs ID=\"{0}\"/>", csid));
								}
							}
						}
					}
					sb.AppendLine("</Root>");
					CommonFunction.SaveFileContent(sb.ToString(), fileName, "utf-8");
				}
			}
			catch (Exception ex)
			{
				Log.WriteLog(ex.ToString());
			}
		}

		/// <summary>
		/// 通过接口取特定指数的按级别子品牌排名
		/// </summary>
		/// <param name="dic">子品牌按级别排名 子品牌ID,排名</param>
		/// <param name="it">指数类型</param>
		private static void GetAllLevelSerialRankToDic(Dictionary<int, int> dic
			, com.bitauto.index.IndexType it)
		{
			com.bitauto.index.IndexData IX = new com.bitauto.index.IndexData();
			// modified by chengl Nov.25.2013
			System.Net.ServicePointManager.Expect100Continue = false;
			Dictionary<string, int> dicLevelSpell = GetSerialLevelSpellIndexDic();
			if (dicLevelSpell != null && dicLevelSpell.Count > 0)
			{
				foreach (KeyValuePair<string, int> kvp in dicLevelSpell)
				{
					// 关注指数
					com.bitauto.index.IndexItem[] ii =
							IX.GetIndexDataListByLastDate(it,
								   com.bitauto.index.BrandType.Level, kvp.Key, 0);
					if (ii != null && ii.Length > 0)
					{
						for (int i = 0; i < ii.Length; i++)
						{
							if (!dic.ContainsKey(ii[i].ID))
							{ dic.Add(ii[i].ID, i + 1); }
						}
					}

				}
			}
		}
		/// <summary>
		/// 级别拼写对照指数级别文件后缀
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, int> GetSerialLevelSpellIndexDic()
		{
			Dictionary<string, int> dic = new Dictionary<string, int>();
			dic.Add("weixingche", 1);
			dic.Add("xiaoxingche", 2);
			dic.Add("jincouxingche", 3);
			dic.Add("zhongdaxingche", 4);
			dic.Add("zhongxingche", 5);
			dic.Add("haohuaxingche", 6);
			dic.Add("mpv", 7);
			dic.Add("suv", 8);
			dic.Add("paoche", 9);
			dic.Add("qita", 10);
			dic.Add("mianbaoche", 11);
			dic.Add("pika", 12);
			return dic;
		}
		#endregion
	}
}
