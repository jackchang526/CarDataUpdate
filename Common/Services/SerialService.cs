using BitAuto.CarDataUpdate.Common.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Services
{
	public class SerialService
	{
		public static Dictionary<int, Dictionary<string, string>> GetAllSerialColorNameRGB()
		{
			Dictionary<int, Dictionary<string, string>> dic = new Dictionary<int, Dictionary<string, string>>();
			try
			{
				// 取有RGB值的车身颜色
				DataSet ds = SerialRespository.GetAllSerialColorRGB(0);
				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						int csid = int.Parse(dr["cs_id"].ToString());
						string colorName = dr["colorName"].ToString().Trim();
						string colorRGB = dr["colorRGB"].ToString().Trim();
						if (dic.ContainsKey(csid))
						{
							if (!dic[csid].ContainsKey(colorName))
							{ dic[csid].Add(colorName, colorRGB); }
						}
						else
						{
							Dictionary<string, string> dicCs = new Dictionary<string, string>();
							dicCs.Add(colorName, colorRGB);
							dic.Add(csid, dicCs);
						}
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
