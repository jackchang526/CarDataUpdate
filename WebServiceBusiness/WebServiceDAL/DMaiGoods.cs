using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.WebServiceModel;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using System.Data;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
	public class DMaiGoods
	{

		public static bool UpdateMaiGoodsYiPai(XElement bodyElement, string opType)
		{
			string GoodsGuid = GetBodyXElementByName(bodyElement, "EntityId");
			string csid = GetInfoXElementByName(bodyElement, "Cs_Id");
			string carid = GetInfoXElementByName(bodyElement, "Car_Id");
			string cityid = GetInfoXElementByName(bodyElement, "City_Id");
			string GoodsUrl = GetInfoXElementByName(bodyElement, "GoodsUrl");
			string MarketPrice = GetInfoXElementByName(bodyElement, "MarketPrice");
			string BitautoPrice = GetInfoXElementByName(bodyElement, "BitautoPrice");

			SqlParameter[] sqlParams = new SqlParameter[]
            {
				new SqlParameter("GoodsGuid", SqlDbType.UniqueIdentifier)
					{Value=Guid.Parse(GoodsGuid)},
				new SqlParameter("SerialId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(csid)?0:int.Parse(csid)},
				new SqlParameter("CarId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(carid)?0:int.Parse(carid)},
				new SqlParameter("CityId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(cityid)?0:int.Parse(cityid)},
				new SqlParameter("GoodsUrl", SqlDbType.VarChar,200)
					{Value=GoodsUrl},
				new SqlParameter("MarketPrice", SqlDbType.Decimal)
					{Value=string.IsNullOrEmpty(MarketPrice)?0:decimal.Parse(MarketPrice)},
				new SqlParameter("BitautoPrice", SqlDbType.Decimal)
					{Value=string.IsNullOrEmpty(BitautoPrice)?0:decimal.Parse(BitautoPrice)},
				new SqlParameter("OperateType", SqlDbType.VarChar,100)
					{Value=opType},
			};

			bool isSuccess = (SqlHelper.ExecuteNonQuery(
				Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, @"SP_Mai_Goods_Car_Update", sqlParams) > 0);
			return isSuccess;
		}

		private static string GetBodyXElementByName(XElement bodyElement, string name)
		{
			if (bodyElement != null)
			{
				var ele = bodyElement.Element(name);
				if (ele != null)
				{
					return ele.Value;
				}
			}
			return null;
		}

		private static string GetInfoXElementByName(XElement bodyElement, string name)
		{
			if (bodyElement != null)
			{
				var ele = bodyElement.Element("GoodsInfo").Element(name);
				if (ele != null)
				{
					return ele.Value;
				}
			}
			return null;
		}

		public static bool UpdateMaiGoods(GoodsSummary goodsSummary)
		{
			SqlParameter[] sqlParams = new SqlParameter[]
            {
new SqlParameter("Id", SqlDbType.Int, 4){Value=goodsSummary.Id},
new SqlParameter("GoodsGUID", SqlDbType.UniqueIdentifier){Value=goodsSummary.GoodsGUID},
new SqlParameter("GoodsUrl", SqlDbType.VarChar, 200){Value=goodsSummary.GoodsUrl},
new SqlParameter("GoodsNumber", SqlDbType.NVarChar, 100){Value=goodsSummary.GoodsNumber},
new SqlParameter("Bs_Id", SqlDbType.Int, 4){Value=goodsSummary.Bs_Id},
new SqlParameter("Cs_Id", SqlDbType.Int, 4){Value=goodsSummary.Cs_Id},
new SqlParameter("PromotTitle", SqlDbType.NVarChar, 50){Value=goodsSummary.PromotTitle},
new SqlParameter("CoverImageUrl", SqlDbType.NVarChar, 200){Value=goodsSummary.CoverImageUrl},
new SqlParameter("StartTime", SqlDbType.DateTime, 8){Value=goodsSummary.StartTime},
new SqlParameter("EndTime", SqlDbType.DateTime, 8){Value=goodsSummary.EndTime},
new SqlParameter("PromotWay", SqlDbType.TinyInt, 1){Value=goodsSummary.PromotWay},
new SqlParameter("DisplayOrder", SqlDbType.Int, 4){Value=goodsSummary.DisplayOrder},
new SqlParameter("OrderBelongTo", SqlDbType.SmallInt, 2){Value=goodsSummary.OrderBelongTo},
new SqlParameter("CreateTime", SqlDbType.DateTime, 8){Value=goodsSummary.CreateTime},
new SqlParameter("GoodsStatus", SqlDbType.TinyInt, 1){Value=goodsSummary.GoodsStatus},
new SqlParameter("MinMarketPrice", SqlDbType.Decimal){Value=goodsSummary.MinMarketPrice},
new SqlParameter("MinBitautoPrice", SqlDbType.Decimal){Value=goodsSummary.MinBitautoPrice},
new SqlParameter("AppId", SqlDbType.Int,4){Value=goodsSummary.AppId}
            };

			bool isSuccess = (SqlHelper.ExecuteNonQuery(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, @"updatemai_Goods_Summary", sqlParams) > 0);

			if (isSuccess)
			{
				UpdateGoodsPromotion(goodsSummary.GoodsPromotions, goodsSummary.Id);

				UpdateGoodsAreaCar(goodsSummary.GoodsAreaCars, goodsSummary.Id);
			}
			return isSuccess;
		}

		private static int UpdateGoodsAreaCar(List<GoodsAreaCar> goodsAreaCars, int goodsId)
		{
			if (goodsId < 1)
				return -1;

			DataTable table = new DataTable("mai_Goods_Area_Cars");
			table.Columns.Add("CityId", typeof(int));
			table.Columns.Add("ProvinceId", typeof(int));
			table.Columns.Add("Bs_Id", typeof(int));
			table.Columns.Add("Cs_Id", typeof(int));
			table.Columns.Add("Car_Id", typeof(int));
			table.Columns.Add("MarketPrice", typeof(decimal));
			table.Columns.Add("BitautoPrice", typeof(decimal));
			table.Columns.Add("TotalStock", typeof(int));
			table.Columns.Add("SalesCount", typeof(int));

			if (goodsAreaCars != null && goodsAreaCars.Count > 0)
			{
				foreach (var row in goodsAreaCars)
				{
					DataRow newRow = table.NewRow();
					newRow["CityId"] = row.CityId;
					newRow["ProvinceId"] = row.ProvinceId;
					newRow["Bs_Id"] = row.Bs_Id;
					newRow["Cs_Id"] = row.Cs_Id;
					newRow["Car_Id"] = row.Car_Id;
					newRow["MarketPrice"] = row.MarketPrice;
					newRow["BitautoPrice"] = row.BitautoPrice;
					newRow["TotalStock"] = row.TotalStock;
					newRow["SalesCount"] = row.SalesCount;
					table.Rows.Add(newRow);
				}
			}
			SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("GoodsId", SqlDbType.Int, 4){Value=goodsId},
                new SqlParameter("newData", SqlDbType.Structured){Value=table}
            };

			return Convert.ToInt32(SqlHelper.ExecuteScalar(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, "updatemai_Goods_Area_Cars", sqlParams));
		}

		private static int UpdateGoodsPromotion(List<GoodsPromotion> goodsPromotions, int goodsId)
		{
			if (goodsId < 1)
				return -1;

			DataTable table = new DataTable("mai_Goods_Promotion");
			table.Columns.Add("Name", typeof(string));
			table.Columns.Add("Description", typeof(string));
			table.Columns.Add("Type", typeof(byte));

			if (goodsPromotions != null || goodsPromotions.Count > 0)
			{
				foreach (var row in goodsPromotions)
				{
					DataRow newRow = table.NewRow();
					newRow["Name"] = row.Name;
					newRow["Description"] = row.Description;
					newRow["Type"] = row.Type;
					table.Rows.Add(newRow);
				}
			}

			SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("GoodsId", SqlDbType.Int, 4){Value=goodsId},
                new SqlParameter("newData", SqlDbType.Structured){Value=table}
            };
			return Convert.ToInt32(SqlHelper.ExecuteScalar(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, "updatemai_Goods_Promotion", sqlParams));
		}


		public static int UpdateGoodsSummaryGoodsStatus(Guid goodsGuId, Int16 status)
		{
			SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("GoodsGuId", SqlDbType.UniqueIdentifier){Value=goodsGuId},
                new SqlParameter("status", SqlDbType.TinyInt, 1){Value=status}
            };

			return Convert.ToInt32(SqlHelper.ExecuteScalar(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, "updatemai_Goods_SummaryGoodsStatus", sqlParams));
		}
	}
}
