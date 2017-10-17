using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
	public class VideoCategoryEntity
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public int ParentId { get; set; }
	}
}
