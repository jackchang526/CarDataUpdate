using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BitAuto.Utils.Data;
using System.Data.SqlClient;

namespace BitAuto.CarDataUpdate.Common.Repository
{
	public class BrandRepository
	{
		/// <summary>
		/// 获取品牌信息
		/// </summary>
		/// <param name="brandId">品牌ID</param>
		/// <returns></returns>
		public static DataSet GetBrandDataById(int brandId)
		{
			string sqlStr = @"SELECT  cb.cb_Id, cmr.bs_id, cb.cp_Id, cb_Name, cb_url, cb_Country AS Cp_Country,
                                        cb_introduction, cb.spell, cb.IsState, cb.allSpell, cb_seoname
                                FROM    Car_Brand cb
                                        LEFT JOIN dbo.Car_MasterBrand_Rel cmr ON cmr.cb_Id = cb.cb_Id
                                WHERE   cb.cb_Id = @brandid
                                        AND cb.isState = 1";
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sqlStr, new SqlParameter("brandid", brandId));
			return ds;
		}
		/// <summary>
		/// 获取所有品牌ID
		/// </summary>
		/// <returns></returns>
		public static DataSet GetBrandIdData()
		{
			string sql = @"SELECT cb_Id FROM Car_Brand WHERE IsState=1";
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
		}
	}
}
