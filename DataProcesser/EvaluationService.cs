using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarUtils.Define;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class EvaluationService
    {
        private static Dictionary<int, int> DicEvaluationIdCarId = CommonData.DicEvaluationIdCarId;

        /// <summary>
        /// 保存评测报告页面中的排行数据到XML文件中
        /// </summary>
        public void SaveEvaluationRankToXml()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>
            {
                { 132, "ASC" },
                { 11, "ASC" },
                { 27, "DESC" },
                { 22, "DESC" }
            };

            foreach (int propertyId in dic.Keys)
            {
                #region 查询语句
                string sql = @"WITH result1 AS 
                (
                    SELECT 
                    se.StyleId
                    ,sv.PropertyValue
                    ,sv.PropertyId
                    ,ROW_NUMBER() OVER( PARTITION BY se.StyleId ORDER BY CAST(sv.PropertyValue AS FLOAT) {0}) Num
                    ,se.Id AS EvaluationId    
                    ,se.EditorsName
                    ,se.Weather
                    ,se.Wind
                    ,se.Temperature
                    ,sjb.StyleName
                    ,sjb.MasterBrandName
                    ,sjb.ModelName
                    ,sjb.ModelDisplayName
                    ,sjb.ModelLevel
                    ,sjb.Year
                    ,sp.Unit
                    ,sjb.FuelType
                    ,sjb.ModelAllSpell
                    FROM [dbo].[StylePropertyValue] AS sv  LEFT JOIN [dbo].[StyleEvaluation] AS se ON sv.EvaluationId = se.Id and sv.PropertyId = @propertyId LEFT JOIN [dbo].[StyleJoinBrand] AS sjb ON sjb.StyleId=se.StyleId LEFT JOIN [dbo].[StyleProperty] AS sp ON sp.Id=sv.PropertyId   
                    WHERE se.[Status]=2
                ) 
                SELECT
                StyleId
                ,PropertyValue
                ,PropertyId
                ,EvaluationId    
                ,EditorsName
                ,Weather
                ,Wind
                ,Temperature
                ,StyleName
                ,MasterBrandName
                ,ModelName
                ,Year
                ,ModelDisplayName
                ,ModelLevel
                ,Unit
                ,FuelType
                ,ModelAllSpell
                ,ROW_NUMBER() OVER( PARTITION BY ModelLevel ORDER BY CAST(PropertyValue AS FLOAT) {0} ) LevelNum 
                ,ROW_NUMBER() OVER(ORDER BY CAST(PropertyValue AS FLOAT) {0} ) RankNum  
                FROM result1 WHERE Num=1";
                #endregion

                SqlParameter[] _params = {
                                         new SqlParameter("@propertyId",SqlDbType.Int)
                                     };
                _params[0].Value = propertyId;
                string exeSql = string.Format(sql, dic[propertyId]);
                try
                {
                    DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarsEvaluationConnString, CommandType.Text, exeSql, _params);
                    List<EvaluationRankEntity> list = GetList(ds);
                    SelectEvaluationRank(list);
                }
                catch (Exception e)
                {
                    var msg = e.Message;
                }                
            }            
        }

        private List<EvaluationRankEntity> GetList(DataSet ds)
        {
            List<EvaluationRankEntity> list = new List<EvaluationRankEntity>();           
            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    EvaluationRankEntity er = new EvaluationRankEntity();
                    er.StyleId = ConvertHelper.GetInteger(dr["StyleId"]);
                    er.PropertyValue = ConvertHelper.GetDouble(dr["PropertyValue"]);
                    er.PropertyId = ConvertHelper.GetInteger(dr["PropertyId"]);
                    er.EvaluationId = ConvertHelper.GetInteger(dr["EvaluationId"]);
                    er.EditorsName = dr["EditorsName"].ToString();
                    er.Weather = dr["Weather"].ToString();
                    er.Wind = dr["Wind"].ToString();
                    er.Temperature = dr["Temperature"].ToString();
                    er.StyleName = dr["StyleName"].ToString();
                    er.MasterBrandName = dr["MasterBrandName"].ToString();
                    er.ModelName = dr["ModelName"].ToString();
                    er.Year = ConvertHelper.GetInteger(dr["Year"]);
                    er.ModelDisplayName = dr["ModelDisplayName"].ToString();
                    er.ModelLevel = dr["ModelLevel"].ToString();
                    er.LevelNum = ConvertHelper.GetInteger(dr["LevelNum"]);
                    er.RankNum = ConvertHelper.GetInteger(dr["RankNum"]);
                    er.Unit = dr["Unit"].ToString();
                    er.FuelType = dr["FuelType"].ToString();
                    er.ModelAllSpell = dr["ModelAllSpell"].ToString();
                    er.CurrentFlag = false;
                    list.Add(er);
                }
            }
            return list;
        }

        private void SelectEvaluationRank(List<EvaluationRankEntity> list)
        {           
            foreach (EvaluationRankEntity item in list)
            {
                List<EvaluationRankEntity> templist = new List<EvaluationRankEntity>();
                int carId = item.StyleId;
                int propertyId = item.PropertyId;
                int currentIndex = list.FindLastIndex(i => i.StyleId == carId);
                int total = 0;
                if (currentIndex >= 0)
                {
                    var levelName = item.ModelLevel;                    
                    total = list.Count(i => i.ModelLevel == levelName);//该级别的评测总数量
                    if (list.Count() <= 3)
                    {
                        templist.AddRange(list);
                    }
                    else
                    {
                        if (currentIndex == 0 || currentIndex == 1)
                        {
                            templist.Add(list[0]);
                            templist.Add(list[1]);
                            templist.Add(list[2]);
                        }
                        else
                        {
                            if (currentIndex == list.Count() - 1)
                            {
                                templist.Add(list[0]);
                                templist.Add(list[currentIndex - 1]);
                                templist.Add(list[currentIndex]);
                            }
                            else
                            {
                                templist.Add(list[0]);
                                templist.Add(list[currentIndex - 1]);
                                templist.Add(list[currentIndex]);
                                templist.Add(list[currentIndex + 1]);
                            }
                        }
                    } 
                }
                //按参数组保存车款的排名数据
                SaveEvaluationRankToXml(templist, carId, propertyId,total);
            }
        }

        private string GetTabText(int propertyId, double propertyValue, int level, string fuelType)
        {
            var tab = "";
            switch (propertyId)
            {
                case 11://冷刹车
                    if (propertyValue > 0 && propertyValue <= 36)
                    {
                        tab = "秒刹";
                    }
                    else if (propertyValue > 36 && propertyValue <= 40)
                    {
                        tab = "刹车灵敏";
                    }
                    else if (propertyValue > 40 && propertyValue <= 45)
                    {
                        tab = "一般般";
                    }
                    else if (propertyValue > 45 && propertyValue <= 47)
                    {
                        tab = "刹车略肉";
                    }
                    else
                    {
                        tab = "刹车太肉";
                    }
                    break;
                case 22://18米绕桩
                    if (level == 9)//level=9是跑车
                    {
                        if (propertyValue > 0 && propertyValue <= 60.9)
                        {
                            tab = "代步级";
                        }
                        else if (propertyValue >= 61 && propertyValue <= 62.9)
                        {
                            tab = "家用级";
                        }
                        else if (propertyValue >= 63 && propertyValue <= 64.9)
                        {
                            tab = "运动级";
                        }
                        else if (propertyValue >= 65 && propertyValue <= 66.9)
                        {
                            tab = "性能级";
                        }
                        else if (propertyValue >= 67)//&& propertyValue <= 71
                        {
                            tab = "赛道级";
                        }
                    }
                    else
                    {
                        if (propertyValue > 0 && propertyValue <= 57.9)
                        {
                            tab = "代步级";
                        }
                        else if (propertyValue >= 58 && propertyValue <= 59.9)
                        {
                            tab = "家用级";
                        }
                        else if (propertyValue >= 60 && propertyValue <= 61.9)
                        {
                            tab = "运动级";
                        }
                        else if (propertyValue >= 62 && propertyValue <= 63.9)
                        {
                            tab = "性能级";
                        }
                        else if (propertyValue >= 64)//&& propertyValue <= 68
                        {
                            tab = "赛道级";
                        }
                    }
                    break;
                case 27://110米变线
                    if (level == 9)
                    {
                        if (propertyValue > 0 && propertyValue <= 122.9)
                        {
                            tab = "寸步难行";
                        }
                        else if (propertyValue > 123 && propertyValue <= 129.9)
                        {
                            tab = "步履蹒跚";
                        }
                        else if (propertyValue > 130 && propertyValue <= 134.9)
                        {
                            tab = "平稳通过";
                        }
                        else if (propertyValue > 135 && propertyValue <= 136.9)
                        {
                            tab = "超级稳定";
                        }
                        else if (propertyValue > 137)//&& propertyValue <= 141
                        {
                            tab = "行云流水";
                        }
                    }
                    else
                    {
                        if (propertyValue > 0 && propertyValue <= 114.9)
                        {
                            tab = "寸步难行";
                        }
                        else if (propertyValue >= 115 && propertyValue <= 118.9)
                        {
                            tab = "步履蹒跚";
                        }
                        else if (propertyValue >= 119 && propertyValue <= 121.9)
                        {
                            tab = "平稳通过";
                        }
                        else if (propertyValue >= 122 && propertyValue <= 124.9)
                        {
                            tab = "超级稳定";
                        }
                        else if (propertyValue >= 125)//&& propertyValue <= 129
                        {
                            tab = "行云流水";
                        }
                    }
                    break;
                case 132://加速
                    if (fuelType == "纯电" || fuelType == "插电混合")
                    {
                        if (propertyValue > 0 && propertyValue <= 4)
                        {
                            tab = "diao爆了";
                        }
                        else if (propertyValue >= 4.1 && propertyValue <= 5)
                        {
                            tab = "炫酷超车";
                        }
                        else if (propertyValue >= 5.1 && propertyValue <= 7.1)
                        {
                            tab = "一般般";
                        }
                        else if (propertyValue >= 7.2 && propertyValue <= 9)
                        {
                            tab = "弱弱哒";
                        }
                        else if (propertyValue >= 9.1)//&& propertyValue <= 11
                        {
                            tab = "弱爆了";
                        }
                    }
                    else
                    {
                        if (level == 9)//跑车
                        {
                            if (propertyValue > 0 && propertyValue <= 3.2)
                            {
                                tab = "diao爆了";
                            }
                            else if (propertyValue >= 3.3 && propertyValue <= 4.5)
                            {
                                tab = "炫酷超车";
                            }
                            else if (propertyValue >= 4.6 && propertyValue <= 5.5)
                            {
                                tab = "一般般";
                            }
                            else if (propertyValue >= 5.6 && propertyValue <= 7)
                            {
                                tab = "弱弱哒";
                            }
                            else if (propertyValue >= 7.1)//&& propertyValue <= 10
                            {
                                tab = "弱爆了";
                            }
                        }
                        else
                        {
                            if (propertyValue > 0 && propertyValue <= 6)
                            {
                                tab = "diao爆了";
                            }
                            else if (propertyValue >= 6.1 && propertyValue <= 9.1)
                            {
                                tab = "炫酷超车";
                            }
                            else if (propertyValue >= 9.2 && propertyValue <= 11.1)
                            {
                                tab = "一般般";
                            }
                            else if (propertyValue >= 11.2 && propertyValue <= 13.9)
                            {
                                tab = "弱弱哒";
                            }
                            else if (propertyValue >= 14)//&& propertyValue <= 14.9
                            {
                                tab = "弱爆了";
                            }
                        }
                    }
                    break;
            }
            return tab;
        }
        
        private void SaveEvaluationRankToXml(List<EvaluationRankEntity> targetList, int carId, int propertyId,int total)
        {
            string fileName = Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"EvaluationRank\Rank_{0}\{1}.xml", propertyId, carId));
            XDocument xdoc = new XDocument();
            XElement Root = new XElement("Root");
            XElement evaluationRankList = new XElement("EvaluationRankList");

            int levelNum = 0;
            var levelName = "";
            foreach (var item in targetList)
            {
                int levelId = CarLevelDefine.GetLevelIdByName(item.ModelLevel);
                string tabText = GetTabText(item.PropertyId, Math.Round(item.PropertyValue,1), levelId, item.FuelType);

                if (item.StyleId == carId)
                {
                    levelNum = item.LevelNum;
                    levelName = item.ModelLevel;
                }

                XElement evaluationRank = new XElement("EvaluationRank");
                evaluationRank.Add(new XElement("StyleId", item.StyleId));
                evaluationRank.Add(new XElement("PropertyId", item.PropertyId));
                evaluationRank.Add(new XElement("PropertyValue", Math.Round(item.PropertyValue, 1)));
                evaluationRank.Add(new XElement("EvaluationId", item.EvaluationId));
                evaluationRank.Add(new XElement("EditorsName", item.EditorsName));
                evaluationRank.Add(new XElement("Weather", item.Weather));
                evaluationRank.Add(new XElement("Wind", item.Wind));
                evaluationRank.Add(new XElement("Temperature", item.Temperature));
                evaluationRank.Add(new XElement("StyleName", item.StyleName));
                evaluationRank.Add(new XElement("MasterBrandName", item.MasterBrandName));
                evaluationRank.Add(new XElement("ModelName", item.ModelName));
                evaluationRank.Add(new XElement("Year", item.Year));
                evaluationRank.Add(new XElement("ModelDisplayName", item.ModelDisplayName));
                evaluationRank.Add(new XElement("ModelLevel", item.ModelLevel));
                evaluationRank.Add(new XElement("LevelNum", item.LevelNum));
                evaluationRank.Add(new XElement("RankNum", item.RankNum));
                evaluationRank.Add(new XElement("CurrentFlag", item.StyleId == carId?true:item.CurrentFlag));
                evaluationRank.Add(new XElement("Unit", item.Unit));
                evaluationRank.Add(new XElement("TabText", tabText));
                evaluationRank.Add(new XElement("FuelType", item.FuelType));
                evaluationRank.Add(new XElement("ModelAllSpell", item.ModelAllSpell));
                if (DicEvaluationIdCarId.ContainsKey(item.EvaluationId)&& DicEvaluationIdCarId[item.EvaluationId] == item.StyleId)
                {
                    evaluationRank.Add(new XElement("IsExistReport", true));
                }
                else
                {
                    evaluationRank.Add(new XElement("IsExistReport", false));
                }            
                evaluationRankList.Add(evaluationRank);
            }
            Root.Add(new XElement("LevelTotal", total));
            Root.Add(new XElement("LevelNum", levelNum));
            Root.Add(new XElement("LevelName", levelName));
            Root.Add(evaluationRankList);
            xdoc.Add(Root);
            try
            {
                //xdoc.Save(fileName);
                CommonFunction.SaveFileContent(xdoc.ToString(), fileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(string.Format("保存车款{0}，propertyId={1}的排行数据出错：{2}", carId,propertyId, ex.Message));
            }
            
        }

        /// <summary>
        /// 保存有评测报告的评测ID和车款ID到XML文件中
        /// </summary>
        public void SaveExistEvaluationReportDataToXml()
        {
            string fileName = Path.Combine(CommonData.CommonSettings.SavePath, @"ExistReportEvaluationIdList\EvaluationIdList.xml");
            XDocument xdoc = new XDocument();
            XElement Root = new XElement("Root");
            XElement evaluationIdList = new XElement("EvaluationIdList");

            foreach (int key in DicEvaluationIdCarId.Keys)
            {
                evaluationIdList.Add(new XElement("Item", new XAttribute("EvaluationId",key), new XAttribute("CarId", DicEvaluationIdCarId[key])));
            }

            Root.Add(evaluationIdList);
            xdoc.Add(Root);
            try
            {
                //xdoc.Save(fileName);
                CommonFunction.SaveFileContent(xdoc.ToString(), fileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(string.Format("保存有评测报告的评测ID和车款ID到XML文件中,报错{0}",  ex.Message));
            }
        }


    }
}
