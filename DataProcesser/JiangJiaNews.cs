using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model.JiangJiaNews;
using System.Data.SqlClient;
using System.Data;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 降价新闻
	/// </summary>
	public class JiangJiaNews
	{
		/// <summary>
		/// 每天执行的计划任务
		/// 处理过期的降价新闻数据
		/// </summary>
		public void PlanTask()
		{

			Log.WriteLog("start 每天更新经销商报价及权重");
			UpdateVendorPrice();
			Log.WriteLog("end 每天更新经销商报价及权重");

			Log.WriteLog("start 查找过期的降价新闻");

			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text
				, "SELECT Id,NewsId,SerialId,CityId,ProvinceId FROM JiangJiaNews WHERE IsState = 1 AND EndDateTime < @CurentDate"
				, new SqlParameter("@CurentDate", DateTime.Now.Date));

			if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				DataRowCollection rows = ds.Tables[0].Rows;
				Log.WriteLog(string.Format("共计[{0}]条", rows.Count));
				SqlParameter idParam = new SqlParameter("@JiangJiaNewsId", SqlDbType.Int, 4);
				List<int> cityIds = new List<int>(1);
				List<int> provinceIds = new List<int>(1);
				foreach (DataRow row in rows)
				{
					Log.WriteLog(string.Format("降价新闻处理 start.rowid:{0},newsid:{1}", row["id"].ToString(), row["newsid"].ToString()));

					cityIds.Clear();
					provinceIds.Clear();

					int rowId = ConvertHelper.GetInteger(row["id"].ToString());
					int newsId = ConvertHelper.GetInteger(row["NewsId"].ToString());
					int serialId = ConvertHelper.GetInteger(row["SerialId"].ToString());
					int cityId = ConvertHelper.GetInteger(row["CityId"].ToString());
					int provinceId = ConvertHelper.GetInteger(row["ProvinceId"].ToString());

					cityIds.Add(cityId);
					provinceIds.Add(provinceId);

					idParam.Value = rowId;

					DataSet carDs = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.StoredProcedure
						, "DeleteJiangJiaNewsAndGetCarList"
						, idParam);
					if (carDs != null && carDs.Tables.Count > 0 && carDs.Tables[0].Rows.Count > 0)
					{
						DataRowCollection carRows = carDs.Tables[0].Rows;
						JiangJiaNewsRelatedData relatedData = new JiangJiaNewsRelatedData()
						{
							SerialId = serialId,
							CityIds = cityIds,
							ProvinceIds = provinceIds,
							CarIds = new List<int>(carRows.Count)
						};
						foreach (DataRow carRow in carRows)
						{
							relatedData.CarIds.Add(ConvertHelper.GetInteger(carRow["CarId"].ToString()));
						}
						UpdateJiangJiaSummary(relatedData);
					}
					else
					{
						Log.WriteLog(string.Format("未获取到车型列表.rowid:{0},newsid:{1}", row["id"].ToString(), row["newsid"].ToString()));
					}
					Log.WriteLog(string.Format("降价新闻处理 end.rowid:{0},newsid:{1}", row["id"].ToString(), row["newsid"].ToString()));
				}
			}

			Log.WriteLog("end 查找过期的降价新闻");
		}

		/// <summary>
		/// 更新降价新闻统计信息
		/// 公用方法，新闻服务与计划任务，都是用该方法进行统计
		/// </summary>
		public void UpdateJiangJiaSummary(List<JiangJiaNewsRelatedData> relatedDataList)
		{
			Log.WriteLog("start UpdateJiangJiaSummaryList!");

			if (relatedDataList == null || relatedDataList.Count < 1)
			{
				Log.WriteLog("error relatedDataList is null!");
				return;
			}

			Log.WriteLog(string.Format("UpdateJiangJiaSummaryList! count:{0}", relatedDataList.Count.ToString()));
			foreach (JiangJiaNewsRelatedData relatedData in relatedDataList)
			{
				UpdateJiangJiaSummary(relatedData);
			}

			Log.WriteLog("end UpdateJiangJiaSummaryList!");
		}

		/// <summary>
		/// 更新降价新闻统计信息
		/// 公用方法，新闻服务与计划任务，都是用该方法进行统计
		/// </summary>
		public void UpdateJiangJiaSummary(JiangJiaNewsRelatedData relatedData)
		{
			Log.WriteLog("start UpdateJiangJiaSummary!");

			if (relatedData.SerialId < 1)
			{
				Log.WriteLog("error relatedData.SerialId < 1!");
				return;
			}
			if (relatedData.CityIds == null || relatedData.CityIds.Count < 1)
			{
				Log.WriteLog("error relatedData.CityIds is null!");
				return;
			}
			if (relatedData.ProvinceIds == null || relatedData.ProvinceIds.Count < 1)
			{
				Log.WriteLog("error relatedData.ProvinceIds is null!");
				return;
			}
			if (relatedData.CarIds == null || relatedData.CarIds.Count < 1)
			{
				Log.WriteLog("error relatedData.CarIds is null!");
				return;
			}

			SqlParameter serialParam = new SqlParameter("@SerialId", SqlDbType.Int, 4);
			serialParam.Value = relatedData.SerialId;
			SqlParameter cityParam = new SqlParameter("@CityId", SqlDbType.Int, 4);
			SqlParameter carParam = new SqlParameter("@CarId", SqlDbType.Int, 4);
			SqlParameter typeParam = new SqlParameter("@Type", SqlDbType.Int, 4);

			SqlParameter[] carParamList = new SqlParameter[]{
				serialParam, cityParam, carParam, typeParam
			};

			SqlParameter[] serialParamList = new SqlParameter[]{
				serialParam, cityParam, typeParam
			};

			Log.WriteLog(string.Format("update info [serialid:{3},citys:{0},provinces:{1},cars:{2}]!", relatedData.CityIds.Count, relatedData.ProvinceIds.Count, relatedData.CarIds.Count, relatedData.SerialId));

			Log.WriteLog("update 车型城市统计信息!");
			//车型城市
			typeParam.Value = 3;
			foreach (int cityId in relatedData.CityIds)
			{
				cityParam.Value = cityId;
				foreach (int carId in relatedData.CarIds)
				{
					carParam.Value = carId;
					SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
						CommandType.StoredProcedure, "UpdateJiangJiaNewsCarSummary", carParamList);
				}
			}

			Log.WriteLog("update 车型省份统计信息!");
			//车型省份
			typeParam.Value = 2;
			foreach (int provinceId in relatedData.ProvinceIds)
			{
				cityParam.Value = provinceId;
				foreach (int carId in relatedData.CarIds)
				{
					carParam.Value = carId;
					SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
						CommandType.StoredProcedure, "UpdateJiangJiaNewsCarSummary", carParamList);
				}
			}

			Log.WriteLog("update 车型全国统计信息!");
			//车型全国
			typeParam.Value = 1;
			cityParam.Value = 0;
			foreach (int carId in relatedData.CarIds)
			{
				carParam.Value = carId;
				SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.StoredProcedure, "UpdateJiangJiaNewsCarSummary", carParamList);
			}

			Log.WriteLog("update 子品牌城市统计信息!");
			//子品牌城市
			typeParam.Value = 3;
			foreach (int cityId in relatedData.CityIds)
			{
				cityParam.Value = cityId;
				SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.StoredProcedure, "UpdateJiangJiaNewsSummary", serialParamList);
			}

			Log.WriteLog("update 子品牌省份统计信息!");
			//子品牌省份
			typeParam.Value = 2;
			foreach (int provinceId in relatedData.ProvinceIds)
			{
				cityParam.Value = provinceId;
				SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.StoredProcedure, "UpdateJiangJiaNewsSummary", serialParamList);
			}

			Log.WriteLog("update 子品牌全国统计信息!");
			//子品牌全国
			typeParam.Value = 1;
			cityParam.Value = 0;
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, "UpdateJiangJiaNewsSummary", serialParamList);

			Log.WriteLog("end UpdateJiangJiaSummary!");
		}

		/// <summary>
		/// 每天更新经销商报价及权重
		/// add by chengl Aug.13.2013
		/// </summary>
		public void UpdateVendorPrice()
		{
			Log.WriteLog("每天更新经销商报价及权重 开始");
			string sqlDelete = "truncate table VendorPrice";
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.Text, sqlDelete);
			Log.WriteLog("每天更新经销商报价及权重 结束");

			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("ID", typeof(System.Int32)));
			dt.Columns.Add(new DataColumn("CityID", typeof(System.Int32)));
			dt.Columns.Add(new DataColumn("VendorID", typeof(System.Int32)));
			dt.Columns.Add(new DataColumn("VendorName", typeof(string)));
			dt.Columns.Add(new DataColumn("SerialID", typeof(System.Int32)));
			dt.Columns.Add(new DataColumn("MinPrice", typeof(System.Decimal)));
			dt.Columns.Add(new DataColumn("MaxPrice", typeof(System.Decimal)));
			dt.Columns.Add(new DataColumn("CreateDateTime", typeof(System.DateTime)));

			com.bitauto.dealer.VendorNews vn = new com.bitauto.dealer.VendorNews();
			DataSet dsVN = new DataSet();

			// 控制取得页数 不能超过10000次
			int loop = 1;
			while (loop <= 10000)
			{
				dsVN = vn.GetDateVendorPrice_CMS(loop, 1000);
				if (dsVN != null && dsVN.Tables.Count > 0 && dsVN.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in dsVN.Tables[0].Rows)
					{
						#region 牛B的接口 第1页和第2页列不一样
						DataRow drNew = dt.NewRow();
						drNew["ID"] = int.Parse(dr["ID"].ToString());
						drNew["CityID"] = int.Parse(dr["CityID"].ToString());
						drNew["VendorID"] = int.Parse(dr["VendorID"].ToString());
						drNew["VendorName"] = dr["VendorName"].ToString().Trim();
						drNew["SerialID"] = int.Parse(dr["SerialID"].ToString());
						drNew["MinPrice"] = decimal.Parse(dr["MinPrice"].ToString());
						drNew["MaxPrice"] = decimal.Parse(dr["MaxPrice"].ToString());
						drNew["CreateDateTime"] = Convert.ToDateTime(dr["CreateDateTime"].ToString());
						dt.Rows.Add(drNew);
						#endregion
					}

					using (SqlBulkCopy bulkCopy = new SqlBulkCopy(CommonData.ConnectionStringSettings.CarDataUpdateConnString))
					{
						bulkCopy.DestinationTableName = "dbo.VendorPrice";
						bulkCopy.BulkCopyTimeout = 180;
						try
						{
							bulkCopy.WriteToServer(dt);
						}
						catch (Exception ex)
						{
							Log.WriteLog("每天更新经销商报价及权重 批量入库异常\r\n" + ex.ToString());
						}
						finally
						{
							dt.Clear();
						}
					}
					loop++;
				}
				else
				{ break; }
				if (dsVN.Tables[0].Rows.Count<100)
				{
 					// 最后1页
				}
			}
		}

	}
}
