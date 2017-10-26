using BitAuto.CarDataUpdate.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class RequestCarserInterface
	{
		private string messageAddress = "http://carser.bitauto.com/ProcessMessage.aspx?MessageText=MessageType:SyncData|TargetObject:{0}|ObjectIdentity:{1}|ActionName:{2}";
		/// <summary>
		/// 发送同步易车业务线的消息
		/// </summary>
		/// <param name="type">操作的实体类型</param>
		/// <param name="id">实体编号</param>
		/// <param name="opearting">操作动作</param>
		public void Send(string type, int id, string opearting)
		{
			try
			{
				string messageAddressTemp = string.Format(messageAddress, type, id, opearting);

				var result = CommonFunction.GetResponseFromUrl(messageAddressTemp);// Utility.GetHttpRequestData(messageAddress, 20 * 1000);

				if (result.Trim() != "True")
				{
					Common.Log.WriteErrorLog(string.Format("同步接口返回消息不是true，返回的信息:{0},URL地址{1}", result, messageAddressTemp));
				}
				Common.Log.WriteLog(string.Format("调用易车接口成功\r\n 实体类型:{0}\r\n 操作类型:{1}\r\n 类型编号:{2}\r\n", type, opearting, id));
			}
			catch (Exception)
			{
				Common.Log.WriteLog(string.Format("调用易车接口失败\r\n 实体类型:{0}\r\n 操作类型:{1}\r\n 类型编号:{2}\r\n", type, opearting, id));
			}
			
		}

		public void RequestCarSer(List<int> carIdList)
		{
			foreach (int carid in carIdList)
			{
				Send("car", carid, "Update");
			}
		}
	}
}
