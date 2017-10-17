using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.DataProcesser;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.Photo
{
	/// <summary>
	/// 子品牌焦点图消息处理
	/// </summary>
	public class SerialFocusImage : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			int serialId = msg.ContentId;
			if (serialId <= 0)
			{
				Log.WriteLog("子品牌焦点图：子品牌ID<=0");
				return;
			}
			Log.WriteLog(string.Format("更新子品牌焦点图开始。serialId:{0}", serialId));
			PhotoImageService photo = new PhotoImageService();
			photo.SerialFocusImage(serialId);
			Log.WriteLog("更新子品牌焦点图结束。");
		}
	}
}
