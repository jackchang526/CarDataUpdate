using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Config
{
	public class MessageQueueConfig
	{
		private Dictionary<string, MessageQueueSetting> _configList = new Dictionary<string, MessageQueueSetting>();

		public Dictionary<string, MessageQueueSetting> ConfigList
		{

			get { return _configList; }
			set { _configList = value; }
		}
	}

	public class MessageQueueSetting
	{
		public string BusinessName { get; set; }
		public string QueueName { get; set; }
	}
}
