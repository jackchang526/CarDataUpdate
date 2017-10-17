using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Enum;
using System.Xml.Linq;
using System.Globalization;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	public class SerialDetailZone : BaseBuilder
	{
		private Dictionary<int, SerialNewCarInfo> _compareSerialIds;
		private SerialNewCarInfo currentSerialCarInfo;
		private StringBuilder sbHtml = new StringBuilder();

		private int SerialId = 0;
		private SerialInfo _serialInfo;
		private XmlDocument xmlParamsStandard;

		public SerialDetailZone()
		{
			xmlParamsStandard = this.GetDataStandard();
		}

		public override void BuilderDataOrHtml(int objId)
		{
			SerialId = objId;
			try
			{
				int carId = GetCarId(objId);
				if (carId > 0)
				{
					_serialInfo = CommonData.SerialDic[objId];
					DataSet ds = GetCarInfo(carId);
					if (ds.Tables[0].Rows.Count > 0)
					{
						_compareSerialIds = new Dictionary<int, SerialNewCarInfo>();

						SerialNewCarInfo serialNewCarInfo = GetSerialNewCarInfo(ds.Tables[0]);
						serialNewCarInfo.CarId = carId;
						serialNewCarInfo.SerialId = SerialId;
						serialNewCarInfo.SerialName = _serialInfo.ShowName;
						serialNewCarInfo.SerialSpell = _serialInfo.AllSpell;
						_compareSerialIds.Add(objId, serialNewCarInfo);
						currentSerialCarInfo = serialNewCarInfo;
					}
					GetCompareCars(objId);
					CreateHtml(carId,objId);
				}
				else
				{
					CommonHtmlService.DeleteCommonHtml(
					SerialId,
					CommonHtmlEnum.TypeEnum.Serial,
					CommonHtmlEnum.TagIdEnum.WirelessSerialSummaryV2,
					CommonHtmlEnum.BlockIdEnum.HexinReport);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
		/// <summary>
		/// 根据车型数据库获取实体
		/// </summary>
		/// <param name="table">车型数据datatable</param>
		/// <returns></returns>
		private SerialNewCarInfo GetSerialNewCarInfo(DataTable table)
		{
			SerialNewCarInfo serialInfo = new SerialNewCarInfo();
			serialInfo.CarName = table.Rows[0]["Car_Name"].ToString();
			serialInfo.Car_YearType = ConvertHelper.GetInteger(table.Rows[0]["Car_YearType"]);
			DataRow[] rows = table.Select("paramId=786");
			if (rows.Length > 0)
			{
				serialInfo.Perf_MeasuredAcceleration = Convert.ToDouble(rows[0]["Pvalue"]);
			}
			rows = table.Select("paramId=787");
			if (rows.Length > 0)
			{
				serialInfo.Perf_BrakingDistance = Convert.ToDouble(rows[0]["Pvalue"]);
			}
			rows = table.Select("paramId=788");
			if (rows.Length > 0)
			{
				serialInfo.Perf_MeasuredFuel = Convert.ToDouble(rows[0]["Pvalue"]);
			}
			return serialInfo;
		}

        private  Dictionary<string, int> dictTag = new Dictionary<string, int>();
		/// <summary>
		/// 生成块内容
		/// </summary>
		/// <param name="carId"></param>
		private void CreateHtml(int carId,int objId)
		{
            DataSet ds = GetCarInfo(carId);
            sbHtml.Append("<div class=\"scroll-card\">");
            sbHtml.Append("<div class=\"swiper-container swiper-info-card\">");
            sbHtml.Append("<div class=\"swiper-wrapper\">");

            dictTag.Add("油耗", 10);
            dictTag.Add("动力", 7);
            dictTag.Add("空间", 4);

            CreateSingleDoubleHtml(788, ds.Tables[0]);
            sbHtml.Append(CreateKongJianDetailHtml(objId));//空间详细视图
            CreateSingleDoubleHtml(786, ds.Tables[0]);
            CreateSingleDoubleHtml(787, ds.Tables[0]);
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");
            sbHtml.Append("<div class=\"pagination-info-card pagination-swiper\"></div>");
            sbHtml.Append("</div>");
		
			bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
			{
				ID = SerialId,
				TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.WirelessSerialSummaryV2,
				BlockID = CommonHtmlEnum.BlockIdEnum.HexinReport,
				HtmlContent = sbHtml.ToString(),
				UpdateTime = DateTime.Now
			});
			if (!success) Log.WriteErrorLog("更新核心关键报告失败：serialId:" + SerialId);
		}
        private PingCeTagEntity GetUrlFromPinceInfo(int key)
        {
            PingCeTagEntity pc;
            Dictionary<int, PingCeTagEntity> dict = CarPingceInfoService.GetPingceTagsBySerialId(SerialId);
            if (dict.ContainsKey(key))
            {
                pc = dict[key];
            }
            else
                pc = null;
            return pc;
        }
		/// <summary>
		/// 单个块内容
		/// </summary>
		/// <param name="paramId"></param>
		/// <param name="title"></param>
		/// <param name="unit"></param>
		/// <param name="dt"></param>
		private void CreateSingleDoubleHtml(int paramId, DataTable dt)
		{
			DataRow[] rows = dt.Select("paramId=" + paramId);
			if (rows.Length <= 0)
				return;
			double currValue = ConvertHelper.GetDouble(rows[0]["pvalue"]);
			if (currValue <= 0)
				return;

			if (paramId == 788)
			{
                PingCeTagEntity pcte=GetUrlFromPinceInfo(dictTag["油耗"]);
                if (pcte != null)
                {
                    pcte.url = pcte.url.Replace("news.bitauto.com", "news.m.yiche.com");
                }

				var list = _compareSerialIds.Select(kvp => kvp.Value).ToList();
				list.Sort((p1, p2) =>
				{
					if (p1.Perf_MeasuredFuel > p2.Perf_MeasuredFuel)
						return 1;
					else if (p1.Perf_MeasuredFuel < p2.Perf_MeasuredFuel)
						return -1;
					else
						return 0;
				});
				var index = this.GetDataStandardIndex(_serialInfo.CarLevel, 788, currValue);
                string fuelRankName=string.Empty;//油耗等级别称
				string[] arrStandard = { "节能先锋", "经济节能", "一般般", "差强人意", "油老虎" };
                if (index < arrStandard.Length)
                {
                    fuelRankName = arrStandard[index];
                }
                sbHtml.Append("<div class=\"swiper-slide card-item\">");
                sbHtml.Append("<div class=\"car-info-box\">");
                sbHtml.AppendFormat("<a href=\"{0}\" data-channelid=\"27.23.1363\"><div class=\"youhao-box\">", pcte == null ? "javascript:void(0)" : pcte.url);
                sbHtml.AppendFormat("<span><em>{0}</em></span>",fuelRankName);
                sbHtml.Append("</div></a>");
                sbHtml.Append("<div class=\"car-info-txt\">");
                sbHtml.AppendFormat("网友油耗：{0}L/百公里",currValue);
                sbHtml.Append("<em data-channelid=\"27.23.1364\">");
                sbHtml.Append("<div class=\"pop-box\" style=\"display:none\">");
                sbHtml.Append("<div class=\"pop-box-inner\">");
                sbHtml.Append("<dl>");
                sbHtml.Append("<dt>数据参考车款：</dt>");
                sbHtml.AppendFormat("<dd>{0}款 {1}</dd>", currentSerialCarInfo.Car_YearType,currentSerialCarInfo.CarName);
                sbHtml.Append("</dl>");
                sbHtml.Append("<div class=\"ico-close\"></div>");
                sbHtml.Append("<div class=\"ico-bottom-arrow\"></div>");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
                sbHtml.Append("</em>");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
				if (list.Count > 1)
				{
					sbHtml.Append("<div class=\"top-arrow-txt\">低</div>");
                    sbHtml.Append("<div class=\"top-arrow\"></div>");
                    sbHtml.Append("<ul class=\"car-lists\" data-channelid=\"27.23.1319\">");
                    int curFlag=0;
					foreach (SerialNewCarInfo serialInfo in list)
					{
                        curFlag++;
                        string strClass = string.Empty;
                        if (serialInfo.SerialId == SerialId)
                            strClass = " class=\"current\"";
                        if (serialInfo.SerialId == SerialId && curFlag == 1)
                            strClass = "class=\"best current\"";
                        if (serialInfo.SerialId != SerialId && curFlag == 1)
                            strClass = "class=\"best\"";
                        sbHtml.AppendFormat("<li {4}><a href=\"/{0}/\" title=\"{2}款 {3}\">{1}</a></li>",
							serialInfo.SerialSpell,
							serialInfo.SerialName,
							serialInfo.Car_YearType,
							serialInfo.CarName,
                            strClass
							);
					}
					sbHtml.Append("</ul>");
                    sbHtml.Append("<div class=\"tit-box\">");
                    sbHtml.Append("<span>油耗</span><i></i>");
                    sbHtml.Append("</div>");
				}
                sbHtml.Append("</div>");
			}
			if (paramId == 786)
			{
                PingCeTagEntity pcte = GetUrlFromPinceInfo(dictTag["动力"]);
                if (pcte != null)
                {
                    pcte.url = pcte.url.Replace("news.bitauto.com", "news.m.yiche.com");
                }
				var list = _compareSerialIds.Select(kvp => kvp.Value).ToList();
				list.Sort((p1, p2) =>
				{
					if (p1.Perf_MeasuredAcceleration > p2.Perf_MeasuredAcceleration)
						return 1;
					else if (p1.Perf_MeasuredAcceleration < p2.Perf_MeasuredAcceleration)
						return -1;
					else
						return 0;
				});
				var index = this.GetDataStandardIndex(_serialInfo.CarLevel, 786, currValue);
				string[] arrStandard = { "弱爆了", "弱弱哒", "一般般", "轻松超车", "diǎo爆了" };
                string accelerationRankName = string.Empty;//加速
				if(index<arrStandard.Length)
				{
                    accelerationRankName = arrStandard[index];
				}
                sbHtml.Append("<div class=\"swiper-slide card-item\">");
                sbHtml.Append("<div class=\"car-info-box\">");
                sbHtml.AppendFormat("<a href=\"{0}\" data-channelid=\"27.23.1363\">", pcte == null ? "javascript:void(0);" : pcte.url);
                sbHtml.Append(string.Format("<div class=\"dongli-box {0}\">",accelerationRankName.Length==2?"youhao-box":""));
                sbHtml.AppendFormat("<span><em>{0}</em></span>",accelerationRankName);
                sbHtml.Append("</div></a>");
                sbHtml.Append("<div class=\"car-info-txt\">");
                sbHtml.AppendFormat("百公里加速：{0}s",currValue);
                sbHtml.Append("<em data-channelid=\"27.23.1364\">");
                sbHtml.Append("<div class=\"pop-box\" style=\"display:none\">");
                sbHtml.Append("<div class=\"pop-box-inner\">");
                sbHtml.Append("<dl>");
                sbHtml.Append("<dt>数据参考车款：</dt>");
                sbHtml.AppendFormat("<dd>{0}款{1}</dd>",currentSerialCarInfo.Car_YearType,currentSerialCarInfo.CarName);
                sbHtml.Append("</dl>");
                sbHtml.Append("<div class=\"ico-close\"></div>");
                sbHtml.Append("<div class=\"ico-bottom-arrow\"></div>");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
                sbHtml.Append("</em>");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
				if (list.Count > 1)
				{
                    sbHtml.Append("<div class=\"top-arrow-txt\">强</div>");
                    sbHtml.Append("<div class=\"top-arrow\"></div>");
                    sbHtml.Append("<ul class=\"car-lists\" data-channelid=\"27.23.1319\">");
                    int curFlag = 0;
                    foreach (SerialNewCarInfo serialInfo in list)
                    {
                        curFlag++;
                        string strClass = string.Empty;
                        if (serialInfo.SerialId == SerialId)
                            strClass = " class=\"current\"";
                        if (serialInfo.SerialId == SerialId && curFlag == 1)
                            strClass = "class=\"best current\"";
                        if (serialInfo.SerialId != SerialId && curFlag == 1)
                            strClass = "class=\"best\"";
                        sbHtml.AppendFormat("<li {4}><a href=\"/{0}/\" title=\"{2}款 {3}\">{1}</a></li>",
                            serialInfo.SerialSpell,
                            serialInfo.SerialName,
                            serialInfo.Car_YearType,
                            serialInfo.CarName,
                            strClass
                            );
                    }
					sbHtml.Append("</ul>");
                    sbHtml.Append("<div class=\"tit-box\">");
                    sbHtml.Append("<span>动力</span><i></i>");
                    sbHtml.Append("</div>");
				}
				sbHtml.Append("</div>");
			}
			if (paramId == 787)
			{
                PingCeTagEntity pcte = GetUrlFromPinceInfo(dictTag["动力"]);
                if (pcte != null)
                {
                    pcte.url = pcte.url.Replace("news.bitauto.com", "news.m.yiche.com");
                }
				var list = _compareSerialIds.Select(kvp => kvp.Value).ToList();
				list.Sort((p1, p2) =>
				{
					if (p1.Perf_BrakingDistance > p2.Perf_BrakingDistance)
						return 1;
					else if (p1.Perf_BrakingDistance < p2.Perf_BrakingDistance)
						return -1;
					else
						return 0;
				});
				var index = this.GetDataStandardIndex(_serialInfo.CarLevel, 787, currValue);
				string[] arrStandard = { "刹车太肉", "刹车略肉", "一般般", "刹车灵敏", "秒刹" };
                string brakingRankName = string.Empty;
                if (index < arrStandard.Length)
                {
                    brakingRankName = arrStandard[index];
                }
                sbHtml.Append("<div class=\"swiper-slide card-item\">");
                sbHtml.Append("<div class=\"car-info-box\">");
                sbHtml.AppendFormat("<a href=\"{0}\" data-channelid=\"27.23.1363\">", pcte == null ? "javascript:void(0);" : pcte.url);
                sbHtml.Append(string.Format("<div class=\"zhidong-box {0}\">", brakingRankName.Length == 2 ? "youhao-box" : ""));
                sbHtml.AppendFormat("<span><em>{0}</em></span>", brakingRankName);
                sbHtml.Append("</div></a>");
                sbHtml.Append("<div class=\"car-info-txt\">");
                sbHtml.AppendFormat("速度100-0Km/h：{0}m",currValue);
                sbHtml.Append("<em data-channelid=\"27.23.1364\">");
                sbHtml.Append("<div class=\"pop-box\" style=\"display:none\">");
                sbHtml.Append("<div class=\"pop-box-inner\">");
                sbHtml.Append("<dl>");
                sbHtml.Append("<dt>数据参考车款：</dt>");
                sbHtml.AppendFormat("<dd>{0}款{1}</dd>",currentSerialCarInfo.Car_YearType,currentSerialCarInfo.CarName);
                sbHtml.Append("</dl>");
                sbHtml.Append("<div class=\"ico-close\"></div>");
                sbHtml.Append("<div class=\"ico-bottom-arrow\"></div>");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
                sbHtml.Append("</em>");
                sbHtml.Append("</div>");
                sbHtml.Append("</div>");
				if (list.Count > 1)
				{
                    sbHtml.Append("<div class=\"top-arrow-txt\">短</div>");
                    sbHtml.Append("<div class=\"top-arrow\"></div>");
                    sbHtml.Append("<ul class=\"car-lists\" data-channelid=\"27.23.1319\">");
                    int curFlag = 0;
                    foreach (SerialNewCarInfo serialInfo in list)
                    {
                        curFlag++;
                        string strClass = string.Empty;
                        if (serialInfo.SerialId == SerialId)
                            strClass = " class=\"current\"";
                        if (serialInfo.SerialId == SerialId && curFlag == 1)
                            strClass = "class=\"best current\"";
                        if (serialInfo.SerialId != SerialId && curFlag == 1)
                            strClass = "class=\"best\"";
                        sbHtml.AppendFormat("<li {4}><a href=\"/{0}/\" title=\"{2}款 {3}\">{1}</a></li>",
                            serialInfo.SerialSpell,
                            serialInfo.SerialName,
                            serialInfo.Car_YearType,
                            serialInfo.CarName,
                            strClass
                            );
                    }
                    sbHtml.Append("</ul>");
                    sbHtml.Append("<div class=\"tit-box\">");
                    sbHtml.Append("<span>制动距离</span><i></i>");
                    sbHtml.Append("</div>");
				}
				sbHtml.Append("</div>");
			}
		}
        private string CreateKongJianDetailHtml(int id)
        {
            //获取看过还看得前4条记录
            List<SerialToAttention> serialToAttentions = CommonData.GetSerialToAttentionByCsID(id, 6);
            List<int> csIdList = GetCsIdList(id, serialToAttentions);
            List<CarInnerSpaceInfo> carInnerSpaceInfos = SelectTargetList(id, csIdList).Take(4).ToList();
            List<int> carIdList = carInnerSpaceInfos.Select(i => i.CarId).ToList();
            var sb = new StringBuilder();
            bool flag = BuildHtml(sb, carInnerSpaceInfos, carIdList);
            return sb.ToString();
        }
        private static List<int> GetCsIdList(int currentCsId, IEnumerable<SerialToAttention> serialToAttentions)
        {
            List<int> list = serialToAttentions.Select(serialToAttention => serialToAttention.ToCsID).ToList();
            list.Add(currentCsId);
            return list;
        }
        private static List<CarInnerSpaceInfo> SelectTargetList(int currentCsId, List<int> list)
        {
            DataTable dataTable = GetDataTable(list);

            List<CarInnerSpaceInfo> carInnerSpaceInfoList = CommonData.GetCarInnerSpaceInfoList(dataTable, currentCsId);

            var carInnerSpaceInfos = new List<CarInnerSpaceInfo>();

            foreach (int csId in list)
            {
                int id = csId;
                IEnumerable<CarInnerSpaceInfo> carEntities = carInnerSpaceInfoList.Where(s => s.CsId == id);
                CarInnerSpaceInfo[] entities = carEntities as CarInnerSpaceInfo[] ?? carEntities.ToArray();
                if (entities.Any())
                {
                    List<CarInnerSpaceInfo> tempList = entities.ToList();
                    int year = tempList.Select(s => s.CarYear).Max();
                    List<CarInnerSpaceInfo> enumerable = tempList.Where(s => s.CarYear == year).ToList();
                    if (enumerable.Count >= 2)
                    {
                        IOrderedEnumerable<CarInnerSpaceInfo> query = null;
                        query = from items in enumerable orderby items.ReferPrice descending select items;
                        foreach (CarInnerSpaceInfo carBootInfo in query)
                        {
                            if (carBootInfo.IsCurrent)
                            {
                                if (IsCompleteModelInfo(carBootInfo))
                                {
                                    carInnerSpaceInfos.Add(carBootInfo);
                                    break;
                                }
                            }
                            else
                            {
                                carInnerSpaceInfos.Add(carBootInfo);
                                break;
                            }
                        }
                    }
                    if (enumerable.Count == 1)
                    {
                        if (enumerable[0].IsCurrent)
                        {
                            if (IsCompleteModelInfo(enumerable[0]))
                            {
                                carInnerSpaceInfos.Add(enumerable[0]);
                            }
                        }
                        else
                        {
                            carInnerSpaceInfos.Add(enumerable[0]);
                        }
                    }
                }
            }
            return carInnerSpaceInfos;
        }
        private static DataTable GetDataTable(IEnumerable<int> list)
        {
            var tb = new DataTable();
            tb.Columns.Add("Id", typeof(int));
            foreach (int i in list)
            {
                DataRow row = tb.NewRow();
                row[0] = i;
                tb.Rows.Add(row);
            }
            return tb;
        }
        private static bool IsCompleteModelInfo(CarInnerSpaceInfo carBootInfo)
        {
            //当前车辆模特数据 所有类型
            List<CarModelInfo> carModelInfoList = CommonData.GetCarModelInfoList(carBootInfo.CarId);

            if (carModelInfoList.Count == 0)
            {
                return false;
            }

            #region 第一排头部模特数据

            List<CarModelInfo> firstSeatToTopModelList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.FirstSeatToTop).ToList();
            IEnumerable<CarModelInfo> a1 = firstSeatToTopModelList.Where(s => s.ImageUrl != "");

            #endregion

            #region 第二排头部模特数据

            List<CarModelInfo> secondSeatToTopModelList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.SecondSeatToTop).ToList();
            IEnumerable<CarModelInfo> a2 = secondSeatToTopModelList.Where(s => s.ImageUrl != "");

            #endregion

            #region 第二排座椅据第一排座椅距离

            List<CarModelInfo> firstSeatDistanceModelList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.FirstSeatDistance).ToList();
            IEnumerable<CarModelInfo> a3 = firstSeatDistanceModelList.Where(s => s.ImageUrl != "");

            #endregion

            return a1.Any() && a2.Any() && a3.Any();
        }
        private  bool BuildHtml(StringBuilder sb, ICollection<CarInnerSpaceInfo> list, IEnumerable<int> carIdList)
        {
            CarModelInfo firstSeatToTopModel = null; //前排头部模特信息
            CarModelInfo secondSeatToTopModel = null; //后排头部模特信息
            CarModelInfo firstSeatDistanceModel = null; //第二排座椅据第一排座椅距离
            CarModelInfo thirdSeatToTopModel = null; //第三排头部空间
            if (list.Count == 0)
            {
                return false;
            }

            IEnumerable<CarInnerSpaceInfo> carInnerSpaceInfos =
                list.Where(carInnerSpaceInfo => carInnerSpaceInfo.IsCurrent);
            if (!carInnerSpaceInfos.Any()) return false;

            CarInnerSpaceInfo currentCarInnerSpaceInfo = carInnerSpaceInfos.ToList()[0];
            if (currentCarInnerSpaceInfo == null) return false;

            //当前车辆模特数据 所有类型
            List<CarModelInfo> carModelInfoList = CommonData.GetCarModelInfoList(currentCarInnerSpaceInfo.CarId);
            if (carModelInfoList.Count == 0)
            {
                return false;
            }
            //空间排序html
            StringBuilder sbKongJianOrderHtml = new StringBuilder();
            #region 第一排头部模特数据

            List<CarModelInfo> firstSeatToTopModelList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.FirstSeatToTop).ToList();
            if (firstSeatToTopModelList.Any())
            {
                firstSeatToTopModel = firstSeatToTopModelList[0];
            }

            if (firstSeatToTopModel == null)
            {
                return false;
            }

            #endregion
            #region 第二排头部模特数据

            List<CarModelInfo> secondSeatToTopModelList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.SecondSeatToTop).ToList();
            if (secondSeatToTopModelList.Any())
            {
                secondSeatToTopModel = secondSeatToTopModelList[0];
            }
            if (secondSeatToTopModel == null)
            {
                return false;
            }

            #endregion
            #region 第二排座椅据第一排座椅距离

            List<CarModelInfo> firstSeatDistanceModelList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.FirstSeatDistance).ToList();
            if (firstSeatDistanceModelList.Any())
            {
                firstSeatDistanceModel = firstSeatDistanceModelList[0];
            }
            if (firstSeatDistanceModel == null)
            {
                return false;
            }

            #endregion
            #region 第三排头部模特数据

            List<CarModelInfo> thirdSeatToTopModelList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.ThirdSeatToTop).ToList();
            if (thirdSeatToTopModelList.Any())
            {
                thirdSeatToTopModel = thirdSeatToTopModelList[0];
            }

            #endregion
            #region 后备箱图片数据

            List<CarModelInfo> backBootImageList =
                carModelInfoList.Where(s => s.Type == (int)CommonEnum.CarInnerSpaceType.BackBoot).ToList();

            #endregion
            #region 后备箱容积

            //车辆后备箱信息
            List<CarBootInfo> carBackBootInfoList = CommonData.GetCarBackBootInfoList(GetDataTable(carIdList),
                                                                                      currentCarInnerSpaceInfo.CarId);
            List<CarBootInfo> carBootList = carBackBootInfoList.Where(s => s.ParamId == 465).ToList();
            CarBootInfo carBoot = null;
            try
            {
                carBoot = carBootList.FirstOrDefault(s => s.IsCurrent); //当前

                if (carBoot == null)
                {
                    carBoot = new CarBootInfo();
                    carBoot.CarId = currentCarInnerSpaceInfo.CarId;
                    carBoot.CarName = currentCarInnerSpaceInfo.CarName;
                    carBoot.IsCurrent = true;
                    carBoot.CsShowName = currentCarInnerSpaceInfo.CsShowName;
                    carBoot.CarYear = currentCarInnerSpaceInfo.CarYear;
                    carBoot.CsId = currentCarInnerSpaceInfo.CsId;
                    carBoot.ParamId = 465;
                    carBoot.Pvalue = "-1";
                    carBoot.SerialAllSpell = currentCarInnerSpaceInfo.SerialAllSpell;
                    carBootList.Add(carBoot);
                }
            }
            catch (Exception)
            {
                Log.WriteLog(string.Format("CarId：{0} 没有后备箱信息", currentCarInnerSpaceInfo.CarId));
            }

            #endregion
            sb.AppendLine("<div class=\"swiper-slide card-item\">");
            sb.AppendLine("<div class=\"kongjian-box\">");
            sb.AppendLine("<div class=\"second-tags\" id=\"kongjianTab\">");
            sb.AppendLine("<ul data-channelid=\"27.23.1382\">");
            sb.AppendLine("<li class=\"current\"><a href=\"#\"><span>前排</span></a></li>");
            sb.AppendLine("<li><a href=\"#\"><span>后排</span></a></li>");
            sb.AppendLine("<li><a href=\"#\"><span>后备箱</span></a></li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"img-box swiper-container swiper-img-box\">");
            sb.AppendLine("<ul class=\"swiper-wrapper\">");


            PingCeTagEntity pcte = GetUrlFromPinceInfo(dictTag["空间"]);
            if (pcte != null)
            {
                pcte.url = pcte.url.Replace("news.bitauto.com", "news.m.yiche.com");
            }
            FirstSeatToTopHtml(sb, list, firstSeatToTopModel, currentCarInnerSpaceInfo, sbKongJianOrderHtml,pcte);
            FirstSeatDistanceHtml(sb, list, firstSeatDistanceModel, currentCarInnerSpaceInfo, sbKongJianOrderHtml,pcte);
            if (backBootImageList.Count > 0 && carBoot != null)
            {
                BackBootHtml(sb, carBootList, carBoot, backBootImageList, sbKongJianOrderHtml,pcte);
            }
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"top-arrow-txt\">宽裕</div>");
            sb.AppendLine("<div class=\"top-arrow\"></div>");

            sb.AppendLine(string.Format("<div id=\"kongjianList\">{0}</div>", sbKongJianOrderHtml.ToString()));
            sb.AppendLine("<div class=\"tit-box\">");
            sb.AppendLine("<span>空间</span><i></i>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return true;
        }
        private static void FirstSeatToTopHtml(StringBuilder sb, IEnumerable<CarInnerSpaceInfo> list,
                                           CarModelInfo carModelInfo,
                                           CarInnerSpaceInfo currentCarInnerSpaceInfo, StringBuilder sbKongJianOrderHtml,PingCeTagEntity pcte)
        {
            sb.AppendLine("<li class=\"swiper-slide\">");
            sb.AppendLine(string.Format("<a href=\"{1}\" data-channelid=\"27.23.1363\"><img src=\"{0}\" />", carModelInfo.ImageUrl, pcte == null ? "javascript:void(0);" : pcte.url));
            sb.AppendLine(string.Format("<span>身高{0}cm，座椅距顶{1}cm</span>",
                                        carModelInfo != null? carModelInfo.Height.ToString(CultureInfo.InvariantCulture): "", currentCarInnerSpaceInfo.FirstSeatToTop));
            sb.AppendLine("          </a></li>");

            #region  前排空间对比
            IEnumerable<CarInnerSpaceInfo> query = null;
            query = from items in list orderby items.FirstSeatToTop descending select items;
            GetKongJiangOrderHtml(query, sbKongJianOrderHtml, true);
            #endregion
        }
        private static void FirstSeatDistanceHtml(StringBuilder sb, IEnumerable<CarInnerSpaceInfo> list,
                                         CarModelInfo carModelInfo,
                                         CarInnerSpaceInfo currentCarInnerSpaceInfo, StringBuilder sbKongJianOrderHtml, PingCeTagEntity pcte)
        {
            sb.AppendLine("<li class=\"swiper-slide\">");
            sb.AppendLine(string.Format("<a href=\"{1}\" data-channelid=\"27.23.1363\"><img src=\"{0}\" />", carModelInfo != null ? carModelInfo.ImageUrl : "", pcte == null ? "javascript:void(0);" : pcte.url));
            sb.AppendLine(string.Format("<span>后排座椅距离前排座椅{0}cm</span>", currentCarInnerSpaceInfo.FirstSeatDistance));
            sb.AppendLine("</a></li>");
            #region  后排空间对比
            IEnumerable<CarInnerSpaceInfo> query = null;
            query = from items in list orderby items.FirstSeatDistance descending select items;
            GetKongJiangOrderHtml(query, sbKongJianOrderHtml, false);
            #endregion        }
        private static void BackBootHtml(StringBuilder sb, IEnumerable<CarBootInfo> carBootList, CarBootInfo carBoot,
                                         List<CarModelInfo> carModelInfo, StringBuilder sbKongJianOrderHtml, PingCeTagEntity pcte)
        {
            CarModelInfo normalCarModelInfo = carModelInfo.FirstOrDefault(s => s.ParaId == 465); //正常
            CarModelInfo expansionCarModelInfo = carModelInfo.FirstOrDefault(s => s.ParaId == 466); //扩展

            sb.AppendLine("<li class=\"swiper-slide\">");
            sb.AppendLine(string.Format("<a href=\"{1}\" data-channelid=\"27.23.1363\"><img src=\"{0}\" />", normalCarModelInfo != null ? normalCarModelInfo.ImageUrl : (expansionCarModelInfo == null ? "" : expansionCarModelInfo.ImageUrl), pcte == null ? "javascript:void(0);" : pcte.url));
            sb.AppendLine(string.Format("<span>{0}，{1}</span>",carBoot.Pvalue == "-1" ? "官方未公布" : "后备箱容积" + carBoot.Pvalue + "L",
                                            Leval(Convert.ToDouble(carBoot.Pvalue), CommonEnum.CarInnerSpaceType.BackBoot)));
            sb.AppendLine("</a></li>");
            #region  后备箱空间对比
           IEnumerable<CarBootInfo> query = null;
			query = from items in carBootList orderby items.Pvalue descending select items;

            sbKongJianOrderHtml.AppendLine("<ul class=\"car-lists\"  style=\"display:none\" data-channelid=\"27.23.1319\">");
            int flagBest = 0;
            foreach (CarBootInfo carBootInfo in query)
            {
                flagBest++;
                string strClass = string.Empty;
                if (flagBest == 1)
                {
                    strClass = "class='best'";
                }
                else if (carBootInfo.IsCurrent)
                {
                    strClass = "class='current'";
                }
                if (flagBest == 1 && carBootInfo.IsCurrent)
                {
                    strClass = "class='best current'";
                }
                sbKongJianOrderHtml.AppendLine(string.Format("<li {0}>", strClass));
                sbKongJianOrderHtml.AppendLine(string.Format("<strong><a href='{0}' title='{2}'>{1}</a></strong>",
                                            GetDetailUrl(carBootInfo.SerialAllSpell),
                                            carBootInfo.CsShowName,
                                            carBootInfo.CarYear + "款 " + carBootInfo.CarName));
                sbKongJianOrderHtml.AppendLine("</li>");
            }
            sbKongJianOrderHtml.AppendLine("</ul>");
            #endregion        }
        private static void GetKongJiangOrderHtml(IEnumerable<CarInnerSpaceInfo> query, StringBuilder sbKongJianOrderHtml,bool firstUlFlag)
        {
            sbKongJianOrderHtml.AppendLine(string.Format("<ul class=\"car-lists\" {0} data-channelid=\"27.23.1319\">", firstUlFlag ? "" : "style=\"display:none\""));
            int flagBest = 0;
            foreach (CarInnerSpaceInfo carInnerSpaceInfo in query)
            {
                flagBest++;
                string strClass = string.Empty;
                if (flagBest == 1)
                {
                    strClass = "class='best'";
                }
                else if (carInnerSpaceInfo.IsCurrent)
                {
                    strClass = "class='current'";
                }
                if (flagBest == 1 && carInnerSpaceInfo.IsCurrent)
                {
                    strClass = "class='best current'";
                }
                sbKongJianOrderHtml.AppendLine(string.Format("<li {0}>", strClass));
                sbKongJianOrderHtml.AppendLine(string.Format("<strong><a href='{0}' title='{2}'>{1}</a></strong>",
                                            GetDetailUrl(carInnerSpaceInfo.SerialAllSpell),
                                            carInnerSpaceInfo.CsShowName,
                                            carInnerSpaceInfo.CarYear + "款 " + carInnerSpaceInfo.CarName));
                sbKongJianOrderHtml.AppendLine("</li>");
            }
            sbKongJianOrderHtml.AppendLine("</ul>");
        }
        private static string Leval(double number, CommonEnum.CarInnerSpaceType carInnerSpaceType)
        {
            string msg = string.Empty;
            switch ((int)carInnerSpaceType)
            {
                case (int)CommonEnum.CarInnerSpaceType.FirstSeatToTop:
                    if (number >= 95)
                    {
                        msg = "宽裕";
                    }
                    if (number >= 90 && number < 95)
                    {
                        msg = "适中";
                    }
                    if (number < 90)
                    {
                        msg = "局促";
                    }
                    break;
                case (int)CommonEnum.CarInnerSpaceType.SecondSeatToTop:
                    if (number >= 95)
                    {
                        msg = "宽裕";
                    }
                    if (number >= 90 && number < 95)
                    {
                        msg = "适中";
                    }
                    if (number < 90)
                    {
                        msg = "局促";
                    }
                    break;
                case (int)CommonEnum.CarInnerSpaceType.FirstSeatDistance:
                    if (number >= 75)
                    {
                        msg = "宽裕";
                    }
                    if (number >= 60 && number < 75)
                    {
                        msg = "适中";
                    }
                    if (number < 60)
                    {
                        msg = "局促";
                    }
                    break;
                case (int)CommonEnum.CarInnerSpaceType.ThirdSeatToTop:
                    if (number >= 95)
                    {
                        msg = "宽裕";
                    }
                    if (number >= 90 && number < 95)
                    {
                        msg = "适中";
                    }
                    if (number < 90)
                    {
                        msg = "局促";
                    }
                    break;
                case (int)CommonEnum.CarInnerSpaceType.BackBoot:
                    if (number >= 500)
                    {
                        msg = "宽裕";
                    }
                    if (number >= 300 && number < 500)
                    {
                        msg = "适中";
                    }
                    if (number > 0 && number < 300)
                    {
                        msg = "局促";
                    }
                    if (number < 0)
                    {
                        msg = "";
                    }
                    break;
                default:
                    msg = "";
                    break;
            }
            return msg;
        }

	
		//
		private XmlDocument GetDataStandard()
		{
			XmlDocument xmlDoc = new XmlDocument();
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\ParamsStandard.config");
			return CommonFunction.GetLocalXmlDocument(filePath);
		}
        private static string GetDetailUrl(string serialAllSpell)
        {
            return "/" + serialAllSpell;
        }
		private int GetDataStandardIndex(int CarLevel, int paramsId, double currentValue)
		{
			int result = 0;
			if (xmlParamsStandard == null) return result;
			XmlNodeList nodeList = null;
			if (paramsId == 788 || paramsId == 786)
			{
				nodeList = xmlParamsStandard.SelectNodes("//Params[@Id=" + paramsId + "]/CarLevel[@Id=" + CarLevel + "]/Item");
			}
			else if (paramsId == 787)
			{
				nodeList = xmlParamsStandard.SelectNodes("//Params[@Id=" + paramsId + "]/Item");
			}
			if (nodeList.Count <= 0) return result;
			var query = nodeList
	  .Cast<XmlElement>()
	  .FirstOrDefault(p =>
	  {
		  double min = ConvertHelper.GetDouble(p.Attributes["Min"].Value);
		  double max = ConvertHelper.GetDouble(p.Attributes["Max"].Value);
		  return currentValue >= min && currentValue < max;
	  });
			if (query == null) return result;
			result = ConvertHelper.GetInteger(query.Attributes["Index"].Value);
			return result;
		}
		/// <summary>
		/// 获取车型信息
		/// </summary>
		/// <param name="carId"></param>
		/// <returns></returns>
		private DataSet GetCarInfo(int carId)
		{
			string sql = @"select ParamId, Pvalue,cr.Car_Name,cr.Car_YearType from cardatabase car
LEFT JOIN Car_relation cr ON cr.Car_Id=car.CarId AND cr.Car_Id=@carid
where CarId=@carid and (paramId=786 or paramId=787 or paramId=788) and Pvalue is not null and Pvalue <> '' and Pvalue <> '待查'";
			SqlParameter[] _params = { new SqlParameter("@carid", SqlDbType.Int) };
			_params[0].Value = carId;
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, _params);
		}
		/// <summary>
		/// 获取 三个参数同时又数值的最新车型
		/// </summary>
		/// <param name="serialId">子品牌ID</param>
		/// <returns></returns>
		private int GetCarId(int serialId)
		{
			string sql = @"SELECT top 1 CASE 
                    WHEN EXISTS(SELECT carid FROM   cardatabase WHERE  carid = car.car_id AND ISNUMERIC(Pvalue) = 1 AND CAST(pvalue AS FLOAT) > 0 AND paramid = 786)
                         AND EXISTS(SELECT carid FROM   cardatabase WHERE  carid = car.car_id AND ISNUMERIC(Pvalue) = 1 AND CAST(pvalue AS FLOAT) > 0 AND paramid = 787)
						AND EXISTS(SELECT carid FROM   cardatabase WHERE  carid = car.car_id AND ISNUMERIC(Pvalue) = 1 AND CAST(pvalue AS FLOAT) > 0 AND paramid = 788) 
					THEN car_id
                    ELSE 0
               END AS issign
				FROM   Car_relation AS car
				WHERE  cs_id = @serialId
					   AND isstate = 0  AND car_yearType in (SELECT DISTINCT TOP 3 car_yeartype FROM car_relation WHERE cs_id = @serialId AND isstate = 0 ORDER BY car_yearType DESC)
				ORDER BY issign DESC";
			SqlParameter[] _param ={
                                      new SqlParameter("@serialId",SqlDbType.Int)
                                  };
			_param[0].Value = serialId;
			return ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, _param));
		}
		/// <summary>
		/// 获取对比子品牌下 满足条件的最新车型信息
		/// </summary>
		private void GetCompareCars(int serialId)
		{
			XmlDocument doc;
			if (ExistsLoaclXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format("SerialCityCompare\\{0}_CityCompare.xml", serialId))
				, out doc))
			{
				XmlNodeList nodeList = doc.SelectNodes("CityCompare/City[@ID=0]/Serial");
				if (nodeList != null && nodeList.Count > 0)
				{
					int loop = 0;
					foreach (XmlNode node in nodeList)
					{
						if (node.Attributes["ID"] != null && !string.IsNullOrEmpty(node.Attributes["ID"].Value)
							&& node.Attributes["ShowName"] != null && !string.IsNullOrEmpty(node.Attributes["ShowName"].Value)
							&& node.Attributes["AllSpell"] != null && !string.IsNullOrEmpty(node.Attributes["AllSpell"].Value))
						{
							int csid = ConvertHelper.GetInteger(node.Attributes["ID"].Value);
							if (csid <= 0 && csid != serialId)
								continue;

							int carId = GetCarId(csid);
							if (carId <= 0)
								continue;

							if (_compareSerialIds == null)
								_compareSerialIds = new Dictionary<int, SerialNewCarInfo>();
							if (_compareSerialIds.ContainsKey(csid))
								continue;
							DataSet ds = GetCarInfo(carId);
							if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
								continue;
							loop++;
							if (loop >3) break;
							DataTable table = ds.Tables[0];
							SerialNewCarInfo serialInfo = GetSerialNewCarInfo(table);
							serialInfo.CarId = carId;
							serialInfo.SerialName = node.Attributes["ShowName"].Value;
							serialInfo.SerialSpell = node.Attributes["AllSpell"].Value;
							_compareSerialIds.Add(csid, serialInfo);
						}
					}
				}
			}
		}
		/// <summary>
		/// 检测XML文件是否存在
		/// </summary>
		/// <returns></returns>
		private bool ExistsLoaclXmlDocument(string xmlPath, out XmlDocument xmlDoc)
		{
			bool result = false;
			xmlDoc = null;
			if (!string.IsNullOrEmpty(xmlPath) && File.Exists(xmlPath))
			{
				FileStream stream = null;
				XmlReader reader = null;
				try
				{
					stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read);
					reader = XmlReader.Create(stream);
					xmlDoc = new XmlDocument();
					xmlDoc.Load(reader);
					result = true;
				}
				catch
				{
				}
				finally
				{
					if (reader != null && reader.ReadState != ReadState.Closed)
						reader.Close();
					if (stream != null)
						stream.Dispose();
				}
			}
			return result;
		}
		private struct SerialNewCarInfo
		{
			public int SerialId;
			public string SerialName;
			public string SerialSpell;
			public int CarId;
			public string CarName;
			public int Car_YearType;
			public double Perf_MeasuredAcceleration;
			public double Perf_BrakingDistance;
			public double Perf_MeasuredFuel;
		}
	}
}
