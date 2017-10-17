using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.Common.Model
{
	public class CommonHtmlEntity
	{
		public int ID { get; set; }
		public CommonHtmlEnum.TypeEnum TypeID { get; set; }
		public CommonHtmlEnum.TagIdEnum TagID { get; set; }
		public CommonHtmlEnum.BlockIdEnum BlockID { get; set; }
		public string HtmlContent { get; set; }
		public DateTime UpdateTime { get; set; }
	}
}
