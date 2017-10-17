using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Enum
{
	public class VideoEnum
	{
		/// <summary>
		/// 视频分类
		/// </summary>
		public enum CategoryTypeEnum
		{
			All = 0,
			Focus = 1,
			Serial = 2
		}

		/// <summary>
		/// 视频来源
		/// </summary>
		public enum VideoSource
		{ 
			All = -1,
			Video = 0,
			Baa = 1
		}
	}
}
