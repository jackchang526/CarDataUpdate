using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common
{
    public class RedisManager
    {
        /// <summary>
        /// redis配置节点信息
        /// </summary>
        private static RedisConfigInfo redisConfigInfo = RedisConfigInfo.GetConfig();
        /// <summary>
        /// master服务器
        /// </summary>
        private static readonly string[] WriteServerListArray = redisConfigInfo.WriteServerList.Split(',');
        /// <summary>
        /// slave服务器
        /// </summary>
        private static readonly string[] ReadServerListArray = redisConfigInfo.ReadServerList.Split(',');

        /// <summary>
        /// redis中key的前缀
        /// </summary>
        public static readonly string PreKey = redisConfigInfo.PreKey;

        private static PooledRedisClientManager _prcm = null;
        private static object LockObj = new object();
        public static PooledRedisClientManager Prcm
        {
            get
            {
                if (_prcm == null)
                {
                    lock (LockObj)
                    {
                        if (_prcm == null)
                        {
                            RedisClientManagerConfig config = new RedisClientManagerConfig()
                            {
                                AutoStart = redisConfigInfo.AutoStart,
                                MaxWritePoolSize = redisConfigInfo.MaxWritePoolSize,
                                MaxReadPoolSize = redisConfigInfo.MaxReadPoolSize,
                                DefaultDb = redisConfigInfo.DefaultDatabase
                            };
                            _prcm = new PooledRedisClientManager(WriteServerListArray, ReadServerListArray, config);
                        }
                    }
                }
                return _prcm;
            }
        }
    }

        /// <summary>
    /// redis配置节点信息
    /// </summary>
    public sealed class RedisConfigInfo : ConfigurationSection
    {
        public static RedisConfigInfo GetConfig()
        {
            RedisConfigInfo section = (RedisConfigInfo)ConfigurationManager.GetSection("RedisConfig");
            return section;
        }


        /// <summary>
        /// 可写的Redis链接地址
        /// </summary>
        [ConfigurationProperty("WriteServerList", IsRequired = true)]
        public string WriteServerList
        {
            get
            {
                return BitAuto.Utils.ConvertHelper.GetString(base["WriteServerList"]);
            }
            set
            {
                base["WriteServerList"] = value;
            }
        }

        /// <summary>
        /// 可读的Redis链接地址
        /// </summary>
        [ConfigurationProperty("ReadServerList", IsRequired = true)]
        public string ReadServerList
        {
            get
            {
                return BitAuto.Utils.ConvertHelper.GetString(base["ReadServerList"]);
            }
            set
            {
                base["ReadServerList"] = value;
            }
        }

        /// <summary>
        /// 最大写链接数
        /// </summary>
        [ConfigurationProperty("MaxWritePoolSize", IsRequired = false, DefaultValue = 5)]
        public int MaxWritePoolSize
        {
            get
            {
                int _maxWritePoolSize = BitAuto.Utils.ConvertHelper.GetInteger(base["MaxWritePoolSize"]);
                return _maxWritePoolSize > 0 ? _maxWritePoolSize : 5;
            }
            set
            {
                base["MaxWritePoolSize"] = value;
            }
        }

        /// <summary>
        /// 最大读链接数
        /// </summary>
        [ConfigurationProperty("MaxReadPoolSize", IsRequired = false, DefaultValue = 5)]
        public int MaxReadPoolSize
        {
            get
            {
                int _maxReadPoolSize = BitAuto.Utils.ConvertHelper.GetInteger(base["MaxReadPoolSize"]);
                return _maxReadPoolSize > 0 ? _maxReadPoolSize : 5;
            }
            set
            {
                base["MaxReadPoolSize"] = value;
            }
        }

        /// <summary>
        /// 自动重启
        /// </summary>
        [ConfigurationProperty("AutoStart", IsRequired = false, DefaultValue = true)]
        public bool AutoStart
        {
            get
            {
                return BitAuto.Utils.ConvertHelper.GetBoolean(base["AutoStart"]);
            }
            set
            {
                base["AutoStart"] = value;
            }
        }

        /// <summary>
        /// 默认数据库
        /// </summary>
        [ConfigurationProperty("DefaultDatabase", IsRequired = false, DefaultValue = 0)]
        public int DefaultDatabase
        {
            get
            {
                return BitAuto.Utils.ConvertHelper.GetInteger(base["DefaultDatabase"]);
            }
            set
            {
                base["DefaultDatabase"] = value;
            }
        }

        /// <summary>
        /// key的前缀
        /// </summary>
        [ConfigurationProperty("PreKey", IsRequired = false, DefaultValue = "SelectCar")]
        public string PreKey
        {
            get
            {
                return BitAuto.Utils.ConvertHelper.GetString(base["PreKey"]);
            }
            set
            {
                base["PreKey"] = value;
            }
        }
    }
}
