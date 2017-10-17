using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using System.Data.SqlClient;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using System.Data;
using BitAuto.CarDataUpdate.Common.Services;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	public class SerialSUVPramatersHtmlBuilder : BaseBuilder
	{
		private int _serialId = 0;
		private SerialInfo _serialInfo;

		public override void BuilderDataOrHtml(int objId)
		{
			_serialId = objId;
			_serialInfo = CommonData.SerialDic[objId];
			try
			{
				//SUV
				if (_serialInfo.CarLevel == 424)
				{
					RenderContent();
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

		private void RenderContent()
		{
			DataSet ds = this.GetCar(_serialId);
			if (ds.Tables[0].Rows.Count > 0)
			{

				var currentEntity = new SerialNewCarInfo();
				currentEntity.SerialId = _serialId;
				currentEntity.SerialName = _serialInfo.ShowName;
				currentEntity.SerialSpell = _serialInfo.AllSpell;
				currentEntity = this.GetEntity(currentEntity, ds);

				var compareCarInfoList = this.GetCompareCarInfo();
				//添加当前子品牌车款
				compareCarInfoList.Add(currentEntity);
				#region 拼接 HTML
				StringBuilder sb = new StringBuilder();
                sb.Append("<div class=\"vertical-tag vertical-tag-suv\" id=\"vertical-tag-suv\" data-channelid=\"2.21.815\">");
				sb.Append("<ul class=\"v-t-list\">");
				sb.Append("<li class=\"current\"><span>通过性能：</span><b></b></li>");
				sb.Append("<li class=\"\"><span>离地间隙：</span><b></b></li>");
				sb.Append("<li class=\"\"><span>涉水深度：</span><b></b></li>");
				sb.Append("</ul>");
				//通过角
				sb.Append("<div class=\"suv-con-box\" style=\"display: block;\">");
				sb.Append("<div class=\"suv-con-box-l\">");
				sb.Append("<img src=\"http://image.bitauto.com/uimg/car/images2013/suv_pic_1.png\">");
				sb.AppendFormat("<span class=\"angle-1\">{0}</span>", currentEntity.OutSet_NearCorner > 0 ? currentEntity.OutSet_NearCorner + "°" : "");
				sb.AppendFormat("<span class=\"angle-2\">{0}</span>", currentEntity.Perf_Throughtheangle > 0 ? currentEntity.Perf_Throughtheangle + "°" : "");
				sb.AppendFormat("<span class=\"angle-3\">{0}</span>", currentEntity.OutSet_AwayCorner > 0 ? currentEntity.OutSet_AwayCorner + "°" : "");
				sb.Append("</div>");
				sb.Append("<div class=\"suv-con-box-r\">");
				if (compareCarInfoList.Count > 0)
				{
					sb.Append("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" class=\"table4\">");
					sb.Append("<tbody>");
					sb.Append("<tr>");
					sb.Append("<th style=\"width:45%;\">车型</th>");
					sb.Append("<th class=\"t-a-r\">接近<em>|</em></th>");
					sb.Append("<th class=\"t-a-r\">通过<em>|</em></th>");
					sb.Append("<th class=\"t-a-r\">离去</th>");
					sb.Append("</tr>");
					//按接近角倒序排列；如果接近角相同，按通过角倒序排列；通过角也相同按离去角倒序排列。数值为空的，视为最小，拍到最后。
					compareCarInfoList.Sort((p1, p2) =>
					{
						if (p1.OutSet_NearCorner == p2.OutSet_NearCorner)
						{
							if (p1.Perf_Throughtheangle == p2.Perf_Throughtheangle)
							{
								if (p1.OutSet_AwayCorner == p2.OutSet_AwayCorner)
								{
									return 0;
								}
								else
									return p2.OutSet_AwayCorner.CompareTo(p1.OutSet_AwayCorner);
							}
							else
								return p2.Perf_Throughtheangle.CompareTo(p1.Perf_Throughtheangle);
						}
						else
							return p2.OutSet_NearCorner.CompareTo(p1.OutSet_NearCorner);
					});
					foreach (var entity in compareCarInfoList)
					{
						sb.AppendFormat("<tr class=\"{0}\">", entity.CarId == currentEntity.CarId ? "red" : "");
						sb.AppendFormat("<td class=\"car-name\" title=\"{1}款 {2}\"><div><a href=\"/{3}/\" target=\"_blank\">{0}</a></div></td>",
							entity.SerialName, entity.Car_YearType, entity.CarName, entity.SerialSpell);
						sb.AppendFormat("<td>{0}<em>|</em></td>", entity.OutSet_NearCorner > 0 ? Math.Round(entity.OutSet_NearCorner, 1) + "°" : "");
						sb.AppendFormat("<td>{0}<em>|</em></td>", entity.Perf_Throughtheangle > 0 ? Math.Round(entity.Perf_Throughtheangle, 1) + "°" : "");
						sb.AppendFormat("<td>{0}</td>", entity.OutSet_AwayCorner > 0 ? Math.Round(entity.OutSet_AwayCorner, 1) + "°" : "");
						sb.Append("</tr>");
					}
					sb.Append("</tbody>");
					sb.Append("</table>");
				}
				sb.Append("</div>");
				sb.Append("</div>");
				sb.Append("<div class=\"suv-con-box\" style=\"display: none;\">");
				sb.Append("<div class=\"suv-con-box-l\">");
				sb.Append("<img src=\"http://image.bitauto.com/uimg/car/images2013/suv_pic_2.png\">");
				sb.AppendFormat("<span class=\"height-1\">{0}</span>", currentEntity.OutSet_MinGapFromEarth > 0 ? currentEntity.OutSet_MinGapFromEarth + "mm" : "");
				sb.Append("</div>");
				sb.Append("<div class=\"suv-con-box-r\">");
				if (compareCarInfoList.Count > 0)
				{
					sb.Append("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" class=\"table2\">");
					sb.Append("<tbody>");
					sb.Append("<tr>");
					sb.Append("<th>车型</th>");
					sb.Append("<th class=\"t-a-r\">离地间隙</th>");
					sb.Append("</tr>");
					compareCarInfoList.Sort((p1, p2) => p2.OutSet_MinGapFromEarth.CompareTo(p1.OutSet_MinGapFromEarth));
					foreach (var entity in compareCarInfoList)
					{
						sb.AppendFormat("<tr class=\"{0}\">", entity.CarId == currentEntity.CarId ? "red" : "");
						sb.AppendFormat("<td class=\"car-name\" title=\"{1}款 {2}\"><div><a href=\"/{3}/\" target=\"_blank\">{0}</a></div></td>",
							entity.SerialName, entity.Car_YearType, entity.CarName, entity.SerialSpell);
						sb.AppendFormat("<td>{0}</td>", entity.OutSet_MinGapFromEarth > 0 ? entity.OutSet_MinGapFromEarth + "mm" : "");
						sb.Append("</tr>");
					}
					sb.Append("</tbody>");
					sb.Append("</table>");
				}
				sb.Append("</div>");
				sb.Append("</div>");
				sb.Append("<div class=\"suv-con-box\" style=\"display: none;\">");
				sb.Append("<div class=\"suv-con-box-l\">");
				sb.Append("<img src=\"http://image.bitauto.com/uimg/car/images2013/suv_pic_3.png\">");
				sb.AppendFormat("<span class=\"water-1\">{0}</span>", currentEntity.Perf_MaxPaddleDepth > 0 ? currentEntity.Perf_MaxPaddleDepth + "mm" : "");
				sb.Append("</div>");
				sb.Append("<div class=\"suv-con-box-r\">");
				if (compareCarInfoList.Count > 0)
				{
					sb.Append("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" class=\"table2\">");
					sb.Append("<tbody>");
					sb.Append("<tr>");
					sb.Append("<th>车型</th>");
					sb.Append("<th class=\"t-a-r\">涉水深度</th>");
					sb.Append("</tr>");
					compareCarInfoList.Sort((p1, p2) => p2.Perf_MaxPaddleDepth.CompareTo(p1.Perf_MaxPaddleDepth));
					foreach (var entity in compareCarInfoList)
					{
						sb.AppendFormat("<tr class=\"{0}\">", entity.CarId == currentEntity.CarId ? "red" : "");
						sb.AppendFormat("<td class=\"car-name\" title=\"{1}款 {2}\"><div><a href=\"/{3}/\" target=\"_blank\">{0}</a></div></td>",
							entity.SerialName, entity.Car_YearType, entity.CarName, entity.SerialSpell);
						sb.AppendFormat("<td>{0}</td>", entity.Perf_MaxPaddleDepth > 0 ? entity.Perf_MaxPaddleDepth + "mm" : "");
						sb.Append("</tr>");
						sb.Append("<tr>");
					}
					sb.Append("</tbody>");
					sb.Append("</table>");
				}
				sb.Append("</div>");
				sb.Append("</div>");
				sb.Append("</div>");
				#endregion

				bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
				{
					ID = _serialId,
					TypeID = CommonHtmlEnum.TypeEnum.Serial,
					TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
					BlockID = CommonHtmlEnum.BlockIdEnum.SuvReport,
					HtmlContent = sb.ToString(),
					UpdateTime = DateTime.Now
				});
				if (!success) Log.WriteErrorLog("更新核心关键报告失败：serialId:" + _serialId);
			}
		}
		/// <summary>
		/// 获取对比车型信息
		/// </summary>
		/// <returns></returns>
		public List<SerialNewCarInfo> GetCompareCarInfo()
		{
			List<SerialNewCarInfo> list = new List<SerialNewCarInfo>();
			var compareSerialList = this.GetCompareSerial(_serialId);

			foreach (SerialNewCarInfo entity in compareSerialList)
			{
				DataSet ds = this.GetCar(entity.SerialId);
				if (ds.Tables[0].Rows.Count > 0)
				{
					list.Add(this.GetEntity(entity, ds));
				}
			}
			return list.Take(3).ToList();
		}

		/// <summary>
		/// 获取对比车型信息
		/// </summary>
		/// <returns></returns>
		public List<SerialNewCarInfo> GetCompareCarInfo(int serialId)
		{
			List<SerialNewCarInfo> list = new List<SerialNewCarInfo>();
			var compareSerialList = this.GetCompareSerial(serialId);

			foreach (SerialNewCarInfo entity in compareSerialList)
			{
				DataSet ds = this.GetCar(entity.SerialId);
				if (ds.Tables[0].Rows.Count > 0)
				{
					list.Add(this.GetEntity(entity, ds));
				}
			}
			return list.Take(3).ToList();
		}
		/// <summary>
		/// 获取车型实体
		/// </summary>
		/// <param name="ds"></param>
		/// <returns></returns>
		public SerialNewCarInfo GetEntity(SerialNewCarInfo entity, DataSet ds)
		{
			DataRow dr = ds.Tables[0].Rows[0];
			int carId = ConvertHelper.GetInteger(dr["CarId"]);
			string carName = dr["Car_Name"].ToString();
			int yearType = ConvertHelper.GetInteger(dr["Car_YearType"]);
			Dictionary<int, string> dictParams = CarService.GetCarAllParamByCarID(carId);
			var suvParams = dictParams.Where(p => new int[] { 890, 591, 581, 589, 662 }.Contains(p.Key));
			entity.CarId = carId;
			entity.CarName = carName;
			entity.Car_YearType = yearType;
			entity.OutSet_AwayCorner = ConvertHelper.GetDouble(suvParams.FirstOrDefault(p => p.Key == 581).Value);
			entity.OutSet_MinGapFromEarth = ConvertHelper.GetDouble(suvParams.FirstOrDefault(p => p.Key == 589).Value);
			entity.OutSet_NearCorner = ConvertHelper.GetDouble(suvParams.FirstOrDefault(p => p.Key == 591).Value);
			entity.Perf_MaxPaddleDepth = ConvertHelper.GetDouble(suvParams.FirstOrDefault(p => p.Key == 662).Value);
			entity.Perf_Throughtheangle = ConvertHelper.GetDouble(suvParams.FirstOrDefault(p => p.Key == 890).Value);

			return entity;
		}

		/// <summary>
		/// 890 通过角 591 接近角 581 离去角 589 最小离地间隙 662 最大涉水深度
		/// </summary>
		/// <returns></returns>
		public DataSet GetCar(int serialId)
		{
			SqlParameter[] _param ={
                                      new SqlParameter("@serialId",SqlDbType.Int),
									  new SqlParameter("@CarPV",SqlDbType.Structured)
                                  };
			_param[0].Value = serialId;
			_param[1].Value = this.GetCarPV(serialId);
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.StoredProcedure, "SP_CarDataBase_GetNewCarIdByParams", _param);
		}

		/// <summary>
		/// 获取子品牌车款 PV热度
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		private DataTable GetCarPV(int serialId)
		{
			string sql = @"SELECT  car.Car_Id AS CarId,ccp.Pv_SumNum AS CarPV
							FROM    dbo.Car_Basic car
									LEFT JOIN (SELECT   Pv_SumNum,car_id
											   FROM     Chart_Car_Pv
											   WHERE    CreateDateStr >= @Date1 AND CreateDateStr < @Date2 ) ccp ON car.Car_Id = ccp.car_id WHERE   car.isState = 1 AND car.cs_Id = @serialId";
			SqlParameter[] _params = { 
                                         new SqlParameter("@serialId", SqlDbType.Int),
                                         new SqlParameter("@Date1",SqlDbType.DateTime),
										 new SqlParameter("@Date2",SqlDbType.DateTime)
                                     };
			_params[0].Value = serialId;
            _params[1].Value = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            _params[2].Value = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
			return BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, System.Data.CommandType.Text, sql, _params).Tables[0];
		}

		/// <summary>
		/// 获取对比车型
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		private List<SerialNewCarInfo> GetCompareSerial(int serialId)
		{
			List<SerialNewCarInfo> list = new List<SerialNewCarInfo>();
			XmlDocument xmlDoc = CommonFunction.GetLocalXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"SerialCityCompare\{0}_CityCompare.xml", serialId)));
			if (xmlDoc != null)
			{
				XmlNodeList nodeList = xmlDoc.SelectNodes("CityCompare/City[@ID=0]/Serial");
				foreach (XmlNode node in nodeList)
				{
					int tempSerialId = ConvertHelper.GetInteger(node.Attributes["ID"].Value);
					string serialName = node.Attributes["ShowName"].Value;
					string allspell = node.Attributes["AllSpell"].Value;
					list.Add(new SerialNewCarInfo()
					{
						SerialId = tempSerialId,
						SerialName = serialName,
						SerialSpell = allspell
					});
				}
			}
			return list;
		}
	}
	//返回车型信息
	public struct SerialNewCarInfo
	{
		public int SerialId;
		public string SerialName;
		public string SerialSpell;
		public int CarId;
		public string CarName;
		public int Car_YearType;
		/// <summary>
		/// 离去角
		/// </summary>
		public double OutSet_AwayCorner;
		/// <summary>
		/// 最小离地间隙
		/// </summary>
		public double OutSet_MinGapFromEarth;
		/// <summary>
		/// 接近角
		/// </summary>
		public double OutSet_NearCorner;
		/// <summary>
		/// 最大涉水深度
		/// </summary>
		public double Perf_MaxPaddleDepth;
		/// <summary>
		/// 通过角
		/// </summary>
		public double Perf_Throughtheangle;
	}
}
