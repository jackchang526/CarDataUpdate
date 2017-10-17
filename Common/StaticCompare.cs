using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.Utils;
using System.Data;
using BitAuto.CarDataUpdate.Common.Model;

namespace BitAuto.CarDataUpdate.Common
{
	public class StaticCompare
	{
		/// <summary>
		/// 按照排量有小到大排序，排量相同的按自然吸气的在前，涡轮增压在后；进气方式相同的按最大功率由小到大排序
		/// 先按年款由新到旧排序，同年款的按变速器排序（手动，半自动，自动，手自一体，无级变速，双离合），同变速箱的按指导价由低到高显示
		/// </summary>
		/// <param name="car1"></param>
		/// <param name="car2"></param>
		/// <returns></returns>
		public static int CompareCarByExhaustAndPowerAndInhaleType(CarInfoForSerialSummaryEntity car1, CarInfoForSerialSummaryEntity car2)
		{
			int result = String.Compare(car1.Engine_Exhaust, car2.Engine_Exhaust);
			if (result == 0)
			{
				result = CompareInhaleType(car1.Engine_InhaleType, car2.Engine_InhaleType);
				if (result == 0)
				{
					result = CompareMaxPower(car1.Engine_MaxPower, car2.Engine_MaxPower);
					if (result == 0)
					{
						result = CompareCarByYear(car1, car2);
					}
				}
			}
			return result;
		}
		/// <summary>
		/// 按车型排量排序
		/// </summary>
		/// <param name="car1"></param>
		/// <param name="car2"></param>
		/// <returns></returns>
		public static int CompareCarByExhaust(CarInfoForSerialSummaryEntity car1, CarInfoForSerialSummaryEntity car2)
		{
			int ret = String.Compare(car1.Engine_Exhaust, car2.Engine_Exhaust);
			if (ret == 0)
			{
				double year1 = ConvertHelper.GetDouble(car1.CarYear);
				double year2 = ConvertHelper.GetDouble(car2.CarYear);
				if (year1 > year2)
					ret = -1;
				else if (year1 < year2)
					ret = 1;
				else
				{
					ret = CompareTransmissionType(car1.TransmissionType, car2.TransmissionType);
					if (ret == 0)
					{
						double price1 = ConvertHelper.GetDouble(car1.ReferPrice);
						double price2 = ConvertHelper.GetDouble(car2.ReferPrice);
						if (price1 > price2)
							ret = 1;
						else if (price2 > price1)
							ret = -1;
					}

				}
			}
			return ret;
		}

		/// <summary>
		/// 排序顺序 年款\排量\变速器\指导价
		/// </summary>
		/// <param name="car1"></param>
		/// <param name="car2"></param>
		/// <returns></returns>
		public static int CompareCarByYear(CarInfoForSerialSummaryEntity car1, CarInfoForSerialSummaryEntity car2)
		{
			int ret = 0;
			double year1 = ConvertHelper.GetDouble(car1.CarYear);
			double year2 = ConvertHelper.GetDouble(car2.CarYear);
			if (year1 > year2)
				ret = -1;
			else if (year1 < year2)
				ret = 1;
			else
			{
				ret = String.Compare(car1.Engine_Exhaust, car2.Engine_Exhaust);
				if (ret == 0)
				{
					ret = CompareTransmissionType(car1.TransmissionType, car2.TransmissionType);
					if (ret == 0)
					{
						double price1 = ConvertHelper.GetDouble(car1.ReferPrice);
						double price2 = ConvertHelper.GetDouble(car2.ReferPrice);
						if (price1 > price2)
							ret = 1;
						else if (price2 > price1)
							ret = -1;
					}
				}
			}

			return ret;
		}

		/// <summary>
		/// 区域车型页报价列表排序
		/// </summary>
		/// <param name="row1"></param>
		/// <param name="row2"></param>
		/// <returns></returns>
		public static int CompareRegionPrice(DataRow row1, DataRow row2)
		{
			int ret = 0;
			int year1 = ConvertHelper.GetInteger(row1["Car_YearType"]);
			int year2 = ConvertHelper.GetInteger(row2["Car_YearType"]);
			if (year1 > year2)
				ret = -1;
			else if (year1 < year2)
				ret = 1;
			else
			{
				double exhaust1 = 0.0;
				double exhaust2 = 0.0;
				Double.TryParse(row1["Engine_Exhaust"].ToString().Trim().Replace("L", ""), out exhaust1);
				Double.TryParse(row2["Engine_Exhaust"].ToString().Trim().Replace("L", ""), out exhaust2);
				if (exhaust1 > exhaust2)
					ret = 1;
				else if (exhaust1 < exhaust2)
					ret = -1;
				else
				{
					string trans1 = row1["UnderPan_TransmissionType"].ToString();
					string trans2 = row2["UnderPan_TransmissionType"].ToString();
					ret = CompareTransmissionType(trans1, trans2);
					if (ret == 0)
					{
						double price1 = ConvertHelper.GetDouble(row1["car_ReferPrice"]);
						double price2 = ConvertHelper.GetDouble(row2["car_ReferPrice"]);
						if (price1 > price2)
							ret = 1;
						else if (price2 > price1)
							ret = -1;
					}
				}
			}
			return ret;
		}


		/// <summary>
		/// 比较变速器类型
		/// </summary>
		/// <param name="trans1"></param>
		/// <param name="trans2"></param>
		/// <returns></returns>
		public static int CompareTransmissionType(string trans1, string trans2)
		{
			if (trans1.IndexOf("手动") > -1)
				trans1 = "a";
			else if (trans1.IndexOf("半自动") > -1)
				trans1 = "b";
			else if (trans1.IndexOf("自动") > -1)
				trans1 = "c";
			else if (trans1.IndexOf("手自一体") > -1)
				trans1 = "d";
			else if (trans1.IndexOf("CVT") > -1)
				trans1 = "e";
			else trans1 = "f";



			if (trans2.IndexOf("手动") > -1)
				trans2 = "a";
			else if (trans2.IndexOf("半自动") > -1)
				trans2 = "b";
			else if (trans2.IndexOf("自动") > -1)
				trans2 = "c";
			else if (trans2.IndexOf("手自一体") > -1)
				trans2 = "d";
			else if (trans2.IndexOf("CVT") > -1)
				trans2 = "e";
			else
				trans2 = "f";

			return String.Compare(trans1, trans2);
		}
		/// <summary>
		/// 进气方式 排序
		/// </summary>
		/// <param name="inhaleType1"></param>
		/// <param name="inhaleType2"></param>
		/// <returns></returns>
		public static int CompareInhaleType(string inhaleType1, string inhaleType2)
		{
			if (inhaleType1.IndexOf("自然吸气") > -1)
				inhaleType1 = "a";
			else
				inhaleType1 = "b";

			if (inhaleType2.IndexOf("自然吸气") > -1)
				inhaleType2 = "a";
			else
				inhaleType2 = "b";

			return String.Compare(inhaleType1, inhaleType2);
		}
		/// <summary>
		/// 马力排序
		/// </summary>
		/// <param name="inhaleType1"></param>
		/// <param name="inhaleType2"></param>
		/// <returns></returns>
		public static int CompareMaxPower(int maxPower1, int maxPower2)
		{
			int reuslt = 0;
			if (maxPower1 > maxPower2)
				reuslt = 1;
			else if (maxPower2 > maxPower1)
				reuslt = -1;
			return reuslt;
		}
		/// <summary>
		/// 字符排序
		/// </summary>
		/// <param name="str1"></param>
		/// <param name="str2"></param>
		/// <returns></returns>
		public static int CompareStringDesc(string str1, string str2)
		{
			return String.Compare(str1, str2) * -1;
		}
	}
}
