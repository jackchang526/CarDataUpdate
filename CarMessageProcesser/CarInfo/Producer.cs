using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.CarInfo
{
	public class Producer : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			Log.WriteLog(msg.ContentBody.OuterXml);
		}
	}
}
