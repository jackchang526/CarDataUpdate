using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.WebServiceModel;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
	public class DemandDAL
	{
		public static bool UpdateDemand(XElement bodyElement, string opType)
		{
			Guid guid;
			string guidStr = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "EntityId" });
			if (Guid.TryParse(guidStr, out guid))
			{ }
			else
			{
				// 没有有效的Guid 记log 忽略此消息
				Log.WriteErrorLog("消息没有Guid");
				return false;
			}
			// 3个时间暂时不关心
			DateTime CreateTime = DateTime.Now;
			DateTime BeginTime = DateTime.Now;
			DateTime EndTime = DateTime.Now;

			string strCreateTime = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "CreateTime" });
			string strBeginTime = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "BeginTime" });
			string strEndTime = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "EndTime" });

			if (!string.IsNullOrEmpty(strCreateTime) && DateTime.TryParse(strCreateTime, out CreateTime))
			{ }
			if (!string.IsNullOrEmpty(strBeginTime) && DateTime.TryParse(strBeginTime, out BeginTime))
			{ }
			if (!string.IsNullOrEmpty(strEndTime) && DateTime.TryParse(strEndTime, out EndTime))
			{ }

			// 经销商ID
			int MemberCode = BitAuto.Utils.ConvertHelper.GetInteger(
				Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "MemberCode" }));
			// 经销商名
			string MemberName = "";
			MemberName = Common.CommonFunction.GetXElementByNamePath
				 (bodyElement, new string[] { "Demand", "MemberName" });
			if (MemberName.Length > 50)
			{ MemberName = MemberName.Substring(0, 50); }

			// 经销商地址
			string ContactAddress = "";
			ContactAddress = Common.CommonFunction.GetXElementByNamePath
				 (bodyElement, new string[] { "Demand", "ContactAddress" });
			if (ContactAddress.Length > 100)
			{ ContactAddress = ContactAddress.Substring(0, 100); }

			// 集客人数
			int Quantity = BitAuto.Utils.ConvertHelper.GetInteger(
				Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "Quantity" }));
			// 城市ID
			int CityID = BitAuto.Utils.ConvertHelper.GetInteger(
				Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "CityID" }));
			// 省份ID
			int ProvinceID = BitAuto.Utils.ConvertHelper.GetInteger(
				Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "ProvinceID" }));
			// 促销信息标题
			string PromotionTitle = "";
			PromotionTitle = Common.CommonFunction.GetXElementByNamePath
				(bodyElement, new string[] { "Demand", "PromotionTitle" });
			if (PromotionTitle.Length > 50)
			{ PromotionTitle = PromotionTitle.Substring(0, 50); }

			// 礼包价值
			decimal GiftBagWorth = BitAuto.Utils.ConvertHelper.GetDecimal(
				Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "GiftBagWorth" }));
			// 礼包内容
			string GiftBagContent = "";
			GiftBagContent = Common.CommonFunction.GetXElementByNamePath
				(bodyElement, new string[] { "Demand", "GiftBagContent" });
			if (GiftBagContent.Length > 50)
			{ GiftBagContent = GiftBagContent.Substring(0, 50); }

			// 其他优惠
			string OtherPrivilege = "";
			OtherPrivilege = Common.CommonFunction.GetXElementByNamePath
				  (bodyElement, new string[] { "Demand", "OtherPrivilege" });
			if (OtherPrivilege.Length > 50)
			{ OtherPrivilege = OtherPrivilege.Substring(0, 50); }

			// 主品牌ID
			int bsID = BitAuto.Utils.ConvertHelper.GetInteger(
				Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "MasterBrandIDs" }));
			// 子品牌ID
			int csID = BitAuto.Utils.ConvertHelper.GetInteger(
				Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "Demand", "SerialIDs" }));

			decimal MaxReducedPrice = 0;
			DataTable dtCarItem = new DataTable();
			dtCarItem.Columns.Add(new DataColumn("DemandID", Type.GetType("System.Guid")));
			dtCarItem.Columns.Add(new DataColumn("CsID", Type.GetType("System.Int32")));
			dtCarItem.Columns.Add(new DataColumn("CarID", Type.GetType("System.Int32")));
			dtCarItem.Columns.Add(new DataColumn("CityID", Type.GetType("System.Int32")));
			dtCarItem.Columns.Add(new DataColumn("ReducedPrice", Type.GetType("System.Decimal")));

			// 车款优惠节点
			List<int> caridList = new List<int>();
			foreach (XElement xCarItem in bodyElement.XPathSelectElements("//Body/Demand/ReducedPriceList/Item"))
			{
				int CarID = BitAuto.Utils.ConvertHelper.GetInteger(Common.CommonFunction.GetXElementByNamePath(xCarItem, new string[] { "CarID" }));
				decimal ReducedPrice = BitAuto.Utils.ConvertHelper.GetDecimal(Common.CommonFunction.GetXElementByNamePath(xCarItem, new string[] { "ReducedPrice" }));
				if (CarID > 0 && ReducedPrice > MaxReducedPrice)
				{ MaxReducedPrice = ReducedPrice; }
				// 保证同一个消息里唯一车款ID
				if (!caridList.Contains(CarID))
				{ caridList.Add(CarID); }
				else { continue; }
				DataRow dr = dtCarItem.NewRow();
				dr["DemandID"] = guid;
				dr["CsID"] = csID;
				dr["CarID"] = CarID;
				dr["CityID"] = CityID;
				dr["ReducedPrice"] = ReducedPrice;
				dtCarItem.Rows.Add(dr);
			}

			List<SqlParameter> listSP = new List<SqlParameter>();
			listSP.Add(new SqlParameter("DemandID", SqlDbType.UniqueIdentifier) { Value = guid });
			listSP.Add(new SqlParameter("CreateTime", SqlDbType.DateTime) { Value = CreateTime });
			listSP.Add(new SqlParameter("BeginTime", SqlDbType.DateTime) { Value = BeginTime });
			listSP.Add(new SqlParameter("EndTime", SqlDbType.DateTime) { Value = EndTime });
			listSP.Add(new SqlParameter("MemberCode", SqlDbType.Int) { Value = MemberCode });
			listSP.Add(new SqlParameter("MemberName", SqlDbType.NVarChar, 50) { Value = MemberName });
			listSP.Add(new SqlParameter("ContactAddress", SqlDbType.NVarChar, 100) { Value = ContactAddress });
			listSP.Add(new SqlParameter("Quantity", SqlDbType.Int) { Value = Quantity });
			listSP.Add(new SqlParameter("CityID", SqlDbType.Int) { Value = CityID });
			listSP.Add(new SqlParameter("ProvinceID", SqlDbType.Int) { Value = ProvinceID });
			listSP.Add(new SqlParameter("SerialIDs", SqlDbType.Int) { Value = csID });
			listSP.Add(new SqlParameter("MasterBrandIDs", SqlDbType.Int) { Value = bsID });
			listSP.Add(new SqlParameter("PromotionTitle", SqlDbType.NVarChar, 50) { Value = PromotionTitle });
			listSP.Add(new SqlParameter("GiftBagWorth", SqlDbType.Decimal) { Value = GiftBagWorth });
			listSP.Add(new SqlParameter("GiftBagContent", SqlDbType.NVarChar, 50) { Value = GiftBagContent });
			listSP.Add(new SqlParameter("OtherPrivilege", SqlDbType.NVarChar, 50) { Value = OtherPrivilege });
			listSP.Add(new SqlParameter("MaxReducedPrice", SqlDbType.Decimal) { Value = MaxReducedPrice });
			if (dtCarItem.Rows.Count > 0)
			{
				listSP.Add(new SqlParameter("dtDemandCar", SqlDbType.Structured) { Value = dtCarItem });
			}
			listSP.Add(new SqlParameter("OperateType", SqlDbType.VarChar, 100) { Value = opType });

			SqlParameter[] sqlParams = listSP.ToArray();

			try
			{
				bool isSuccess = (SqlHelper.ExecuteNonQuery(
					Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.StoredProcedure, @"SP_Demand_Item_Update", sqlParams) > 0);

				//同步到购车服务中
				UpdateBuyCarService(opType, guid, GiftBagContent, dtCarItem);

				return isSuccess;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
				return false;
			}
		}

		private static void UpdateBuyCarService(string opType, Guid guid, string GiftBagContent, DataTable dtCarItem)
		{
			try
			{
				if (opType == "down")
				{
					var entity = new BuyCarServiceEntity()
					{
						Guid = guid
					};
					BuyCarServiceDAL.Update(entity, opType == "down" ? "delete" : "update", Define.ProductType.Mai);

					UpdateCarResult(guid);
					UpdateCarPreferentialResult(guid);
				}
				else
				{
					if (dtCarItem.Rows.Count > 0)
					{
						//首先删除特卖数据
						DeleteDemandCarByDemandId(guid);

						foreach (DataRow dr in dtCarItem.Rows)
						{
							var carId = ConvertHelper.GetInteger(dr["CarID"]);
							var reducedPrice = ConvertHelper.GetDecimal(dr["ReducedPrice"]);
							var carEntity = Common.Services.CarService.GetCarInfoById(carId);
							//var referPrice = (carEntity != null) ? (carEntity.ReferPrice - reducedPrice) : 0;
							if (carEntity != null)
							{
								var referPrice = carEntity.ReferPrice - reducedPrice;
								if (referPrice <= 0)
								{
									Log.WriteErrorLog("特卖车款价格为<=0,guid=" + guid.ToString() + ",carid=" + carId);
									return;
								}
								//数据同步到购车服务
								var entity = new BuyCarServiceEntity()
								{
									Guid = guid,
									CarId = carId,
									CityId = ConvertHelper.GetInteger(dr["CityID"]),
									CsId = ConvertHelper.GetInteger(dr["CsID"]),
									Price = referPrice,
									Remarks = GiftBagContent
								};
								BuyCarServiceDAL.Update(entity, opType == "down" ? "delete" : "update", Define.ProductType.Mai);
							}

						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

		private static void DeleteDemandCarByDemandId(Guid DemandID)
		{
			try
			{
				//删除结果表 特卖数据
				string sql = @"DELETE FROM dbo.Buy_MaiCar WHERE [Guid]=@DemandID";
				SqlParameter[] _params = { 
									 new SqlParameter("@DemandID",SqlDbType.UniqueIdentifier)
									 };
				_params[0].Value = DemandID;
				bool isSuccess = (SqlHelper.ExecuteNonQuery(
					Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
					CommandType.Text, sql, _params) > 0);
				if (!isSuccess)
				{
					Log.WriteLog("删除特卖数据失败：" + DemandID);
				}
				////删除结果表 特卖数据
				//string sqlResult = @"DELETE FROM dbo.Buy_SerialResult WHERE ExtGuid=@DemandID AND ProductType=3";
				//SqlParameter[] _paramsResult = { 
				//					 new SqlParameter("@DemandID",SqlDbType.UniqueIdentifier)
				//					 };
				//_paramsResult[0].Value = DemandID;
				//bool isSuccessResult = (SqlHelper.ExecuteNonQuery(
				//	Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
				//	CommandType.Text, sqlResult, _paramsResult) > 0);
				//if (!isSuccessResult)
				//{
				//	//Log.WriteErrorLog("删除结果表特卖数就失败：" + DemandID);
				//}

				UpdateCarResult(DemandID);

				UpdateCarPreferentialResult(DemandID);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("删除结果表特卖数就失败：" + DemandID + ex.ToString());
			}
		}
		/// <summary>
		/// 根据 guid 重新计算 车款 结果表数据
		/// </summary>
		/// <param name="DemandID"></param>
		private static void UpdateCarResult(Guid DemandID)
		{
			try
			{
				string sqlResult = @"SELECT CarId,CityId FROM  [dbo].[Buy_CarResult] WHERE [ExtGuid]=@DemandID AND ProductType=3";
				SqlParameter[] _paramsResult = { 
									 new SqlParameter("@DemandID",SqlDbType.UniqueIdentifier)
									 };
				_paramsResult[0].Value = DemandID;
				DataSet ds = SqlHelper.ExecuteDataset(Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
					CommandType.Text, sqlResult, _paramsResult);
				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						int carId = ConvertHelper.GetInteger(dr["carid"]);
						int cityId = ConvertHelper.GetInteger(dr["cityid"]);
						RecalculateCarResult(carId, cityId);
  					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("特卖重新计算车款结果表失败：DemandID=" + DemandID.ToString() + ex.ToString());
			}
		}
		/// <summary>
		/// 根据 车款id 和 城市id 重新计算 车款结果表
		/// </summary>
		/// <param name="carId"></param>
		/// <param name="cityId"></param>
		private static void RecalculateCarResult(int carId, int cityId)
		{
			try
			{
				SqlParameter[] _paramsCarResult = { 
									 new SqlParameter("@CarId",SqlDbType.Int),
									 new SqlParameter("@CityId",SqlDbType.Int)
									 };
				_paramsCarResult[0].Value = carId;
				_paramsCarResult[1].Value = cityId;

				bool isSuccessResult = (SqlHelper.ExecuteNonQuery(
			Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
			CommandType.StoredProcedure, "[SP_Buy_CarResult_Recalculate]", _paramsCarResult) > 0);
				if (!isSuccessResult)
				{
					Log.WriteErrorLog(string.Format("特卖重新计算失败：carid={0},cityid={1}", carId, cityId));
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

		private static void UpdateCarPreferentialResult(Guid DemandID)
		{
			try
			{
				string sqlResult = @"SELECT CarId,CityId FROM  [dbo].[Buy_CarPreferentialResult] WHERE [ExtGuid]=@DemandID AND ProductType=3";
				SqlParameter[] _paramsResult = { 
									 new SqlParameter("@DemandID",SqlDbType.UniqueIdentifier)
									 };
				_paramsResult[0].Value = DemandID;
				DataSet ds = SqlHelper.ExecuteDataset(Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
					CommandType.Text, sqlResult, _paramsResult);
				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						int carId = ConvertHelper.GetInteger(dr["carid"]);
						int cityId = ConvertHelper.GetInteger(dr["cityid"]);
						RecalculateCarPreferentialResult(carId, cityId);
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("特卖重新计算车款结果表失败：DemandID=" + DemandID.ToString() + ex.ToString());
			}
		}

		/// <summary>
		/// 根据 车款id 和 城市id 重新计算 优惠车款结果表
		/// </summary>
		/// <param name="carId"></param>
		/// <param name="cityId"></param>
		private static void RecalculateCarPreferentialResult(int carId, int cityId)
		{
			try
			{
				SqlParameter[] _paramsCarResult = { 
									 new SqlParameter("@CarId",SqlDbType.Int),
									 new SqlParameter("@CityId",SqlDbType.Int)
									 };
				_paramsCarResult[0].Value = carId;
				_paramsCarResult[1].Value = cityId;

				bool isSuccessResult = (SqlHelper.ExecuteNonQuery(
			Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
			CommandType.StoredProcedure, "[SP_Buy_CarPreferentialResult_Recalculate]", _paramsCarResult) > 0);
				if (!isSuccessResult)
				{
					Log.WriteErrorLog(string.Format("特卖重新计算失败：carid={0},cityid={1}", carId, cityId));
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
	}
}
