using BitAuto.CarDataUpdate.Common;
using BitAuto.CarUtils.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 超级评测相关
    /// </summary>
    public class CarEvaluation
    {
        private const string _DataBaseName = "CarsEvaluationReport";
        private const string _CollectionName = "assessmentdata";

        public static List<CarEvaluationReport> GetList()
        {
            List<CarEvaluationReport> target = new List<CarEvaluationReport>();

            List<CarEvaluationReport> list = new List<CarEvaluationReport>();            
            List<int> esixt = new List<int>();
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
                MongoCursor<BsonDocument> mongoCursor = MongoDBHelper.GetFields(CommonData.ConnectionStringSettings.MongoDBCarsEvaluationConnString, _DataBaseName, _CollectionName, query,paraList.ToArray());

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


                foreach (int item in esixt)
                {
                    CarEvaluationReport temp = list.Where(i => i.SerialId == item).OrderByDescending(j => j.CreateDateTime).First();
                    target.Add(temp);
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("超级评测报告报错：" + ex.ToString());
                return null;
            }
            return target;
        }
    }

}
