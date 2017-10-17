using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
	public class CarBaseInfoEntity
	{
		public int CarId { get; set; }
		public string CarName { get; set; }
        public int SerialId { get; set; }
		public int YearType { get; set; }
        public decimal ReferPrice { get; set; }        
        public string SaleState { get; set; }
        public string ProduceState { get; set; }
        public string EngineExhaust { get; set; }
        public string TransmissionType { get; set; }
	}
}
