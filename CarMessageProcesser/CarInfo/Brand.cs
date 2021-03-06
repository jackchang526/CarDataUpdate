﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.DataProcesser;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.CarInfo
{
	public class Brand : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
            Log.WriteLog("start Brand processer news [" + msg.ContentId + "] !");
            if (msg.ContentId > 0)
            {
                new Common.CommonProcesser().UpdateTreeData();
            }
            else
            {
                Log.WriteLog("ContentId无效!");
            }
            Log.WriteLog("end Brand processer news [" + msg.ContentId + "] !");
        }
	}
}
