using BitAuto.CarDataUpdate.WebServiceDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
	/// <summary>
	/// 易车惠
	/// </summary>
	public class YiCheHuiBLL
	{
		public void Add(XElement bodyElement)
		{
			YiCheHuiDAL.Update(bodyElement, "add");
		}

		public void Update(XElement bodyElement)
		{
			YiCheHuiDAL.Update(bodyElement, "update");
		}

		public void Delete(XElement bodyElement)
		{
			YiCheHuiDAL.Update(bodyElement, "delete");
		}
	}
}
