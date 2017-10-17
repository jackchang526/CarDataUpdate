using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using System.IO;
using BitAuto.Utils.Data;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Config;
using System.Data;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
	/// <summary>
	/// 子品牌编辑评测，和子品牌车型详解编辑评测块
	/// </summary>
	public class EditorCommentHtmlBuilder : BaseBuilder
	{
		#region 变量声明及构造函数
		private readonly static string _RootPath;

		private readonly static string _HtmlPath;
		private readonly static string _HtmlFileFormat;

		private readonly static string _PingceHtmlPath;
		private readonly static string _PingceHtmlFileFormat;

		private readonly static string _WirelessHtmlFilePath;

		private readonly static string _SelectParamString;

		private XmlDocument _EditorUserDoc;
		private XmlDocument EditorUserDoc
		{
			get
			{
				if (_EditorUserDoc == null)
				{
					try
					{
						_EditorUserDoc = CommonFunction.GetXmlDocument(CommonData.CommonSettings.EidtorUserUrl);
					}
					catch (Exception exp)
					{
						Log.WriteErrorLog("Get EditorUser Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...");
					}
				}
				return _EditorUserDoc;
			}
		}

		private SerialInfo _serialInfo;

		static EditorCommentHtmlBuilder()
		{
			_RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialSet");

			_HtmlPath = Path.Combine(_RootPath, "EditorCommentHtml");

			// add by chengl Oct.14.2013
			// 改用子品牌ID 不用车型ID
			_HtmlFileFormat = Path.Combine(_HtmlPath, "Serial_EditorComment_cs_{0}.html");
			// _HtmlFileFormat = Path.Combine(_HtmlPath, "Serial_EditorComment_{0}.html");

			_PingceHtmlPath = Path.Combine(_RootPath, "PingceEditorCommentHtml");
			_PingceHtmlFileFormat = Path.Combine(_PingceHtmlPath, "Serial_Comment_{0}.html");
			//移动版 试驾评测 生成地址
			//_WirelessHtmlFilePath = Path.Combine(_RootPath, @"WirelessEditorCommentHtml/Serial_EditorComment_{0}.html");
			_WirelessHtmlFilePath = Path.Combine(_RootPath, @"WirelessEditorCommentHtml/Serial_EditorComment_cs_{0}.html");

			_SelectParamString = "select a.ParamId,a.Pvalue,b.* from cardatabase a join (select b.Car_Id,b.Car_YearType,a.csShowName,b.Car_Name,a.cs_seoname from dbo.Car_Serial a join dbo.Car_relation b on a.cs_id=b.cs_id ) b on a.carid=b.car_id where carid = @carid and ParamId in ('786','787','788','789','857','790','858','859','860','861','712','724') and Pvalue is not null and Pvalue <> ''";
		}
		public EditorCommentHtmlBuilder()
		{
		}
		#endregion

		public override void BuilderDataOrHtml(int objId)
		{
			Log.WriteLog("start EditorCommentHtmlBuilder! id:" + objId.ToString());
			if (objId <= 0 || !CommonData.SerialDic.ContainsKey(objId))
			{
				Log.WriteLog("error not found serial! id:" + objId.ToString());
				return;
			}

			_serialInfo = CommonData.SerialDic[objId];

			if (!ExistsDirectory())
				return;
			if (!ExistsDirectoryHTML())
				return;
			if (!ExistsDirectoryPingceHTML())
				return;

			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "DELETE EditorComment WHERE SerialId=@SerialId", new SqlParameter("@SerialId", _serialInfo.Id));
			string sql = @"SELECT sn.CmsNewsId,sn.FilePath,sn.CarId,n.[Content] 
							FROM SerialNews sn
							INNER JOIN News n ON n.ID=sn.CarNewsId
							WHERE sn.CarNewsTypeId=@CarNewsTypeId AND sn.SerialId=@SerialId AND sn.CarId>0 ORDER BY sn.PublishTime DESC";
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, new SqlParameter("@CarNewsTypeId", (int)CarNewsTypes.pingce), new SqlParameter("@SerialId", _serialInfo.Id));
			if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				DataRowCollection rows = ds.Tables[0].Rows;
				// modified by chengl Oct.14.2013
				Update(objId, rows);
				//List<int> carIdList = new List<int>(rows.Count);

				//int carId;
				//foreach (DataRow row in rows)
				//{
				//    carId = ConvertHelper.GetInteger(row["CarId"]);
				//    if (carId <= 0 || carIdList.Contains(carId))
				//        continue;

				//    if (Update(carId, row, (carIdList.Count == 0)))
				//        carIdList.Add(carId);
				//}
			}
			else
			{
				Log.WriteLog("error no data! id:" + objId.ToString());
			}

			Log.WriteLog("end EditorCommentHtmlBuilder! id:" + objId.ToString());
		}

		/// <summary>
		/// modified by chengl Oct.14.2013 车型DataRow集合传入，便于老版子品牌综述页试驾评测
		/// </summary>
		/// <param name="csid">子品牌ID</param>
		/// <param name="newsRows">车型数据源</param>
		private void Update(int csid, DataRowCollection newsRows)
		{
			List<int> carIdList = new List<int>(newsRows.Count);
			bool hasGenerate = false;
			foreach (DataRow newsRow in newsRows)
			{
				int carId = 0;
				carId = ConvertHelper.GetInteger(newsRow["CarId"]);

				if (carId <= 0 || carIdList.Contains(carId))
					continue;

				DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, _SelectParamString, new SqlParameter("@carid", carId));
				if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
					continue;

				CarDataEntity entity = new CarDataEntity()
				{
					CarId = carId,
					NewsId = ConvertHelper.GetInteger(newsRow["CmsNewsId"]),
					NewsUrl = newsRow["FilePath"].ToString(),
					Content = ConvertHelper.GetString(newsRow["content"])
				};

				DataTable table = ds.Tables[0];
				DataRow dataRow = table.Rows[0];
				entity.YearType = ConvertHelper.GetInteger(dataRow["Car_YearType"]);
				entity.CarName = ConvertHelper.GetString(dataRow["Car_Name"]);
				entity.SerialSeoName = ConvertHelper.GetString(dataRow["cs_seoname"]);
				entity.ShowName = GetShowName(dataRow);
				entity.ShortName = GetShortName(dataRow);
				float pValue;
				DataRow[] rows = null;

				#region 基本参数
				//786		加速时间（0—100km/h）	Perf_MeasuredAcceleration
				rows = table.Select("ParamId=786");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredAcceleration = pValue;
				}
				//787		制动距离（100—0km/h）	Perf_BrakingDistance
				rows = table.Select("ParamId=787");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.BrakingDistance = pValue;
				}
				//788		油耗					Perf_MeasuredFuel
				rows = table.Select("ParamId=788");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredFuel = pValue;
				}
				//789		车内怠速噪音			Perf_MeasuredSlackSpeedNoise
				rows = table.Select("ParamId=789");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredSlackSpeedNoise = pValue;
				}
				//857		车内等速（40km/h）噪音	Perf_MeasuredAveSpeedNoise40
				rows = table.Select("ParamId=857");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredAveSpeedNoise40 = pValue;
				}
				//790		车内等速（60km/h）噪音	Perf_MeasuredAveSpeedNoise
				rows = table.Select("ParamId=790");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredAveSpeedNoise = pValue;
				}
				//858		车内等速（80km/h）噪音	Perf_MeasuredAveSpeedNoise80
				rows = table.Select("ParamId=858");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredAveSpeedNoise80 = pValue;
				}
				//859		车内等速（100km/h）噪音	Perf_MeasuredAveSpeedNoise100
				rows = table.Select("ParamId=859");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredAveSpeedNoise100 = pValue;
				}
				//860		车内等速（120km/h）噪音	Perf_MeasuredAveSpeedNoise120
				rows = table.Select("ParamId=860");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.MeasuredAveSpeedNoise120 = pValue;
				}
				//861		180米绕桩速度			Perf_Slalomspeed180
				rows = table.Select("ParamId=861");
				if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
				{
					entity.Slalomspeed180 = pValue;
				}
				//变速箱
				rows = table.Select("ParamId=712");
				if (rows != null && rows.Length > 0)
				{
					entity.UnderPan_TransmissionType = ConvertHelper.GetString(rows[0]["Pvalue"]);
				}
				//档位个数
				rows = table.Select("ParamId=724");
				if (rows != null && rows.Length > 0)
				{
					entity.UnderPan_ForwardGearNum = ConvertHelper.GetInteger(rows[0]["Pvalue"]);
				}
				#endregion

				SetComment(entity);
				// 子品牌评测页-实测数据
				UpdateSerialPingceHtml(entity);
				//新综述页编辑点评内容
				UpdateSerialPingceHtmlForNew(entity);

				// 子品牌综述页 试驾评测改为按子品牌生成 每个子品牌生成1个车型
				if (!hasGenerate)
				{
					// 每个子品牌只生成1次
					UpdateSerialHtml(csid, entity, ref hasGenerate);
					UpdateSerialHtmlForWireless(csid, entity);
				}

				if (carIdList.Count == 0)
				{
					SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "INSERT INTO EditorComment(SerialId,CarId) VALUES(@SerialId,@CarId)", new SqlParameter("@SerialId", _serialInfo.Id), new SqlParameter("@CarId", carId));
				}
				carIdList.Add(carId);
			}
		}
		/// <summary>
		/// 子品牌综述页
		/// </summary>
		private void UpdateSerialHtml(int csid, CarDataEntity entity, ref bool hasGenerate)
		{
			if (entity == null)
				return;
			// modified by chengl Oct.14.2013
			// 加速时间，油耗，制动距离值的才显示
			if (entity.MeasuredAcceleration <= 0 || entity.MeasuredFuel <= 0 || entity.BrakingDistance <= 0)
			{ return; }

			string href = string.Format("/{0}/pingce/", _serialInfo.AllSpell);
			int newId = entity.NewsId;
			if (newId > 0)
			{
				href = string.Concat(href, "p", newId.ToString(), "/");
			}
			StringBuilder html = new StringBuilder();
			html.Append("<div class=\"line_box newcar_test\">");
			html.Append("	<h3><span><a href=\"").Append(href).Append("\" target=\"_blank\">试驾评测 - ");
			html.Append(entity.ShowName);
			html.Append("</a></span></h3>");
			html.Append("	<div class=\"tablebox\">");
			html.Append("		<table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");
			html.Append("			<tbody>");
			html.Append("           <tr>");
			html.Append("				<td class=\"td_tt cGray\">加速时间(0-100km/h)</td>");
			html.Append("				<td class=\"w162\">");
			html.Append(GetCarDataEntityValue(entity.MeasuredAcceleration, "秒", "-"));
			html.Append("</td>");
			html.Append("				<td class=\"td_tt cGray\">油耗(100km)</td>");
			html.Append("				<td class=\"w162\">");
			html.Append(GetCarDataEntityValue(entity.MeasuredFuel, "升", "-"));
			html.Append("</td>");
			html.Append("			</tr>");
			html.Append("			<tr>");
			html.Append("				<td class=\"td_tt cGray\">180米绕桩速度</td>");
			html.Append("				<td class=\"w162\">");
			html.Append(GetCarDataEntityValue(entity.Slalomspeed180, "公里/小时", "-"));
			html.Append("</td>");
			html.Append("				<td class=\"td_tt cGray\">制动距离(100-0km/h)</td>");
			html.Append("				<td class=\"w162\">");
			html.Append(GetCarDataEntityValue(entity.BrakingDistance, "米", "-"));
			html.Append("</td>");
			html.Append("			</tr>");
			html.Append("			<tr>");
			html.Append("				<td class=\"td_tt cGray\">噪音</td>");
			html.Append("				<td colspan=\"3\" class=\"w535\">");
			html.Append("					<ul>");
			html.Append("						<li><span>车内怠速：</span>").Append(GetCarDataEntityValue(entity.MeasuredSlackSpeedNoise, "分贝", "-")).Append("</li>");
			html.Append("						<li><span>40km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise40, "分贝", "-")).Append("</li>");
			html.Append("						<li><span>60km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise, "分贝", "-")).Append("</li>");
			html.Append("						<li><span>80km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise80, "分贝", "-")).Append("</li>");
			html.Append("						<li><span>100km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise100, "分贝", "-")).Append("</li>");
			html.Append("						<li><span>120km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise120, "分贝", "-")).Append("</li>");
			html.Append("					</ul>");
			html.Append("				</td>");
			html.Append("			</tr>");
			if (entity.Editors != null && entity.Editors.Count > 0)
			{
				foreach (EditorDataEntity editor in entity.Editors)
				{
					html.Append("			<tr>");
					html.Append("				<td class=\"td_tt\">");
					html.Append("					<dl>");

					html.Append("						<dt><a ");
					if (!string.IsNullOrEmpty(editor.UserBlogUrl))
					{
						html.Append("href=\"").Append(editor.UserBlogUrl).Append("\" ");
					}
					html.Append("target=\"_blank\"><img src=\"").Append(editor.SquarePhoto).Append("\"></a></dt>");
					html.Append("						<dd>");
					if (!string.IsNullOrEmpty(editor.UserBlogUrl))
					{
						html.Append("<a target=\"_blank\" href=\"").Append(editor.UserBlogUrl).Append("\">");
						html.Append(editor.UserName).Append("</a>");
					}
					else
					{
						html.Append("<p><b>").Append(editor.UserName).Append("</b></p>");
					}
					html.Append("<p>车讯编辑</p></dd>");

					html.Append("					</dl>");
					html.Append("				</td><td colspan=\"3\" class=\"w535\">").Append(editor.Comment).Append("</td>");
					html.Append("			</tr>");
				}
			}

			html.Append("		</tbody></table>");
			html.Append("	</div>");
			html.Append("	<div class=\"more\"><a rel=\"nofollow\" href=\"").Append(href).Append("\" targest=\"_blank\">详解&gt;&gt;</a></div>");
			html.Append("</div>");

			CommonFunction.SaveFileContent(html.ToString(), string.Format(_HtmlFileFormat, csid.ToString()), Encoding.UTF8);
			hasGenerate = true;
		}
		/// <summary>
		/// 生成无线版子品牌综述页 试驾评测块
		/// </summary>
		/// <param name="entity"></param>
		private void UpdateSerialHtmlForWireless(int csid, CarDataEntity entity)
		{
			if (entity == null)
				return;

			// modified by chengl Oct.14.2013
			// 加速时间，油耗，制动距离值的才显示
			if (entity.MeasuredAcceleration <= 0 || entity.MeasuredFuel <= 0 || entity.BrakingDistance <= 0)
			{ return; }

			StringBuilder html = new StringBuilder();
			html.Append("<section class=\"m-line-box m-summary-report\">");
			html.Append("		<div class=\"m-tabs-box\">");
			html.Append("			<ul class=\"m-tabs  m-tabs-first\">");
			html.Append("				<li class=\"current\"><span>评测<s></s></span></li>");
			html.Append("			</ul>");
			//html.AppendFormat("<a href=\"{0}\" class=\"more\">查看详情&gt;&gt;</a>",
			//	entity.NewsUrl.Replace("news.bitauto.com", "news.m.yiche.com") + "?ref=pingce");
			html.Append("		</div>");
			//if (entity.Editors != null && entity.Editors.Count > 0)
			//{
			//    foreach (EditorDataEntity editor in entity.Editors)
			//    {
			//        html.AppendFormat("		<p>{0}<a href=\"{1}\">查看详情&gt;&gt;</a></p>", editor.Comment, entity.NewsUrl);
			//        html.AppendFormat("		<p class=\"textar\">——<strong>{0}</strong> 车讯编辑</p>", editor.UserName);
			//    }
			//}
			html.AppendFormat("<p>评测车款：{0}{1}&nbsp;&nbsp;</p>",
				entity.YearType > 0 ? entity.YearType + "款 " : "",
				entity.CarName);
			html.Append("		<div class=\"m-sub-tabs-box\"><span>动力及操控</span></div>");
			html.Append("<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\">");
			html.Append("	<tbody><tr>");
			html.Append("		<td>加速时间(0-100km/h)：</td>");
			html.AppendFormat("				<td><span>{0}</span></td>", GetCarDataEntityValue(entity.MeasuredAcceleration, "秒", "-"));
			html.Append("	</tr>");
			html.Append("	<tr>");
			html.Append("		<td>油耗(100km)：</td>");
			html.AppendFormat("				<td><span>{0}</span></td>", GetCarDataEntityValue(entity.MeasuredFuel, "升", "-"));
			html.Append("	</tr>			");
			html.Append("	<tr>");
			html.Append("		<td>180米绕桩速度：</td>");
			html.AppendFormat("		<td><span>{0}</span></td>", GetCarDataEntityValue(entity.Slalomspeed180, "公里/小时", "-"));
			html.Append("	</tr>			");
			html.Append("	<tr>");
			html.Append("		<td>制动距离(100-0km/h)：</td>");
			html.AppendFormat("		<td><span>{0}</span></td>", GetCarDataEntityValue(entity.BrakingDistance, "米", "-"));
			html.Append("	</tr>			");
			html.Append("</tbody></table>");
			//html.Append("		<div class=\"m-sub-tabs-box\"><span>噪音</span></div>");
			//html.Append("		<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" >");
			//html.Append("			<tr>");
			//html.Append("				<td>");
			//html.Append("					怠速噪音：");
			//html.AppendFormat("					<span>{0}</span>", GetCarDataEntityValue(entity.MeasuredSlackSpeedNoise, "分贝", "-"));
			//html.Append("				</td>");
			//html.Append("				<td>");
			//html.Append("					40km/h：");
			//html.AppendFormat("					<span>{0}</span>", GetCarDataEntityValue(entity.MeasuredAveSpeedNoise40, "分贝", "-"));
			//html.Append("				</td>");
			//html.Append("			</tr>");
			//html.Append("			<tr>");
			//html.Append("				<td>");
			//html.Append("					60km/h：");
			//html.AppendFormat("					<span>{0}</span>", GetCarDataEntityValue(entity.MeasuredAveSpeedNoise, "分贝", "-"));
			//html.Append("				</td>");
			//html.Append("				<td>");
			//html.Append("					80km/h：");
			//html.AppendFormat("					<span>{0}</span>", GetCarDataEntityValue(entity.MeasuredAveSpeedNoise80, "分贝", "-"));
			//html.Append("				</td>");
			//html.Append("			</tr>");
			//html.Append("		</table>");
			html.AppendFormat("<a href=\"{0}\" class=\"m-btn-line m-btn-gray\">{1}评测详情&gt;&gt;</a>",
				entity.NewsUrl.Replace("news.bitauto.com", "news.m.yiche.com") + "?ref=pingce", entity.SerialSeoName);
			html.Append("	</section>");

			CommonFunction.SaveFileContent(html.ToString(), string.Format(_WirelessHtmlFilePath, csid), Encoding.UTF8);
		}
		/// <summary>
		/// 子品牌评测页-实测数据
		/// </summary>
		private void UpdateSerialPingceHtml(CarDataEntity entity)
		{
			if (entity == null)
				return;

			StringBuilder html = new StringBuilder();
			html.Append("<div class=\"newcar_test\">");
			html.Append("    <div class=\"newcar_test_tt\">").Append(entity.ShowName).Append("实测数据</div>");
			html.Append("    <table cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">");
			html.Append("        <tbody>");
			html.Append("            <tr>");
			html.Append("                <td class=\"td_tt\">");
			html.Append("                    加速时间(0-100km/h)</TH>");
			html.Append("                    <td class=\"w154\">").Append(GetCarDataEntityValue(entity.MeasuredAcceleration, "秒", "-")).Append("</td>");
			html.Append("                    <td class=\"td_tt\">");
			html.Append("                        油耗(100km)</TH>");
			html.Append("                        <td class=\"w154\">").Append(GetCarDataEntityValue(entity.MeasuredFuel, "升", "-")).Append("</td>");
			html.Append("            </tr>");
			html.Append("            <tr>");
			html.Append("                <td class=\"td_tt\">");
			html.Append("                    180米绕桩速度</TH>");
			html.Append("                    <td class=\"w154\">").Append(GetCarDataEntityValue(entity.Slalomspeed180, "公里/小时", "-")).Append("</td>");
			html.Append("                    <td class=\"td_tt\">");
			html.Append("                        制动距离(100-0km/h)</TH>");
			html.Append("                        <td class=\"w154\">").Append(GetCarDataEntityValue(entity.BrakingDistance, "米", "-")).Append("</td>");
			html.Append("            </tr>");
			html.Append("            <tr>");
			html.Append("                <td class=\"td_tt\">");
			html.Append("                    噪音</TH>");
			html.Append("                    <td class=\"w445\" colspan=\"3\">");
			html.Append("                        <ul>");
			html.Append("                            <li><span>车内怠速：</span>").Append(GetCarDataEntityValue(entity.MeasuredSlackSpeedNoise, "分贝", "-")).Append(" </li>");
			html.Append("                            <li><span>40km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise40, "分贝", "-")).Append(" </li>");
			html.Append("                            <li><span>60km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise, "分贝", "-")).Append(" </li>");
			html.Append("                            <li><span>80km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise80, "分贝", "-")).Append(" </li>");
			html.Append("                            <li><span>100km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise100, "分贝", "-")).Append(" </li>");
			html.Append("                            <li><span>120km/h等速：</span>").Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise120, "分贝", "-")).Append(" </li>");
			html.Append("                        </ul>");
			html.Append("                    </td>");
			html.Append("            </tr>");

			if (entity.Editors != null && entity.Editors.Count > 0)
			{
				foreach (EditorDataEntity editor in entity.Editors)
				{
					html.Append("			<tr>");
					html.Append("				<td class=\"td_tt\">");
					html.Append("					<dl>");
					html.Append("						<dt><a ");
					if (!string.IsNullOrEmpty(editor.UserBlogUrl))
					{
						html.Append("href=\"").Append(editor.UserBlogUrl).Append("\" ");
					}
					html.Append("target=\"_blank\"><img src=\"").Append(editor.SquarePhoto).Append("\"></a></dt>");
					html.Append("						<dd>");
					if (!string.IsNullOrEmpty(editor.UserBlogUrl))
					{
						html.Append("<a target=\"_blank\" href=\"").Append(editor.UserBlogUrl).Append("\">");
						html.Append(editor.UserName).Append("</a>");
					}
					else
					{
						html.Append("<p><b>").Append(editor.UserName).Append("</b></p>");
					}
					html.Append("<p>车讯编辑</p></dd>");

					html.Append("					</dl>");
					html.Append("				</td><td colspan=\"3\" class=\"w445\">").Append(editor.Comment).Append("</td>");
					html.Append("			</tr>");
				}
			}

			html.Append("        </tbody>");
			html.Append("    </table>");
			html.Append("</div>");
			CommonFunction.SaveFileContent(html.ToString(), string.Format(_PingceHtmlFileFormat, entity.CarId.ToString()), Encoding.UTF8);
		}
		/// <summary>
		/// 更新编辑点评块内容 (新版综述页)
		/// </summary>
		/// <param name="entity"></param>
		private void UpdateSerialPingceHtmlForNew(CarDataEntity entity)
		{
			try
			{
				string href = string.Format("/{0}/pingce/", _serialInfo.AllSpell);
				int newId = entity.NewsId;
				if (newId > 0)
				{
					href = string.Concat(href, "p", newId.ToString(), "/");
				}

				StringBuilder sb = new StringBuilder();
				if (entity.Editors != null && entity.Editors.Count > 0)
				{
					foreach (EditorDataEntity editor in entity.Editors)
					{
						sb.Append("<div class=\"car_grade\">");
						sb.Append("	<dl>");
						sb.AppendFormat("    	<dt><a href=\"{0}\" target=\"_blank\"><img src=\"{1}\" width=\"60\" height=\"60\"/></a></dt>", editor.UserBlogUrl, editor.SquarePhoto);
						var userName = editor.UserName;
						if (editor.UserName.IndexOf('(') != -1)
							userName = userName.Substring(0, userName.IndexOf("("));
						sb.AppendFormat("        <dd class=\"car_grade_n\"><label>编辑：</label>{0}</dd>", userName);
						sb.AppendFormat("        <dd><label>车款：</label><a href=\"{0}\" target=\"_blank\">{1}款 {2}</a></dd>", href, entity.YearType, entity.CarName);
						sb.AppendFormat("        <dd class=\"car_grade_gearbox\"><label>变速箱：</label>{0}{1}</dd>",
							entity.UnderPan_ForwardGearNum > 0 ? entity.UnderPan_ForwardGearNum + "速" : "",
							entity.UnderPan_TransmissionType);
						sb.Append("    </dl>");
						sb.Append("    <p>");
						sb.AppendFormat("    	<strong>编辑点评：</strong>{0}", editor.Comment);
						sb.Append("    </p>");
						sb.Append("    <div class=\"clear\"></div>");
						sb.Append("</div>");
					}
					bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
					{
						ID = entity.CarId,
						TypeID = CommonHtmlEnum.TypeEnum.Car,
						TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
						BlockID = CommonHtmlEnum.BlockIdEnum.EditorComment,
						HtmlContent = sb.ToString(),
						UpdateTime = DateTime.Now
					});
					if (!success) Log.WriteErrorLog("更新编辑评论失败：carId:" + entity.CarId);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

		private void SetComment(CarDataEntity carEntity)
		{
			if (carEntity == null)
				return;
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(string.Format(CommonData.CommonSettings.EditorCommentUrl, carEntity.CarId.ToString()));
				XmlNodeList nodeList = doc.SelectNodes("//CarEstimate");
				if (nodeList.Count > 0)
				{
					carEntity.Editors = new List<EditorDataEntity>(2);
					XmlNode ele;
					int userId;
					foreach (XmlNode node in nodeList)
					{
						userId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CommentUserId"), string.Empty));

						ele = EditorUserDoc.SelectSingleNode(string.Format("//User[UserId='{0}']", userId.ToString()));
						if (ele == null)
							continue;

						carEntity.Editors.Add(new EditorDataEntity()
						{
							Comment = GetNodeValue(node.SelectSingleNode("Comment"), string.Empty),
							UserId = userId,
							UserName = GetNodeValue(node.SelectSingleNode("CommentUserName"), string.Empty),
							CommentTime = ConvertHelper.GetDateTime(GetNodeValue(node.SelectSingleNode("CommentTime"), string.Empty)),
							UserImg = GetNodeValue(ele.SelectSingleNode("UserImageUrl"), string.Empty),
							SquarePhoto = GetNodeValue(ele.SelectSingleNode("SquarePhoto"), "http://img1.bitautoimg.com/images/not.gif"),
							UserBlogUrl = GetNodeValue(ele.SelectSingleNode("UserBlogUrl"), string.Empty)
						});
						if (carEntity.Editors.Count == 2)
							break;
					}
				}
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog("Get ServiceData Error (carId:" + carEntity.CarId.ToString() + ";Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...");
			}
		}

		string GetNodeValue(XmlNode node, string defaultStr)
		{
			return node != null && !string.IsNullOrEmpty(node.InnerText.Trim()) ? node.InnerText.Trim() : defaultStr;
		}
		private string GetCarDataEntityValue(float pValue, string unit, string defStr)
		{
			string result;
			if (pValue > 0)
				result = string.Concat(pValue.ToString("#0.0#"), unit);
			else
				result = defStr;
			return result;
		}
		private string GetShowName(DataRow dataRow)
		{
			StringBuilder result = new StringBuilder();
			if (!(dataRow["Car_YearType"] is DBNull) && !string.IsNullOrEmpty(dataRow["Car_YearType"].ToString()))
			{
				result.Append(dataRow["Car_YearType"].ToString());
				result.Append("款 ");
			}
			if (!(dataRow["csShowName"] is DBNull) && !string.IsNullOrEmpty(dataRow["csShowName"].ToString()))
			{
				result.Append(dataRow["csShowName"].ToString());
				result.Append(" ");
			}
			if (!(dataRow["Car_Name"] is DBNull) && !string.IsNullOrEmpty(dataRow["Car_Name"].ToString()))
			{
				result.Append(dataRow["Car_Name"].ToString());
			}
			return result.ToString();
		}
		private string GetShortName(DataRow dataRow)
		{
			StringBuilder result = new StringBuilder();
			//if (!(dataRow["Car_YearType"] is DBNull) && !string.IsNullOrEmpty(dataRow["Car_YearType"].ToString()))
			//{
			//    result.Append(dataRow["Car_YearType"].ToString());
			//    result.Append("款 ");
			//}
			if (!(dataRow["csShowName"] is DBNull) && !string.IsNullOrEmpty(dataRow["csShowName"].ToString()))
			{
				result.Append(dataRow["csShowName"].ToString());
			}
			return result.ToString();
		}
		#region 其他
		/// <summary>
		/// 检测目录是否存在，如果不存在将创建
		/// </summary>
		/// <returns></returns>
		private bool ExistsDirectory()
		{
			if (!Directory.Exists(_RootPath))
			{
				Log.WriteLog("Start Create SerialSet Directory (Path:" + _RootPath + ")...");
				try
				{
					Directory.CreateDirectory(_RootPath);
				}
				catch (Exception exp)
				{
					Log.WriteErrorLog("Create SerialSet Directory Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...");
					return false;
				}
				Log.WriteLog("End Create SerialSet Directory ...");
			}
			return true;
		}
		/// <summary>
		/// 检测目录是否存在，如果不存在将创建
		/// </summary>
		/// <returns></returns>
		private bool ExistsDirectoryHTML()
		{
			if (!Directory.Exists(_HtmlPath))
			{
				Log.WriteLog("Start Create EditorCommentHtml Directory (Path:" + _HtmlPath + ")...");
				try
				{
					Directory.CreateDirectory(_HtmlPath);
				}
				catch (Exception exp)
				{
					Log.WriteErrorLog("Create EditorCommentHtml Directory Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...");
					return false;
				}
				Log.WriteLog("End Create EditorCommentHtml Directory ...");
			}
			return true;
		}
		/// <summary>
		/// 检测评测目录是否存在，如果不存在将创建
		/// </summary>
		/// <returns></returns>
		private bool ExistsDirectoryPingceHTML()
		{
			if (!Directory.Exists(_PingceHtmlPath))
			{
				Log.WriteLog("Start Create PingceEditorCommentHtml Directory (Path:" + _PingceHtmlPath + ")...");
				try
				{
					Directory.CreateDirectory(_PingceHtmlPath);
				}
				catch (Exception exp)
				{
					Log.WriteErrorLog("Create PingceEditorCommentHtml Directory Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...");
					return false;
				}
				Log.WriteLog("End Create PingceEditorCommentHtml Directory ...");
			}
			return true;
		}
		#endregion

		#region 数据对象
		class CarDataEntity
		{
			/// <summary>
			/// 车型ID
			/// </summary>
			public int CarId;
			/// <summary>
			/// 年款
			/// </summary>
			public int YearType;
			/// <summary>
			/// 车款名称
			/// </summary>
			public string CarName;
			/// <summary>
			/// 新闻id
			/// </summary>
			public int NewsId;
			/// <summary>
			/// 新闻url
			/// </summary>
			public string NewsUrl;
			/// <summary>
			/// 新闻内容简介
			/// </summary>
			public string Content;
			/// <summary>
			/// 显示名称
			/// </summary>
			public string ShowName;
			/// <summary>
			/// Seo名称
			/// </summary>
			public string SerialSeoName;
			/// <summary>
			/// 年款 + 子品牌名称
			/// </summary>
			public string ShortName;
			/// <summary>
			/// 加速时间（0—100km/h）
			/// </summary>
			public float MeasuredAcceleration;
			/// <summary>
			/// 制动距离（100—0km/h）
			/// </summary>
			public float BrakingDistance;
			/// <summary>
			/// 油耗
			/// </summary>
			public float MeasuredFuel;
			/// <summary>
			/// 车内怠速噪音
			/// </summary>
			public float MeasuredSlackSpeedNoise;
			/// <summary>
			/// 车内等速（40km/h）噪音
			/// </summary>
			public float MeasuredAveSpeedNoise40;
			/// <summary>
			/// 车内等速（60km/h）噪音
			/// </summary>
			public float MeasuredAveSpeedNoise;
			/// <summary>
			/// 车内等速（80km/h）噪音
			/// </summary>
			public float MeasuredAveSpeedNoise80;
			/// <summary>
			/// 车内等速（100km/h）噪音
			/// </summary>
			public float MeasuredAveSpeedNoise100;
			/// <summary>
			/// 车内等速（120km/h）噪音
			/// </summary>
			public float MeasuredAveSpeedNoise120;
			/// <summary>
			/// 180米绕桩速度
			/// </summary>
			public float Slalomspeed180;
			/// <summary>
			/// 档位个数
			/// </summary>
			public int UnderPan_ForwardGearNum;
			/// <summary>
			/// 变速箱类型
			/// </summary>
			public string UnderPan_TransmissionType;

			public List<EditorDataEntity> Editors;
		}
		class EditorDataEntity
		{
			/// <summary>
			/// 编辑ID
			/// </summary>
			public int UserId;
			/// <summary>
			/// 编辑图片
			/// </summary>
			public string UserImg;
			/// <summary>
			/// 编辑 方图
			/// </summary>
			public string SquarePhoto { get; set; }
			/// <summary>
			/// 编辑名称
			/// </summary>
			public string UserName;
			/// <summary>
			/// 编辑博客
			/// </summary>
			public string UserBlogUrl;
			/// <summary>
			/// 评测时间
			/// </summary>
			public DateTime CommentTime;
			/// <summary>
			/// 评测内容
			/// </summary>
			public string Comment;
		}
		#endregion
	}
}
