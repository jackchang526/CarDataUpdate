using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    /// <summary>
    /// 子品牌编辑评测 新 第四级
    /// </summary>
    public class EditorCommentHtmlBuilderNew : BaseBuilder
    {
        #region 变量声明及构造函数
        private static readonly string SelectParamString;
        private int _serialId = 0;
        private SerialInfo _serialInfo;
        private XmlDocument _editorUserDoc;
        private XmlDocument EditorUserDoc
        {
            get
            {
                if (_editorUserDoc == null)
                {
                    try
                    {
                        _editorUserDoc = CommonFunction.GetXmlDocument(CommonData.CommonSettings.EidtorUserUrl);
                    }
                    catch (Exception exp)
                    {
                        Log.WriteErrorLog("Get EditorUser Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...");
                    }
                }
                return _editorUserDoc;
            }
        }

        private static readonly string _RootPath;
        private static readonly string _PingceHtmlPathV2;
        private static readonly string _PingceHtmlFileFormatV2;

        static EditorCommentHtmlBuilderNew()
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialSet");
            _PingceHtmlPathV2 = Path.Combine(_RootPath, "PingceEditorCommentHtmlV2");
            _PingceHtmlFileFormatV2 = Path.Combine(_PingceHtmlPathV2, "Serial_Comment_{0}.html");

            //_selectParamString = "select b.Car_Id,b.Car_YearType,a.csShowName,b.Car_Name,a.cs_seoname from dbo.Car_Serial a join dbo.Car_relation b on a.cs_id=b.cs_id  where b.car_id = @carid";
            SelectParamString =
                "select a.ParamId,a.Pvalue,b.* from cardatabase a join (select b.Car_Id,b.Car_YearType,a.csShowName,b.Car_Name,a.cs_seoname from dbo.Car_Serial a join dbo.Car_relation b on a.cs_id=b.cs_id ) b on a.carid=b.car_id where carid = @carid and ParamId in ('786','787','788','789','857','790','858','859','860','861','712','724') and Pvalue is not null and Pvalue <> ''";
        }

        #endregion

        public override void BuilderDataOrHtml(int objId)
        {
            Log.WriteLog("start EditorCommentHtmlBuilderNew! id:" + objId.ToString());
            if (objId <= 0 || !CommonData.SerialDic.ContainsKey(objId))
            {
                Log.WriteLog("error not found serial! id:" + objId.ToString());
                return;
            }
            _serialId = objId;
            _serialInfo = CommonData.SerialDic[objId];
            Update(objId);
            Log.WriteLog("end EditorCommentHtmlBuilderNew! id:" + objId.ToString());
        }

        /// <summary>
        /// H5子品牌综述页编辑评测
        /// </summary>
        /// <param name="csid">子品牌ID</param>
        private void Update(int csid)
        {
            
            List<EditorDataEntity> list = GetEditorData(2);

            //UpdateSerialPingceHtmlForNew(list);

            UpdateSerialPingceHtmlForH5V2(list.Take(1).ToList());
            
            #region 1200版 lisf 1206-10-10
            List<EditorDataEntity> listNew = GetEditorDataNew(2);
            UpdateSerialEditComment(listNew);
            #endregion

            //H5综述页编辑点评内容
            //UpdateSerialPingceHtmlForH5(csid);

            //车型==>评测文章==>编辑有话说
            UpdateSerialPingceHtmlV2();
        }
        private void UpdateSerialPingceHtmlV2()
        {
            var sql = @"SELECT sn.CmsNewsId,sn.FilePath,sn.CarId,n.[Content] 
                            FROM SerialNews sn
                            INNER JOIN News n ON n.ID=sn.CarNewsId
                            WHERE sn.CarNewsTypeId=@CarNewsTypeId AND sn.SerialId=@SerialId AND sn.CarId>0 ORDER BY sn.PublishTime DESC";
            var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
                CommandType.Text, sql, new SqlParameter("@CarNewsTypeId", (int)CarNewsTypes.pingce),
                new SqlParameter("@SerialId", _serialInfo.Id));
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var rows = ds.Tables[0].Rows;
                Update(rows);
            }
        }
        private void Update(DataRowCollection newsRows)
        {
            var carIdList = new List<int>(newsRows.Count);
            foreach (DataRow newsRow in newsRows)
            {
                var carId = 0;
                carId = ConvertHelper.GetInteger(newsRow["CarId"]);

                if (carId <= 0 || carIdList.Contains(carId))
                    continue;

                var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString,
                    CommandType.Text, SelectParamString, new SqlParameter("@carid", carId));
                if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
                    continue;

                var entity = new CarDataEntity
                {
                    CarId = carId,
                    NewsId = ConvertHelper.GetInteger(newsRow["CmsNewsId"]),
                    NewsUrl = newsRow["FilePath"].ToString(),
                    Content = ConvertHelper.GetString(newsRow["content"])
                };
                var table = ds.Tables[0];
                var dataRow = table.Rows[0];
                entity.YearType = ConvertHelper.GetInteger(dataRow["Car_YearType"]);
                entity.CarName = ConvertHelper.GetString(dataRow["Car_Name"]);
                entity.SerialSeoName = ConvertHelper.GetString(dataRow["cs_seoname"]);
                entity.ShowName = GetShowName(dataRow);
                entity.ShortName = GetShortName(dataRow);
                float pValue;
                DataRow[] rows = null;

                #region 基本参数

                //786       加速时间（0—100km/h） Perf_MeasuredAcceleration
                rows = table.Select("ParamId=786");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredAcceleration = pValue;
                }
                //787       制动距离（100—0km/h） Perf_BrakingDistance
                rows = table.Select("ParamId=787");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.BrakingDistance = pValue;
                }
                //788       油耗                  Perf_MeasuredFuel
                rows = table.Select("ParamId=788");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredFuel = pValue;
                }
                //789       车内怠速噪音          Perf_MeasuredSlackSpeedNoise
                rows = table.Select("ParamId=789");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredSlackSpeedNoise = pValue;
                }
                //857       车内等速（40km/h）噪音  Perf_MeasuredAveSpeedNoise40
                rows = table.Select("ParamId=857");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredAveSpeedNoise40 = pValue;
                }
                //790       车内等速（60km/h）噪音  Perf_MeasuredAveSpeedNoise
                rows = table.Select("ParamId=790");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredAveSpeedNoise = pValue;
                }
                //858       车内等速（80km/h）噪音  Perf_MeasuredAveSpeedNoise80
                rows = table.Select("ParamId=858");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredAveSpeedNoise80 = pValue;
                }
                //859       车内等速（100km/h）噪音 Perf_MeasuredAveSpeedNoise100
                rows = table.Select("ParamId=859");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredAveSpeedNoise100 = pValue;
                }
                //860       车内等速（120km/h）噪音 Perf_MeasuredAveSpeedNoise120
                rows = table.Select("ParamId=860");
                if (rows != null && rows.Length > 0 && float.TryParse(rows[0]["Pvalue"].ToString(), out pValue))
                {
                    entity.MeasuredAveSpeedNoise120 = pValue;
                }
                //861       180米绕桩速度            Perf_Slalomspeed180
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
                GenerateHtml(entity);
                carIdList.Add(carId);
            }
        }
        private string GetShowName(DataRow dataRow)
        {
            var result = new StringBuilder();
            if (!(dataRow["Car_YearType"] is DBNull) && !string.IsNullOrEmpty(dataRow["Car_YearType"].ToString()))
            {
                result.Append(dataRow["Car_YearType"]);
                result.Append("款 ");
            }
            if (!(dataRow["csShowName"] is DBNull) && !string.IsNullOrEmpty(dataRow["csShowName"].ToString()))
            {
                result.Append(dataRow["csShowName"]);
                result.Append(" ");
            }
            if (!(dataRow["Car_Name"] is DBNull) && !string.IsNullOrEmpty(dataRow["Car_Name"].ToString()))
            {
                result.Append(dataRow["Car_Name"]);
            }
            return result.ToString();
        }
        private string GetShortName(DataRow dataRow)
        {
            var name = string.Empty;
            if (!(dataRow["csShowName"] is DBNull) && !string.IsNullOrEmpty(dataRow["csShowName"].ToString()))
            {
                name = dataRow["csShowName"].ToString();
            }
            return name;
        }
        /// <summary>
        ///     子品牌评测页-实测数据V2
        /// </summary>
        private void GenerateHtml(CarDataEntity entity)
        {
            if (entity == null)
                return;

            var html = new StringBuilder();
            html.Append("<div class=\"newcar_test\">");
            html.Append("    <div class=\"newcar_test_tt\">").Append(entity.ShowName).Append("实测数据</div>");
            html.Append("    <table cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">");
            html.Append("        <tbody>");
            html.Append("            <tr>");
            html.Append("                <td class=\"td_tt\">");
            html.Append("                    加速时间(0-100km/h)</TH>");
            html.Append("                    <td class=\"w154\">")
                .Append(GetCarDataEntityValue(entity.MeasuredAcceleration, "秒", "-"))
                .Append("</td>");
            html.Append("                    <td class=\"td_tt\">");
            html.Append("                        油耗(100km)</TH>");
            html.Append("                        <td class=\"w154\">")
                .Append(GetCarDataEntityValue(entity.MeasuredFuel, "升", "-"))
                .Append("</td>");
            html.Append("            </tr>");
            html.Append("            <tr>");
            html.Append("                <td class=\"td_tt\">");
            html.Append("                    180米绕桩速度</TH>");
            html.Append("                    <td class=\"w154\">")
                .Append(GetCarDataEntityValue(entity.Slalomspeed180, "公里/小时", "-"))
                .Append("</td>");
            html.Append("                    <td class=\"td_tt\">");
            html.Append("                        制动距离(100-0km/h)</TH>");
            html.Append("                        <td class=\"w154\">")
                .Append(GetCarDataEntityValue(entity.BrakingDistance, "米", "-"))
                .Append("</td>");
            html.Append("            </tr>");
            html.Append("            <tr>");
            html.Append("                <td class=\"td_tt\">");
            html.Append("                    噪音</TH>");
            html.Append("                    <td class=\"w445\" colspan=\"3\">");
            html.Append("                        <ul>");
            html.Append("                            <li><span>车内怠速：</span>")
                .Append(GetCarDataEntityValue(entity.MeasuredSlackSpeedNoise, "分贝", "-"))
                .Append(" </li>");
            html.Append("                            <li><span>40km/h等速：</span>")
                .Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise40, "分贝", "-"))
                .Append(" </li>");
            html.Append("                            <li><span>60km/h等速：</span>")
                .Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise, "分贝", "-"))
                .Append(" </li>");
            html.Append("                            <li><span>80km/h等速：</span>")
                .Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise80, "分贝", "-"))
                .Append(" </li>");
            html.Append("                            <li><span>100km/h等速：</span>")
                .Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise100, "分贝", "-"))
                .Append(" </li>");
            html.Append("                            <li><span>120km/h等速：</span>")
                .Append(GetCarDataEntityValue(entity.MeasuredAveSpeedNoise120, "分贝", "-"))
                .Append(" </li>");
            html.Append("                        </ul>");
            html.Append("                    </td>");
            html.Append("            </tr>");

            if (entity.Editors != null && entity.Editors.Count > 0)
            {
                foreach (var editor in entity.Editors)
                {
                    html.Append("           <tr>");
                    html.Append("               <td class=\"td_tt\">");
                    html.Append("                   <dl>");
                    html.Append("                       <dt><a ");
                    if (!string.IsNullOrEmpty(editor.UserBlogUrl))
                    {
                        html.Append("href=\"").Append(editor.UserBlogUrl).Append("\" ");
                    }
                    html.Append("target=\"_blank\"><img src=\"").Append(editor.SquarePhoto).Append("\"></a></dt>");
                    html.Append("                       <dd>");
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

                    html.Append("                   </dl>");
                    html.Append("               </td><td colspan=\"3\" class=\"w445\">").Append(editor.Comment).Append("</td>");
                    html.Append("           </tr>");
                }
            }

            html.Append("        </tbody>");
            html.Append("    </table>");
            html.Append("</div>");
            CommonFunction.SaveFileContent(html.ToString(), string.Format(_PingceHtmlFileFormatV2, entity.CarId),
                Encoding.UTF8);
        }
        private void SetComment(CarDataEntity carEntity)
        {
            if (carEntity == null)
                return;
            try
            {
                var doc = new XmlDocument();
                doc.Load(string.Format(CommonData.CommonSettings.EditorCommentUrl, carEntity.CarId));
                var nodeList = doc.SelectNodes("//CarEstimate");
                if (nodeList.Count > 0)
                {
                    carEntity.Editors = new List<EditorDataEntity>(2);
                    XmlNode ele;
                    int userId;
                    foreach (XmlNode node in nodeList)
                    {
                        userId =
                            ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CommentUserId"), string.Empty));

                        ele = EditorUserDoc.SelectSingleNode(string.Format("//User[UserId='{0}']", userId));
                        if (ele == null)
                            continue;

                        carEntity.Editors.Add(new EditorDataEntity
                        {
                            Comment = GetNodeValue(node.SelectSingleNode("Comment"), string.Empty),
                            UserId = userId,
                            UserName = GetNodeValue(node.SelectSingleNode("CommentUserName"), string.Empty),
                            CommentTime =
                                ConvertHelper.GetDateTime(GetNodeValue(node.SelectSingleNode("CommentTime"),
                                    string.Empty)),
                            UserImg = GetNodeValue(ele.SelectSingleNode("UserImageUrl"), string.Empty),
                            SquarePhoto =
                                GetNodeValue(ele.SelectSingleNode("SquarePhoto"),
                                    "http://img1.bitautoimg.com/images/not.gif"),
                            UserBlogUrl = GetNodeValue(ele.SelectSingleNode("UserBlogUrl"), string.Empty)
                        });
                        if (carEntity.Editors.Count == 2)
                            break;
                    }
                }
            }
            catch (Exception exp)
            {
                Log.WriteErrorLog("Get ServiceData Error (carId:" + carEntity.CarId + ";Message:" + exp.Message +
                                  ";StackTrace:" + exp.StackTrace + ")...");
            }
        }
        private string GetCarDataEntityValue(float pValue, string unit, string defStr)
        {
            var result = pValue > 0 ? string.Concat(pValue.ToString("#0.0#"), unit) : defStr;
            return result;
        }
        private void UpdateSerialPingceHtmlForH5V2(List<EditorDataEntity> list)
        {
            if (list.Count <= 0)
            {
                bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                          _serialId,
                          CommonHtmlEnum.TypeEnum.Serial,
                          CommonHtmlEnum.TagIdEnum.H5SerialSummary,
                          CommonHtmlEnum.BlockIdEnum.EditorComment);
                return;
            }

            EditorDataEntity editor = list[0];

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"contain\">");
            sb.Append("<div class=\"koubei\">");
            sb.AppendFormat("<div class=\"koubei_img\"><img src=\"{0}\" /></div>", editor.SquarePhoto);
            var userName = editor.UserName;
            if (editor.UserName.IndexOf('(') != -1)
                userName = userName.Substring(0, userName.IndexOf("("));
            sb.AppendFormat("       <ul><li><span>编辑：</span>{0}</li>", userName);
            sb.AppendFormat("        <li><span>车款：</span>{0}款 {1}</li></ul>", editor.CarEntity.YearType, editor.CarEntity.CarName);
            sb.Append("    </div>");
            sb.Append("    <div class=\"koubei_txt\"><p>");
            // sb.AppendFormat("    	<strong>编辑点评：</strong>{0}", editor.Comment);
            sb.AppendFormat("{0}", editor.Comment);
            sb.Append("    </p></div>");
            sb.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
            {
                ID = _serialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.H5SerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.EditorComment,
                HtmlContent = sb.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新H5编辑评论失败：serialId:" + _serialId);
        }


        //private void UpdateSerialPingceHtmlForH5(int csId)
        //{
        //	if (csId <= 0)
        //	{
        //		return;
        //	}
        //	var editors = new List<EditorDataEntity>();
        //	try
        //	{
        //		XmlDocument doc = new XmlDocument();
        //		doc.Load(string.Format(CommonData.CommonSettings.EditorCommentUrlNew, csId.ToString()));
        //		XmlNodeList nodeList = doc.SelectNodes("//CarEstimate");
        //		if (doc == null || nodeList.Count <= 0)
        //		{
        //			bool delSuccess = CommonHtmlService.DeleteCommonHtml(
        //			  csId,
        //			  CommonHtmlEnum.TypeEnum.Serial,
        //			  CommonHtmlEnum.TagIdEnum.H5SerialSummary,
        //			  CommonHtmlEnum.BlockIdEnum.EditorComment);
        //			return;
        //		}
        //		if (nodeList.Count > 0)
        //		{
        //			XmlNode ele;
        //			int userId;
        //			foreach (XmlNode node in nodeList)
        //			{
        //				userId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CommentUserId"), string.Empty));
        //				ele = EditorUserDoc.SelectSingleNode(string.Format("//User[UserId='{0}']", userId.ToString()));
        //				if (ele == null)
        //				{
        //					continue;
        //				}

        //				editors.Add(new EditorDataEntity()
        //				{
        //					Comment = GetNodeValue(node.SelectSingleNode("Comment"), string.Empty),
        //					UserId = userId,
        //					UserName = GetNodeValue(node.SelectSingleNode("CommentUserName"), string.Empty),
        //					CommentTime = ConvertHelper.GetDateTime(GetNodeValue(node.SelectSingleNode("CommentTime"), string.Empty)),
        //					UserImg = GetNodeValue(ele.SelectSingleNode("UserPhotoPath"), string.Empty),
        //					SquarePhoto = GetNodeValue(ele.SelectSingleNode("SquarePhoto"), "http://img1.bitautoimg.com/images/not.gif"),
        //					UserBlogUrl = GetNodeValue(ele.SelectSingleNode("UserBlogUrl"), string.Empty)
        //				});
        //				if (editors.Count > 1)
        //				{
        //					break;
        //				}
        //				int carId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CarId"), string.Empty));
        //				if (carId <= 0)
        //				{
        //					continue;
        //				}
        //				var content = GetHtmlContent(editors.FirstOrDefault(), carId);
        //				bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
        //				{
        //					ID = csId,
        //					TypeID = CommonHtmlEnum.TypeEnum.Serial,
        //					TagID = CommonHtmlEnum.TagIdEnum.H5SerialSummary,
        //					BlockID = CommonHtmlEnum.BlockIdEnum.EditorComment,
        //					HtmlContent = content,
        //					UpdateTime = DateTime.Now
        //				});
        //				if (!success) Log.WriteErrorLog("更新H5编辑评论失败：carId:" + carId);
        //			}
        //		}
        //	}
        //	catch (Exception exp)
        //	{
        //		Log.WriteErrorLog("Get ServiceData Error (csId:" + csId.ToString() + ";Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...");
        //	}

        //}

        /// <summary>
        /// 编辑点评 1200版
        /// </summary>
        /// <param name="editorList"></param>
        private void UpdateSerialEditComment(List<EditorDataEntity> editorList)
        {
            try
            {
                if (editorList.Count <= 0)
                {
                    bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                          _serialId,
                          CommonHtmlEnum.TypeEnum.Serial,
                          CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                          CommonHtmlEnum.BlockIdEnum.EditorComment);
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("<div class=\"row\" data-channelid=\"2.21.1519\">");
                foreach (EditorDataEntity editor in editorList)
                {
                    var userName = editor.UserName;
                    if (editor.UserName.IndexOf('(') != -1)
                        userName = userName.Substring(0, userName.IndexOf("("));
                    sb.Append("<div class=\"special-layout-7\">");
                    sb.Append("    <div class=\"left-box\">");
                    sb.AppendFormat("        <a class=\"figure\" href=\"{0}\" target=\"_blank\">", editor.UserBlogUrl);
                    sb.Append("            <span class=\"img\">");
                    sb.AppendFormat("                <img src=\"{0}\" />", editor.SquarePhoto);
                    sb.Append("            </span>");
                    sb.AppendFormat("            <h5>{0}</h5>", userName);
                    sb.AppendFormat("            <p class=\"job\">易车网评测编辑</p>");
                    sb.Append("        </a>");
                    sb.Append("    </div>");
                    sb.Append("    <div class=\"right-box\">");
                    sb.AppendFormat("        <div class=\"title\">评：<a href=\"/{0}/m{1}/\" target=\"_blank\" class=\"link\">{2}款 {3}</a></div>"
                        , _serialInfo.AllSpell
                        , editor.CarEntity.CarId
                        , editor.CarEntity.YearType
                        , editor.CarEntity.CarName);
                    sb.AppendFormat("        <p class=\"details\">{0}</p>", editor.Comment);
                    sb.Append("    </div>");
                    sb.Append("</div>");
                }
                sb.Append("</div>");
                bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
                {
                    ID = _serialId,
                    TypeID = CommonHtmlEnum.TypeEnum.Serial,
                    TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                    BlockID = CommonHtmlEnum.BlockIdEnum.EditorComment,
                    HtmlContent = sb.ToString(),
                    UpdateTime = DateTime.Now
                });
                if (!success) Log.WriteErrorLog("更新PC编辑评论失败：serialid:" + _serialId);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

        private void UpdateSerialPingceHtmlForNew(List<EditorDataEntity> editorList)
        {
            try
            {
                if (editorList.Count <= 0)
                {
                    bool delSuccess = CommonHtmlService.DeleteCommonHtml(
                          _serialId,
                          CommonHtmlEnum.TypeEnum.Serial,
                          CommonHtmlEnum.TagIdEnum.SerialSummary,
                          CommonHtmlEnum.BlockIdEnum.EditorComment);
                    return;
                }

                StringBuilder sb = new StringBuilder();

                foreach (EditorDataEntity editor in editorList)
                {
                    sb.Append("<div class=\"car_grade\">");
                    sb.Append("	<dl>");
                    sb.AppendFormat("    	<dt><a href=\"{0}\" target=\"_blank\"><img src=\"{1}\" width=\"60\" height=\"60\"/></a></dt>", editor.UserBlogUrl, editor.SquarePhoto);
                    var userName = editor.UserName;
                    if (editor.UserName.IndexOf('(') != -1)
                        userName = userName.Substring(0, userName.IndexOf("("));
                    sb.AppendFormat("        <dd class=\"car_grade_n\"><label>编辑：</label>{0}</dd>", userName);
                    sb.AppendFormat("        <dd><label>车款：</label>{0}款 {1}</dd>", editor.CarEntity.YearType, editor.CarEntity.CarName);
                    sb.AppendFormat("        <dd class=\"car_grade_gearbox\"><label>变速箱：</label>{0}{1}</dd>",
                        editor.CarEntity.UnderPan_ForwardGearNum > 0 ? editor.CarEntity.UnderPan_ForwardGearNum + "速" : "",
                        editor.CarEntity.UnderPan_TransmissionType);
                    sb.Append("    </dl>");
                    sb.Append("    <p>");
                    sb.AppendFormat("    	<strong>编辑点评：</strong>{0}", editor.Comment);
                    sb.Append("    </p>");
                    sb.Append("    <div class=\"clear\"></div>");
                    sb.Append("</div>");
                }
                bool success = CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
                {
                    ID = _serialId,
                    TypeID = CommonHtmlEnum.TypeEnum.Serial,
                    TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                    BlockID = CommonHtmlEnum.BlockIdEnum.EditorComment,
                    HtmlContent = sb.ToString(),
                    UpdateTime = DateTime.Now
                });
                if (!success) Log.WriteErrorLog("更新PC编辑评论失败：serialid:" + _serialId);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        /// 编辑点评 1200版
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        private List<EditorDataEntity> GetEditorDataNew(int top = 2)
        {
            List<EditorDataEntity> editorList = new List<EditorDataEntity>();
            Dictionary<int, CarInfoEntity> dictCar = CarRepository.GetNewYearCarData(_serialId);
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(string.Format(CommonData.CommonSettings.EditorCommentUrlNew, _serialId));
                XmlNodeList nodeList = doc.SelectNodes("//CarEstimate");
                int count = 0;
                foreach (XmlNode node in nodeList)
                {
                    if (count == top) break;
                    int userId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CommentUserId"), string.Empty));
                    int carId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CarId"), string.Empty));
                    XmlNode ele = EditorUserDoc.SelectSingleNode(string.Format("//User[UserId='{0}']", userId.ToString()));

                    if (carId <= 0 || !dictCar.ContainsKey(carId) || ele == null)
                    {
                        continue;
                    }
                    editorList.Add(new EditorDataEntity()
                    {
                        Comment = GetNodeValue(node.SelectSingleNode("Comment"), string.Empty),
                        UserId = userId,
                        UserName = GetNodeValue(node.SelectSingleNode("CommentUserName"), string.Empty),
                        CommentTime = ConvertHelper.GetDateTime(GetNodeValue(node.SelectSingleNode("CommentTime"), string.Empty)),
                        UserImg = GetNodeValue(ele.SelectSingleNode("UserPhotoPath"), string.Empty),
                        SquarePhoto = GetNodeValue(ele.SelectSingleNode("SquarePhoto"), "http://img1.bitautoimg.com/images/not.gif"),
                        UserBlogUrl = GetNodeValue(ele.SelectSingleNode("UserBlogUrl"), string.Empty),
                        CarEntity = dictCar[carId]
                    });
                    count++;
                }
                //点评时间 倒序
                editorList.Sort((a, b) => b.CommentTime.CompareTo(a.CommentTime));
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }

            return editorList;
        }

        private List<EditorDataEntity> GetEditorData(int top = 2)
        {
            List<EditorDataEntity> editorList = new List<EditorDataEntity>();

            Dictionary<int, CarInfoEntity> dictCar = CarRepository.GetNewYearCarData(_serialId);
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(string.Format(CommonData.CommonSettings.EditorCommentUrlNew, _serialId));
                XmlNodeList nodeList = doc.SelectNodes("//CarEstimate");
                foreach (XmlNode node in nodeList)
                {
                    int userId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CommentUserId"), string.Empty));
                    int carId = ConvertHelper.GetInteger(GetNodeValue(node.SelectSingleNode("CarId"), string.Empty));
                    XmlNode ele = EditorUserDoc.SelectSingleNode(string.Format("//User[UserId='{0}']", userId.ToString()));

                    if (carId <= 0 || ele == null)
                    {
                        continue;
                    }

                    if (!dictCar.ContainsKey(carId)) continue;

                    editorList.Add(new EditorDataEntity()
                    {
                        Comment = GetNodeValue(node.SelectSingleNode("Comment"), string.Empty),
                        UserId = userId,
                        UserName = GetNodeValue(node.SelectSingleNode("CommentUserName"), string.Empty),
                        CommentTime = ConvertHelper.GetDateTime(GetNodeValue(node.SelectSingleNode("CommentTime"), string.Empty)),
                        UserImg = GetNodeValue(ele.SelectSingleNode("UserPhotoPath"), string.Empty),
                        SquarePhoto = GetNodeValue(ele.SelectSingleNode("SquarePhoto"), "http://img1.bitautoimg.com/images/not.gif"),
                        UserBlogUrl = GetNodeValue(ele.SelectSingleNode("UserBlogUrl"), string.Empty),
                        CarEntity = dictCar[carId]
                    });
                    //点评时间 倒序
                    editorList.Sort((a, b) => b.CommentTime.CompareTo(a.CommentTime));
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }

            return editorList.Take(top).ToList();
        }

        //private string GetHtmlContent(EditorDataEntity editor, int carId)
        //{
        //	if (carId <= 0)
        //	{
        //		return string.Empty;
        //	}

        //	DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, SelectParamString, new SqlParameter("@carid", carId));
        //	if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
        //	{
        //		return string.Empty;
        //	}
        //	DataTable table = ds.Tables[0];
        //	DataRow dataRow = table.Rows[0];
        //	var yearType = ConvertHelper.GetInteger(dataRow["Car_YearType"]);
        //	var carName = ConvertHelper.GetString(dataRow["Car_Name"]);

        //	StringBuilder sb = new StringBuilder();
        //	sb.Append("<div class=\"contain\">");
        //	sb.Append("<div class=\"koubei\">");
        //	sb.AppendFormat("<div class=\"koubei_img\"><img src=\"{0}\" /></div>", editor.SquarePhoto);
        //	var userName = editor.UserName;
        //	if (editor.UserName.IndexOf('(') != -1)
        //		userName = userName.Substring(0, userName.IndexOf("("));
        //	sb.AppendFormat("       <ul><li><span>编辑：</span>{0}</li>", userName);
        //	sb.AppendFormat("        <li><span>车款：</span>{0}款 {1}</li></ul>", yearType, carName);
        //	sb.Append("    </div>");
        //	sb.Append("    <div class=\"koubei_txt\"><p>");
        //	// sb.AppendFormat("    	<strong>编辑点评：</strong>{0}", editor.Comment);
        //	sb.AppendFormat("{0}", editor.Comment);
        //	sb.Append("    </p></div>");
        //	sb.Append("</div>");
        //	return sb.ToString();
        //}

        string GetNodeValue(XmlNode node, string defaultStr)
        {
            return node != null && !string.IsNullOrEmpty(node.InnerText.Trim()) ? node.InnerText.Trim() : defaultStr;
        }

        #region 数据对象
        public class EditorDataEntity
        {
            /// <summary>
            ///     评测内容
            /// </summary>
            public string Comment;

            /// <summary>
            ///     评测时间
            /// </summary>
            public DateTime CommentTime;

            /// <summary>
            ///     编辑博客
            /// </summary>
            public string UserBlogUrl;

            /// <summary>
            /// 编辑ID
            /// </summary>
            public int UserId;

            /// <summary>
            /// 编辑图片
            /// </summary>
            public string UserImg;

            /// <summary>
            /// 编辑名称
            /// </summary>
            public string UserName;

            /// <summary>
            /// 编辑方图
            /// </summary>
            public string SquarePhoto { get; set; }
            
            public CarInfoEntity CarEntity { get; set; }
        }

        public class CarDataEntity
        {
            /// <summary>
            ///     制动距离（100—0km/h）
            /// </summary>
            public float BrakingDistance;

            /// <summary>
            ///     车型ID
            /// </summary>
            public int CarId;

            /// <summary>
            ///     车款名称
            /// </summary>
            public string CarName;

            /// <summary>
            ///     新闻内容简介
            /// </summary>
            public string Content;

            public List<EditorDataEntity> Editors;

            /// <summary>
            ///     加速时间（0—100km/h）
            /// </summary>
            public float MeasuredAcceleration;

            /// <summary>
            ///     车内等速（60km/h）噪音
            /// </summary>
            public float MeasuredAveSpeedNoise;

            /// <summary>
            ///     车内等速（100km/h）噪音
            /// </summary>
            public float MeasuredAveSpeedNoise100;

            /// <summary>
            ///     车内等速（120km/h）噪音
            /// </summary>
            public float MeasuredAveSpeedNoise120;

            /// <summary>
            ///     车内等速（40km/h）噪音
            /// </summary>
            public float MeasuredAveSpeedNoise40;

            /// <summary>
            ///     车内等速（80km/h）噪音
            /// </summary>
            public float MeasuredAveSpeedNoise80;

            /// <summary>
            ///     油耗
            /// </summary>
            public float MeasuredFuel;

            /// <summary>
            ///     车内怠速噪音
            /// </summary>
            public float MeasuredSlackSpeedNoise;

            /// <summary>
            ///     新闻id
            /// </summary>
            public int NewsId;

            /// <summary>
            ///     新闻url
            /// </summary>
            public string NewsUrl;

            /// <summary>
            ///     Seo名称
            /// </summary>
            public string SerialSeoName;

            /// <summary>
            ///     年款 + 子品牌名称
            /// </summary>
            public string ShortName;

            /// <summary>
            ///     显示名称
            /// </summary>
            public string ShowName;

            /// <summary>
            ///     180米绕桩速度
            /// </summary>
            public float Slalomspeed180;

            /// <summary>
            ///     档位个数
            /// </summary>
            public int UnderPan_ForwardGearNum;

            /// <summary>
            ///     变速箱类型
            /// </summary>
            public string UnderPan_TransmissionType;

            /// <summary>
            ///     年款
            /// </summary>
            public int YearType;
        }
        #endregion
    }
}

