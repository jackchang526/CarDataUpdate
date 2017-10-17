using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Configuration;
using System.Threading.Tasks;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 第四级车型对比参数文档
    /// </summary>
    public class CarDataCompare
    {
        private string FilePath = Path.Combine(CommonData.CommonSettings.SavePath, "Compare/CarComparePrice.xml");
        /// <summary>
        /// 二手车估价接口
        /// </summary>
        private string TaoChePingGuApi = ConfigurationManager.AppSettings["TaoChePingGuApi"];
        /// <summary>
        /// 生成车型参数对比文档（第四级）
        /// </summary>
        /// <param name="carId"></param>
        public void GenerateDataXml(string[] funcArgs)
        {
            //if ((funcArgs == null || funcArgs.Length == 0) && DateTime.Now.Day != 1) return;//每月1号跑一次，如果单独跑，随便加个参数
            try
            {
                Common.Log.WriteLog("开始生成车款参数对比文档");
                List<CarCompareEntity> list = GetCarList();
                if (list == null) return;
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sb.Append("<root>");
                //ParallelOptions opt = new ParallelOptions();
                //opt.MaxDegreeOfParallelism = 10;
                //Parallel.ForEach(list, opt, entity =>
                XmlDocument oldXml = null;
                try
                {
                    if (File.Exists(FilePath))
                    {
                        oldXml = new XmlDocument();
                        oldXml.Load(FilePath);
                    }
                }
                catch (Exception ex)
                {
                    Common.Log.WriteErrorLog("读取车款参数对比文档生成，信息：" + ex.ToString());
                    return;
                }
                
                foreach (CarCompareEntity entity in list)
                {
                    Common.Log.WriteLog("carid=" + entity.CarId);
                    double referPrice = entity.ReferPrice;//指导价
                    double acquisitionTax = 0;
                    double vesselTax = 0;
                    int baoXian = 0;
                    int chePai = 0;
                    double price3 = 0;
                    if (referPrice > 0)
                    {
                        acquisitionTax = calcAcquisitionTax(referPrice, entity.Exhaustforfloat);//购置税
                        vesselTax = CalculateVehicleAndVesselTax(entity.CarId, entity.Exhaustforfloat, entity.TravelTax);//车船税
                        baoXian = Baoxian(entity);//保险
                        chePai = 500;//车牌
                    }
                    if (oldXml == null || DateTime.Now.Day == 1 || (funcArgs != null && funcArgs.Length > 0))
                    {
                        price3 = BaoZhilv(entity.CarId);
                    }
                    else if(oldXml != null)
                    {
                        XmlNode carNode = oldXml.SelectSingleNode("root/car[@id='" + entity.CarId + "']");
                        if (carNode != null)
                        {
                            price3 = ConvertHelper.GetDouble(carNode.Attributes["price3"].Value);
                        }
                    }

                    double koubei = GetKoubeirating(entity.SerialId);//口碑评分
                    //三年保值率
                    sb.AppendFormat("<car id=\"{0}\" gouZhiShui=\"{1}\" cheChuanShui=\"{2}\" baoXian=\"{3}\" chePai=\"{4}\" koubei=\"{5}\" price3=\"{6}\"/>"
                        , entity.CarId, acquisitionTax, vesselTax, baoXian, chePai, koubei, price3);
                }//);
                sb.Append("</root>");
                CommonFunction.SaveFileContent(sb.ToString(), FilePath, Encoding.UTF8);
                Common.Log.WriteLog("车款参数对比文档生成完成");
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("车款参数对比文档生成错误，信息：" + ex.ToString());
            }
        }



        /// <summary>
        /// 保值率
        /// </summary>
        private double BaoZhilv(int carId)
        {
            double price3 = 0;//三年保值率
            string op = "c-intra_Yungujia";
            var dic = new Dictionary<string, string>(); 
            dic.Add("carid", carId.ToString());
            dic.Add("buycardate", DateTime.Now.AddMonths(-1).ToString("yyyy-MM-01"));
            dic.Add("cityid", "201");
            //dic.Add("type", "2");
            dic.Add("drivingmileage", "1");
            dic.Add("username", "Yiche");
            string param = string.Empty; 
            foreach (KeyValuePair<string, string> item in dic) 
            { 
                param += item.Key + "%3d" + item.Value + "%26"; 
            }
            param = param.Remove(param.Length - 3);

            string urlparam = "op=" + op + "&param=" + param +  "&token=&sign="; 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TaoChePingGuApi + urlparam); 
            request.Method = "POST"; 
            request.ContentType = "application/x-www-form-urlencoded;";
            byte[] byteArray = Encoding.UTF8.GetBytes(""); 
            request.ContentLength = byteArray.Length;
            Stream dataStream = null;
            WebResponse response = null;
            Stream responseStream = null;
            StreamReader reader = null;

            try
            {
                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                response = request.GetResponse();
                responseStream = response.GetResponseStream();
                reader = new StreamReader(responseStream);
                string responseStr = reader.ReadToEnd();
                var entity = JsonConvert.DeserializeObject<CarGuJia>(responseStr);
                if (entity.Status == 100 && entity.Data.Trend.Status == 0)
                {
                    price3 = entity.Data.Trend.Price3;
                }
                else
                {
                    Common.Log.WriteLog("获取二手车车款估价数据错误，carid=" + carId + ";返回结果：" + responseStr);
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("获取二手车估价接口错误， carid= " + carId + ";信息：" + ex.ToString());
            }
            finally
            {
                if (reader != null) reader.Close();
                if (responseStream != null) responseStream.Close();
                if (response != null) response.Close();
            }
            return price3;
        }

        private double GetKoubeirating(int carId)
        {
            if (CommonData._koubeiRatingDic != null && CommonData._koubeiRatingDic.ContainsKey(carId))
            {
                return Math.Round(ConvertHelper.GetDouble(CommonData._koubeiRatingDic[carId]["Ratings"]), 2);
            }
            return 0;
        }
        #region 保险详细值计算
        

        /// <summary>
        /// 保险总数
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private int Baoxian(CarCompareEntity entity)
        {
            int calcTPLValue = calcTPL(entity.SeatNum);
            int calcCarDamageValue = ConvertHelper.GetInteger(calcCarDamage(entity.SeatNum, entity.ReferPrice));
            int calcAbatementValue = ConvertHelper.GetInteger(calcAbatement(calcTPLValue, calcCarDamageValue));
            int calcCarTheftValue = ConvertHelper.GetInteger(calcCarTheft(entity.SeatNum, entity.ReferPrice));
            int calcBreakageOfGlassValue = ConvertHelper.GetInteger(calcBreakageOfGlass(entity.IsGuoChan, entity.SeatNum, entity.ReferPrice));
            int calcSelfigniteValue = ConvertHelper.GetInteger(calcSelfignite(entity.ReferPrice));
            int calcCarexhaustforfloatDamageValue = ConvertHelper.GetInteger(calcCarexhaustforfloatDamage(calcCarDamageValue));
            int calcCarDamageDWValue = ConvertHelper.GetInteger(calcCarDamageDW(entity.ReferPrice));
            int calcLimitofDriverValue = ConvertHelper.GetInteger(calcLimitofDriver(entity.SeatNum));
            int calcLimitofPassengerValue = ConvertHelper.GetInteger(calcLimitofPassenger(entity.SeatNum));
            return calcTPLValue + calcCarDamageValue + calcAbatementValue + calcCarTheftValue + calcBreakageOfGlassValue + calcSelfigniteValue
                + calcCarexhaustforfloatDamageValue + calcCarDamageDWValue + calcLimitofDriverValue + calcLimitofPassengerValue;
        }

        /// <summary>
        /// 车船税
        /// </summary>
        /// <param name="carId"></param>
        /// <param name="exhaustforfloat"></param>
        /// <returns></returns>
        private double CalculateVehicleAndVesselTax(int carId, double exhaustforfloat, string travelTax)
        {
            double tex = 0;
            if (exhaustforfloat > 0 && exhaustforfloat <= 1)
            {
                tex = 100;
            }
            else if (exhaustforfloat <= 1.6)
            {
                tex = 420;
            }
            else if (exhaustforfloat <= 2.0)
            {
                tex = 480;
            }
            else if (exhaustforfloat <= 2.5)
            {
                tex = 900;
            }
            else if (exhaustforfloat <= 3.0)
            {
                tex = 1920;
            }
            else if (exhaustforfloat <= 4.0)
            {
                tex = 3480;
            }
            else if (exhaustforfloat > 4.0)
            {
                tex = 5280;
            }
            if (travelTax == "免征")
            {
                tex = 0;
            }
            else if (travelTax == "减半")
            {
                tex = Math.Round(tex / 2);
            }
            return tex;
        }

        /// <summary>
        /// 购置税
        /// </summary>
        /// <returns></returns>
        private double calcAcquisitionTax(double referPrice, double exhaustforfloat)
        {
            double acquisitionTax = (referPrice * 10000) / (1.17) * 0.1;
            if (exhaustforfloat > 0 && exhaustforfloat <= 1.6 && DateTime.Now > DateTime.Parse("2015-10-01") && DateTime.Now < DateTime.Parse("2017-01-01"))
            {
                acquisitionTax = acquisitionTax / 2;
            }
            return Math.Round(acquisitionTax);
        }

        /// <summary>
        /// 第三方责任险(默认赔付额度20W)
        /// </summary>
        private int calcTPL(int seatNum)
        {
            if (seatNum < 6)
            {
                return 1270;
            }
            return 1131;
        }

        /// <summary>
        /// 车辆损失险
        /// </summary>
        private double calcCarDamage(int seatNum, double referPrice)
        {
            double rate = 0.0095;
            int baseCost = 285;
            if (seatNum >= 6 && seatNum < 10)
            {
                rate = 0.009;
                baseCost = 342;
            }
            else if (seatNum >= 10 && seatNum < 20)
            {
                rate = 0.0095;
                baseCost = 342;
            }
            else if (seatNum >= 20)
            {
                rate = 0.0095;
                baseCost = 357;
            }
            return Math.Round(referPrice * 10000 * rate + baseCost);
        }

        /// <summary>
        /// 不计免赔特约险
        /// </summary>
        private double calcAbatement(int calcTPL, int calcCarDamage)
        {
            return Math.Round((calcTPL + calcCarDamage) * 0.2);
        }

        /// <summary>
        /// 全车盗抢险
        /// </summary>
        private double calcCarTheft(int seatNum, double referPrice)
        {
            if (seatNum < 6)
            {
                return Math.Round(referPrice * 10000 * 0.0049 + 120);
            }
            else
            {
                return Math.Round(referPrice * 10000 * 0.0044 + 140);
            }
        }
        /// <summary>
        /// 玻璃单独破碎险
        /// </summary>
        private double calcBreakageOfGlass(bool isGuoChan, int seatNum, double referPrice)
        {
            if (isGuoChan)
            {
                return Math.Round(referPrice * 10000 * 0.0019);
            }
            else
            {
                if (seatNum < 6)
                {
                    return Math.Round(referPrice * 10000 * 0.0031);
                }
                return Math.Round(referPrice * 10000 * 0.003);
            }
        }

        /// <summary>
        /// 自燃损失险
        /// </summary>
        private double calcSelfignite(double referPrice)
        {
            return Math.Round(referPrice * 10000 * 0.0015);
        }
        /// <summary>
        /// 涉水险/发动机特别损失险
        /// </summary>
        private double calcCarexhaustforfloatDamage(int calcCarDamage)
        {
            return Math.Round(calcCarDamage * 0.05);
        }
        /// <summary>
        /// 车身划痕险(赔付额度，默认5000)
        /// </summary>
        private int calcCarDamageDW(double referPrice)
        {
            if (referPrice * 10000 < 300000)
            {
                return 570;
            }
            else if (referPrice * 10000 > 500000)
            {
                return 1100;
            }
            return 900;
        }
        /// <summary>
        /// 司机座位责任险(赔付额度，默认20000)
        /// </summary>
        private double calcLimitofDriver(int seatNum)
        {
            if (seatNum < 6)
            {
                return Math.Round(20000 * 0.0042);
            }
            return Math.Round(20000 * 0.004);
        }

        /// <summary>
        /// 乘客责任险(赔付额度，默认20000)
        /// </summary>
        private double calcLimitofPassenger(int seatNum)
        {
            int calCount = seatNum < 4 ? 4 : seatNum - 1;
            if (seatNum < 6)
            {
                return Math.Round(20000 * 0.0027 * calCount);
            }
            return Math.Round(20000 * 0.0026 * calCount);
        }

        //车牌费500
        #endregion

        #region 获取车型基础信息
        /// <summary>
        /// 获取车款列表
        /// </summary>
        /// <param name="carId"></param>
        /// <returns></returns>
        private List<CarCompareEntity> GetCarList()
        {
            DataSet dsCar = GetAllCarBaseInfo();
            if (dsCar == null || dsCar.Tables.Count == 0 || dsCar.Tables[0].Rows.Count == 0)
            {
                Common.Log.WriteLog("没有查到车款数据");
                return null;
            }
            DataSet dsParmas = GetAllCarParams(785, 665, 895);//查询排量和座位数
            if (dsParmas == null || dsParmas.Tables.Count == 0 || dsParmas.Tables[0].Rows.Count == 0)
            {
                Common.Log.WriteLog("没有查到车款参数信息");
                return null;
            }
            List<CarCompareEntity> list = new List<CarCompareEntity>();
            DataTable dtParams = dsParmas.Tables[0];
            foreach (DataRow dr in dsCar.Tables[0].Rows)
            {
                DataRow[] drs = dtParams.Select(" carid = " + dr["car_id"]);
                double exhaustforfloat = 0;
                int seatNum = 0;
                string travelTax = string.Empty;
                if (drs.Length > 0)
                {
                    foreach (DataRow drParma in drs)
                    {
                        int paramId = ConvertHelper.GetInteger(drParma["ParamId"]);
                        if (paramId == 785)// 排量
                        {
                            exhaustforfloat = ConvertHelper.GetDouble(drParma["Pvalue"]);
                        }
                        else if (paramId == 665)// 座位数
                        {
                            seatNum = ConvertHelper.GetInteger(drParma["Pvalue"]);
                        }
                        else if (paramId == 895)
                        {
                            travelTax = drParma["Pvalue"].ToString();
                        }
                    }
                }
                CarCompareEntity carCompareEntity = new CarCompareEntity();
                carCompareEntity.CarId = ConvertHelper.GetInteger(dr["car_id"]);
                carCompareEntity.SerialId = ConvertHelper.GetInteger(dr["cs_id"]);
                carCompareEntity.Exhaustforfloat = exhaustforfloat;
                carCompareEntity.SeatNum = seatNum;
                carCompareEntity.IsGuoChan = dr["Cp_Country"].ToString() == "中国" ? true : false;
                carCompareEntity.ReferPrice = ConvertHelper.GetDouble(dr["car_ReferPrice"]);
                list.Add(carCompareEntity);
            }
            return list;
        }

        /// <summary>
        /// 获取所有车款信息
        /// </summary>
        /// <returns></returns>
        private DataSet GetAllCarBaseInfo()
        {
            try
            {
                DataSet ds = new DataSet();
                string sqlCarBaseInfo = @"SELECT  car.car_id, car.car_ReferPrice, cb.cb_Country AS Cp_Country, car.cs_id
                                            FROM    car_basic car
                                                    LEFT JOIN Car_serial cs ON car.cs_id = cs.cs_id
                                                    LEFT JOIN Car_Brand cb ON cs.cb_id = cb.cb_id
                                            WHERE   car.isState = 1
                                                    AND cs.isState = 1
                                                    AND ( cs.CsSaleState = '在销'
                                                          OR cs.CsSaleState = '待销'
                                                        )";
                ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(BitAuto.CarDataUpdate.Common.CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sqlCarBaseInfo, null);
                return ds;
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("车款对比，查询所有车款信息报错：" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取相关参数信息
        /// </summary>
        /// <param name="paramIds"></param>
        /// <returns></returns>
        private DataSet GetAllCarParams(params int[] paramIds)
        {
            try
            {
                DataSet ds = new DataSet();
                string sql = string.Format(@"select cdb.carid,cdb.ParamId,cdb.Pvalue
									from CarDataBase cdb
									where cdb.ParamId in ({0})", string.Join(",", paramIds));
                ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(BitAuto.CarDataUpdate.Common.CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, null);
                return ds;
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("车款对比，查询所有车款参数信息报错：" + ex.ToString());
                return null;
            }
        }
        #endregion
    }

    /// <summary>
    /// 车型对比实体
    /// </summary>
    internal class CarCompareEntity
    {
        /// <summary>
        /// 车款id
        /// </summary>
        public int CarId { get; set; }
        /// <summary>
        /// 子品牌id
        /// </summary>
        public int SerialId { get; set; }
        /// <summary>
        /// 是否国产
        /// </summary>
        public bool IsGuoChan { get; set; }
        /// <summary>
        /// 排量
        /// </summary>
        public double Exhaustforfloat { get; set; }
        /// <summary>
        /// 座位数
        /// </summary>
        public int SeatNum { get; set; }
        /// <summary>
        /// 指导价
        /// </summary>
        public double ReferPrice { get; set; }

        /// <summary>
        /// 车船税
        /// </summary>
        public string TravelTax { get; set; }
    }
    internal class CarGuJia
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("data")]
        public CarGuJiaData Data { get; set; }
    }
    internal class CarGuJiaData
    {
        [JsonProperty("b2cPrice")]
        public double B2cPrice { get; set; }
        [JsonProperty("c2bPrice")]
        public double C2bPrice { get; set; }
        [JsonProperty("c2cPrice")]
        public double C2cPrice { get; set; }

        [JsonProperty("serialImgUrl")]
        public string SerialImgUrl { get; set; }

        [JsonProperty("trend")]
        public CarGuJiaTrend Trend { get; set; }
    }

    internal class CarGuJiaTrend
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("price1")]
        public double Price1 { get; set; }

        [JsonProperty("price2")]
        public double Price2 { get; set; }

        [JsonProperty("price3")]
        public double Price3 { get; set; }
    }
}
