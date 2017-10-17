using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
	public class CarEntity
	{
		public int CarId { get; set; }
		public int CsId { get; set; }
		public int Year { get; set; }
        public double ReferPrice { get; set; }
	}
}
