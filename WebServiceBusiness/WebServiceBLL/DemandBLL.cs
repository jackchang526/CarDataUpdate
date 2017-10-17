using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.WebServiceModel;
using BitAuto.CarDataUpdate.WebServiceDAL;
using System.Xml;
using BitAuto.Utils;


namespace BitAuto.CarDataUpdate.WebServiceBLL
{
	/// <summary>
	/// 易集客 白玉
	/// </summary>
	public class DemandBLL
	{
		public void UpDemand(XElement bodyElement)
		{
			DemandDAL.UpdateDemand(bodyElement, "up");
		}

		public void DownDemand(XElement bodyElement)
		{
			DemandDAL.UpdateDemand(bodyElement, "down");
		}
	}
}
