using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Common.Services
{
	public class MasterBrandService
	{
		/// <summary>
		/// 获取主品牌信息
		/// </summary>
		/// <param name="id">主品牌ID</param>
		/// <returns></returns>
		public static DataSet GetMasterBrandDataById(int id)
		{
			DataSet ds = null;
			try
			{
				return MasterBrandRepository.GetMasterBrandDataById(id);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return ds;
		}
		/// <summary>
		/// 获取所有有效主品牌ID
		/// </summary>
		/// <returns></returns>
		public static List<int> GetMasterBrandIdList()
		{
			List<int> list = new List<int>();
			try
			{
				DataSet ds = MasterBrandRepository.GetMasterBrandIdData();
				if (ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						list.Add(ConvertHelper.GetInteger(dr["bs_id"]));
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}

        /// <summary>
        /// 获取所有有效主品牌字典(key:Id value:name)
        /// author:songcl date:2014-11-27
        /// </summary>
        /// <returns></returns>
	    public static Dictionary<int, string> GetMasterDic()
	    {
            var dic=new Dictionary<int, string>();
            try
            {
                DataSet ds = MasterBrandRepository.GetMasterBrandIdData();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        dic.Add(ConvertHelper.GetInteger(dr["bs_id"]), dr["bs_Name"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
	        return dic;
	    }
	}
}
