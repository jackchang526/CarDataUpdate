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
    public class SerialHeXinReport : BaseBuilder
    {
        private Dictionary<int, SerialNewCarInfo> _compareSerialIds;
        private SerialNewCarInfo currentSerialCarInfo;
        private StringBuilder sbHtml = new StringBuilder();

        private int SerialId = 0;
        private SerialInfo _serialInfo;
        private XmlDocument xmlParamsStandard;

        public SerialHeXinReport()
        {
            xmlParamsStandard = this.GetDataStandard();
        }

        public override void BuilderDataOrHtml(int objId)
        {
            SerialId = objId;
            try
            {
                //add by 2017.04.26 
                if (!CommonData.SerialDic.ContainsKey(objId))
                {
                    Log.WriteErrorLog(string.Format("子品牌Id不存在，id={0}，func：SerialHeXinReport", objId));
                    return;
                }
                _serialInfo = CommonData.SerialDic[objId];
                int carId = GetCarId(objId);

                if (carId > 0)
                {
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
                    //CreateHtml(carId);
                }
                /*
			else
			{
				CommonHtmlService.DeleteCommonHtml(
				SerialId,
				CommonHtmlEnum.TypeEnum.Serial,
				CommonHtmlEnum.TagIdEnum.SerialSummary,
				CommonHtmlEnum.BlockIdEnum.HexinReport);
			}
			*/
                //GetCompareCars(objId);
                CreateHtmlNew(carId);//1200版核心报告
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

        #region 子品牌综述页核心报告 1200版 2016-10-12 lsf
        /// <summary>
        /// 子品牌综述页核心报告 1200版 2016-10-12 lsf
        /// </summary>
        /// <param name="carId"></param>
        private void CreateHtmlNew(int carId)
        {
            Dictionary<int, string> serialWhilePic = CommonData.dicSerialNewPhoto;// CommonData.GetAllSerialPicURL(true);
            string youHaoHtml = string.Empty;
            string dongLiHtml = string.Empty;
            string zhiDongHtml = string.Empty;
            string kongJianHtml = string.Empty;
            string tongGuoXingHtml = string.Empty;
            bool isShow = true;
            if (carId > 0)
            {
                DataSet ds = GetCarInfo(carId);
                Dictionary<int, PingCeTagEntity> dicAllTagInfo = CarPingceInfoService.GetPingceTagsBySerialId(SerialId);
                string[] tagNameArr = { "外观", "内饰", "空间", "动力", "操控", "油耗", "配置", "总结" };
                //取标签的页码
                Dictionary<string, int> dictTagPageNumber = new Dictionary<string, int>();
                int tempPageNum = 0;
                foreach (KeyValuePair<int, PingCeTagEntity> kvp in dicAllTagInfo)
                {
                    tempPageNum++;
                    dictTagPageNumber.Add(kvp.Value.tagName, tempPageNum);
                }
                youHaoHtml = CreateYouHaoHtml(788, ds.Tables[0], serialWhilePic, dictTagPageNumber, ref isShow);
                dongLiHtml = CreateDongLiHtml(786, ds.Tables[0], serialWhilePic, dictTagPageNumber, ref isShow);
                zhiDongHtml = CreateZhiDongHtml(787, ds.Tables[0], serialWhilePic, dictTagPageNumber, ref isShow);
            }
            kongJianHtml = CreateKongJianHtml(SerialId, serialWhilePic, ref isShow);//kongjian
            tongGuoXingHtml = CreateTongGuoXingHtml(SerialId, serialWhilePic, ref isShow);//suv

            if (string.IsNullOrWhiteSpace(youHaoHtml)
                && string.IsNullOrWhiteSpace(dongLiHtml)
                && string.IsNullOrWhiteSpace(zhiDongHtml)
                && string.IsNullOrWhiteSpace(kongJianHtml)
                && string.IsNullOrWhiteSpace(tongGuoXingHtml))
            {
                CommonHtmlService.DeleteCommonHtml(
                    SerialId,
                    CommonHtmlEnum.TypeEnum.Serial,
                    CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                    CommonHtmlEnum.BlockIdEnum.HexinReport
                );
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"row main-box\" id=\"hexin-report\">");
            sb.Append("<div class=\"col-auto left\">");
            sb.Append("    <div class=\"tabs-left tabs-left-default2\">");
            sb.Append("        <ul>");
            bool isCurrent = true;
            if (!string.IsNullOrEmpty(youHaoHtml))
            {
                sb.Append("            <li class=\"current\" data=\"main-youhao\">油耗</li>");
                isCurrent = false;
            }
            if (!string.IsNullOrEmpty(kongJianHtml))
            {
                sb.AppendFormat("            <li data=\"main-kongjian\"{0}>空间</li>", isCurrent ? " class=\"current\"" : "");//
                isCurrent = false;
            }
            if (!string.IsNullOrEmpty(dongLiHtml))
            {
                sb.AppendFormat("            <li data=\"main-dongli\"{0}>动力</li>", isCurrent ? " class=\"current\"" : "");
                isCurrent = false;
            }
            if (!string.IsNullOrEmpty(zhiDongHtml))
            {
                sb.AppendFormat("            <li data=\"main-zhidong\"{0}>制动</li>", isCurrent ? " class=\"current\"" : "");
                isCurrent = false;
            }
            if (!string.IsNullOrEmpty(tongGuoXingHtml))
            {
                sb.AppendFormat("            <li data=\"main-tongguoxing\"{0}>通过性</li>", isCurrent ? " class=\"current\"" : ""); //suv
            }
            sb.Append("        </ul>");
            sb.AppendFormat("        <a class=\"btn btn-primary2\" href=\"/{0}/pingce/\" target=\"_blank\" data-channelid=\"2.21.1547\">查看详情</a>", _serialInfo.AllSpell);
            sb.Append("    </div>");
            sb.Append("</div>");
            sb.Append(youHaoHtml).Append(kongJianHtml).Append(dongLiHtml).Append(zhiDongHtml).Append(tongGuoXingHtml);
            sb.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
                BlockID = CommonHtmlEnum.BlockIdEnum.HexinReport,
                HtmlContent = sb.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新新版核心关键报告失败：serialId:" + SerialId);
        }

        private string CreateYouHaoHtml(int paramId, DataTable dt, Dictionary<int, string> serialWhilePic, Dictionary<string, int> dictTagPageNumber, ref bool isShow)
        {
            StringBuilder sb = new StringBuilder();
            DataRow[] rows = dt.Select("paramId=" + paramId);
            if (rows.Length <= 0)
                return string.Empty;
            double currValue = ConvertHelper.GetDouble(rows[0]["pvalue"]);
            if (currValue <= 0)
                return string.Empty;


            var index = this.GetDataStandardIndex(_serialInfo.CarLevel, 788, currValue);
            string[] arrStandard = { "优秀", "良好", "中等", "较差", "很差" };
            string[] arrStandardDesc = { "节能之星", "经济节能", "一般般", "差强人意", "油老虎" };
            sb.AppendFormat("<div class=\"col-auto tab-main\" group=\"main-youhao\" data-channelid=\"2.21.1546\"{0}>", !isShow ? "  style=\"display:none;\"" : "");
            sb.Append("<div class=\"col-auto mid\"><div><div class=\"special-layout-15\" id=\"circleProgress-youhao\">");

            for (int i = 0; i < arrStandard.Length; i++)
            {
                sb.AppendFormat("<span class=\"info level-{0}{1}\">{2}</span>", (i + 1), i == index ? " active" : "", arrStandard[i]);
            }
            sb.Append("<div class=\"center-info\">");
            sb.AppendFormat("<h5>{0}</h5>", arrStandardDesc[index]);
            sb.AppendFormat("<p>{0}<span>L</span></p>", currValue);
            sb.Append("<div class=\"title\">综合油耗</div>");
            sb.Append("</div>");
            if (dictTagPageNumber.ContainsKey("油耗"))
            {
                sb.AppendFormat("<a class=\"btn btn-default\" target=\"_blank\" href=\"http://car.bitauto.com/{0}/pingce/{1}/\">查看油耗测试</a>", _serialInfo.AllSpell, dictTagPageNumber["油耗"]);
            }
            else
            {
                sb.Append("<a class=\"btn btn-default\" href=\"javascript:;\" target=\"_blank\" style=\"visibility: hidden;\">查看油耗测试</a>");
            }
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");
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
            if (list.Count > 0)
            {
                sb.Append("<div class=\"col-auto right\"><div><div class=\"special-layout-8\"><h5>竞品排名</h5><div class=\"list-txt list-txt-m list-txt-style-num\"><ul>");
                foreach (SerialNewCarInfo serialInfo in list)
                {
                    Dictionary<int, string> allParams = Common.Repository.CarRepository.GetCarAllParamByCarID(serialInfo.CarId);
                    sb.AppendFormat(" <li{8}><a class=\"figure\"target=\"_blank\" title=\"{6}款 {7}\" href=\"/{0}/\"><img src=\"{1}\"><h6>{2}</h6><p>{3} / {4}</p></a><span class=\"data\">{5}L</span></li>"
                        , serialInfo.SerialSpell
                        , serialWhilePic.ContainsKey(serialInfo.SerialId) ? serialWhilePic[serialInfo.SerialId].Replace("_2.", "_5.") : CommonData.CommonSettings.DefaultCarPic2
                        , serialInfo.SerialName
                        , allParams.ContainsKey(785) && !string.IsNullOrWhiteSpace(allParams[785]) ? (allParams[785] + (allParams.ContainsKey(425) && allParams[425] == "增压" ? "T" : "L")) : "暂无"
                        , allParams.ContainsKey(430) ? allParams[430] + "kw " : "暂无"
                        , serialInfo.Perf_MeasuredFuel
                        , serialInfo.Car_YearType
                        , serialInfo.CarName
                        , serialInfo.SerialId == SerialId ? " class=\"current\"" : "");
                }
                sb.Append("</ul></div></div></div></div>");
            }
            sb.Append("</div>");
            isShow = false;
            return sb.ToString();
        }

        private string CreateDongLiHtml(int paramId, DataTable dt, Dictionary<int, string> serialWhilePic, Dictionary<string, int> dictTagPageNumber, ref bool isShow)
        {
            StringBuilder sb = new StringBuilder();
            DataRow[] rows = dt.Select("paramId=" + paramId);
            if (rows.Length <= 0)
                return string.Empty;
            double currValue = ConvertHelper.GetDouble(rows[0]["pvalue"]);
            if (currValue <= 0)
                return string.Empty;

            string[] arrStandard = { "很差", "较差", "中等", "良好", "优秀" };
            string[] arrStandardDesc = { "弱爆了", "弱弱哒", "一般般", "轻松超车", "diǎo爆了" };
            var index = this.GetDataStandardIndex(_serialInfo.CarLevel, paramId, currValue);
            sb.AppendFormat("<div class=\"col-auto tab-main\" group=\"main-dongli\" data-channelid=\"2.21.1517\"{0}>", !isShow ? "  style=\"display:none;\"" : "");
            sb.Append("<div class=\"col-auto mid\"><div><div class=\"special-layout-15\" id=\"circleProgress-dongli\">");
            for (var i = 0; i < arrStandard.Length; i++)
            {
                sb.AppendFormat("<span class=\"info level-{0}{1}\">{2}</span>", (i + 1), i == index ? " active" : "", arrStandard[i]);
            }
            sb.AppendFormat("<div class=\"center-info\"><h5>{0}</h5><p>{1}<span>秒</span></p><div class=\"title\">0-100Km 加速时间</div></div>"
                , arrStandardDesc[index]
                , currValue);
            if (dictTagPageNumber.ContainsKey("动力"))
            {
                sb.AppendFormat("<a class=\"btn btn-default\" href=\"/{0}/pingce/{1}/\" target=\"_blank\">查看动力测试</a>", _serialInfo.AllSpell, dictTagPageNumber["动力"]);
            }
            else
            {
                sb.Append("<a class=\"btn btn-default\" href=\"javascript:;\" target=\"_blank\" style=\"visibility: hidden;\">查看动力测试</a>");
            }
            sb.Append("</div></div></div>");

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
            if (list.Count > 0)
            {
                sb.Append("<div class=\"col-auto right\"><div><div class=\"special-layout-8\"><h5>竞品排名</h5><div class=\"list-txt list-txt-m list-txt-style-num\"><ul>");
                foreach (SerialNewCarInfo serialInfo in list)
                {
                    Dictionary<int, string> allParams = Common.Repository.CarRepository.GetCarAllParamByCarID(serialInfo.CarId);
                    sb.AppendFormat(" <li{8}><a class=\"figure\"target=\"_blank\" title=\"{6}款 {7}\" href=\"/{0}/\"><img src=\"{1}\"><h6>{2}</h6><p>{3} / {4}</p></a><span class=\"data\">{5}s</span></li>"
                        , serialInfo.SerialSpell
                        , serialWhilePic.ContainsKey(serialInfo.SerialId) ? serialWhilePic[serialInfo.SerialId].Replace("_2.", "_5.") : CommonData.CommonSettings.DefaultCarPic2
                        , serialInfo.SerialName
                        , allParams.ContainsKey(785) && !string.IsNullOrWhiteSpace(allParams[785]) ? (allParams[785] + (allParams.ContainsKey(425) && allParams[425] == "增压" ? "T" : "L")) : "暂无"
                        , allParams.ContainsKey(430) ? allParams[430] + "kw " : "暂无"
                        , serialInfo.Perf_MeasuredAcceleration
                        , serialInfo.Car_YearType
                        , serialInfo.CarName
                        , serialInfo.SerialId == SerialId ? " class=\"current\"" : "");
                }
                sb.Append("</ul></div></div></div></div>");
            }
            sb.Append("</div>");
            isShow = false;
            return sb.ToString();
        }

        private string CreateZhiDongHtml(int paramId, DataTable dt, Dictionary<int, string> serialWhilePic, Dictionary<string, int> dictTagPageNumber, ref bool isShow)
        {
            StringBuilder sb = new StringBuilder();
            DataRow[] rows = dt.Select("paramId=" + paramId);
            if (rows.Length <= 0)
                return string.Empty;
            double currValue = ConvertHelper.GetDouble(rows[0]["pvalue"]);
            if (currValue <= 0)
                return string.Empty;

            string[] arrStandard = { "很差", "较差", "中等", "良好", "优秀" };
            string[] arrStandardDesc = { "刹车太肉", "刹车略肉", "一般般", "刹车灵敏", "秒刹" }; 
            var index = this.GetDataStandardIndex(_serialInfo.CarLevel, paramId, currValue);
            sb.AppendFormat("<div class=\"col-auto tab-main\" group=\"main-zhidong\" data-channelid=\"2.21.1518\"{0}>", !isShow ? "  style=\"display:none;\"" : "");
            sb.Append("<div class=\"col-auto mid\"><div><div class=\"special-layout-15\" id=\"circleProgress-zhidong\">");
            for (var i = 0; i < arrStandard.Length; i++)
            {
                sb.AppendFormat("<span class=\"info level-{0}{1}\">{2}</span>", (i + 1), i == index ? " active" : "", arrStandard[i]);
            }
            sb.AppendFormat("<div class=\"center-info\"><h5>{0}</h5><p>{1}<span>米</span></p><div class=\"title\">100Km-0 刹车距离</div></div>"
                , arrStandardDesc[index]
                , currValue);
            if (dictTagPageNumber.ContainsKey("操控"))
            {
                sb.AppendFormat("<a class=\"btn btn-default\" href=\"/{0}/pingce/{1}/\" target=\"_blank\">查看制动测试</a>", _serialInfo.AllSpell, dictTagPageNumber["操控"]);
            }
            else
            {
                sb.AppendFormat("<a class=\"btn btn-default\" href=\"javascript:;\" target=\"_blank\" style=\"visibility: hidden;\">查看制动测试</a>");
            }
            sb.Append("</div></div></div>");

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
            if (list.Count > 0)
            {
                sb.Append("<div class=\"col-auto right\"><div><div class=\"special-layout-8\"><h5>竞品排名</h5><div class=\"list-txt list-txt-m list-txt-style-num\"><ul>");
                foreach (SerialNewCarInfo serialInfo in list)
                {
                    Dictionary<int, string> allParams = Common.Repository.CarRepository.GetCarAllParamByCarID(serialInfo.CarId);
                    sb.AppendFormat(" <li{8}><a class=\"figure\"target=\"_blank\" title=\"{6}款 {7}\" href=\"/{0}/\"><img src=\"{1}\"><h6>{2}</h6><p>{3} / {4}</p></a><span class=\"data\">{5}m</span></li>"
                        , serialInfo.SerialSpell
                        , serialWhilePic.ContainsKey(serialInfo.SerialId) ? serialWhilePic[serialInfo.SerialId].Replace("_2.", "_5.") : CommonData.CommonSettings.DefaultCarPic2
                        , serialInfo.SerialName
                        , allParams.ContainsKey(785) && !string.IsNullOrWhiteSpace(allParams[785]) ? (allParams[785] + (allParams.ContainsKey(425) && allParams[425] == "增压" ? "T" : "L")) : "暂无"
                        , allParams.ContainsKey(430) ? allParams[430] + "kw " : "暂无"
                        , serialInfo.Perf_BrakingDistance
                        , serialInfo.Car_YearType
                        , serialInfo.CarName
                        , serialInfo.SerialId == SerialId ? " class=\"current\"" : "");
                }
                sb.Append("</ul></div></div></div></div>");
            }
            sb.Append("</div>");
            isShow = false;
            return sb.ToString();
        }

        private string CreateTongGuoXingHtml(int serialId, Dictionary<int, string> serialWhilePic, ref bool isShow)
        {
            SerialSUVPramatersHtmlBuilder suvBuilder = new SerialSUVPramatersHtmlBuilder();
            DataSet ds = suvBuilder.GetCar(serialId);
            if (ds.Tables[0].Rows.Count > 0)
            {
                HtmlBuilder.SerialNewCarInfo currentEntity = new HtmlBuilder.SerialNewCarInfo();
                currentEntity.SerialId = serialId;
                currentEntity.SerialName = _serialInfo.ShowName;
                currentEntity.SerialSpell = _serialInfo.AllSpell;
                currentEntity = suvBuilder.GetEntity(currentEntity, ds);

                var compareCarInfoList = suvBuilder.GetCompareCarInfo(serialId);
                //添加当前子品牌车款
                compareCarInfoList.Add(currentEntity);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<div class=\"col-auto tab-main\" group=\"main-tongguoxing\" id=\"tongguoxing\" data-channelid=\"2.21.815\"{0}>", !isShow ? "  style=\"display:none;\"" : "");
                sb.Append("<div class=\"col-auto mid\"><div><div class=\"special-layout-20\">");
                sb.Append("<ul class=\"list\">");
                sb.AppendFormat("<li class=\"tab current\"  data=\"tongguoxing\"><a href=\"javascript:;\"><div class=\"title\">通过性</div><h3 class=\"info\">{0}</h3></a></li>"
                    , GetTongGuoXingLevel("tongguoxing", currentEntity));
                sb.AppendFormat("<li class=\"tab\" data=\"lidijianxi\"><a href=\"javascript:;\"><div class=\"title\">离地间隙</div><h3 class=\"info\">{0}</h3></a></li>"
                    , GetTongGuoXingLevel("lidijianxi", currentEntity));
                sb.AppendFormat("<li class=\"tab\" data=\"sheshuishendu\"><a href=\"javascript:;\"><div class=\"title\">涉水深度</div><h3 class=\"info\">{0}</h3></a></li>"
                    , GetTongGuoXingLevel("sheshuishendu", currentEntity));
                sb.Append("</ul>");
                sb.Append("<div group=\"tongguoxing\"><div class=\"content\">");
                sb.Append("<div class=\"img\"><div class=\"pic-map\"><img src=\"http://image.bitauto.com/uimg/car/images2013/suv_pic_1.png\">");
                sb.AppendFormat("<span class=\"map-dot jiejinjiao\">{0}</span><span class=\"map-dot tongguojiao\">{1}</span><span class=\"map-dot liqujiao\">{2}</span>"
                    , currentEntity.OutSet_NearCorner > 0 ? currentEntity.OutSet_NearCorner + "°" : ""
                    , currentEntity.Perf_Throughtheangle > 0 ? currentEntity.Perf_Throughtheangle + "°" : ""
                    , currentEntity.OutSet_AwayCorner > 0 ? currentEntity.OutSet_AwayCorner + "°" : "");
                sb.Append("</div></div>");
                sb.AppendFormat("<h5 class=\"title\">{0}{1}{2}</h5>"
                    , currentEntity.OutSet_NearCorner > 0 ? string.Format("接近角{0}°", currentEntity.OutSet_NearCorner) : ""
                    , currentEntity.Perf_Throughtheangle > 0 ? string.Format(" 通过角{0}°", currentEntity.Perf_Throughtheangle) : ""
                    , currentEntity.OutSet_AwayCorner > 0 ? string.Format(" 离去角{0}°", currentEntity.OutSet_AwayCorner) : "");
                sb.Append("</div></div>");
                sb.Append("<div group=\"lidijianxi\" style=\"display:none;\"><div class=\"content\">");
                sb.Append("<div class=\"img\"><div class=\"pic-map\"><img src=\"http://image.bitauto.com/uimg/car/images2013/suv_pic_2.png\">");
                sb.AppendFormat("<span class=\"map-dot lidijianxi\">{0}</span>"
                    , currentEntity.OutSet_MinGapFromEarth > 0 ? currentEntity.OutSet_MinGapFromEarth + "mm" : "");
                sb.Append("</div></div>");
                sb.AppendFormat("<h5 class=\"title\">{0}</h5>", currentEntity.OutSet_MinGapFromEarth > 0 ? string.Format("最大离地间隙 {0}mm", currentEntity.OutSet_MinGapFromEarth) : string.Empty);
                sb.Append("</div></div>");
                sb.Append("<div group=\"sheshuishendu\" style=\"display:none;\"><div class=\"content\">");
                sb.Append("<div class=\"img\"><div class=\"pic-map\"><img src=\"http://image.bitauto.com/uimg/car/images2013/suv_pic_3.png\">");
                sb.AppendFormat("<span class=\"map-dot sheshuishendu\">{0}</span>"
                    , currentEntity.Perf_MaxPaddleDepth > 0 ? currentEntity.Perf_MaxPaddleDepth + "mm" : "");
                sb.Append("</div></div>");
                sb.AppendFormat("<h5 class=\"title\">{0}</h5>", currentEntity.Perf_MaxPaddleDepth > 0 ? string.Format("最大涉水深度 {0}mm", currentEntity.Perf_MaxPaddleDepth) : string.Empty);
                sb.Append("</div></div>");
                sb.Append("</div></div></div>");

                if (compareCarInfoList.Count > 0)
                {
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
                    sb.Append(GetCompareCarHtml(compareCarInfoList, "tongguoxing", serialWhilePic));
                    //离地间隙
                    compareCarInfoList.Sort((p1, p2) => p2.OutSet_MinGapFromEarth.CompareTo(p1.OutSet_MinGapFromEarth));
                    sb.Append(GetCompareCarHtml(compareCarInfoList, "lidijianxi", serialWhilePic));
                    //涉水深度
                    compareCarInfoList.Sort((p1, p2) => p2.Perf_MaxPaddleDepth.CompareTo(p1.Perf_MaxPaddleDepth));
                    sb.Append(GetCompareCarHtml(compareCarInfoList, "sheshuishendu", serialWhilePic));
                }
                sb.Append("</div>");
                return sb.ToString();
            }
            isShow = false;
            return string.Empty;
        }

        private string GetTongGuoXingLevel(string tagName, HtmlBuilder.SerialNewCarInfo currentEntity)
        {
            switch (tagName)
            {
                case "tongguoxing"://小于等于接近角25°，如果没有接近角，则判断离去角20° 
                    if ((currentEntity.OutSet_NearCorner > 0 && currentEntity.OutSet_NearCorner <= 25)
                        || (currentEntity.OutSet_NearCorner == 0 && currentEntity.OutSet_AwayCorner <= 20))
                    {
                        return "一般";
                    }
                    else if ((currentEntity.OutSet_NearCorner > 25 && currentEntity.OutSet_NearCorner < 35)
                        || (currentEntity.OutSet_NearCorner == 0 && currentEntity.OutSet_AwayCorner > 20 && currentEntity.OutSet_AwayCorner < 25))
                    {
                        return "良好";
                    }
                    else if (currentEntity.OutSet_NearCorner >= 35
                        || (currentEntity.OutSet_NearCorner == 0 && currentEntity.OutSet_AwayCorner > 25))
                    {
                        return "较高";
                    }
                    break;
                case "lidijianxi":
                    if (currentEntity.OutSet_MinGapFromEarth <= 190)
                    {
                        return "一般";
                    }
                    else if (currentEntity.OutSet_MinGapFromEarth > 190 && currentEntity.OutSet_MinGapFromEarth < 220)
                    {
                        return "良好";
                    }
                    else if (currentEntity.OutSet_MinGapFromEarth >= 220)
                    {
                        return "较高";
                    }
                    break;
                case "sheshuishendu":
                    if (currentEntity.Perf_MaxPaddleDepth <= 500)
                    {
                        return "一般";
                    }
                    else if (currentEntity.Perf_MaxPaddleDepth > 500 && currentEntity.Perf_MaxPaddleDepth < 700)
                    {
                        return "良好";
                    }
                    else if (currentEntity.Perf_MaxPaddleDepth >= 700)
                    {
                        return "较高";
                    }
                    break;
            }
            return string.Empty;
        }

        private string GetCompareCarHtml(List<HtmlBuilder.SerialNewCarInfo> compareCarInfoList, string type, Dictionary<int, string> serialWhilePic)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div class=\"col-auto right\" group=\"{0}\"{1}><div><div class=\"special-layout-8\"><h5>竞品排名</h5><div class=\"list-txt list-txt-m list-txt-style-num\"><ul>"
                    , type
                    , type == "tongguoxing" ? "" : " style=\"display:none;\"");
            foreach (HtmlBuilder.SerialNewCarInfo serialInfo in compareCarInfoList)
            {
                Dictionary<int, string> allParams = Common.Repository.CarRepository.GetCarAllParamByCarID(serialInfo.CarId);
                string value = string.Empty;
                if (type == "sheshuishendu")
                {
                    value = serialInfo.Perf_MaxPaddleDepth > 0 ? (serialInfo.Perf_MaxPaddleDepth.ToString() + "mm") : "暂无";
                }
                else if (type == "lidijianxi")
                {
                    value = serialInfo.OutSet_MinGapFromEarth > 0 ? (serialInfo.OutSet_MinGapFromEarth.ToString() + "mm") : "暂无";
                }
                else if (type == "tongguoxing")
                {
                    value = serialInfo.OutSet_NearCorner > 0 ? (serialInfo.OutSet_NearCorner.ToString() + "°") : "暂无";
                }
                sb.AppendFormat(" <li{8}><a class=\"figure\"target=\"_blank\" title=\"{6}款 {7}\" href=\"/{0}/\"><img src=\"{1}\"><h6>{2}</h6><p>{3} / {4}</p></a><span class=\"data\">{5}</span></li>"
                        , serialInfo.SerialSpell
                        , serialWhilePic.ContainsKey(serialInfo.SerialId) ? serialWhilePic[serialInfo.SerialId].Replace("_2.", "_5.") : CommonData.CommonSettings.DefaultCarPic2
                        , serialInfo.SerialName
                        , allParams.ContainsKey(785) && !string.IsNullOrWhiteSpace(allParams[785]) ? (allParams[785] + (allParams.ContainsKey(425) && allParams[425] == "增压" ? "T" : "L")) : "暂无"
                        , allParams.ContainsKey(430) ? allParams[430] + "kw " : "暂无"
                        , value
                        , serialInfo.Car_YearType
                        , serialInfo.CarName
                        , serialInfo.SerialId == SerialId ? " class=\"current\"" : "");
            }
            sb.Append("</ul></div></div></div></div>");
            return sb.ToString();
        }


        private string CreateKongJianHtml(int serialId, Dictionary<int, string> serialWhilePic, ref bool isShow)
        {
            List<SerialToAttention> serialToAttentions = CommonData.GetSerialToAttentionByCsID(serialId, 6);
            List<int> csIdList = GetCsIdList(serialId, serialToAttentions);
            //List<int> carIdList = GetCarIdList(csIdList);
            List<CarInnerSpaceInfo> list = SelectTargetList(serialId, csIdList).Take(5).ToList();
            List<int> carIdList = list.Select(i => i.CarId).ToList();
            if (list.Count == 0)
            {
                return string.Empty;
            }

            CarModelInfo firstSeatToTopModel = null; //前排头部模特信息
            CarModelInfo secondSeatToTopModel = null; //后排头部模特信息
            CarModelInfo firstSeatDistanceModel = null; //第二排座椅据第一排座椅距离
            CarBootInfo carBoot = null; //后备箱

            IEnumerable<CarInnerSpaceInfo> carInnerSpaceInfos =
                list.Where(carInnerSpaceInfo => carInnerSpaceInfo.IsCurrent);
            if (!carInnerSpaceInfos.Any()) return string.Empty;

            CarInnerSpaceInfo currentCarInnerSpaceInfo = carInnerSpaceInfos.ToList()[0];
            if (currentCarInnerSpaceInfo == null) return string.Empty;

            //当前车辆模特数据 所有类型
            List<CarModelInfo> carModelInfoList = CommonData.GetCarModelInfoList(currentCarInnerSpaceInfo.CarId);

            if (carModelInfoList.Count == 0)
            {
                return string.Empty;
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
                return string.Empty;
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
                return string.Empty;
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
                return string.Empty;
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
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div class=\"col-auto tab-main\" group=\"main-kongjian\" id=\"kongjian\" data-channelid=\"2.21.814\"{0}>", !isShow ? "  style=\"display:none;\"" : "");
            sb.Append("<div class=\"col-auto mid\"><div><div class=\"special-layout-20\">");
            sb.Append("<ul class=\"list\">");
            sb.AppendLine(string.Format("<li class=\"tab current\" data=\"qianpaitoubu\"><a href=\"javascript:;\"><div class=\"title\">前排头部</div><h3 class=\"info\">{0}</h3></a></li>",
                                        Leval(currentCarInnerSpaceInfo.FirstSeatToTop, CommonEnum.CarInnerSpaceType.FirstSeatToTop)));
            sb.AppendLine(string.Format("<li class=\"tab\" data=\"houpaitoubu\"><a href=\"javascript:;\"><div class=\"title\">后排头部</div><h3 class=\"info\">{0}</h3></a></li>",
                                        Leval(currentCarInnerSpaceInfo.SecondSeatToTop,
                                              CommonEnum.CarInnerSpaceType.SecondSeatToTop)));
            sb.AppendLine(string.Format("<li class=\"tab\" data=\"houpaituibu\"><a href=\"javascript:;\"><div class=\"title\">后排腿部</div><h3 class=\"info\">{0}</h3></a></li>",
                                        Leval(currentCarInnerSpaceInfo.FirstSeatDistance,
                                              CommonEnum.CarInnerSpaceType.FirstSeatDistance)));
            sb.AppendLine(string.Format("<li class=\"tab\" data=\"houbeixiang\"><a href=\"javascript:;\"><div class=\"title\">后备箱</div><h3 class=\"info\">{0}</h3></a></li>",
                                        Leval(Convert.ToDouble(carBoot.Pvalue),
                                                  CommonEnum.CarInnerSpaceType.BackBoot)));
            sb.Append("</ul>");
            //sb.Append("</div></div></div>");
            //sb.Append("<div class=\"col-auto mid\" group=\"kongjian\" style=\"display:none;\"><div><div class=\"special-layout-15\">空间");
            sb.Append(KongJianMidHtml("qianpaitoubu", firstSeatToTopModel, currentCarInnerSpaceInfo, serialWhilePic));
            sb.Append(KongJianMidHtml("houpaitoubu", secondSeatToTopModel, currentCarInnerSpaceInfo, serialWhilePic));
            sb.Append(KongJianMidHtml("houpaituibu", firstSeatDistanceModel, currentCarInnerSpaceInfo, serialWhilePic));
            sb.Append(BackBootHtml(carBoot, backBootImageList, serialWhilePic));
            sb.Append("</div></div></div>");
            sb.Append(MakeKongJianCompriveHtml("qianpaitoubu", list, serialWhilePic));
            sb.Append(MakeKongJianCompriveHtml("houpaitoubu", list, serialWhilePic));
            sb.Append(MakeKongJianCompriveHtml("houpaituibu", list, serialWhilePic));
            sb.Append(MakeBackBootHtmlCompriveHtml(carBootList, serialWhilePic));
            sb.Append("</div>");
            isShow = false;
            return sb.ToString();
        }

        private string MakeBackBootHtmlCompriveHtml(IEnumerable<CarBootInfo> carBootList, Dictionary<int, string> serialWhilePic)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div class=\"col-auto right\" group=\"houbeixiang\" style=\"display:none;\"><div><div class=\"special-layout-8\"><h5>竞品排名</h5><div class=\"list-txt list-txt-m list-txt-style-num\"><ul>");
            IEnumerable<CarBootInfo> query = from items in carBootList orderby items.Pvalue descending select items;
            foreach (CarBootInfo bootInfo in query)
            {
                Dictionary<int, string> allParams = Common.Repository.CarRepository.GetCarAllParamByCarID(bootInfo.CarId);
                sb.AppendFormat(" <li{8}><a class=\"figure\"target=\"_blank\" title=\"{6}款 {7}\" href=\"/{0}/\"><img src=\"{1}\"><h6>{2}</h6><p>{3} / {4}</p></a><span class=\"data\">{5}</span></li>"
                        , bootInfo.SerialAllSpell
                        , serialWhilePic.ContainsKey(bootInfo.CsId) ? serialWhilePic[bootInfo.CsId].Replace("_2.", "_5.") : CommonData.CommonSettings.DefaultCarPic2
                        , bootInfo.CsShowName
                        , allParams.ContainsKey(785) && !string.IsNullOrWhiteSpace(allParams[785]) ? (allParams[785] + (allParams.ContainsKey(425) && allParams[425] == "增压" ? "T" : "L")) : "暂无"
                        , allParams.ContainsKey(430) ? allParams[430] + "kw " : "暂无"
                        , bootInfo.Pvalue == "-1" ? "" : bootInfo.Pvalue + "L"
                        , bootInfo.CarYear
                        , bootInfo.CarName
                        , bootInfo.CsId == SerialId ? " class=\"current\"" : "");
            }
            sb.Append("</ul></div></div></div></div>");
            return sb.ToString();
        }
        private string MakeKongJianCompriveHtml(string tabName, IEnumerable<CarInnerSpaceInfo> list, Dictionary<int, string> serialWhilePic)
        {
            IEnumerable<CarInnerSpaceInfo> query;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div class=\"col-auto right\" group=\"{0}\"{1}><div><div class=\"special-layout-8\"><h5>竞品排名</h5><div class=\"list-txt list-txt-m list-txt-style-num\"><ul>"
                      , tabName
                      , tabName == "qianpaitoubu" ? "" : " style=\"display:none;\"");
            switch (tabName)
            {
                case "qianpaitoubu":
                    query = from items in list orderby items.FirstSeatToTop descending select items;
                    break;
                case "houpaitoubu":
                    query = from items in list orderby items.SecondSeatToTop descending select items;
                    break;
                case "houpaituibu":
                    query = from items in list orderby items.FirstSeatDistance descending select items;
                    break;
                default:
                    query = from items in list orderby items.FirstSeatToTop descending select items;
                    break;
            }
            foreach (CarInnerSpaceInfo serialInfo in query)
            {
                Dictionary<int, string> allParams = Common.Repository.CarRepository.GetCarAllParamByCarID(serialInfo.CarId);
                sb.AppendFormat(" <li{8}><a class=\"figure\"target=\"_blank\" title=\"{6}款 {7}\" href=\"/{0}/\"><img src=\"{1}\"><h6>{2}</h6><p>{3} / {4}</p></a><span class=\"data\">{5}cm</span></li>"
                        , serialInfo.SerialAllSpell
                        , serialWhilePic.ContainsKey(serialInfo.CsId) ? serialWhilePic[serialInfo.CsId].Replace("_2.", "_5.") : CommonData.CommonSettings.DefaultCarPic2
                        , serialInfo.CsShowName
                        , allParams.ContainsKey(785) && !string.IsNullOrWhiteSpace(allParams[785]) ? (allParams[785] + (allParams.ContainsKey(425) && allParams[425] == "增压" ? "T" : "L")) : "暂无"
                        , allParams.ContainsKey(430) ? allParams[430] + "kw " : "暂无"
                        , tabName == "qianpaitoubu" ? serialInfo.FirstSeatToTop
                            : tabName == "houpaitoubu" ? serialInfo.SecondSeatToTop
                            : tabName == "houpaituibu" ? serialInfo.FirstSeatDistance : 0
                        , serialInfo.CarYear
                        , serialInfo.CarName
                        , serialInfo.CsId == SerialId ? " class=\"current\"" : "");
            }
            sb.Append("</ul></div></div></div></div>");

            return sb.ToString();
        }
        /// <summary>
        /// 后备箱
        /// </summary>
        /// <param name="carBootList"></param>
        /// <param name="carBoot"></param>
        /// <param name="carModelInfo"></param>
        /// <returns></returns>
        private string BackBootHtml(CarBootInfo carBoot, List<CarModelInfo> carModelInfo, Dictionary<int, string> serialWhilePic)
        {
            StringBuilder sb = new StringBuilder();
            CarModelInfo normalCarModelInfo = carModelInfo.FirstOrDefault(s => s.ParaId == 465); //正常
            CarModelInfo expansionCarModelInfo = carModelInfo.FirstOrDefault(s => s.ParaId == 466); //扩展
            sb.AppendFormat("<div group=\"houbeixiang\" style=\"display:none;\" id=\"houbeixiang-content\">");
            sb.Append("    <div class=\"content\">");
            if (normalCarModelInfo != null)
            {
                sb.AppendLine("<div class='img'>");
                sb.AppendLine(string.Format("   <img src='{0}' width='300px' height='200px' >", normalCarModelInfo.ImageUrl));
                sb.AppendLine("</div>");
            }
            if (expansionCarModelInfo != null)
            {
                sb.AppendLine(string.Format("<div class='img' {0}>", normalCarModelInfo != null ? "style='display:none'" : ""));
                sb.AppendLine(string.Format("   <img src='{0}' width='300px' height='200px' >", expansionCarModelInfo.ImageUrl));
                sb.AppendLine("</div>");
            }
            sb.AppendLine("<div class=\"btn-tab\">");
            if (expansionCarModelInfo != null && normalCarModelInfo != null)
            {
                sb.AppendLine("     <a class=\"btn btn-primary2\" href=\"javascript:;\">后排座椅未放倒时</a>");
                sb.AppendLine("     <a class=\"btn btn-default\" href=\"javascript:;\">后排座椅放倒后</a>");
            }
            else
            {
                sb.AppendLine("     <a class=\"btn btn-default\" href=\"javascript:;\">后备箱空间</a>");
            }
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");


            return sb.ToString();
        }

        private string KongJianMidHtml(string tabName, CarModelInfo carModelInfo, CarInnerSpaceInfo currentCarInnerSpaceInfo, Dictionary<int, string> serialWhilePic)
        {
            StringBuilder sb = new StringBuilder();
            double value = 0;
            string desc = string.Empty;

            switch (tabName)
            {
                case "qianpaitoubu":
                    value = currentCarInnerSpaceInfo.FirstSeatToTop;
                    desc = Leval(currentCarInnerSpaceInfo.FirstSeatToTop, CommonEnum.CarInnerSpaceType.FirstSeatToTop);
                    break;
                case "houpaitoubu":
                    value = currentCarInnerSpaceInfo.SecondSeatToTop;
                    desc = Leval(currentCarInnerSpaceInfo.SecondSeatToTop, CommonEnum.CarInnerSpaceType.SecondSeatToTop);
                    break;
                case "houpaituibu":
                    value = currentCarInnerSpaceInfo.FirstSeatDistance;
                    desc = Leval(currentCarInnerSpaceInfo.FirstSeatDistance, CommonEnum.CarInnerSpaceType.FirstSeatDistance);
                    break;
            }
            sb.AppendFormat("<div group=\"{0}\"{1}>", tabName, tabName != "qianpaitoubu" ? " style=\"display:none;\"" : "");
            sb.Append("    <div class=\"content\">");
            sb.Append("        <div class=\"img\">");
            sb.AppendFormat("            <img src=\"{0}\" alt=\"\" width=\"300\" height=\"200\">", carModelInfo != null ? carModelInfo.ImageUrl : "");
            sb.Append("        </div>");
            sb.AppendFormat("        <h5 class=\"title\">{0}cm {1}</h5>", value, desc);
            sb.AppendFormat("        <p class=\"info\">测试模特：{0}cm {1}kg</p>"
                , carModelInfo != null ? carModelInfo.Height.ToString(CultureInfo.InvariantCulture) : ""
                , carModelInfo != null ? carModelInfo.Weight.ToString(CultureInfo.InvariantCulture) : "");
            sb.Append("    </div>");
            sb.Append("</div>");

            return sb.ToString();
        }


        /// <summary>
        ///     数据提取规则
        /// </summary>
        /// <param name="currentCsId"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<CarInnerSpaceInfo> SelectTargetList(int currentCsId, List<int> list)
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


        private bool IsCompleteModelInfo(CarInnerSpaceInfo carBootInfo)
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

        private List<int> GetCsIdList(int currentCsId, IEnumerable<SerialToAttention> serialToAttentions)
        {
            List<int> list = serialToAttentions.Select(serialToAttention => serialToAttention.ToCsID).ToList();
            list.Add(currentCsId);
            return list;
        }

        /// <summary>
        /// </summary>
        /// <param name="number"></param>
        /// <param name="carInnerSpaceType"></param>
        /// <returns></returns>
        private string Leval(double number, CommonEnum.CarInnerSpaceType carInnerSpaceType)
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
        ///     查询条件数据
        /// </summary>
        /// <returns></returns>
        private DataTable GetDataTable(IEnumerable<int> list)
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
        #endregion


        /// <summary>
        /// 生成块内容
        /// </summary>
        /// <param name="carId"></param>
        private void CreateHtml(int carId)
        {

            DataSet ds = GetCarInfo(carId);
            //sbHtml.Append("<div class=\"line_box stat_box_line\">");
            //sbHtml.Append("<h3>");
            //sbHtml.AppendFormat("<span><b><a href=\"/{1}/pingce/\" target=\"_blank\">{0}关键报告</a></b></span>", _serialInfo.SeoName, _serialInfo.AllSpell);
            //sbHtml.Append("                <div class=\"h3_tab car-comparetable-tab\">");
            ////sbHtml.Append("                    <ul id=\"data_tab\">");
            ////sbHtml.Append("	                    <li class=\"current\">2.0手动</li>");
            ////sbHtml.Append("                        <li>2.5自动</li>");
            ////sbHtml.Append("                    </ul>");
            //sbHtml.Append("                </div>");
            //sbHtml.Append("</h3>");
            sbHtml.Append("<div class=\"stat_box_bg\" data-channelid=\"2.21.813\">");
            CreateSingleDoubleHtml(788, ds.Tables[0]);
            CreateSingleDoubleHtml(786, ds.Tables[0]);
            CreateSingleDoubleHtml(787, ds.Tables[0]);
            sbHtml.Append("</div>");
            //sbHtml.Append("	<div class=\"clear\"></div>");
            //sbHtml.AppendFormat("<div class=\"more\"><a href=\"/{0}/pingce/\" target=\"_blank\">详细报告&gt;&gt;</a></div>", _serialInfo.AllSpell);
            //sbHtml.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.HexinReport,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新核心关键报告失败：serialId:" + SerialId);
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
                sbHtml.Append("    <div class=\"stat_box\">");
                sbHtml.Append("    	<h4>综合油耗<span>(100km)</span></h4>");
                sbHtml.AppendFormat("        <h5>{0}升</h5>", currValue);
                var index = this.GetDataStandardIndex(_serialInfo.CarLevel, 788, currValue);
                string[] arrStandard = { "节能", "省油", "一般", "较高", "油霸" };
                sbHtml.Append("        <ul class=\"stat_grade\">");
                for (int i = 0; i < arrStandard.Length; i++)
                {
                    string strClass = string.Empty;
                    if (i == 0)
                        strClass = "noborder";
                    if (i == index)
                        strClass += " current";
                    sbHtml.AppendFormat("<li class=\"{0}\">{1}</li>",
                        strClass,
                        arrStandard[i]);
                }
                sbHtml.Append("</ul>");
                if (list.Count > 1)
                {
                    sbHtml.Append("<ul class=\"stat_car\">");
                    int loop = 0;
                    foreach (SerialNewCarInfo serialInfo in list)
                    {
                        loop++;
                        string strClass = string.Empty;
                        string lastClass = string.Empty;
                        if (serialInfo.SerialId == SerialId)
                            strClass = " class=\"current\"";
                        if (serialInfo.SerialId == SerialId && list.Count == loop)
                            strClass = "class=\"current noborder\"";
                        else if (list.Count == loop)
                            strClass = "class=\"noborder\"";
                        sbHtml.AppendFormat("<li {6}><a target=\"_blank\" href=\"/{0}/\" title=\"{4}款 {5}\">{1}</a> <div><span style=\"width:{3}%\" title=\"{4}款 {5}\"></span></div> <small>{2}L</small></li>",
                            serialInfo.SerialSpell,
                            serialInfo.SerialName,
                            serialInfo.Perf_MeasuredFuel,
                            (int)((serialInfo.Perf_MeasuredFuel / list[list.Count - 1].Perf_MeasuredFuel) * 100),
                            serialInfo.Car_YearType,
                            serialInfo.CarName,
                            strClass);
                    }
                    sbHtml.Append("        </ul>");
                }
                sbHtml.Append("    </div>");
            }
            if (paramId == 786)
            {
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

                sbHtml.Append("    <div class=\"stat_box\">");
                sbHtml.Append("    	<h4>加速时间<span>(0-100km/h)</span></h4>");
                sbHtml.AppendFormat("        <h5>{0}秒</h5>", currValue);

                var index = this.GetDataStandardIndex(_serialInfo.CarLevel, 786, currValue);
                string[] arrStandard = { "很差", "较差", "中等", "良好", "优秀" };
                sbHtml.Append("        <ul class=\"stat_grade\">");
                for (int i = 0; i < arrStandard.Length; i++)
                {
                    string strClass = string.Empty;
                    if (i == 0)
                        strClass = "noborder";
                    if (i == index)
                        strClass += " current";
                    sbHtml.AppendFormat("<li class=\"{0}\">{1}</li>",
                        strClass,
                        arrStandard[i]);
                }
                sbHtml.Append("        </ul>");
                if (list.Count > 1)
                {
                    sbHtml.Append("<ul class=\"stat_car\">");
                    int loop = 0;
                    foreach (SerialNewCarInfo serialInfo in list)
                    {
                        loop++;
                        string strClass = string.Empty;
                        string lastClass = string.Empty;
                        if (serialInfo.SerialId == SerialId)
                            strClass = " class=\"current\"";
                        if (serialInfo.SerialId == SerialId && list.Count == loop)
                            strClass = "class=\"current noborder\"";
                        else if (list.Count == loop)
                            strClass = "class=\"noborder\"";
                        sbHtml.AppendFormat("<li {6}><a target=\"_blank\" href=\"/{0}/\" title=\"{4}款 {5}\">{1}</a> <div><span style=\"width:{3}%\" title=\"{4}款 {5}\"></span></div> <small>{2}s</small></li>",
                            serialInfo.SerialSpell,
                            serialInfo.SerialName,
                            serialInfo.Perf_MeasuredAcceleration,
                            (int)((serialInfo.Perf_MeasuredAcceleration / list[list.Count - 1].Perf_MeasuredAcceleration) * 100),
                            serialInfo.Car_YearType,
                            serialInfo.CarName,
                            strClass);
                    }
                    sbHtml.Append("        </ul>");
                }
                sbHtml.Append("    </div>");
            }
            if (paramId == 787)
            {
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

                sbHtml.Append("    <div class=\"stat_box noborder\">");
                sbHtml.Append("    	<h4>制动距离<span>(100-0km/h)</span></h4>");
                sbHtml.AppendFormat("        <h5>{0}米</h5>", currValue);
                var index = this.GetDataStandardIndex(_serialInfo.CarLevel, 787, currValue);
                string[] arrStandard = { "很差", "较差", "中等", "良好", "优秀" };
                sbHtml.Append("        <ul class=\"stat_grade\">");
                for (int i = 0; i < arrStandard.Length; i++)
                {
                    string strClass = string.Empty;
                    if (i == 0)
                        strClass = "noborder";
                    if (i == index)
                        strClass += " current";
                    sbHtml.AppendFormat("<li class=\"{0}\">{1}</li>",
                        strClass,
                        arrStandard[i]);
                }
                sbHtml.Append("        </ul>");
                if (list.Count > 1)
                {
                    sbHtml.Append("<ul class=\"stat_car\">");
                    int loop = 0;
                    foreach (SerialNewCarInfo serialInfo in list)
                    {
                        loop++;
                        string strClass = string.Empty;
                        string lastClass = string.Empty;
                        if (serialInfo.SerialId == SerialId)
                            strClass = " class=\"current\"";
                        if (serialInfo.SerialId == SerialId && list.Count == loop)
                            strClass = "class=\"current noborder\"";
                        else if (list.Count == loop)
                            strClass = "class=\"noborder\"";
                        sbHtml.AppendFormat("<li {6}><a target=\"_blank\" href=\"/{0}/\" title=\"{4}款 {5}\">{1}</a> <div><span style=\"width:{3}%\" title=\"{4}款 {5}\"></span></div> <small>{2}m</small></li>",
                            serialInfo.SerialSpell,
                            serialInfo.SerialName,
                            serialInfo.Perf_BrakingDistance,
                            (int)((serialInfo.Perf_BrakingDistance / list[list.Count - 1].Perf_BrakingDistance) * 100),
                            serialInfo.Car_YearType,
                            serialInfo.CarName,
                            strClass);
                    }
                    sbHtml.Append("        </ul>");
                }
                sbHtml.Append("    </div>");
            }
        }
        //
        private XmlDocument GetDataStandard()
        {
            XmlDocument xmlDoc = new XmlDocument();
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\ParamsStandard.config");
            return CommonFunction.GetLocalXmlDocument(filePath);
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
                            if (loop > 4) break;
                            DataTable table = ds.Tables[0];
                            SerialNewCarInfo serialInfo = GetSerialNewCarInfo(table);
                            serialInfo.CarId = carId;
                            serialInfo.SerialId = csid;
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
