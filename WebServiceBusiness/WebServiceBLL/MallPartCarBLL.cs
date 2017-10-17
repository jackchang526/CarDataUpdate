using BitAuto.CarDataUpdate.WebServiceDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
	public class MallPartCarBLL
	{
		public void Add(XElement bodyElement)
		{
			MallPartCarDAL.Update(bodyElement, "add");
		}

		public void Update(XElement bodyElement)
		{
			MallPartCarDAL.Update(bodyElement, "update");
		}

		public void Delete(XElement bodyElement)
		{
			MallPartCarDAL.Update(bodyElement, "delete");
		}
	}
}
