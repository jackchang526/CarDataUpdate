using BitAuto.CarUtils;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Services
{
    /// <summary>
    /// 超级评测相关
    /// </summary>
    public class CarEvaluationService
    {
        private const string _DataBaseName = "CarsEvaluationReport";
        private const string _CollectionName = "assessmentdata";

        private static MongoCursor<BsonDocument> GetList()
        {
            MongoCursor<BsonDocument> mongoCursor = null;
            try
            {
                IMongoQuery query = Query.EQ("Status", 1);
                List<string> paraList = new List<string>
                {
                    "SerialId",
                    "CarId",
                    "Status",
                    "EvaluationId",
                    "CreateDateTime"
                };
                mongoCursor =BitAuto.CarUtils.MongoDB.MongoDBHelper.GetFields(CommonData.ConnectionStringSettings.MongoDBCarsEvaluationConnString, _DataBaseName, _CollectionName, query, paraList.ToArray());               
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("获取所有超级评测报告报错列表报错：" + ex.ToString());
            }
            return mongoCursor;
        }

        /// <summary>
        /// 从评测报告数据库中获取车系下车款的最新评测报告
        /// </summary>
        /// <returns></returns>
        public static List<CarEvaluationReport> GetNewCarEvaluationReport()
        {
            List<CarEvaluationReport> target = new List<CarEvaluationReport>();
            List<CarEvaluationReport> list = new List<CarEvaluationReport>();
            List<int> esixt = new List<int>();
            MongoCursor<BsonDocument> mongoCursor = GetList();
            try
            {
                foreach (BsonDocument item in mongoCursor)
                {
                    int serialId = item["SerialId"].AsInt32;
                    int evaluationId = item["EvaluationId"].AsInt32;
                    DateTime createDateTime = item["CreateDateTime"].ToUniversalTime();
                    //排重
                    if (!esixt.Contains(serialId))
                    {
                        esixt.Add(serialId);
                    }
                    CarEvaluationReport carEvaluationReport = new CarEvaluationReport();
                    carEvaluationReport.EvaluationId = evaluationId;
                    carEvaluationReport.SerialId = serialId;
                    carEvaluationReport.CreateDateTime = createDateTime;
                    list.Add(carEvaluationReport);
                }
                //取最新
                foreach (int item in esixt)
                {
                    CarEvaluationReport temp = list.Where(i => i.SerialId == item).OrderByDescending(j => j.CreateDateTime).First();
                    target.Add(temp);
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("从评测报告数据库中获取车系下车款的最新评测报告报错：" + ex.ToString());
                return null;
            }
            return target;
        }

        /// <summary>
        /// 获取评测报告和车款对应关系字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetEvaluationReportDicForCar()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            MongoCursor<BsonDocument> mongoCursor = GetList();
            try
            {
                foreach (BsonDocument item in mongoCursor)
                {
                    int carId = item["CarId"].AsInt32;
                    int serialId = item["SerialId"].AsInt32;
                    int evaluationId = item["EvaluationId"].AsInt32;
                    DateTime createDateTime = item["CreateDateTime"].ToUniversalTime();
                    dic.Add(evaluationId, carId);
                }
               
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("获取评测报告和车款对应关系字典报错：" + ex.ToString());
            }
            return dic;
        }
    }
}