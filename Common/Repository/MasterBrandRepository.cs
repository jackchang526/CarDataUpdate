using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.Common.Repository
{
	public class MasterBrandRepository
	{
		/// <summary>
		/// 获取主品牌信息
		/// </summary>
		/// <param name="id">主品牌ID</param>
		/// <returns></returns>
		public static DataSet GetMasterBrandDataById(int id)
		{
			string sqlStr = "SELECT bs_Id,bs_Name,bs_Country,bs_LogoInfo,spell,IsState,urlspell,bs_introduction,bs_seoname "
				+ " FROM Car_MasterBrand WHERE bs_Id=@bsid and isState=1";
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sqlStr, new SqlParameter("@bsid", id));
			return ds;
		}
		/// <summary>
		/// 获取所有有效主品牌ID
		/// </summary>
		/// <returns></returns>
		public static DataSet GetMasterBrandIdData()
		{
            string sql = @"SELECT bs_Id,bs_Name FROM Car_MasterBrand WHERE IsState=1";
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
		}
	}
}
