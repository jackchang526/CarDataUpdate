using BitAuto.CarDataUpdate.WebServiceDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
	public class HuimaicheBLL
	{
		public void Add(XElement bodyElement)
		{
			HuimaicheDAL.Update(bodyElement, "add");
		}

		public void Update(XElement bodyElement)
		{
			HuimaicheDAL.Update(bodyElement, "update");
		}

		public void Delete(XElement bodyElement)
		{
			HuimaicheDAL.Update(bodyElement, "delete");
		}
	}
}
