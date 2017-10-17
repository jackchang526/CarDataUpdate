using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.Common.NewsNumXml
{
	/// <summary>
	/// 更新新闻数队列
	/// </summary>
	public static class NewsNumQueue
	{
		private readonly static Queue<NewsNumMessage> _queue = new Queue<NewsNumMessage>();

		public static void AddNewsNumMessage(NewsNumMessage msg)
		{
			lock (_queue)
			{
				if (msg != null)
				{
					_queue.Enqueue(msg);
				}
			}
		}
		public static List<NewsNumMessage> GetNewsNumMessages()
		{
			lock (_queue)
			{
				if (_queue.Count > 0)
				{
					List<NewsNumMessage> result = new List<NewsNumMessage>(_queue.Count);
					do
					{
						result.Add(_queue.Dequeue());
					} while (_queue.Count > 0);
					return result;
				}
				return null;
			}
		}
	}
}
