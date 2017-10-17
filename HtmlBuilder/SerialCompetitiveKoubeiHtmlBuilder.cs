using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.HtmlBuilder.com.bitauto.baa.api1;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    /// <summary>
    /// 综述页竞品口碑排名
    /// </summary>
    public class SerialCompetitiveKoubeiHtmlBuilder : BaseBuilder
    {
        private int SerialId = 0;
        private SerialInfo _serialInfo;

        private carService carService = null;
		//private static Dictionary<int, double> AverageRatingDic = new Dictionary<int, double>();
		//private static Dictionary<int, int> KoubeiCountDic = new Dictionary<int, int>();
		public readonly Dictionary<int, Dictionary<string, string>> _koubeiRatingDic = null;
		public SerialCompetitiveKoubeiHtmlBuilder(Dictionary<int, Dictionary<string, string>> _koubeiRatingDic)
        {
			this._koubeiRatingDic = _koubeiRatingDic;
			/*
            carService = new carService()
            {
                ApiSoapHeaderValue = new com.bitauto.baa.api1.ApiSoapHeader()
                {
                    AppKey = "100049",
                    AppPwd = "EAB934E5-6021-48DF-BA69-B063FCFEA72B"
                }
            };
            List<int> serialList = CommonFunction.GetSerialList();
            foreach (int csID in serialList)
            {
                try
                {
                    XmlDocument xmlRating = this.GetTrimRating(csID);
                    double AverageRating = 0;
                    int koubeiCount = 0;
                    if (xmlRating != null)
                    {
                        XmlNode ratingNode = xmlRating.SelectSingleNode("Serial/Item/Rating");
                        var ratingStr = ratingNode == null ? "0" : ratingNode.InnerText;
                        var rating = Double.Parse(ratingStr);
                        AverageRating = Math.Round(rating, 1);
                        AverageRatingDic.Add(csID, AverageRating);

                        XmlNode totalNode = xmlRating.SelectSingleNode("Serial/Item/TotalCount");
                        var countStr = totalNode == null ? "0" : totalNode.InnerText;
                        koubeiCount = int.Parse(countStr);
                        KoubeiCountDic.Add(csID, koubeiCount);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteErrorLog(string.Format("竞品口碑排行异常，serialId={0},\r\n{1}", csID, ex.ToString()));
                }
            }
			 * */
        }

        public override void BuilderDataOrHtml(int objId)
        {
            SerialId = objId;
            try
            {
                _serialInfo = CommonData.SerialDic[objId];
                if (_serialInfo == null) return;
                var serialList = this.GetCompareSerial(SerialId);
				var totalCount = _koubeiRatingDic.ContainsKey(objId) ? ConvertHelper.GetInteger(_koubeiRatingDic[objId]["TopicCount"]) : 0;
                if (serialList != null && serialList.Count > 0 && totalCount > 0)
                {
                    List<SerialKoubeiInfo> results = new List<SerialKoubeiInfo>();
                    foreach (var tempSerial in serialList)
                    {
                        SerialKoubeiInfo serialNew = new SerialKoubeiInfo();
                        serialNew.Id = tempSerial.Id;
                        serialNew.ShowName = tempSerial.ShowName;
                        serialNew.AllSpell = tempSerial.AllSpell;
						serialNew.AverageRating = _koubeiRatingDic.ContainsKey(tempSerial.Id) && _koubeiRatingDic[tempSerial.Id].ContainsKey("Ratings") ? ConvertHelper.GetDouble(_koubeiRatingDic[tempSerial.Id]["Ratings"]) : 0;
						if (results.Count < 4 && serialNew.AverageRating > 0)
						{
							results.Add(serialNew);
						}
                    }
                    results.Sort((p1, p2) =>
                    {
                        if (p1.AverageRating < p2.AverageRating)
                            return 1;
                        else if (p1.AverageRating > p2.AverageRating)
                            return -1;
                        else
                            return 0;
                    });
                    SerialKoubeiInfo serial = new SerialKoubeiInfo();
                    serial.Id = objId;
					serial.AverageRating = _koubeiRatingDic.ContainsKey(objId) && _koubeiRatingDic[objId].ContainsKey("Ratings") ? ConvertHelper.GetDouble(_koubeiRatingDic[objId]["Ratings"]) : 0;
                    serial.ShowName = _serialInfo.ShowName;
                    serial.AllSpell = _serialInfo.AllSpell;
                    bool isMin = true;
                    foreach (var ser in results)
                    {
                        if (ser.AverageRating <= serial.AverageRating)
                        {
                            results.Insert(results.IndexOf(ser), serial);
                            isMin = false;
                            break;
                        }
                    }
                    if (isMin)
                    {
                        results.Add(serial);
                    }

                    //Dictionary<int, string> whiteImg = CommonData.GetAllSerialPicURL(true);

                    //更新竞品口碑内容块
                    //RenderCompetitiveKoubeiHtml(results);
                    //更新竞品口碑内容块  1200版
                    //RenderCompetitiveKoubeiHtmlNew(results, whiteImg);

					//更新竞品口碑内容块
					RenderCompetitiveKoubeiHtmlV2(results);
                }
                else
                {
					CommonHtmlService.DeleteCommonHtml(
					SerialId,
					CommonHtmlEnum.TypeEnum.Serial,
					CommonHtmlEnum.TagIdEnum.SerialSummary,
					CommonHtmlEnum.BlockIdEnum.CompetitiveKoubeiV2);

					//CommonHtmlService.DeleteCommonHtml(
					//SerialId,
					//CommonHtmlEnum.TypeEnum.Serial,
					//CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
					//CommonHtmlEnum.BlockIdEnum.CompetitiveKoubei);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

        private XmlDocument GetTrimRating(int serialId)
        {
            string result = carService.GetTrimRating(serialId);
            if (string.IsNullOrEmpty(result)) return null;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            return xmlDoc;
        }

        public DataSet GetAllSerialToAttention()
        {
            SqlParameter csSqlParam = new SqlParameter("@CsId", SerialId);
            string sql = " select TOP 4 sts.CS_Id,sts.PCs_Id,sts.Pv_Num,cs.cs_name,cs.cs_showname,cs.allspell ";
            sql += " from dbo.Serial_To_Serial sts ";
            sql += " left join Car_Serial cs on sts.PCs_Id = cs.cs_id ";
            sql += " where sts.CS_Id=@CsId";
            sql += " order by sts.Pv_Num desc ";
            DataSet executeDataset = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString,
                                                              CommandType.Text, sql, csSqlParam);
            return executeDataset;
        }

		///// <summary>
		///// 竞争车型，1200版 2016-09-28 lisf
		///// </summary>
		///// <param name="results"></param>
		//private void RenderCompetitiveKoubeiHtmlNew(List<SerialKoubeiInfo> results, Dictionary<int, string> whiteImg)
		//{
		//	StringBuilder sbHtml = new StringBuilder();
		//	sbHtml.Append("<div class=\"layout-2 compete-sidebar\" data-channelid=\"2.21.831\">");
		//	sbHtml.Append("    <h3 class=\"top-title\">用户对比口碑排行</h3>");
		//	sbHtml.Append("    <div class=\"list-txt-m list-txt-style-num2 list-rank-pic\">");
		//	sbHtml.Append("        <ul>");
		//	foreach (var serial in results)
		//	{
		//		string rating = "0";
		//		if (AverageRatingDic.ContainsKey(serial.Id))
		//		{
		//			rating = AverageRatingDic[serial.Id].ToString("0.0");
		//		}
		//		sbHtml.AppendFormat("<li{0}>", serial.Id == _serialInfo.Id ? " class=\"current\"" : "");
		//		sbHtml.Append("<div class=\"car9060\">");
		//		sbHtml.AppendFormat("<a href=\"/{0}/\" target=\"_blank\"><img src=\"{1}\">", serial.AllSpell, whiteImg.ContainsKey(serial.Id) ? whiteImg[serial.Id] : CommonData.CommonSettings.DefaultCarPic);
		//		sbHtml.AppendFormat("</a><a class=\"txt\" target=\"_blank\" href=\"/{0}/\">{1}</a>", serial.AllSpell, serial.ShowName);
		//		sbHtml.Append("</div>");
		//		sbHtml.AppendFormat("<span><em>{0}分</em></span></li>", rating);
		//	}
		//	sbHtml.Append("        </ul>");
		//	sbHtml.Append("    </div>");
		//	sbHtml.Append("</div>");

		//	bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
		//	{
		//		ID = SerialId,
		//		TypeID = CommonHtmlEnum.TypeEnum.Serial,
		//		TagID = CommonHtmlEnum.TagIdEnum.SerialSummaryNew,
		//		BlockID = CommonHtmlEnum.BlockIdEnum.CompetitiveKoubei,
		//		HtmlContent = sbHtml.ToString(),
		//		UpdateTime = DateTime.Now
		//	});
		//	if (!success) Log.WriteErrorLog("更新新版竞品口碑失败：serialId:" + SerialId);
		//}

		//private void RenderCompetitiveKoubeiHtml(List<SerialKoubeiInfo> results)
		//{
		//	StringBuilder sbHtml = new StringBuilder();
		//	int count = 0;
		//	int order = 0;
		//	sbHtml.Append("<div class=\"line-box kb-t-m\">");
		//	sbHtml.Append("<div class=\"rank-list-5 rank-list-5t jpph_box\" data-channelid=\"2.21.831\">");

		//	StringBuilder sbTemp = new StringBuilder();
		//	sbTemp.Append("<ol class=\"rank-list\">");
		//	foreach (var serial in results)
		//	{
		//		count++;
		//		int csId = serial.Id;
		//		string name = serial.ShowName;
		//		string allSpell = serial.AllSpell;
		//		string className = string.Empty;
		//		string rating = "0";
		//		if (AverageRatingDic.ContainsKey(csId))
		//		{
		//			rating = AverageRatingDic[csId].ToString("0.0");
		//		}
		//		if (csId == SerialId)
		//		{
		//			order = count;
		//			className = "class=\"current\"";
		//			sbTemp.AppendFormat("<li {4}><i class=\"p{0}\"></i><a href=\"javascript:;\">{1}</a><span class=\"youhao\">{3}分</span></li>", count, name, allSpell, rating, className);
		//		}
		//		else
		//		{
		//			sbTemp.AppendFormat("<li {4}><i class=\"p{0}\"></i><a href=\"/{2}/\" target=\"_blank\">{1}</a><span class=\"youhao\">{3}分</span></li>", count, name, allSpell, rating, className);
		//		}
		//	}
		//	sbTemp.Append("</ol>");
		//	sbHtml.Append("<div class=\"side_title\">");
		//	sbHtml.AppendFormat("<h4>用户对比口碑排行<em>第{0}名</em>：</h4>", order);
		//	sbHtml.Append("</div>");

		//	sbHtml.Insert(sbHtml.Length, sbTemp.ToString());
		//	sbHtml.Append("</div>");
		//	sbHtml.Append("<div class=\"clear\"></div>");
		//	sbHtml.Append("</div>");
		//	bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
		//	{
		//		ID = SerialId,
		//		TypeID = CommonHtmlEnum.TypeEnum.Serial,
		//		TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
		//		BlockID = CommonHtmlEnum.BlockIdEnum.CompetitiveKoubei,
		//		HtmlContent = sbHtml.ToString(),
		//		UpdateTime = DateTime.Now
		//	});
		//	if (!success) Log.WriteErrorLog("更新竞品口碑失败：serialId:" + SerialId);
		//}

        private void RenderCompetitiveKoubeiHtmlV2(List<SerialKoubeiInfo> results)
        {
            StringBuilder sbHtml = new StringBuilder();
            int count = 0;
            int order = 0;
            StringBuilder sbTemp = new StringBuilder();
            foreach (var serial in results)
            {
                count++;
                int csId = serial.Id;
                string name = serial.ShowName;
                string allSpell = serial.AllSpell;
                string rating = "0";
                if (_koubeiRatingDic.ContainsKey(csId))
                {
					rating = ConvertHelper.GetDouble(_koubeiRatingDic[csId]["Ratings"]).ToString("0.0");
                }
                if (csId == SerialId)
                {
                    order = count;
                }
                sbTemp.AppendFormat("<li><a href=\"/{0}/\" target=\"_blank\">{1}</a> <span>{2}分</span> </li>", allSpell, name, rating);

            }

            sbHtml.Append("<div class=\"kb-rank\">");
            sbHtml.AppendFormat("<h3>竞品车型口碑排行<em>第{0}名</em></h3>", order);
            sbHtml.Append("<div class=\"list-txt-m list-txt-style-num2\">");
            sbHtml.Append("    <ul>");
            sbHtml.Insert(sbHtml.Length, sbTemp.ToString());
            sbHtml.Append("    </ul>");
            sbHtml.Append("</div>");
            sbHtml.Append("</div>");

            bool success = CommonHtmlService.UpdateCommonHtml(new Common.Model.CommonHtmlEntity()
            {
                ID = SerialId,
                TypeID = CommonHtmlEnum.TypeEnum.Serial,
                TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                BlockID = CommonHtmlEnum.BlockIdEnum.CompetitiveKoubeiV2,
                HtmlContent = sbHtml.ToString(),
                UpdateTime = DateTime.Now
            });
            if (!success) Log.WriteErrorLog("更新竞品口碑失败：serialId:" + SerialId);
        }
    }
    //返回车型信息
    public class SerialKoubeiInfo : SerialInfo
    {
        /// <summary>
        /// 评分
        /// </summary>
        public double AverageRating;
    }
}

