using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser.Repository
{
	public class SerialCityPVRepository
	{
		/// <summary>
		/// 获取 全国 或者 城市的 子品牌 pv
		/// </summary>
		/// <param name="cityId">城市ID 0:全国 </param>
		/// <returns></returns>
		public static DataSet GetSerialCityPVRank(int cityId)
		{
			string sql = @"SELECT csID,SUM(uvcount) AS uvcount
  FROM  [dbo].[StatisticSerialPVUVCity] {0} GROUP BY csID ORDER BY uvcount DESC";
			if (cityId > 0)
			{
				sql = string.Format(sql, "WHERE CityID=@cityId");
			}else
				sql = string.Format(sql, "");
			SqlParameter[] _params = { new SqlParameter("@cityId", SqlDbType.Int) };
			_params[0].Value = cityId;
			return BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, System.Data.CommandType.Text, sql, _params);
		}
	}
}
