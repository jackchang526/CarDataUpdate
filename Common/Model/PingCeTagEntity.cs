using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
	public class PingCeTagEntity
	{
		public string tagName { get; set; }
		public string tagRegularExpressions { get; set; }
		public int tagId { get; set; }
		public string url { get; set; }
	}
}
