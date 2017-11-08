using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BitAuto.CarDataUpdate.Config
{
    /// <summary>
    /// 数据库连接字符串配置
    /// </summary>
    public class ConnStringSettings : ConfigurationSection
    {
        /// <summary>
        /// 新闻库连接字符串
        /// </summary>
        [ConfigurationProperty("CarDataUpdateConnString")]
        public string CarDataUpdateConnString
        {
            get { return (string)base["CarDataUpdateConnString"]; }
        }
        /// <summary>
        /// 车型库后台连接字符串
        /// </summary>
        [ConfigurationProperty("AutoStroageConnString")]
        public string AutoStroageConnString
        {
            get { return (string)base["AutoStroageConnString"]; }
        }
        /// <summary>
        /// 车型频道数据库连接字符串
        /// </summary>
        [ConfigurationProperty("CarChannelConnString")]
        public string CarChannelConnString
        {
            get { return (string)base["CarChannelConnString"]; }
        }

		/// <summary>
		/// 车型频道管理后台数据库连接字符串
		/// </summary>
		[ConfigurationProperty("CarChannelManageConnString")]
		public string CarChannelManageConnString
		{
			get { return (string)base["CarChannelManageConnString"]; }
		}

        [ConfigurationProperty("CarsEvaluationConnString")]
        public string CarsEvaluationConnString
        {
            get { return (string)base["CarsEvaluationConnString"]; }
        }

        /// <summary>
        /// MongoDBConnectionString
        /// </summary>
        [ConfigurationProperty("MongoDBConnectionString")]
        public string MongoDBConnectionString
        {
            get { return (string)base["MongoDBConnectionString"]; }
        }

		[ConfigurationProperty("BuyCarServiceConnectionString")]
		public string BuyCarServiceConnectionString
		{
			get { return (string)base["BuyCarServiceConnectionString"]; }
		}

        [ConfigurationProperty("MongoDBCarsEvaluationConnString")]
        public string MongoDBCarsEvaluationConnString
        {
            get { return (string)base["MongoDBCarsEvaluationConnString"]; }
        }
    }
}
