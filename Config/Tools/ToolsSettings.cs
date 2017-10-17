using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BitAuto.CarDataUpdate.Config.Tools
{
    /// <summary>
    /// 定时服务配置类
    /// </summary>
    public class ToolsSettings : ConfigurationSection
    {
        /// <summary>
        /// 定时执行方法类表
        /// </summary>
        [ConfigurationProperty("FunctionCollection")]
        public FunctionCollection FunctionCollection
        {
            get { return (FunctionCollection)base["FunctionCollection"]; }
        }
    }
}
