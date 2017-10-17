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
	/// 购车返现 for 曹浩兵
	/// </summary>
	public class CashBackBLL
	{
		#region

		public void AddCashBack(XElement bodyElement)
		{
			CashBackDAL.UpdateCashBack(bodyElement, "add");
		}

		public void UpdateCashBack(XElement bodyElement)
		{
			CashBackDAL.UpdateCashBack(bodyElement, "update");
		}

		public void DeleteCashBack(XElement bodyElement)
		{
			CashBackDAL.UpdateCashBack(bodyElement, "delete");
		}

		#endregion
	}
}
