using System;
using System.Xml.Linq;
using System.Collections;
using System.Reflection;
using BitAuto.CarDataUpdate.Common;
using System.IO;

namespace BitAuto.CarDataUpdate.WebService.DataSync
{
    public class DataSyncProvider
	{
		/// <summary>
		/// xml配置
		/// </summary>
		static XElement _dataSyncList;
		static XElement DataSyncList
		{
			get
			{
				if (_dataSyncList == null)
				{
					_dataSyncList = XElement.Load(
						Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\DataSync.xml")
						);
				}
				return _dataSyncList;
			}
		}

		private static object lockForMethodInfos = new object();

		public static void IntiMethodInfos()
		{
			if (_methodInfos == null)
			{
				lock (lockForMethodInfos)
				{
					_methodInfos = new Hashtable();//Hashtable.Synchronized(new Hashtable());
					foreach (var item in DataSyncList.Elements("item"))
					{
						if (!_methodInfos.ContainsKey(item.Element("key").Value))
						{
							Type supType = Type.GetType(Assembly.CreateQualifiedName("WebServiceBLL", item.Element("sourceClass").Value));

							_methodInfos[item.Element("key").Value] = new object[] 
							{ 
                                Activator.CreateInstance(supType), 
                                supType.GetMethod(item.Element("sourceFunction").Value) 
							};
						}
					}
				}
			}
		}

		/// <summary>
		/// 对应方法hashtable
		/// </summary>
		static Hashtable _methodInfos;
		static Hashtable MethodInfos
		{
			get
			{
				if (_methodInfos == null)
				{
					lock (lockForMethodInfos)
					{
						_methodInfos = new Hashtable();//Hashtable.Synchronized(new Hashtable());
						foreach (var item in DataSyncList.Elements("item"))
						{
							if (!_methodInfos.ContainsKey(item.Element("key").Value))
							{
								Type supType = Type.GetType(Assembly.CreateQualifiedName("WebServiceBLL", item.Element("sourceClass").Value));

								_methodInfos[item.Element("key").Value] = new object[] 
							{ 
                                Activator.CreateInstance(supType), 
                                supType.GetMethod(item.Element("sourceFunction").Value) 
							};
							}
						}
					}
				}
				return _methodInfos;
			}
		}

		/// <summary>
		/// 接收过来的消息反射到方法
		/// </summary>
		/// <param name="message">传过来的message.body的xml的格式的内容</param>
		public static void Execut(XElement message)
		{
			XElement messageHearder = message.Element("Header");

			string appId = messageHearder.Element("AppId").Value;
			string entityType = messageHearder.Element("EntityType").Value;
			string operateType = messageHearder.Element("OperateType").Value;

			string key = string.Format("{0}_{1}_{2}", appId, entityType, operateType);

			if (MethodInfos != null && MethodInfos.ContainsKey(key))
			{
				Log.WriteLog(string.Format("开始执行消息处理。key:[{0}]", key));

				object[] method = (object[])MethodInfos[key];
				MethodInfo methodInfo = (MethodInfo)method[1];

				methodInfo.Invoke(method[0], new object[] { message.Element("Body") });

				Log.WriteLog(string.Format("结束执行消息处理。key:[{0}]", key));
			}
			else
			{
				Log.WriteLog(string.Format("没有匹配的执行程序。key:[{0}]  {1}"
					, key, (MethodInfos == null ? "MethodInfos==null" : "MethodInfos.Count:" + MethodInfos.Count.ToString())));
			}
		}
		
		/// <summary>
		/// 获取帖子的实体
		/// </summary>
		/// <param name="appId">请求业务线的appid</param>
		/// <param name="entityType">操作类型</param>
		/// <param name="entityId">postInfo的guid</param>
		/// <returns></returns>
		public static string GetPostInfo(int appId, string entityType, string entityId)
		{
			string result = string.Empty;
			string key = string.Format("{0}_{1}_{2}",
				appId, entityType, "Get");

			if (MethodInfos != null && MethodInfos.ContainsKey(key))
			{
				Log.WriteLog(string.Format("开始执行消息处理。key:[{0}]", key));

				object[] method = (object[])MethodInfos[key];
				MethodInfo methodInfo = (MethodInfo)method[1];
				result = methodInfo.Invoke(method[0], new object[] { entityId }).ToString();

				Log.WriteLog(string.Format("结束执行消息处理。key:[{0}]", key));
			}
			else
			{
				Log.WriteLog(string.Format("没有匹配的执行程序。key:[{0}] {1}"
					, key, (MethodInfos == null ? "MethodInfos==null" : "MethodInfos.Count:" + MethodInfos.Count.ToString())));
			}
			return result;
		}
	}
}