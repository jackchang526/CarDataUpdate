using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Config
{
	public class VideoCategoryConfig
	{
		private Dictionary<string, VideoCategoryConfigSetting> _configList = new Dictionary<string, VideoCategoryConfigSetting>();

		public Dictionary<string, VideoCategoryConfigSetting> ConfigList
		{

			get { return _configList; }
			set { _configList = value; }
		}

	}

	public class VideoCategoryConfigSetting
	{
		public string Key { get; set; }
		public string Name { get; set; }
		public string CategoryIds { get; set; }
		public int CategoryType { get; set; }
	}

}
