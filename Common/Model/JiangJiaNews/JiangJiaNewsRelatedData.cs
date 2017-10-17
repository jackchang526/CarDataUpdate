using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model.JiangJiaNews
{
	/// <summary>
	/// 降价关联数据
	/// </summary>
	public struct JiangJiaNewsRelatedData
	{
		public int SerialId;
		public List<int> CityIds;
		public List<int> ProvinceIds;
		public List<int> CarIds;
	}
}
