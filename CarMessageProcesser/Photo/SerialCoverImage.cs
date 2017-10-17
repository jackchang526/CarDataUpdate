using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.DataProcesser;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.Photo
{
	public class SerialCoverImage : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			try
			{
				PhotoImageService photo = new PhotoImageService();
                Log.WriteLog("消息更新子品牌封面");  
				photo.SerialCover();
				Log.WriteLog("消息更新子品牌非白底封面");
				//photo.SerialCoverWithout();
				photo.SerialCoverImageAndCount();

				//modified by sk 2014.12.22
				////向晶赞推送子品牌数据
				//if (msg.ContentId > 0)
				//{
				//	new SerialDataToJingZan().PostSerialDataToJingZan(3, msg.ContentId);
				//}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.Message + ex.StackTrace);
			}
		}
	}
}
