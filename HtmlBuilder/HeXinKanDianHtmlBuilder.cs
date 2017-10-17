using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using System.IO;
using System.Xml;
using BitAuto.Utils;
using System.Data;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public class HeXinKanDianHtmlBuilder : BaseBuilder
    {
        #region 级别枚举、结构
        private enum CarLevelType
        {
            /// <summary>
            /// 未定义
            /// </summary>
            UnDefined,
            /// <summary>
            /// 321	微型车
            /// 338	小型车
            /// 339	紧凑型车
            /// 482	面包车
            /// 428	其它
            /// 483	皮卡
            /// </summary>
            Small,
            /// <summary>
            /// 340	中型车
            /// 341	中大型车
            /// 342	豪华车
            /// 426	跑车
            /// </summary>
            Big,
            SUV,
            MVP
        }
        private struct CarInfo
        {
            public string SerialName;
            //public string CarName;
            ////785
            //public double Engine_ExhaustForFloat;
            ////712
            //public string UnderPan_TransmissionType;
            public string ShowName;
            public string AllSpell;
        }
        #endregion

        private const string _Prefix = "carParam_";
        private const int _CmpareCarCount = 4;
        private SerialInfo _serialInfo;
        private int _car_id;
        //private string _car_Name; 
        //private double _engine_ExhaustForFloat;
        //private string _underPan_TransmissionType;
        private string _showName;
        private string _seoName;
        private CarLevelType _carLevelType = CarLevelType.UnDefined;
        private DataSet _dataSet;
        private DataTable _dataTable;
        private int _contentIndex = 0;
        private int _serialId;

        private Dictionary<int, CarInfo> _compareCarIds;

        /// <summary>
        /// key:<li class="current"><span>越野性能：</span>18°/20°/25.2°</li>
        /// value 显示的html代码
        /// </summary>
        private Dictionary<string, string> _htmlList;

        public override void BuilderDataOrHtml(int objId)
        {
            _serialId = objId;

            if (!CommonData.SerialDic.ContainsKey(_serialId)) { DeleteFile(); return; }

            _serialInfo = CommonData.SerialDic[_serialId];
            _seoName = _serialInfo.SeoName;
			#region del old by sk 2016.01.26
 			/* del by sk 2016.01.26
            GetCarLevel();
            if (_carLevelType == CarLevelType.UnDefined)
            { DeleteFile(); return; }

            _car_id = GetCarId(_serialInfo.Id);
            if (_car_id <= 0)
            { DeleteFile(); return; }

            GetCarInfo();

            LoadSerialParamValue(0);

            if (_dataSet == null || _dataSet.Tables.Count <= 0 || _dataSet.Tables[0].Rows.Count <= 0)
            { DeleteFile(); return; }

            _dataTable = _dataSet.Tables[0];

            if (!CheckData())
            { DeleteFile(); return; }

            GetCompareCars();

            LoadSerialParamValue();

            CreateHtml();
			*/
			#endregion
			new SerialHeXinReport().BuilderDataOrHtml(objId);
        }

        #region 生成html部分
        private void CreateHtml()
        {
            _htmlList = new Dictionary<string, string>();
            switch (_carLevelType)
            {
                case CarLevelType.Small:
                    CreateHtmlSmall();
                    break;
                case CarLevelType.Big:
                    CreateHtmlBig();
                    break;
                case CarLevelType.SUV:
                    CreateHtmlSUV();
                    break;
                case CarLevelType.MVP:
                    CreateHtmlMVP();
                    break;
            }
            if (_htmlList.Count < 3) { DeleteFile(); return; }

            try
            {
                string href = string.Format("/{0}/pingce/", _serialInfo.AllSpell);
                int news_id = GetNewsId();
                if (news_id != 0)
                {
                    href = string.Concat(href, "p", news_id.ToString(), "/");
                    //File.AppendAllText(@"e:\newsids.txt", _serialInfo.AllSpell + "\r\n");
                }

                StringBuilder builder = new StringBuilder(10000);
                builder.Append("<div class=\"line_box car_core\">");
                builder.AppendFormat("<h3><span>{0}-核心看点</span></h3>", _seoName);

                //左侧标签部分
                builder.Append("<ul id=\"car_core\">");
                foreach (string title in _htmlList.Keys)
                {
                    builder.Append(title);
                }
                builder.Append("</ul>");
                //右侧内容部分
                builder.Append("<div class=\"chartarea\">");
                foreach (string value in _htmlList.Values)
                {
                    builder.Append(value);
                }
                builder.Append("</div>");

                builder.AppendFormat("<div class=\"more\"><a target=\"_blank\" href=\"{0}\">试驾评测&gt;&gt;</a></div>", href);
                builder.Append("</div>");

                CommonFunction.SaveFileContent(builder.ToString()
                    , Path.Combine(CommonData.CommonSettings.SavePath, string.Format("SerialSet\\HeXinLianDianHtml\\Serial_{0}.html", _serialInfo.Id.ToString()))
                    , Encoding.UTF8);

                //File.AppendAllText(@"e:\核心看点_生成.txt", string.Format("{0} {1} {2}\r\n",_serialInfo.Id, _serialInfo.SeoName, _serialInfo.AllSpell));
            }
			catch (Exception ex) { DeleteFile(); Log.WriteErrorLog(ex); }
        }

        private int GetNewsId()
        {
            return ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
				"SELECT TOP 1 CmsNewsId FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND SerialId=@SerialId AND CarId=@CarId ORDER BY PublishTime DESC", new SqlParameter("@CarNewsTypeId", (int)CarNewsTypes.pingce), new SqlParameter("@SerialId", _serialId), new SqlParameter("@CarId", _car_id)));
        }
        private void CreateHtmlSmall()
        {
            CreateCommen();
            Create180RaoZhuangSuDu();
            CreateZhouJu();
            CreateHouPaiTuiBuKouJian();
            CreateXingLiKongJian();
        }
        private void CreateHtmlBig()
        {
            CreateCommen();
            Create180RaoZhuangSuDu();
            CreateZhouJu();
            CreateHouPaiTuiBuKouJian();
            CreateQuDongFangShi();
        }
        private void CreateHtmlSUV()
        {
            CreateCommen();
            CreateYueYeXingNeng();
            CreateZuiXiaoLiDiJianXi();
            CreateZuiDaSheShuiShenDu();
            CreateQuDongFangShi();
        }
        private void CreateHtmlMVP()
        {
            CreateCommen();
            CreateZhouJu();
            CreateHouPaiTuiBuKouJian();
            CreateDiSanPaiTuiBuKongJian();
            CreateHouBeiXiangKongJian();
        }
        #region 生成各项的html代码
        private void CreateCommen()
        {
            CreateJiaSu();
            CreateZhiDongJuLi();
            CreateCheNeiZaoYin();
        }
        #region 单一值
        /// <summary>
        /// 加速 Double
        /// </summary>
        private void CreateJiaSu()
        {
            CreateSingleDoubleHtml(786, "加速时间(0-100km/h)", "s");
        }
        /// <summary>
        /// 制动距离 Double
        /// </summary>
        private void CreateZhiDongJuLi()
        {
            CreateSingleDoubleHtml(787, "制动距离(100-0km/h)", "m");
        }
        /// <summary>
        /// 180米绕桩速度
        /// </summary>
        private void Create180RaoZhuangSuDu()
        {
            CreateSingleDoubleHtml(861, "180米绕桩速度", "km/h");
        }
        /// <summary>
        /// 轴距 Integer
        /// </summary>
        private void CreateZhouJu()
        {
            CreateSingleIntegerHtml(592, "轴距", "mm");
        }
        /// <summary>
        /// 行李箱容积 Double
        /// </summary>
        private void CreateXingLiKongJian()
        {
            CreateSingleDoubleHtml(465, "行李箱容积", "L");
        }
        /// <summary>
        /// 最小离地间隙 Double
        /// </summary>
        private void CreateZuiXiaoLiDiJianXi()
        {
            CreateSingleDoubleHtml(589, "最小离地间隙", "mm");
        }
        /// <summary>
        /// 最大涉水深度 Integer
        /// </summary>
        private void CreateZuiDaSheShuiShenDu()
        {
            CreateSingleIntegerHtml(662, "最大涉水深度", "mm");
        }
        /// <summary>
        /// 后排腿部空间 Double
        /// </summary>
        private void CreateHouPaiTuiBuKouJian()
        {
            CreateSingleDoubleHtml(888, "后排腿部空间", "mm");
        }
        /// <summary>
        /// 第三排腿部空间 Double
        /// </summary>
        private void CreateDiSanPaiTuiBuKongJian()
        {
            CreateSingleDoubleHtml(892, "第三排腿部空间", "mm");
        }
        private void CreateSingleIntegerHtml(int paramId, string title, string unit)
        {
            DataRow[] rows = _dataTable.Select("paramId=" + paramId.ToString());
            if (rows.Length <= 0)
                return;
            int currValue = ConvertHelper.GetInteger(rows[0]["pvalue"]);
            if (currValue <= 0)
                return;
            double maxValue = currValue;
            string key = string.Format("<li{0}><span>{1}：</span>{2} {3}</li>", _htmlList.Count <= 0 ? " class=\"current\"" : string.Empty
                , title, currValue.ToString(), unit);
            StringBuilder content = new StringBuilder();
            Dictionary<int, int> carValues = new Dictionary<int, int>();
            if (_compareCarIds != null && _compareCarIds.Count > 0)
            {
                foreach (int carId in _compareCarIds.Keys)
                {
                    if (!_dataSet.Tables.Contains(_Prefix + carId.ToString()))
                        continue;

                    DataTable table = _dataSet.Tables[_Prefix + carId.ToString()];
                    DataRow[] tmpRows = table.Select("paramId=" + paramId.ToString());
                    if (tmpRows.Length <= 0)
                        continue;
                    int tmpValue = ConvertHelper.GetInteger(tmpRows[0]["pvalue"]);
                    carValues.Add(carId, tmpValue);
                    if (tmpValue > maxValue)
                        maxValue = tmpValue;
                }
            }
            content.AppendFormat("<div id=\"car_core_con_{0}\"{1}>", _contentIndex++, _htmlList.Count > 0 ? " style=\"display: none;\"" : string.Empty);
            content.AppendFormat("<div class=\"tt\">{0}</div>", title);

            content.Append("<table cellspacing=\"0\" cellpadding=\"0\" width=\"100%\"><tbody>");

            content.Append("<tr>");
            content.AppendFormat("<td class=\"name\"><strong>{0}</strong></td>", _seoName);
            content.AppendFormat("<td class=\"sp\"><div class=\"w\"><div style=\"width:{0}%\" class=\"p\">&nbsp;{1}</div></div></td>", (int)((currValue / maxValue) * 100), _showName);
            content.AppendFormat("<td class=\"oil\"><span>{0}{1}</span></td>", currValue.ToString(), unit);
            content.Append("</tr>");

            int i = 0;
            foreach (KeyValuePair<int, int> keyValue in carValues)
            {
                CarInfo carInfo = _compareCarIds[keyValue.Key];
                content.Append("<tr>");
                content.AppendFormat("<td class=\"name\"><a href=\"/{1}/\" target=\"_blank\">{0}</a></td>", carInfo.SerialName, carInfo.AllSpell);
                content.AppendFormat("<td class=\"sp\"><div class=\"w\"><div style=\"width:{0}%\" class=\"p\">&nbsp;{1}</div></div></td>", (int)((keyValue.Value / maxValue) * 100), carInfo.ShowName);
                content.AppendFormat("<td class=\"oil\"><span>{0}{1}</span></td>", keyValue.Value.ToString(), unit);
                content.Append("</tr>");
                i++;
                if (i >= _CmpareCarCount)
                    break;
            }
            content.Append("</tbody></table>");
            content.Append("</div>");
            _htmlList.Add(key, content.ToString());
        }
        private void CreateSingleDoubleHtml(int paramId, string title, string unit)
        {
            DataRow[] rows = _dataTable.Select("paramId=" + paramId.ToString());
            if (rows.Length <= 0)
                return;
            double currValue = ConvertHelper.GetDouble(rows[0]["pvalue"]);
            if (currValue <= 0)
                return;
            double maxValue = currValue;
            string key = string.Format("<li{0}><span>{1}：</span>{2} {3}</li>", _htmlList.Count <= 0 ? " class=\"current\"" : string.Empty
                , title, currValue.ToString(), unit);
            StringBuilder content = new StringBuilder();
            Dictionary<int, double> carValues = new Dictionary<int, double>();
            if (_compareCarIds != null && _compareCarIds.Count > 0)
            {
                foreach (int carId in _compareCarIds.Keys)
                {
                    if (!_dataSet.Tables.Contains(_Prefix + carId.ToString()))
                        continue;

                    DataTable table = _dataSet.Tables[_Prefix + carId.ToString()];
                    DataRow[] tmpRows = table.Select("paramId=" + paramId.ToString());
                    if (tmpRows.Length <= 0)
                        continue;
                    double tmpValue = ConvertHelper.GetDouble(tmpRows[0]["pvalue"]);
                    carValues.Add(carId, tmpValue);
                    if (tmpValue > maxValue)
                        maxValue = tmpValue;
                }
            }

            content.AppendFormat("<div id=\"car_core_con_{0}\"{1}>", _contentIndex++, _htmlList.Count > 0 ? " style=\"display: none;\"" : string.Empty);
            content.AppendFormat("<div class=\"tt\">{0}</div>", title);

            content.AppendFormat("<table cellspacing=\"0\" cellpadding=\"0\" width=\"100%\"><tbody>");

            content.Append("<tr>");
            content.AppendFormat("<td class=\"name\"><strong>{0}</strong></td>", _seoName);
            content.AppendFormat("<td class=\"sp\"><div class=\"w\"><div style=\"width:{0}%\" class=\"p\">&nbsp;{1}</div></div></td>", (int)((currValue / maxValue) * 100), _showName);
            content.AppendFormat("<td class=\"oil\"><span>{0}{1}</span></td>", currValue.ToString(), unit);
            content.Append("</tr>");

            int i = 0;
            foreach (KeyValuePair<int, double> keyValue in carValues)
            {
                CarInfo carInfo = _compareCarIds[keyValue.Key];
                content.Append("<tr>");
                content.AppendFormat("<td class=\"name\"><a href=\"/{1}/\" target=\"_blank\">{0}</a></td>", carInfo.SerialName, carInfo.AllSpell);
                content.AppendFormat("<td class=\"sp\"><div class=\"w\"><div style=\"width:{0}%\" class=\"p\">&nbsp;{1}</div></div></td>", (int)((keyValue.Value / maxValue) * 100), carInfo.ShowName);
                content.AppendFormat("<td class=\"oil\"><span>{0}{1}</span></td>", keyValue.Value.ToString(), unit);
                content.Append("</tr>");
                i++;
                if (i >= _CmpareCarCount)
                    break;
            }
            content.Append("</tbody></table>");
            content.Append("</div>");
            _htmlList.Add(key, content.ToString());
        }
        #endregion

        #region 图片
        /// <summary>
        /// 驱动方式
        /// </summary>
        private void CreateQuDongFangShi()
        {
            string qudong = GetQuDongFangShi(_dataTable);
            if (string.IsNullOrEmpty(qudong))
                return;

            string key = string.Format("<li{0}><span>驱动方式：</span>{1}</li>", _htmlList.Count <= 0 ? " class=\"current\"" : string.Empty, qudong);
            StringBuilder content = new StringBuilder();
            content.AppendFormat("<div id=\"car_core_con_{0}\" class=\"drivemode\"{1}>", _contentIndex++, _htmlList.Count > 0 ? " style=\"display: none;\"" : string.Empty);
            content.Append("<div class=\"tt\">驱动方式</div>");

            content.Append("<dl>");
            content.AppendFormat("<dt><div><i></i><p><strong>{0}</strong></p></div></dt>", _seoName);
            content.AppendFormat("<dd><div class=\"{0}\"></div><p><em>{1}</em></p></dd>", GetQuDongFangShiClass(qudong), qudong);
            content.Append("</dl>");

            if (_compareCarIds != null && _compareCarIds.Count > 0)
            {
                int i = 0;
                foreach (KeyValuePair<int, CarInfo> keyValue in _compareCarIds)
                {
                    if (!_dataSet.Tables.Contains(_Prefix + keyValue.Key.ToString()))
                        continue;
                    string tmp = GetQuDongFangShi(_dataSet.Tables[_Prefix + keyValue.Key.ToString()]);
                    if (string.IsNullOrEmpty(tmp))
                        continue;

                    content.Append("<dl>");
                    content.AppendFormat("<dt><div><i></i><p><a href=\"/{1}/\" target=\"_blank\">{0}</a></p></div></dt>", keyValue.Value.SerialName, keyValue.Value.AllSpell);
                    content.AppendFormat("<dd><div class=\"{0}\"></div><p><em>{1}</em></p></dd>", GetQuDongFangShiClass(tmp), tmp);
                    content.Append("</dl>");
                    i++;
                    if (i >= _CmpareCarCount)
                        break;
                }
            }

            content.Append("</div>");

            _htmlList.Add(key, content.ToString());
        }
        private string GetQuDongFangShiClass(string qudong)
        {
            switch (qudong)
            {
                case "前置前驱":
                    return "dm_2";
                case "前置后驱":
                    return "dm_1";
                case "中置后驱":
                    return "dm_4";
                case "后置后驱":
                    return "dm_3";
                case "全时四驱":
                    return "dm_6";
                case "分时四驱":
                    return "dm_5";
                case "适时四驱":
                    return "dm_7";
            }
            return string.Empty;
        }
        private string GetQuDongFangShi(DataTable paramTable)
        {
            //全时四驱、分时四驱、适时四驱
            DataRow[] rows = paramTable.Select("paramId=655");
            if (rows.Length > 0)
            {
                string qudong = rows[0]["pvalue"].ToString();
                if (qudong == "全时四驱" || qudong == "分时四驱" || qudong == "适时四驱")
                {
                    return qudong;
                }
                else
                {
                    string weizhi;
                    rows = paramTable.Select("paramId=428");
                    if (rows.Length <= 0)
                        return null;

                    if (qudong.StartsWith("前"))
                        qudong = "前驱";
                    else if (qudong.StartsWith("后"))
                        qudong = "后驱";

                    weizhi = rows[0]["pvalue"].ToString();
                    if (string.IsNullOrEmpty(weizhi))
                        return null;

                    return weizhi + qudong;
                }
            }
            return null;
        }
        #endregion

        #region 多值
        /// <summary>
        /// 车内噪音 Double
        /// </summary>
        private void CreateCheNeiZaoYin()
        {
            Dictionary<int, string> paramValue = new Dictionary<int, string>() 
                    { 
                        { 789, "怠速" }, 
                        { 790, "60km等速" }
                    };
            if (_carLevelType == CarLevelType.Small)
            {
                paramValue.Add(859, "100km等速");
            }
            else
            {
                paramValue.Add(860, "120km等速");
            }

            CreateMultiDoubleHtml(paramValue, "车内噪音", "db");
        }
        /// <summary>
        /// 越野性能 Double
        /// </summary>
        private void CreateYueYeXingNeng()
        {
            CreateMultiDoubleHtml(new Dictionary<int, string>() 
                    { 
                        { 591, "接近角" }, 
                        { 581, "离去角" }, 
                        { 890, "通过角" } 
                    }
                , "越野性能", "°");
        }
        /// <summary>
        /// 后备箱空间 Double
        /// </summary>
        private void CreateHouBeiXiangKongJian()
        {
            CreateMultiDoubleHtml(new Dictionary<int, string>() 
                    { 
                        { 465, "标准容积" }, 
                        { 466, "拓展容积" } 
                    }
                , "后备箱空间", "L");
        }
        private void CreateMultiDoubleHtml(Dictionary<int, string> paramList, string title, string unit)
        {
            if (paramList == null || paramList.Count <= 0)
                return;

            string select = string.Empty;
            int i = 0;
            foreach (int key in paramList.Keys)
            {
                if (i == 0)
                {
                    select = "paramId=" + key.ToString();
                }
                else
                {
                    select = select + " or paramId=" + key.ToString();
                }
                i++;
            }
            DataRow[] rows = _dataTable.Select(select);
            if (rows.Length <= 0)
                return;
            //carid:[{pid,pvalue}]
            Dictionary<int, Dictionary<int, double>> dataDictionary = new Dictionary<int, Dictionary<int, double>>(5);
            Dictionary<int, double> paramIdValue = new Dictionary<int, double>(3);
            dataDictionary.Add(_car_id, paramIdValue);

            #region 获取最大值，并生成数据字典
            double tmpMaxValue = 0;
            foreach (DataRow row in rows)
            {
                double tmpValue = ConvertHelper.GetDouble(row["pvalue"]);
                if (tmpValue <= 0)
                    continue;
                int pId = ConvertHelper.GetInteger(row["paramId"]);
                paramIdValue.Add(pId, tmpValue);

                if (tmpValue > tmpMaxValue)
                    tmpMaxValue = tmpValue;
            }
            if (_compareCarIds != null && _compareCarIds.Count > 0)
            {
                i = 0;
                foreach (int carId in _compareCarIds.Keys)
                {
                    string tableName = _Prefix + carId.ToString();
                    if (!_dataSet.Tables.Contains(tableName))
                        continue;
                    DataTable tmpTable = _dataSet.Tables[tableName];
                    DataRow[] tmpRows = tmpTable.Select(select);
                    if (tmpRows.Length <= 0)
                        continue;

                    foreach (DataRow tmpRow in tmpRows)
                    {
                        double tmpValue = ConvertHelper.GetDouble(tmpRow["pvalue"]);
                        if (tmpValue <= 0)
                            continue;

                        if (!dataDictionary.ContainsKey(carId))
                        {
                            paramIdValue = new Dictionary<int, double>(3);
                            dataDictionary.Add(carId, paramIdValue);
                        }

                        int pId = ConvertHelper.GetInteger(tmpRow["paramId"]);
                        paramIdValue.Add(pId, tmpValue);

                        if (tmpValue > tmpMaxValue)
                            tmpMaxValue = tmpValue;
                    }
                    i++;
                    if (i >= _CmpareCarCount)
                        break;
                }
            }
            int maxValue = Convert.ToInt32(Math.Ceiling(tmpMaxValue));
            while (maxValue % 5 != 0)
            {
                maxValue++;
            }
            int baseNumber = maxValue / 5;
            #endregion

            if (dataDictionary.Count <= 0)
                return;

            string htmlKey = string.Format("<li{0}><span>{1}:</span>", _htmlList.Count <= 0 ? " class=\"current\"" : string.Empty, title);
            StringBuilder htmlValue = new StringBuilder(5000);
            htmlValue.AppendFormat("<div id=\"car_core_con_{0}\" class=\"car_vertical_table\"{1}>", _contentIndex++, _htmlList.Count <= 0 ? string.Empty : " style=\"display:none;\"");
            htmlValue.AppendFormat("<div class=\"tt\">{0}</div>", title);

            #region 左侧
            htmlValue.Append("<div class=\"rowLeft\">");
            for (i = 5; i >= 0; i--)
            {
                htmlValue.AppendFormat("<p>{0}{1}</p>", (baseNumber * i), unit);
            }
            htmlValue.Append("</div>");
            #endregion

            #region 中间部分
            htmlValue.Append("<div class=\"rowMid\">");
            foreach (KeyValuePair<int, string> param in paramList)
            {
                int index = 1;
                htmlValue.Append("<div class=\"rowitembox\">");
                htmlValue.Append("<div class=\"rowlinebox\">");
                htmlValue.Append("<div class=\"sub_rowlinebox\">");
                foreach (int carId in dataDictionary.Keys)
                {
                    Dictionary<int, double> paramValue = dataDictionary[carId];
                    if (index == 1)
                    {
                        if (paramValue.ContainsKey(param.Key))
                        {
                            htmlKey = htmlKey + paramValue[param.Key] + unit + "/";
                        }
                        else
                        {
                            htmlKey = htmlKey + "-" + "/";
                        }
                    }
                    if (paramValue.ContainsKey(param.Key))
                    {
                        string tmpTitle = string.Empty;
                        int per = (int)((paramValue[param.Key] / maxValue) * 100);
                        if (index == 1)
                        {
                            tmpTitle = string.Format("{0}{1}（{2}）", paramValue[param.Key], unit, _seoName);
                        }
                        else
                        {
                            tmpTitle = string.Format("{0}{1}（{2}）", paramValue[param.Key], unit, _compareCarIds[carId].SerialName);
                        }
                        htmlValue.AppendFormat("<p><span style=\"height:{0}%\" class=\"car_color_{1}\" title=\"{2}\"></span></p>", per, index, tmpTitle);
                    }
                    index++;
                }
                htmlValue.Append("</div>");
                htmlValue.Append("</div>");
                htmlValue.Append("<div class=\"clear\"></div>");
                htmlValue.AppendFormat("<strong>{0}</strong>", param.Value);
                htmlValue.Append("</div>");
            }
            htmlValue.Append("</div>");
            #endregion

            #region 右侧
            htmlValue.Append("<div class=\"rowRight\">");
            i = 1;
            foreach (int carId in dataDictionary.Keys)
            {
                if (i == 1)
                {
                    //<p title="1.4L AT"><span class="car_color_1"></span><strong>东风悦达起亚K5</strong></p>
                    htmlValue.AppendFormat("<p title=\"{2} {0}\"><span class=\"car_color_{1}\"></span><strong>{2}</strong></p>"
                        , _showName, i, _seoName);
                }
                else
                {
                    CarInfo carInfo = _compareCarIds[carId];
                    //<p title="1.4L AT"><span class="car_color_2"></span><b>智跑</b></p>
                    htmlValue.AppendFormat("<p title=\"{2} {0}\"><span class=\"car_color_{1}\"></span><b><a href=\"/{3}/\" target=\"_blank\">{2}</a></b></p>",
                        carInfo.ShowName, i, carInfo.SerialName, carInfo.AllSpell);
                }
                i++;
            }
            htmlValue.Append("</div>");
            #endregion

            htmlValue.Append("</div>");

            htmlKey = htmlKey.Remove(htmlKey.Length - 1, 1) + "</li>";

            _htmlList.Add(htmlKey, htmlValue.ToString());
        }
        #endregion
        #endregion
        #endregion

        #region 读数据
        private void LoadSerialParamValue(int carId)
        {
            string paramIds = string.Empty;
            switch (_carLevelType)
            {
                case CarLevelType.Small:
                    paramIds = "786, 787, 789, 790, 859, 861, 592, 889, 888, 465";
                    break;
                case CarLevelType.Big:
                    paramIds = "786, 787, 789, 790, 860, 861, 592, 889, 888, 428, 655";
                    break;
                case CarLevelType.SUV:
                    paramIds = "786, 787, 789, 790, 860, 591, 581, 890, 589, 662, 428, 655";
                    break;
                case CarLevelType.MVP:
                    paramIds = "786, 787, 789, 790, 860, 592, 889, 888, 892, 465, 466";
                    break;
            }
            if (string.IsNullOrEmpty(paramIds))
                return;

            string sql = string.Format("select ParamId, Pvalue from cardatabase where carid=@carid and Pvalue is not null and Pvalue <> '' and Pvalue <> '待查' and paramid in ({0})", paramIds);
            if (carId == 0)
                _dataSet = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text
                    , sql, new SqlParameter("@carid", _car_id));
            else
                SqlHelper.FillDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, _dataSet, new string[] { _Prefix + carId.ToString() }, new SqlParameter("@carid", carId));
        }
        private void LoadSerialParamValue()
        {
            if (_compareCarIds == null || _compareCarIds.Count <= 0)
                return;

            foreach (int carId in _compareCarIds.Keys)
            {
                LoadSerialParamValue(carId);
            }
        }
        /// <summary>
        /// 车型名称
        /// </summary>
        private void GetCarInfo()
        {
            //DataSet ds = SqlHelper.ExecuteDataset(HtmlBuilderData.AutoStroageConnString, CommandType.Text, "select Car_Name from Car_relation where car_id=@carid and isstate=0", new SqlParameter("@carid", _car_id));
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, "select ParamId, Pvalue from cardatabase where CarId=@carid and (paramId=785 or paramId=712 or paramId=425) and Pvalue is not null and Pvalue <> '' and Pvalue <> '待查'", new SqlParameter("@carid", _car_id));
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable table = ds.Tables[0];
                DataRow[] rows = table.Select("paramId=785");
                if (rows.Length > 0)
                {
                    DataRow[] tmpRows = table.Select("paramId=425");
                    _showName = ConvertHelper.GetDouble(rows[0]["Pvalue"]).ToString("0.0") + ((tmpRows.Length > 0 && tmpRows[0]["Pvalue"].ToString() == "增压") ? "T " : "L ");
                }
                rows = table.Select("paramId=712");
                if (rows.Length > 0)
                {
                    _showName = _showName + rows[0]["Pvalue"].ToString();
                }
            }
        }
        /// <summary>
        /// 车型级别
        /// </summary>
        private void GetCarLevel()
        {
            object obj = SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text
                , "select carlevel from Car_Serial where isstate=0 and cs_id=@cs_id", new SqlParameter("@cs_id", _serialInfo.Id));
            if (obj == null || obj is DBNull)
                return;

            int carlevel = ConvertHelper.GetInteger(obj);
            switch (carlevel)
            {
                case 321:
                case 338:
                case 339:
                case 482:
                case 428:
                case 483:
                    _carLevelType = CarLevelType.Small;
                    break;
                case 340:
                case 341:
                case 342:
                case 426:
                    _carLevelType = CarLevelType.Big;
                    break;
                case 424:
                    _carLevelType = CarLevelType.SUV;
                    break;
                case 425:
                    _carLevelType = CarLevelType.MVP;
                    break;
            }
        }
        #region GetCarId()
        private int GetCarId(int serialId)
        {
            string paramSql = string.Empty;
            switch (_carLevelType)
            {
                case CarLevelType.Small:
                    //paramSql = "786, 787, 789, 790, 859, 861, 592, 889, 888, 465";
                    paramSql = @"select 1 as num from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 786 
union all
select 2 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 787 
union all
select 3 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (789, 790, 859)
union all
select 4 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 861
union all
select 5 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 592
union all
select 6 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (889, 888)
union all
select 7 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 465";
                    break;
                case CarLevelType.Big:
                    //paramSql = "786, 787, 789, 790, 860, 861, 592, 889, 888, 428, 655";
                    paramSql = @"select 1 as num from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 786 
union all
select 2 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 787 
union all
select 3 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (789, 790, 860)
union all
select 4 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 861
union all
select 5 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 592
union all
select 6 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (889, 888)
union all
select 7 from cardatabase where carid=car.car_id and Pvalue is not null and Pvalue <> '' and Pvalue <> '待查' and paramid in (428, 655)";
                    break;
                case CarLevelType.SUV:
                    //paramSql = "786, 787, 789, 790, 860, 591, 581, 890, 589, 662, 428, 655";
                    paramSql = @"select 1 as num from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 786 
union all
select 2 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 787 
union all
select 3 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (789, 790, 860)
union all
select 4 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (591, 581, 890)
union all
select 5 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 589
union all
select 6 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 662
union all
select 7 from cardatabase where carid=car.car_id and Pvalue is not null and Pvalue <> '' and Pvalue <> '待查' and paramid in (428, 655)";
                    break;
                case CarLevelType.MVP:
                    //paramSql = "786, 787, 789, 790, 860, 592, 889, 888, 892, 465, 466";
                    paramSql = @"select 1 as num from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 786 
union all
select 2 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 787 
union all
select 3 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (789, 790, 860)
union all
select 4 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 592
union all
select 5 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (889, 888)
union all
select 6 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid = 892
union all
select 7 from cardatabase where carid=car.car_id and Isnumeric(Pvalue)=1 and cast(pvalue as float) > 0 and paramid in (465, 466)";
                    break;
            }
            if (string.IsNullOrEmpty(paramSql))
                return 0;

            return ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text
                , string.Format(@"SELECT TOP 1 car_id
FROM   (
           SELECT car_id,
                  CASE 
                       WHEN EXISTS(SELECT 1 FROM cardatabase WHERE carid = car.car_id AND ISNUMERIC(Pvalue) = 1 AND CAST(pvalue AS FLOAT) > 0 AND paramid = 786)
						AND EXISTS(SELECT 1 FROM cardatabase WHERE carid = car.car_id AND ISNUMERIC(Pvalue) = 1 AND CAST(pvalue AS FLOAT) > 0 AND paramid = 787) 
					   THEN 1 ELSE 0 END AS num 
               FROM Car_relation AS car WHERE cs_id = @cs_id
           AND isstate = 0
           AND ((SELECT COUNT(DISTINCT num) FROM   ({0}) AS a) >= 3)
       ) a
ORDER BY num DESC, car_id DESC", paramSql), new SqlParameter("@cs_id", serialId)));
        }
        /*
                /// <summary>
                /// 当前车型id
                /// </summary>
                private void GetCarId()
                {
                    XmlDocument doc;
                    if (ExistsLoaclXmlDocument(Path.Combine(HtmlBuilderData.DataBlockPath, string.Format("SerialNews\\pingce\\Xml\\Serial_All_News_{0}.xml", _serialInfo.Id))
                        , out doc))
                    {
                        XmlNode node = doc.SelectSingleNode("root/listNews/carId");
                        if (node == null)
                            return;
                        _car_id = ConvertHelper.GetInteger(node.InnerText);
                        node = node.ParentNode.SelectSingleNode("newsid");
                        if (node == null)
                            _news_id = 0;
                        else
                            _news_id = ConvertHelper.GetInteger(node.InnerText);
                    }
                } 
        */

        #endregion
        /// <summary>
        /// 对比车型id
        /// </summary>
        private void GetCompareCars()
        {
            XmlDocument doc;
            if (ExistsLoaclXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format("SerialCityCompare\\{0}_CityCompare.xml", _serialInfo.Id))
                , out doc))
            {
                XmlNodeList nodeList = doc.SelectNodes("CityCompare/City[@ID=0]/Serial");
                if (nodeList != null && nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        //ID="1655" Name="黑金刚" ShowName="黑金刚"
                        if (node.Attributes["ID"] != null && !string.IsNullOrEmpty(node.Attributes["ID"].Value)
                            && node.Attributes["ShowName"] != null && !string.IsNullOrEmpty(node.Attributes["ShowName"].Value)
                            && node.Attributes["AllSpell"] != null && !string.IsNullOrEmpty(node.Attributes["AllSpell"].Value))
                        {
                            int csid = ConvertHelper.GetInteger(node.Attributes["ID"].Value);
                            if (csid <= 0)
                                continue;

                            int carId = GetCarId(csid);
                            if (carId <= 0)
                                continue;

                            if (_compareCarIds == null)
                                _compareCarIds = new Dictionary<int, CarInfo>();
                            if (_compareCarIds.ContainsKey(carId))
                                continue;

                            //DataSet ds = SqlHelper.ExecuteDataset(HtmlBuilderData.AutoStroageConnString, CommandType.Text, "select Car_Name from Car_relation where car_id=@carid and isstate=0", new SqlParameter("@carid", carId));
                            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, "select ParamId, Pvalue from cardatabase where CarId=@carid and (paramId=785 or paramId=712 or paramId=425) and Pvalue is not null and Pvalue <> '' and Pvalue <> '待查'", new SqlParameter("@carid", carId));
                            if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
                                continue;
                            DataTable table = ds.Tables[0];

                            CarInfo tmp = new CarInfo();
                            //tmp.CarName = ds.Tables[0].Rows[0]["Car_Name"].ToString();
                            tmp.SerialName = node.Attributes["ShowName"].Value;
                            tmp.ShowName = string.Empty;
                            DataRow[] rows = table.Select("paramId=785");
                            if (rows.Length > 0)
                            {
                                DataRow[] tmpRows = table.Select("paramId=425");
                                tmp.ShowName = ConvertHelper.GetDouble(rows[0]["Pvalue"]).ToString("0.0") + ((tmpRows.Length > 0 && tmpRows[0]["Pvalue"].ToString() == "增压") ? "T " : "L ");
                            }
                            rows = table.Select("paramId=712");
                            if (rows.Length > 0)
                            {
                                tmp.ShowName = tmp.ShowName + rows[0]["Pvalue"].ToString();
                            }
                            tmp.AllSpell = node.Attributes["AllSpell"].Value;

                            _compareCarIds.Add(carId, tmp);
                        }
                    }
                }
            }
        }
        #endregion

        #region 验证数据
        private bool CheckData()
        {
            switch (_carLevelType)
            {
                case CarLevelType.Small:
                    return CheckSmall();
                case CarLevelType.Big:
                    return CheckBig();
                case CarLevelType.SUV:
                    return CheckSUV();
                case CarLevelType.MVP:
                    return CheckMVP();
            }

            return false;
        }
        private bool CheckSmall()
        {
            int classCount = CheckCommen();
            if (classCount >= 3)
                return true;
            //4、180米绕桩速度：63.54km/h	
            //5、轴距：3321mm
            //6、后排腿部空间：899mm-2345mm	条图显示最大腿部空间数据
            //7、行李箱容积：523L
            if (Check180RaoZhuangSuDu())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckZhouJu())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckHouPaiTuiBuKouJian())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckXingLiKongJian())
                classCount++;
            if (classCount >= 3)
                return true;
            return false;
        }
        private bool CheckBig()
        {
            int classCount = CheckCommen();
            if (classCount >= 3)
                return true;
            //4、180米绕桩速度：
            //5、轴距：
            //6、后排腿部空间：
            //7、驱动方式：
            if (Check180RaoZhuangSuDu())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckZhouJu())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckHouPaiTuiBuKouJian())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckQuDongFangShi())
                classCount++;
            if (classCount >= 3)
                return true;

            return false;
        }
        private bool CheckSUV()
        {
            int classCount = CheckCommen();
            if (classCount >= 3)
                return true;

            //4、越野性能：18°/20°/25.2°	详细信息显示：接近角 通过角 离去角
            //5、最小离地间隙：2222mm
            //6、最大涉水深度：2222mm
            //7、驱动方式：
            if (CheckYueYeXingNeng())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckZuiXiaoLiDiJianXi())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckZuiDaSheShuiShenDu())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckQuDongFangShi())
                classCount++;
            if (classCount >= 3)
                return true;
            return false;
        }
        private bool CheckMVP()
        {
            int classCount = CheckCommen();
            if (classCount >= 3)
                return true;

            //4、轴距：
            //5、第二排腿部空间：899mm-2345mm	条图显示最大腿部空间数据
            //6、第三排腿部空间： 2345mm	
            //7、后备箱空间：最小/最大      

            if (CheckZhouJu())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckHouPaiTuiBuKouJian())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckDiSanPaiTuiBuKongJian())
                classCount++;
            if (classCount >= 3)
                return true;

            if (CheckHouBeiXiangKongJian())
                classCount++;
            if (classCount >= 3)
                return true;

            return false;
        }
        #region 验证具体参数项
        private int CheckCommen()
        {
            int count = 0;
            if (CheckJiaSu())
                count++;
            if (CheckZhiDongJuLi())
                count++;
            if (CheckCheNeiZaoYin())
                count++;
            return count;
        }
        /// <summary>
        /// 加速 Double
        /// </summary>
        private bool CheckJiaSu()
        {
            return CheckDataDouble(_dataTable.Select("paramId=786"));
        }
        /// <summary>
        /// 制动距离 Double
        /// </summary>
        private bool CheckZhiDongJuLi()
        {
            return CheckDataDouble(_dataTable.Select("paramId=787"));
        }
        /// <summary>
        /// 车内噪音 Double
        /// </summary>
        private bool CheckCheNeiZaoYin()
        {
            DataRow[] rows = null;
            if (_carLevelType == CarLevelType.Small)
                rows = _dataTable.Select("paramId=789 or paramId=790 or paramId=859");
            else
                rows = _dataTable.Select("paramId=789 or paramId=790 or paramId=860");
            return CheckDataDouble(rows);
        }
        /// <summary>
        /// 180米绕桩速度 Double
        /// </summary>
        private bool Check180RaoZhuangSuDu()
        {
            return CheckDataDouble(_dataTable.Select("paramId=861"));
        }
        /// <summary>
        /// 轴距 Integer
        /// </summary>
        private bool CheckZhouJu()
        {
            return CheckDataInteger(_dataTable.Select("paramId=592"));
        }
        /// <summary>
        /// 后排腿部空间 Double
        /// </summary>
        private bool CheckHouPaiTuiBuKouJian()
        {
            return CheckDataDouble(_dataTable.Select("paramId=888"));
        }
        /// <summary>
        /// 行李箱容积 Double
        /// </summary>
        private bool CheckXingLiKongJian()
        {
            return CheckDataDouble(_dataTable.Select("paramId=465"));
        }
        /// <summary>
        /// 驱动方式
        /// </summary>
        private bool CheckQuDongFangShi()
        {
            //全时四驱、分时四驱、适时四驱
            DataRow[] rows = _dataTable.Select("paramId=655");
            if (rows.Length > 0)
            {
                string qudong = rows[0]["pvalue"].ToString();
                if (qudong == "全时四驱" || qudong == "分时四驱" || qudong == "适时四驱")
                    return true;
                else
                {
                    if (qudong.StartsWith("前") || qudong.StartsWith("后"))
                    {
                        rows = _dataTable.Select("paramId=428");
                        if (rows.Length > 0)
                        {
                            switch (rows[0]["pvalue"].ToString())
                            {
                                case "前置":
                                case "中置":
                                case "后置":
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 越野性能 Double
        /// </summary>
        private bool CheckYueYeXingNeng()
        {
            return CheckDataDouble(_dataTable.Select("paramId=591 or paramId=581 or paramId=890")); ;
        }
        /// <summary>
        /// 最小离地间隙 Double
        /// </summary>
        private bool CheckZuiXiaoLiDiJianXi()
        {
            return CheckDataDouble(_dataTable.Select("paramId=589"));
        }
        /// <summary>
        /// 最大涉水深度 Integer
        /// </summary>
        private bool CheckZuiDaSheShuiShenDu()
        {
            return CheckDataInteger(_dataTable.Select("paramId=662"));
        }
        /// <summary>
        /// 第三排腿部空间 Double
        /// </summary>
        private bool CheckDiSanPaiTuiBuKongJian()
        {
            return CheckDataDouble(_dataTable.Select("paramId=892"));
        }
        /// <summary>
        /// 后备箱空间 Double
        /// </summary>
        private bool CheckHouBeiXiangKongJian()
        {
            return CheckDataDouble(_dataTable.Select("paramId=465 or paramId=466"));
        }
        private bool CheckDataDouble(DataRow[] rows)
        {
            if (rows == null || rows.Length <= 0)
                return false;
            if (rows.Length == 1)
            {
                return (ConvertHelper.GetDouble(rows[0]["pvalue"]) > 0);
            }
            else
            {
                foreach (DataRow row in rows)
                {
                    if (ConvertHelper.GetDouble(rows[0]["pvalue"]) > 0)
                        return true;
                }
                return false;
            }
        }
        private bool CheckDataInteger(DataRow[] rows)
        {
            if (rows == null || rows.Length <= 0)
                return false;
            if (rows.Length == 1)
            {
                return (ConvertHelper.GetInteger(rows[0]["pvalue"]) > 0);
            }
            else
            {
                foreach (DataRow row in rows)
                {
                    if (ConvertHelper.GetInteger(rows[0]["pvalue"]) > 0)
                        return true;
                }
                return false;
            }
        }
        #endregion
        #endregion

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

        private void DeleteFile()
        {
            if (_serialId <= 0)
                return;
            try
            {
                string filePath = Path.Combine(CommonData.CommonSettings.SavePath, string.Format("SerialSet\\HeXinLianDianHtml\\Serial_{0}.html", _serialId));
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }
    }
}
