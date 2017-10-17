using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Common.Services
{
	public class BrandService
	{
		/// <summary>
		/// 获取品牌信息
		/// </summary>
		/// <param name="brandId">品牌ID</param>
		/// <returns></returns>
		public static DataSet GetBrandDataById(int brandId)
		{
			DataSet ds = null;
			try
			{
				ds = BrandRepository.GetBrandDataById(brandId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return ds;
		}
		/// <summary>
		/// 获取所有品牌id(有效)
		/// </summary>
		/// <returns></returns>
		public static List<int> GetBrandIdList()
		{
			List<int> list = new List<int>();
			try
			{
				DataSet ds = BrandRepository.GetBrandIdData();
				if (ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						list.Add(ConvertHelper.GetInteger(dr["cb_id"]));
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}
	}
}
