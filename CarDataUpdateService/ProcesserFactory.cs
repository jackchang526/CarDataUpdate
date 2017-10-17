using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using System.Xml;
using System.Reflection;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.Service
{
    public static class ProcesserFactory
    {
        private static Dictionary<string, BaseProcesser> _ProcesserPool;
        private readonly static string _ConfigStr = "ContentType_";
        static ProcesserFactory()
        {
            _ProcesserPool = new Dictionary<string, BaseProcesser>(1);
        }
        public static BaseProcesser CreateProcesser(string contentType)
        {
            BaseProcesser result = null;
            if (!string.IsNullOrEmpty(contentType))
            {
                contentType = contentType.ToLower();
                if (_ProcesserPool.ContainsKey(contentType))
                    return _ProcesserPool[contentType];

				if (System.Configuration.ConfigurationManager.AppSettings[_ConfigStr + contentType] != null)
				{
					string typeStr = System.Configuration.ConfigurationManager.AppSettings[_ConfigStr + contentType];

					Type type = Type.GetType(typeStr, false, true);
					if (type != null)
					{
						try
						{
							result = type.InvokeMember(null, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null) as BaseProcesser;
							result.DelayEvent += new DelayEventHandler(DelayProcesser.DelayEventProcesser);
							//result = Assembly.GetAssembly(type).CreateInstance(type.FullName) as IProcesser;
							_ProcesserPool.Add(contentType, result);
						}
						catch (Exception exp)
						{
							Log.WriteErrorLog(exp);
						}
					}
				}
            }
            return result;
        }
    }
}
