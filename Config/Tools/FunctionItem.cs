using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BitAuto.CarDataUpdate.Config.Tools
{
    public class FunctionItem : ConfigurationElement
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        [ConfigurationProperty("FuncName", IsRequired = true)]
        public string FunctionName
        {
            get{return (string)base["FuncName"];}
        }
        /// <summary>
        /// 说明
        /// </summary>
        [ConfigurationProperty("Description", IsRequired = true)]
        public string Description
        {
            get { return (string)base["Description"]; }
        }
    }
}
