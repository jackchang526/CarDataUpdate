using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.HtmlBuilder;

namespace BitAuto.CarDataUpdate.CarMessageProcesser
{
	public class SerialPingjiaBlock : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			try
			{
				int serialId = msg.ContentId;
				if (serialId <= 0)
				{
					Log.WriteLog("买车必看：子品牌ID<=0");
					return;
				}
				Log.WriteLog(string.Format("开始更新买车必看块：子品牌ID：{0}", serialId));
				new WatchMustHtmlBuilder().BuilderDataOrHtml(serialId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
	}
}
