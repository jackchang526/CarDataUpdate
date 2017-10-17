using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BitAuto.Utils;
using MongoDB.Driver;
using BitAuto.CarDataUpdate.Common;
using MongoDB.Bson;
using System.Configuration;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.dealer.api;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 获取经销商google地图坐标
    /// </summary>
    public class VendorListMapInfor
    {
        private const string _Column_Id = "_id";
        private const string _Column_VendorID = "vendorID";
        private const string _Column_GoogleMapLat = "GoogleMapLat";
        private const string _Column_GoogleMapLng = "GoogleMapLng";

        private const string _DataBaseName = "DealerPrice";
        private const string _CollectionName = "DealerGoogleMap";

        List<BsonDocument> _List;

        public event LogHandler Log;

        public VendorListMapInfor()
        {
        }
        /// <summary>
        /// 获取经销商google地图坐标
        /// </summary>
        public void GetVendorListMapInfor()
        {
            OnLog("		开始获取经销商google地图坐标", true);
            if (GetList())
            {
                CreateData();
            }
            OnLog("		结束获取经销商google地图坐标", true);
        }

        private void CreateData()
        {
            try
            {
                OnLog("数据更新至mongodb中...", true);
                MongoServer server = MongoServer.Create(CommonData.ConnectionStringSettings.MongoDBConnectionString);
                MongoDatabase database = server.GetDatabase(_DataBaseName);
                var dealers = database.GetCollection(_CollectionName);
                if (dealers == null)
                {
                    CommandResult result = database.CreateCollection(_CollectionName);
                    if (result.Ok)
                    {
                        dealers = database.GetCollection(_CollectionName);
                    }
                    else
                    {
                        OnLog("创建mongodbCollection失败！错误信息：" + result.ErrorMessage, true);
                        return;
                    }
                }
                else if (dealers.Count() > 0)
                {
                    dealers.RemoveAll(SafeMode.True);
                }
                if (_List != null && _List.Count > 0)
                {
                    dealers.InsertBatch(_List, SafeMode.True);
                    dealers.CreateIndex("vendorID");
                }

                OnLog("完成 数据更新至mongodb...", true);
            }
            catch (Exception exp)
            {
                OnLog("mongodbCollection交互中出现异常！错误信息：" + exp.Message, true);
            }
        }

        private bool GetList()
        {
            try
            {
                OnLog("通过webservice获取数据中...", true);
                _List = new List<BsonDocument>();
                BsonDocument bsonTable;
                DataSet dataset = new VendorInfor().GetVendorListMapInfor();
                if (dataset != null && dataset.Tables.Count > 0 && dataset.Tables[0].Rows.Count > 0)
                {
                    int vendorId;
                    double lat, lng;
                    DataRowCollection rows = dataset.Tables[0].Rows;
                    foreach (DataRow row in rows)
                    {
                        //<Table>
                        //    <vendorID>3301124</vendorID>
                        //    <GoogleMapLat>22.780311455342637</GoogleMapLat>
                        //    <GoogleMapLng>108.31776022911072</GoogleMapLng>
                        //</Table>
                        vendorId = ConvertHelper.GetInteger(row["vendorID"]);
                        if (vendorId > 0)
                        {
                            lat = ConvertHelper.GetDouble(row["GoogleMapLat"]);
                            lng = ConvertHelper.GetDouble(row["GoogleMapLng"]);
                            if (lat <= 0 && lng <= 0)
                                continue;

                            bsonTable = new BsonDocument();
                            bsonTable.Add(_Column_VendorID, new BsonInt32(vendorId));
                            bsonTable.Add(_Column_GoogleMapLat, new BsonDouble(lat));
                            bsonTable.Add(_Column_GoogleMapLng, new BsonDouble(lng));
                            _List.Add(bsonTable);
                        }
                    }
                }
                OnLog("完成 通过webservice获取数据...", true);
                return true;
            }
            catch (Exception exp)
            {
                OnLog("获取坐标出现异常：" + exp.Message, true);
                return false;
            }
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
