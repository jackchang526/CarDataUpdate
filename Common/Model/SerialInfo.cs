using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class SerialInfo
    {
        public int Id;
        public string Name;
        public string ShowName;
        public string SeoName;
        public string AllSpell;
        public string BrandName;
		public int CarLevel { get; set; }
		public string CsSaleState { get; set; }
    }
}
