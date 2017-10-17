using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using System.Xml;
using BitAuto.CarDataUpdate.DataProcesser;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.Photo
{
	/// <summary>
	/// 子品牌年款焦点图消息处理
	/// </summary>
	public class SerialYearFocusImage : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			int serialId = msg.ContentId;
			XmlDocument xmlDoc = msg.ContentBody;
			if (serialId <= 0)
			{
				Log.WriteLog("子品牌年款焦点图：子品牌ID<=0");
				return;
			}
			PhotoImageService photo = new PhotoImageService();
			if (xmlDoc != null)
			{
				XmlNode node = xmlDoc.SelectSingleNode("//Year");
				int year = 0;
				if (node != null)
					year = ConvertHelper.GetInteger(node.InnerText);
				if (year > 0)
				{
					Log.WriteLog(string.Format("更新子品牌年款焦点图开始。serialId:{0},year:{1}", serialId, year));
					photo.SerialYearFocusImage(serialId, year);
					Log.WriteLog("更新子品牌年款焦点图结束。");
				}
			}
		}
	}
}
