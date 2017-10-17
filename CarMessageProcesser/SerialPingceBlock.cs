using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.HtmlBuilder;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.CarMessageProcesser
{
	/// <summary>
	/// 车型综述页 车型详解块及时更新
	/// </summary>
	public class SerialPingceBlock : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			int serialId = msg.ContentId;
			if (serialId <= 0)
			{
				Log.WriteLog("车型详解：子品牌ID<=0");
				return;
			}
			Log.WriteLog("开始更新子品牌评测块：serialId=" + serialId);
			new PingceBlockHtmlBuilder().BuilderDataOrHtml(serialId);
			Log.WriteLog("更新子品牌评测块结束");

			Log.WriteLog("开始更新待销子品牌焦点新闻块：serialId=" + serialId);
			new FocusNewsForWaitSaleHtmlBuilder().BuilderDataOrHtml(serialId);
			Log.WriteLog("更新待销子品牌焦点新闻块内容结束");
		}
	}
}
