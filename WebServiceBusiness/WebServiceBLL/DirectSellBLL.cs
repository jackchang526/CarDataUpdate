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
	/// 直销 商城事业部
	/// </summary>
	public class DirectSellBLL
	{
		#region

		public void AddDirectSell(XElement bodyElement)
		{
			DirectSellDAL.UpdateDirectSell(bodyElement, "add");
		}

		public void UpdateDirectSell(XElement bodyElement)
		{
			DirectSellDAL.UpdateDirectSell(bodyElement, "update");
		}

		public void DeleteDirectSell(XElement bodyElement)
		{
			DirectSellDAL.UpdateDirectSell(bodyElement, "delete");
		}

		#endregion
	}
}
