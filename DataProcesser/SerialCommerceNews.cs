using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 第四级 子品牌 商配新闻
    /// </summary>
    public class SerialCommerceNews
    {
        public event LogHandler Log;
        private Dictionary<int, string> _serialList = new Dictionary<int, string>();
        /// <summary>
        /// 生成商配新闻
        /// </summary>
        /// <param name="serialId"></param>
        public void GetSerialSerialCommerceNewsData(int serialId)
        {
            InitSerialList(serialId);
            GenerateNewsXml();
            //补商配新闻
            GenerateBackupNewsXml();
        }
        /// <summary>
        /// 初始化子品牌列表
        /// </summary>
        /// <param name="serialId"></param>
        private void InitSerialList(int serialId)
        {
            OnLog("初始化 车型列表", false);
            DataSet ds = null;
            if (serialId <= 0)
            {
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, @"select cs_id, isnull(cs_seoname, csName) as cs_seoname from Car_Serial where isState=0");
            }
            else
            {
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, @"select cs_id, isnull(cs_seoname, csName) as cs_seoname from Car_Serial where isState=0 and cs_id=@cs_id", new SqlParameter("@cs_id", serialId));
            }
            if (ds == null || ds.Tables.Count < 1)
                return;

            DataRowCollection rows = ds.Tables[0].Rows;
            if (rows.Count < 1)
                return;
            int tempSerialId;
            foreach (DataRow row in rows)
            {
                tempSerialId = ConvertHelper.GetInteger(row["cs_id"].ToString());
                _serialList.Add(tempSerialId, row["cs_seoname"].ToString());
            }
            OnLog(string.Format("初始化 车型列表 完成。获取数量:{0}", _serialList.Count), false);
        }
        /// <summary>
        /// 生成后补商配新闻XML
        /// </summary>
        private void GenerateBackupNewsXml()
        {
            if (_serialList.Count < 1)
            {
                return;
            }
            string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\CommerceNewsBackup\\Xml");
            foreach (var ser in _serialList)
            {
                try
                {
                    OnLog("生成 后补商配新闻 XML：" + ser.Key.ToString(), true);
                    string xmlFile = "Serial_CommerceNews_Backup_" + ser.Key.ToString() + ".xml"; 
                    xmlFile = Path.Combine(newsPath, xmlFile);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(string.Format(CommonData.CommonSettings.BackupCommerceNewsUrl, ser.Key.ToString()));

                    CommonFunction.SaveXMLDocument(doc, xmlFile);
                }
                catch (Exception exp)
                {
                    OnLog("Get BackupNews Error (seriald:" + ser.Key.ToString() + ";Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...", false);
                }
            }
        }
        /// <summary>
        /// 生成商配XML
        /// </summary>
        private void GenerateNewsXml()
        {
            if (_serialList.Count < 1)
            {
                return;
            }
            string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\CommerceNews\\Xml");
            foreach (var ser in _serialList)
            {
                try
                {
                    OnLog("生成 商配新闻 XML：" + ser.Key.ToString(), true);
                    string xmlFile = "Serial_CommerceNews_" + ser.Key.ToString() + ".xml";
                    xmlFile = Path.Combine(newsPath, xmlFile);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(string.Format(CommonData.CommonSettings.SerialCommerceNewsUrl, ser.Key.ToString()));

                    CommonFunction.SaveXMLDocument(doc, xmlFile);
                }
                catch (Exception exp)
                {
                    OnLog("Get NewsXml Error (seriald:" + ser.Key.ToString() + ";Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...", false);
                }
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
