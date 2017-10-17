using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	/// <summary>
	///     生成子品牌综述页关键报告中的车内空间HTML块
	///     author:songcl date:2014-12-10
	/// </summary>
	public class KongJianHtmlBuilder
	{
		public void BuilderDataOrHtml(int id)
		{
			var commonHtmlEntity = new CommonHtmlEntity
				{
					BlockID = CommonHtmlEnum.BlockIdEnum.CarInnerSpace,
					UpdateTime = DateTime.Now,
					TypeID = CommonHtmlEnum.TypeEnum.Serial,
					TagID = CommonHtmlEnum.TagIdEnum.SerialSummary
				};

			#region 数据准备

			if (id == 0)
			{
				#region 所有

				Dictionary<int, SerialInfo> serialInfos = CommonData.SerialDic;
				foreach (var serialInfo in serialInfos)
				{
					//获取看过还看得前4条记录
					List<SerialToAttention> serialToAttentions = CommonData.GetSerialToAttentionByCsID(serialInfo.Key, 6);
					List<int> csIdList = GetCsIdList(serialInfo.Key, serialToAttentions);
					//List<int> carIdList = GetCarIdList(csIdList);
					List<CarInnerSpaceInfo> carInnerSpaceInfos = SelectTargetList(serialInfo.Key, csIdList).Take(4).ToList();
					List<int> carIdList = carInnerSpaceInfos.Select(i => i.CarId).ToList();
					var sb = new StringBuilder();
					bool flag = BuildHtml(sb, carInnerSpaceInfos, carIdList);
					if (flag)
					{
						commonHtmlEntity.ID = serialInfo.Key;
						commonHtmlEntity.HtmlContent = sb.ToString();
						UpdateHtml(commonHtmlEntity);
					}
					else
					{
                        CommonHtmlService.DeleteCommonHtml(serialInfo.Key, commonHtmlEntity.TypeID, commonHtmlEntity.TagID, commonHtmlEntity.BlockID);
						Log.WriteLog(string.Format("CsId：{0} 空间块***没有生成***", serialInfo.Key));
					}
				}

				#endregion
			}
			else
			{
				//获取看过还看得前4条记录
				List<SerialToAttention> serialToAttentions = CommonData.GetSerialToAttentionByCsID(id, 6);
				List<int> csIdList = GetCsIdList(id, serialToAttentions);
				//List<int> carIdList = GetCarIdList(csIdList);
				List<CarInnerSpaceInfo> carInnerSpaceInfos = SelectTargetList(id, csIdList).Take(4).ToList();
				List<int> carIdList = carInnerSpaceInfos.Select(i => i.CarId).ToList();
				var sb = new StringBuilder();
				bool flag = BuildHtml(sb, carInnerSpaceInfos, carIdList);
				if (flag)
				{
					commonHtmlEntity.ID = id;
					commonHtmlEntity.HtmlContent = sb.ToString();
					UpdateHtml(commonHtmlEntity);
				}
				else
				{
                    CommonHtmlService.DeleteCommonHtml(id, commonHtmlEntity.TypeID, commonHtmlEntity.TagID, commonHtmlEntity.BlockID);
                    Log.WriteLog(string.Format("CsId：{0} 删除***空间块***", id));
				}
			}

			#endregion
		}

		/// <summary>
		///     数据提取规则
		/// </summary>
		/// <param name="currentCsId"></param>
		/// <param name="list"></param>
		/// <returns></returns>
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

		/// <summary>
		///     数据筛选规则
		/// </summary>
		/// <param name="csIdList"></param>
		/// <returns></returns>
		private static List<int> GetCarIdList(List<int> csIdList)
		{
			var carIdList = new List<int>();
			DataTable dataTable = GetDataTable(csIdList);
			List<CarEntity> serialToSeeAllCarsList = CommonData.GetSerialToSeeAllCarsList(dataTable);
			foreach (int csId in csIdList)
			{
				int id = csId;
				IEnumerable<CarEntity> carEntities = serialToSeeAllCarsList.Where(s => s.CsId == id);
				CarEntity[] entities = carEntities as CarEntity[] ?? carEntities.ToArray();
				if (entities.Any())
				{
					List<CarEntity> tempList = entities.ToList();
					int year = tempList.Select(s => s.Year).Max();
					List<CarEntity> enumerable = tempList.Where(s => s.Year == year).ToList();
					if (enumerable.Count > 2)
					{
						IOrderedEnumerable<CarEntity> query = null;
						query = from items in enumerable orderby items.ReferPrice descending select items;
						foreach (CarEntity carBootInfo in query)
						{
							carIdList.Add(carBootInfo.CarId);
							break;
						}
					}
					if (enumerable.Count == 1)
					{
						carIdList.Add(enumerable[0].CarId);
					}
				}
			}
			return carIdList;
		}

		private static List<int> GetCsIdList(int currentCsId, IEnumerable<SerialToAttention> serialToAttentions)
		{
			List<int> list = serialToAttentions.Select(serialToAttention => serialToAttention.ToCsID).ToList();
			list.Add(currentCsId);
			return list;
		}

		/// <summary>
		///     数据拼装
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="list"></param>
		/// <param name="carIdList"></param>
		private static bool BuildHtml(StringBuilder sb, ICollection<CarInnerSpaceInfo> list, IEnumerable<int> carIdList)
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

			//<!--空间 关键报告 开始-->
			sb.AppendLine("<div class='space_box' id='space_box' data-channelid=\"2.21.814\">");

			#region 左

			sb.AppendLine("     <ul class='v-t-list'>");


			sb.AppendLine(string.Format("       <li class='current'><span>前排头部：<em>{0}</em></span><b></b></li>",
										Leval(currentCarInnerSpaceInfo.FirstSeatToTop,
											  CommonEnum.CarInnerSpaceType.FirstSeatToTop)));

			sb.AppendLine(string.Format("       <li><span>后排头部：<em>{0}</em></span><b></b></li>",
										Leval(currentCarInnerSpaceInfo.SecondSeatToTop,
											  CommonEnum.CarInnerSpaceType.SecondSeatToTop)));

			sb.AppendLine(string.Format("       <li><span>后排腿部：<em>{0}</em></span><b></b></li>",
										Leval(currentCarInnerSpaceInfo.FirstSeatDistance,
											  CommonEnum.CarInnerSpaceType.FirstSeatDistance)));


			if (thirdSeatToTopModel != null && currentCarInnerSpaceInfo.ThirdSeatToTop > 0 &&
				!string.IsNullOrEmpty(thirdSeatToTopModel.ImageUrl))
			{
				sb.AppendLine(string.Format("       <li><span>第三排头部：<em>{0}</em></span><b></b></li>",
											Leval(currentCarInnerSpaceInfo.ThirdSeatToTop,
												  CommonEnum.CarInnerSpaceType.ThirdSeatToTop)));
			}
			if (backBootImageList.Count > 0)
			{
				sb.AppendLine(string.Format("       <li><span>后备箱：<em>{0}</em></span><b></b></li>",
											Leval(Convert.ToDouble(carBoot.Pvalue),
												  CommonEnum.CarInnerSpaceType.BackBoot)));
			}
			sb.AppendLine("     </ul>");

			#endregion

			#region 右


			FirstSeatToTopHtml(sb, list, firstSeatToTopModel, currentCarInnerSpaceInfo);
			SecondSeatToTopHtml(sb, list, secondSeatToTopModel, currentCarInnerSpaceInfo);
			FirstSeatDistanceHtml(sb, list, firstSeatDistanceModel, currentCarInnerSpaceInfo);

			if (thirdSeatToTopModel != null && currentCarInnerSpaceInfo.ThirdSeatToTop > 0 &&
				!string.IsNullOrEmpty(thirdSeatToTopModel.ImageUrl))
			{
				ThirdSeatToTopHtml(sb, list, thirdSeatToTopModel, currentCarInnerSpaceInfo);
			}

			if (backBootImageList.Count > 0 && carBoot != null)
			{
				BackBootHtml(sb, carBootList, carBoot, backBootImageList);
			}

			#endregion

			sb.AppendLine("</div>");

			//<!--空间 关键报告 结束-->
			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="carBootList"></param>
		/// <param name="carBoot"></param>
		/// <param name="carModelInfo">图片列表</param>
		private static void BackBootHtml(StringBuilder sb, IEnumerable<CarBootInfo> carBootList, CarBootInfo carBoot,
										 List<CarModelInfo> carModelInfo)
		{
			CarModelInfo normalCarModelInfo = carModelInfo.FirstOrDefault(s => s.ParaId == 465); //正常

			CarModelInfo expansionCarModelInfo = carModelInfo.FirstOrDefault(s => s.ParaId == 466); //扩展


			sb.AppendLine("     <div class='space_con_box' id='5' style='display:none;'>");
			sb.AppendLine(string.Format("<div class='{0}' id='space_con_ltwo'>",
										expansionCarModelInfo != null && normalCarModelInfo != null
											? "space_con_l space_con_ltwo"
											: "space_con_l"));

			if (normalCarModelInfo != null)
			{
				sb.AppendLine("             <div class='space_pic'>");

				sb.AppendLine(string.Format("   <img src='{0}' width='300px' height='200px' >", normalCarModelInfo.ImageUrl));

				sb.AppendLine("             </div>");
			}

			if (expansionCarModelInfo != null)
			{
				sb.AppendLine(string.Format("<div class='space_pic' {0}>",
											normalCarModelInfo != null ? "style='display:none'" : ""));
				sb.AppendLine(string.Format("   <img src='{0}' width='300px' height='200px' >", expansionCarModelInfo.ImageUrl));
				sb.AppendLine("             </div>");
			}
			sb.AppendLine("             <ul class='space_pic_info'>");

			if (expansionCarModelInfo != null && normalCarModelInfo != null)
			{
				sb.AppendLine("                 <li class='current'>后排座椅未放倒时</li>");
				sb.AppendLine("                 <li class='end'>后排座椅放倒后</li>");
			}
			else
			{
				sb.AppendLine("                 <li class='current'>后备箱空间</li>");
			}

			sb.AppendLine("             </ul>");

			sb.AppendLine("         </div>");
			sb.AppendLine("         <div class='space_con_r'>");
			sb.AppendLine("             <div class='cartop_h'>");
			sb.AppendLine("                 <span>后备箱容积：</span>");

			sb.AppendLine(string.Format("   <p><strong>{0} {1}</strong>{2}</p>",
				carBoot.Pvalue == "-1" ? "官方未公布" : carBoot.Pvalue + "L",
										Leval(Convert.ToDouble(carBoot.Pvalue),
											  CommonEnum.CarInnerSpaceType.BackBoot),
										ClassLeval(Convert.ToDouble(carBoot.Pvalue),
										CommonEnum.CarInnerSpaceType.BackBoot) == "" ? "" : string.Format("<i class='{0}'></i>", ClassLeval(Convert.ToDouble(carBoot.Pvalue),
										CommonEnum.CarInnerSpaceType.BackBoot))));

			sb.AppendLine("             </div>");
			sb.AppendLine("             <div class='car_wading'>");
			sb.AppendLine("                 <ul>");
			sb.AppendLine("                     <li class='title'><strong>车型</strong><small>后备箱容积</small></li>");

			IEnumerable<CarBootInfo> query = null;
			query = from items in carBootList orderby items.Pvalue descending select items;

			foreach (CarBootInfo bootInfo in query)
			{
				sb.AppendLine(
					string.Format("<li {0}><strong><a href='{1}' title='{4}'  target='_blank'>{2}</a></strong><small>{3}</small></li>",
								  bootInfo.IsCurrent ? "class='current'" : "",
								  GetDetailUrl(bootInfo.SerialAllSpell),
								  bootInfo.CsShowName, bootInfo.Pvalue == "-1" ? "" : bootInfo.Pvalue + "L", bootInfo.CarYear + "款 " + bootInfo.CarName));
			}

			sb.AppendLine("                 </ul>");
			sb.AppendLine("             </div>");
			sb.AppendLine("         </div>");
			sb.AppendLine("     </div>");
		}

		private static void ThirdSeatToTopHtml(StringBuilder sb, IEnumerable<CarInnerSpaceInfo> list,
											   CarModelInfo carModelInfo,
											   CarInnerSpaceInfo currentCarInnerSpaceInfo)
		{
			//<!--空间2-->
			sb.AppendLine("     <div class='space_con_box' id='4' style='display:none;'>");

			#region 中

			sb.AppendLine("         <div class='space_con_l'>");
			sb.AppendLine("             <ul class='space_pic'>");
			sb.AppendLine("                 <li>");
			sb.AppendLine(string.Format("      <img src='{0}' width='300px' height='200px' />", carModelInfo != null ? carModelInfo.ImageUrl : ""));
			sb.AppendLine("                 </li>");
			sb.AppendLine("             </ul>");
			sb.AppendLine("             <ul class='space_pic_info'>");
			sb.AppendLine(string.Format("   <li>模特：身高{0}cm，体重{1}kg</li>",
										carModelInfo != null
											? carModelInfo.Height.ToString(CultureInfo.InvariantCulture)
											: "",
										carModelInfo != null
											? carModelInfo.Weight.ToString(CultureInfo.InvariantCulture)
											: "")); //<i class='mt'></i>
			sb.AppendLine("             </ul>");
			sb.AppendLine("         </div>");

			#endregion

			#region 右

			sb.AppendLine("         <div class='space_con_r'>");
			sb.AppendLine("             <div class='cartop_h'> <span>第三排座椅距离车顶高度：</span>");
			sb.AppendLine(string.Format("                 <p><strong>{0}cm {1}</strong><i class='{2}'></i></p>",
										currentCarInnerSpaceInfo.ThirdSeatToTop,
										Leval(currentCarInnerSpaceInfo.ThirdSeatToTop,
											  CommonEnum.CarInnerSpaceType.ThirdSeatToTop),
										ClassLeval(currentCarInnerSpaceInfo.ThirdSeatToTop,
												   CommonEnum.CarInnerSpaceType.ThirdSeatToTop)));
			sb.AppendLine("             </div>");
			sb.AppendLine("             <div class='car_wading'>");

			#region 右下角对比

			sb.AppendLine("                 <ul>");
			sb.AppendLine("                     <li class='title'>");
			sb.AppendLine("                         <strong>车型</strong>");
			sb.AppendLine("                         <small>第三排座椅距离车顶高度</small>");
			sb.AppendLine("                     </li>");

			IEnumerable<CarInnerSpaceInfo> query = null;
			query = from items in list orderby items.ThirdSeatToTop descending select items;
			foreach (CarInnerSpaceInfo carInnerSpaceInfo in query)
			{
				sb.AppendLine(string.Format("   <li {0}>", carInnerSpaceInfo.IsCurrent ? "class='current'" : ""));
				sb.AppendLine(string.Format("        <strong><a href='{0}' title={2} target='_blank'>{1}</a></strong>",
											GetDetailUrl(carInnerSpaceInfo.SerialAllSpell),
											carInnerSpaceInfo.CsShowName,
											carInnerSpaceInfo.CarYear + "款 " + carInnerSpaceInfo.CarName));
				sb.AppendLine(string.Format("        <small class='t_r'>{0}cm</small>", carInnerSpaceInfo.ThirdSeatToTop));
				sb.AppendLine("                 </li>");
			}
			sb.AppendLine("                </ul>");

			#endregion

			sb.AppendLine("             </div>");
			sb.AppendLine("         </div>");

			#endregion

			sb.AppendLine("     </div>");
		}

		private static void FirstSeatDistanceHtml(StringBuilder sb, IEnumerable<CarInnerSpaceInfo> list,
												  CarModelInfo carModelInfo,
												  CarInnerSpaceInfo currentCarInnerSpaceInfo)
		{
			//<!--空间2-->
			sb.AppendLine("     <div class='space_con_box' id='3' style='display:none;'>");

			#region 中

			sb.AppendLine("         <div class='space_con_l'>");
			sb.AppendLine("             <ul class='space_pic'>");
			sb.AppendLine("                 <li>");
			sb.AppendLine(string.Format("      <img src='{0}' width='300px' height='200px' />", carModelInfo != null ? carModelInfo.ImageUrl : ""));
			sb.AppendLine("                 </li>");
			sb.AppendLine("             </ul>");
			sb.AppendLine("             <ul class='space_pic_info'>");
			sb.AppendLine(string.Format("   <li>模特：身高{0}cm，体重{1}kg</li>",
										carModelInfo != null
											? carModelInfo.Height.ToString(CultureInfo.InvariantCulture)
											: "",
										carModelInfo != null
											? carModelInfo.Weight.ToString(CultureInfo.InvariantCulture)
											: ""));
			sb.AppendLine("             </ul>");
			sb.AppendLine("         </div>");

			#endregion

			#region 右

			sb.AppendLine("         <div class='space_con_r'>");
			sb.AppendLine("             <div class='cartop_h'> <span>后排座椅到前排座椅距离：</span>");
			sb.AppendLine(string.Format("                 <p><strong>{0}cm {1}</strong><i class='{2}'></i></p>",
										currentCarInnerSpaceInfo.FirstSeatDistance,
										Leval(currentCarInnerSpaceInfo.FirstSeatDistance,
											  CommonEnum.CarInnerSpaceType.FirstSeatDistance),
										ClassLeval(currentCarInnerSpaceInfo.FirstSeatDistance,
												   CommonEnum.CarInnerSpaceType.FirstSeatDistance)));
			sb.AppendLine("             </div>");
			sb.AppendLine("             <div class='car_wading'>");

			#region 右下角对比

			sb.AppendLine("                 <ul>");
			sb.AppendLine("                     <li class='title'>");
			sb.AppendLine("                         <strong>车型</strong>");
			sb.AppendLine("                         <small>后排座椅到前排座椅距离</small>");
			sb.AppendLine("                     </li>");

			IEnumerable<CarInnerSpaceInfo> query = null;
			query = from items in list orderby items.FirstSeatDistance descending select items;
			foreach (CarInnerSpaceInfo carInnerSpaceInfo in query)
			{
				sb.AppendLine(string.Format("   <li {0}>", carInnerSpaceInfo.IsCurrent ? "class='current'" : ""));
				sb.AppendLine(string.Format("        <strong><a href='{0}' title='{2}' target='_blank'>{1}</a></strong>",
											GetDetailUrl(carInnerSpaceInfo.SerialAllSpell),
											carInnerSpaceInfo.CsShowName,
											carInnerSpaceInfo.CarYear + "款 " + carInnerSpaceInfo.CarName));
				sb.AppendLine(string.Format("        <small class='t_r'>{0}cm</small>",
											carInnerSpaceInfo.FirstSeatDistance));
				sb.AppendLine("                 </li>");
			}
			sb.AppendLine("                </ul>");

			#endregion

			sb.AppendLine("             </div>");
			sb.AppendLine("         </div>");

			#endregion

			sb.AppendLine("     </div>");
		}

		private static void SecondSeatToTopHtml(StringBuilder sb, IEnumerable<CarInnerSpaceInfo> list,
												CarModelInfo carModelInfo,
												CarInnerSpaceInfo currentCarInnerSpaceInfo)
		{
			//<!--空间2-->
			sb.AppendLine("     <div class='space_con_box' id='2' style='display:none;'>");

			#region 中

			sb.AppendLine("         <div class='space_con_l'>");
			sb.AppendLine("             <ul class='space_pic'>");
			sb.AppendLine("                 <li>");
			sb.AppendLine(string.Format("      <img src='{0}' width='300px' height='200px' />", carModelInfo != null ? carModelInfo.ImageUrl : ""));
			sb.AppendLine("                 </li>");
			sb.AppendLine("             </ul>");
			sb.AppendLine("             <ul class='space_pic_info'>");
			sb.AppendLine(string.Format("   <li>模特：身高{0}cm，体重{1}kg</li>",
										carModelInfo != null
											? carModelInfo.Height.ToString(CultureInfo.InvariantCulture)
											: "",
										carModelInfo != null
											? carModelInfo.Weight.ToString(CultureInfo.InvariantCulture)
											: ""));
			sb.AppendLine("             </ul>");
			sb.AppendLine("         </div>");

			#endregion

			#region 右

			sb.AppendLine("         <div class='space_con_r'>");
			sb.AppendLine("             <div class='cartop_h'> <span>后排座椅距离车顶高度：</span>");
			sb.AppendLine(string.Format("   <p><strong>{0}cm {1}</strong><i class='{2}'></i></p>",
										currentCarInnerSpaceInfo.SecondSeatToTop,
										Leval(currentCarInnerSpaceInfo.SecondSeatToTop,
											  CommonEnum.CarInnerSpaceType.SecondSeatToTop),
										ClassLeval(currentCarInnerSpaceInfo.SecondSeatToTop,
												   CommonEnum.CarInnerSpaceType.SecondSeatToTop)));
			sb.AppendLine("             </div>");
			sb.AppendLine("             <div class='car_wading'>");

			#region 右下角对比

			sb.AppendLine("                 <ul>");
			sb.AppendLine("                     <li class='title'>");
			sb.AppendLine("                         <strong>车型</strong>");
			sb.AppendLine("                         <small>后排座椅距离车顶高度</small>");
			sb.AppendLine("                     </li>");

			IEnumerable<CarInnerSpaceInfo> query = null;
			query = from items in list orderby items.SecondSeatToTop descending select items;
			foreach (CarInnerSpaceInfo carInnerSpaceInfo in query)
			{
				sb.AppendLine(string.Format("   <li {0}>", carInnerSpaceInfo.IsCurrent ? "class='current'" : ""));
				sb.AppendLine(string.Format("        <strong><a href='{0}' title='{2}' target='_blank'>{1}</a></strong>",
											GetDetailUrl(carInnerSpaceInfo.SerialAllSpell),
											carInnerSpaceInfo.CsShowName,
											carInnerSpaceInfo.CarYear + "款 " + carInnerSpaceInfo.CarName));
				sb.AppendLine(string.Format("        <small class='t_r'>{0}cm</small>",
											carInnerSpaceInfo.SecondSeatToTop));
				sb.AppendLine("                 </li>");
			}
			sb.AppendLine("                </ul>");

			#endregion

			sb.AppendLine("             </div>");
			sb.AppendLine("         </div>");

			#endregion

			sb.AppendLine("     </div>");
		}

		private static void FirstSeatToTopHtml(StringBuilder sb, IEnumerable<CarInnerSpaceInfo> list,
											   CarModelInfo carModelInfo,
											   CarInnerSpaceInfo currentCarInnerSpaceInfo)
		{
			//<!--空间1-->
			sb.AppendLine("     <div class='space_con_box' id='1'>");

			#region 中

			sb.AppendLine("         <div class='space_con_l'>");
			sb.AppendLine("             <ul class='space_pic'>");
			sb.AppendLine("                 <li>");
			sb.AppendLine(string.Format("      <img src='{0}' width='300px' height='200px' />", carModelInfo != null ? carModelInfo.ImageUrl : ""));
			sb.AppendLine("                 </li>");
			sb.AppendLine("             </ul>");
			sb.AppendLine("             <ul class='space_pic_info'>");
			sb.AppendLine(string.Format("   <li>模特：身高{0}cm，体重{1}kg</li>",
										carModelInfo != null
											? carModelInfo.Height.ToString(CultureInfo.InvariantCulture)
											: "",
										carModelInfo != null
											? carModelInfo.Weight.ToString(CultureInfo.InvariantCulture)
											: ""));
			sb.AppendLine("             </ul>");
			sb.AppendLine("         </div>");

			#endregion

			#region 右

			sb.AppendLine("         <div class='space_con_r'>");
			sb.AppendLine("             <div class='cartop_h'> <span>前排座椅距离车顶高度：</span>");
			sb.AppendLine(string.Format("                 <p><strong>{0}cm {1}</strong><i class='{2}'></i></p>",
										currentCarInnerSpaceInfo.FirstSeatToTop,
										Leval(currentCarInnerSpaceInfo.FirstSeatToTop,
											  CommonEnum.CarInnerSpaceType.FirstSeatToTop),
										ClassLeval(currentCarInnerSpaceInfo.FirstSeatToTop,
												   CommonEnum.CarInnerSpaceType.FirstSeatToTop)));
			sb.AppendLine("             </div>");
			sb.AppendLine("             <div class='car_wading'>");

			#region 右下角对比

			sb.AppendLine("                 <ul>");
			sb.AppendLine("                     <li class='title'>");
			sb.AppendLine("                         <strong>车型</strong>");
			sb.AppendLine("                         <small>前排座椅距离车顶高度</small>");
			sb.AppendLine("                     </li>");

			IEnumerable<CarInnerSpaceInfo> query;
			query = from items in list orderby items.FirstSeatToTop descending select items;
			foreach (CarInnerSpaceInfo carInnerSpaceInfo in query)
			{
				sb.AppendLine(string.Format("   <li {0}>", carInnerSpaceInfo.IsCurrent ? "class='current'" : ""));
				sb.AppendLine(string.Format("        <strong><a href='{0}' title='{2}' target='_blank'>{1}</a></strong>",
											GetDetailUrl(carInnerSpaceInfo.SerialAllSpell),
											carInnerSpaceInfo.CsShowName,
											carInnerSpaceInfo.CarYear + "款 " + carInnerSpaceInfo.CarName));
				sb.AppendLine(string.Format("        <small class='t_r'>{0}cm</small>", carInnerSpaceInfo.FirstSeatToTop));
				sb.AppendLine("                 </li>");
			}
			sb.AppendLine("                </ul>");

			#endregion

			sb.AppendLine("             </div>");
			sb.AppendLine("         </div>");

			#endregion

			sb.AppendLine("     </div>");
		}

		/// <summary>
		///     获取车款详细页地址
		/// </summary>
		/// <param name="serialAllSpell"></param>
		/// <returns></returns>
		private static string GetDetailUrl(string serialAllSpell)
		{
			return "/" + serialAllSpell;
		}

		/// <summary>
		///     查询条件数据
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// </summary>
		/// <param name="number"></param>
		/// <param name="carInnerSpaceType"></param>
		/// <returns></returns>
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

		/// <summary>
		/// </summary>
		/// <param name="number"></param>
		/// <param name="carInnerSpaceType"></param>
		/// <returns></returns>
		private static string ClassLeval(double number, CommonEnum.CarInnerSpaceType carInnerSpaceType)
		{
			string msg = string.Empty;
			switch ((int)carInnerSpaceType)
			{
				case (int)CommonEnum.CarInnerSpaceType.FirstSeatToTop:
					if (number >= 95)
					{
						msg = "kuanyu";
					}
					if (number >= 90 && number < 95)
					{
						msg = "shizhong";
					}
					if (number < 90)
					{
						msg = "jucu";
					}
					break;
				case (int)CommonEnum.CarInnerSpaceType.SecondSeatToTop:
					if (number >= 95)
					{
						msg = "kuanyu";
					}
					if (number >= 90 && number < 95)
					{
						msg = "shizhong";
					}
					if (number < 90)
					{
						msg = "jucu";
					}
					break;
				case (int)CommonEnum.CarInnerSpaceType.FirstSeatDistance:
					if (number >= 75)
					{
						msg = "kuanyu";
					}
					if (number >= 60 && number < 75)
					{
						msg = "shizhong";
					}
					if (number < 60)
					{
						msg = "jucu";
					}
					break;
				case (int)CommonEnum.CarInnerSpaceType.ThirdSeatToTop:
					if (number >= 95)
					{
						msg = "kuanyu";
					}
					if (number >= 90 && number < 95)
					{
						msg = "shizhong";
					}
					if (number < 90)
					{
						msg = "jucu";
					}
					break;
				case (int)CommonEnum.CarInnerSpaceType.BackBoot:
					if (number >= 500)
					{
						msg = "kuanyu";
					}
					if (number >= 300 && number < 500)
					{
						msg = "shizhong";
					}
					if (number > 0 && number < 300)
					{
						msg = "jucu";
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

		/// <summary>
		/// </summary>
		/// <param name="commonHtmlEntity"></param>
		private static void UpdateHtml(CommonHtmlEntity commonHtmlEntity)
		{
			try
			{
				bool success = CommonHtmlService.UpdateCommonHtml(commonHtmlEntity);

				string messsage = string.Format("内部空间块生成成功：ID:{0},TypeID:{1},TagID:{2},BlockID:{3},UpdateTime:{4}",
												commonHtmlEntity.ID, commonHtmlEntity.TypeID, commonHtmlEntity.TagID,
												commonHtmlEntity.BlockID, commonHtmlEntity.UpdateTime);

				if (success) Log.WriteLog(messsage);
			}
			catch (Exception)
			{
				string messsage = string.Format("内部空间块生成失败：ID:{0},TypeID:{1},TagID:{2},BlockID:{3},UpdateTime:{4}",
												commonHtmlEntity.ID, commonHtmlEntity.TypeID, commonHtmlEntity.TagID,
												commonHtmlEntity.BlockID, commonHtmlEntity.UpdateTime);
				Log.WriteErrorLog(messsage);
			}
		}
	}
}