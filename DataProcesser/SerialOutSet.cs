using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;
using System.IO;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class SerialOutSet
    {
        public event LogHandler Log;
        private readonly static string _SelectRelationString;
        private const string _DataTableSelectFormat = "cs_id={0}";
        private const string _DataTableSelectRowsFormat = "cs_id={0} and caryear={1}";
        private string _XmlFileName = string.Empty;
        private string _RootPath = string.Empty;

        static SerialOutSet()
        {
            _SelectRelationString = @"select a.cs_id, a.caryear, a.hassizeimage, b.paramid, b.pvalue, d.classvalue carLevel,e.classvalue carbodyform from (
	                                select (select top 1 Car_Id from car_relation where isstate=0 and cs_id=a.cs_id and Car_YearType=a.caryear order by CreateTime desc) 
		                                as Car_Id, 
		                                cs_id, 
		                                caryear, 
		                                hassizeimage 
	                                  from Car_SerialYear a 
	                                  where isstate=0 
                                ) as a
                                inner join Car_Serial as c on c.cs_id=a.cs_id
                                inner join class as d on c.carlevel=d.classid
                                inner join class as e on c.CsBodyForm=e.classid
                                inner join CarDataBase as b on a.Car_Id=b.carid
                                where b.ParamId in (588,593,586,592,585,582)
                                order by a.cs_id, a.caryear desc";
        }

        public SerialOutSet()
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialSet");
            _XmlFileName = Path.Combine(_RootPath, "SerialOutSet.xml");
        }

        /// <summary>
        /// 创建SerialOutSetXml的主方法
        /// 生成子品牌年款外围尺寸xml
        /// </summary>
        public void CreateSerialOutSetXml()
        {
            OnLog("		Start SerialOutSet ......", true);

            DataSet ds = GetData();
            DataTable dt = null;
            if (ds != null || ds.Tables.Count > 0 || ds.Tables[0].Rows.Count > 1)
            {
                dt = ds.Tables[0];
                OnLog("		Get Data Row Count (" + dt.Rows.Count.ToString() + ")", true);
                Dictionary<int, List<int>> carYearList = GetYearList(ds);
                List<int> yearList = null;

                if (carYearList != null && carYearList.Count > 0)
                {
                    OnLog("		Select Serial Count (" + carYearList.Count.ToString() + ")", true);
                    //car_id, cs_id, caryear, hassizeimage, paramid, pvalue
                    bool hassizeimage;
                    XmlDocument doc = new XmlDocument();
                    XmlElement root = doc.CreateElement("Root");
                    doc.AppendChild(root);
                    XmlElement serialEle, yearEle;
                    DataRow[] rows = null;
                    DataRow row = null;
                    foreach (int cs_id in carYearList.Keys)
                    {
                        try
                        {
                            serialEle = doc.CreateElement("Serial");
                            serialEle.SetAttribute("id", cs_id.ToString());
                            root.AppendChild(serialEle);

                            yearList = carYearList[cs_id];
                            foreach (int yearInt in yearList)
                            {
                                yearEle = doc.CreateElement("YearType");
                                serialEle.AppendChild(yearEle);

                                yearEle.SetAttribute("year", yearInt.ToString());

                                rows = dt.Select(string.Format(_DataTableSelectRowsFormat, cs_id.ToString(), yearInt.ToString()));
                                row = rows[0];

                                hassizeimage = (row["hassizeimage"] is DBNull) ? false : Convert.ToBoolean(row["hassizeimage"]);
                                yearEle.SetAttribute("hasimg", (hassizeimage ? "1" : "0"));

                                yearEle.SetAttribute("carlevel", row["carLevel"].ToString());
                                yearEle.SetAttribute("carbodyform", row["carbodyform"].ToString());

                                //588	长 Length
                                SetOutSetAttributeValue(yearEle, rows, 588, "length");
                                //593	宽 Width
                                SetOutSetAttributeValue(yearEle, rows, 593, "width");
                                //586	高 Height
                                SetOutSetAttributeValue(yearEle, rows, 586, "height");
                                //592	轴距 WheelBase
                                SetOutSetAttributeValue(yearEle, rows, 592, "wheelbase");
                                //585	前轮距 FrontTread
                                SetOutSetAttributeValue(yearEle, rows, 585, "fronttread");
                                //582	后轮距 BackTread
                                SetOutSetAttributeValue(yearEle, rows, 582, "backtread");
                            }
                        }
                        catch (Exception exp)
                        {
                            OnLog("Serial To Xml Error (cs_id:" + cs_id.ToString() + ";Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")", true);
                        }
                    }

                    OnLog("		XML Row Count (" + root.ChildNodes.Count.ToString() + ")", true);
                    CommonFunction.SaveXMLDocument(doc, _XmlFileName);
                }
            }
            OnLog("End SerialOutSet ......", true);
        }
        /// <summary>
        /// 检测目录是否存在，如果不存在将创建
        /// </summary>
        /// <returns></returns>
        private bool ExistsDirectory()
        {
            if (!Directory.Exists(_RootPath))
            {
                OnLog("Start Create SerialSet Directory (Path:" + _RootPath + ")...", true);
                try
                {
                    Directory.CreateDirectory(_RootPath);
                }
                catch (Exception exp)
                {
                    OnLog("Create SerialSet Directory Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...", true);
                    return false;
                }
                OnLog("End Create SerialSet Directory ...", true);
            }
            return true;
        }

        /// <summary>
        /// 设置年款节点的长、宽、高等属性值
        /// </summary>
        /// <param name="currentEle">年款节点</param>
        /// <param name="rows">子品牌与年款为条件查出的datarows</param>
        /// <param name="paramId">参数id(长、宽、高...)</param>
        /// <param name="attName">属性名称</param>
        private void SetOutSetAttributeValue(XmlElement currentEle, DataRow[] rows, int paramId, string setAttributeName)
        {
            string pvalueStr = string.Empty;
            int pvalue = 0;
            foreach (DataRow pRow in rows)
            {
                if (Convert.ToInt32(pRow["paramid"]) == paramId)
                {
                    pvalueStr = pRow["pvalue"].ToString();
                    if (string.IsNullOrEmpty(pvalueStr))
                        return;
                    if (!int.TryParse(pvalueStr, out pvalue))
                        return;

                    currentEle.SetAttribute(setAttributeName, pvalue.ToString());
                }
            }
        }

        /// <summary>
        /// 得到子品牌与年款的数据字典 Dictionary<子品牌id, List<年款>>
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private Dictionary<int, List<int>> GetYearList(DataSet ds)
        {
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            List<int> years = null;
            if (ds != null || ds.Tables.Count > 0 || ds.Tables[0].Rows.Count > 1)
            {
                //car_id, cs_id, caryear, hassizeimage, paramid, pvalue
                DataTable dt = ds.Tables[0];
                int cs_id, year;
                foreach (DataRow row in dt.Rows)
                {
                    if ((row["cs_id"] is DBNull))
                        continue;
                    cs_id = (int)row["cs_id"];
                    if (!result.ContainsKey(cs_id))
                    {
                        DataRow[] rows = dt.Select(string.Format(_DataTableSelectFormat, cs_id.ToString()));
                        years = new List<int>();
                        foreach (DataRow r in rows)
                        {
                            if ((r["caryear"] is DBNull))
                                continue;
                            year = Convert.ToInt32(r["caryear"]);

                            if (!years.Contains(year))
                                years.Add(year);
                        }
                        result.Add(cs_id, years);
                    }
                }
            }
            return result;
        }

        private DataSet GetData()
        {
            DataSet ds = null;
            try
            {
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, _SelectRelationString);
            }
            catch (Exception exp)
            {
                OnLog("Get Data Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...", true);
            }
            return ds;
        }

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="logText"></param>
        public void OnLog(string logText, bool nextLine)
        {
            if (Log != null)
                Log(this, new LogArgs(logText, nextLine));
        }
    }
}
