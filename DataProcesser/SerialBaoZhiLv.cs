using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;


namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 精真估5年旧车保值率
	/// </summary>
	public class SerialBaoZhiLv
	{
		public event LogHandler Log;

		private string _XmlFileName = string.Empty;
		private string _RootPath = string.Empty;

		private readonly string PartnerId = string.Empty;//合作id号，精真估分配
		private readonly string Key = string.Empty; //key，精真估分配
		private readonly string JZGUrl = string.Empty;//精真估保值率接口
		private readonly int PageSize = 2000;//每次请求数量
		List<BaoZhilvObj> list = new List<BaoZhilvObj>();

		Dictionary<string, string> param = new Dictionary<string, string>() {//接口请求参数
				{"SequenceId",string.Empty},{"PartnerId",string.Empty},{"Operate","residualratioranking"},{"Sign",string.Empty},{"Body",string.Empty}
			};

		public SerialBaoZhiLv()
		{
			_RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialSet");
			_XmlFileName = Path.Combine(_RootPath, "BaoZhiLv.xml");

			PartnerId = ConfigurationManager.AppSettings["BaoZhiLvPid"];
			Key = ConfigurationManager.AppSettings["BaoZhiLvKey"];
			JZGUrl = ConfigurationManager.AppSettings["BaoZhiLvUrl"];

			param["PartnerId"] = PartnerId;
		}

		public void GetBaoZhiLv()
		{
			OnLog("开始生成5年旧车保值率文档", true);
			if (string.IsNullOrEmpty(PartnerId) || string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(JZGUrl))
			{
				OnLog(string.Format("精真估5年旧车保值率初始化参数错误;PartnerId={0},Key={1},url={2}", PartnerId, Key, JZGUrl), true);
				return;
			}
			GetBaoZhiLv(1);
			if (list != null && list.Count > 0)
			{
				XmlDocument doc = new XmlDocument();
				XmlElement root = doc.CreateElement("Root");
				doc.AppendChild(root);
				//root.SetAttribute("Count", list.Count.ToString());
				root.SetAttribute("Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				list.Sort(BaoZhilvObj.CompareByResidualRatio5);
				foreach (BaoZhilvObj obj in list)
				{
					if (!CommonData.SerialDic.ContainsKey(obj.ModelId))
					{
						continue;
					}

					SerialInfo serialInfo = CommonData.SerialDic[obj.ModelId];
					string level = CommonData.SerialLevelDic.ContainsKey(obj.ModelId) ? CommonData.SerialLevelDic[obj.ModelId] : serialInfo.CarLevel.ToString();
					if (level == "豪华型")
					{
						level = "豪华车";
					}
					XmlElement serial = doc.CreateElement("Serial");
					root.AppendChild(serial);

					serial.SetAttribute("Id", obj.ModelId.ToString());
					serial.SetAttribute("ShowName", serialInfo.ShowName);
					serial.SetAttribute("AllSpell", serialInfo.AllSpell);
					serial.SetAttribute("Level", level);
					serial.SetAttribute("ResidualRatio1", obj.ResidualRatio1.ToString());
					serial.SetAttribute("ResidualRatio2", obj.ResidualRatio2.ToString());
					serial.SetAttribute("ResidualRatio3", obj.ResidualRatio3.ToString());
					serial.SetAttribute("ResidualRatio4", obj.ResidualRatio4.ToString());
					serial.SetAttribute("ResidualRatio5", obj.ResidualRatio5.ToString());
					serial.SetAttribute("OrderId", obj.OrderId.ToString());
				}

				CommonFunction.SaveXMLDocument(doc, _XmlFileName);
			}
			OnLog("5年旧车保值率文档生成结束", true);
		}

		/// <summary>
		/// 按级别请求，0：全部级别
		/// </summary>
		/// <param name="pageIndex"></param>
		private void GetBaoZhiLv(int pageIndex)
		{
			if (pageIndex <= 0) return;

			string sequenceId = System.Guid.NewGuid().ToString();
			string body = Encrypt3DESToBase64(string.Format("{{\"ModelLevel\":\"0\",\"PageSize\":\"{0}\",\"PageIndex\":\"{1}\"}}", PageSize, pageIndex), Key);
			string sign = ToBase64(MD5(string.Concat(sequenceId, PartnerId, "residualratioranking", body, Key)));
			param["SequenceId"] = sequenceId;
			param["Sign"] = sign;
			param["Body"] = body;
			string paramStr = GetParamFromDic(param);
			string result = PostWebRequest(JZGUrl, paramStr, Encoding.GetEncoding("utf-8"));

			BaoZhiLvMsg msg = JsonConvert.DeserializeObject(result, typeof(BaoZhiLvMsg)) as BaoZhiLvMsg;
			if (msg != null && msg.MsgCode == 1)
			{
				string listJson = Decrypt3DESFromBase64(msg.Body, Key);
				if (!string.IsNullOrEmpty(listJson))
				{
					BaoZhilvLevel level = JsonConvert.DeserializeObject(listJson, typeof(BaoZhilvLevel)) as BaoZhilvLevel;
					if (level.RecCount > 0)
					{
						list.AddRange(level.ResidualRatioList);
					}

					if (level.RecCount > pageIndex * PageSize)
					{
						GetBaoZhiLv(pageIndex + 1);
					}
				}
			}
			else
			{
				OnLog("精真估接口请求失败，请求结果：" + result, true);
			}
		}

		#region 接口请求 加密解密方法，由精真估提供


		/// <summary>
		/// 吧dic拼成字符串
		/// </summary>
		/// <param name="dic"></param>
		/// <returns></returns>
		private static string GetParamFromDic(Dictionary<string, string> dic)
		{
			StringBuilder sb = new StringBuilder("json={");
			foreach (KeyValuePair<string, string> kv in dic)
			{
				sb.Append(string.Format("\"{0}\":\"{1}\",", kv.Key, kv.Value));
			}
			if (sb.Length > 0)
			{
				sb = sb.Remove(sb.Length - 1, 1);
			}
			sb.Append("}");
			return sb.ToString();
		}

		public static string Encrypt3DESToBase64(string strEncrypt, string strKey)
		{
			return ToBase64(Encrypt3DES(Encoding.UTF8.GetBytes(strEncrypt), strKey));
		}

		public static byte[] Encrypt3DES(byte[] arrEncrypt, string strKey)
		{
			ICryptoTransform DESEncrypt = null;
			try
			{
				TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();

				DES.Key = ASCIIEncoding.ASCII.GetBytes(strKey);
				DES.Mode = CipherMode.ECB;

				DESEncrypt = DES.CreateEncryptor();
			}
			catch (System.Exception e)
			{
				return null;
			}

			return DESEncrypt.TransformFinalBlock(arrEncrypt, 0, arrEncrypt.Length);
		}
		public static string ToBase64(byte[] str)
		{
			return Convert.ToBase64String(str);
		}

		public static string Decrypt3DESFromBase64(string strDecrypt, string strKey)
		{
			return Encoding.UTF8.GetString(Decrypt3DES(FromBase64(strDecrypt), strKey));
		}
		public static byte[] FromBase64(string str)
		{
			return Convert.FromBase64String(str);
		}

		public static byte[] Decrypt3DES(byte[] arrDecrypt, string strKey)
		{
			ICryptoTransform DESDecrypt = null;
			try
			{
				TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();

				DES.Key = ASCIIEncoding.ASCII.GetBytes(strKey);
				DES.Mode = CipherMode.ECB;
				DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

				DESDecrypt = DES.CreateDecryptor();
			}
			catch (System.Exception e)
			{
				return null;
			}

			return DESDecrypt.TransformFinalBlock(arrDecrypt, 0, arrDecrypt.Length);
		}

		public static string Sign(string str)
		{
			return ToBase64(MD5(str));
		}

		public static byte[] MD5(string str)
		{
			return MD5(Encoding.UTF8.GetBytes(str));
		}

		public static byte[] MD5(byte[] str)
		{
			MD5 m = new MD5CryptoServiceProvider();
			return m.ComputeHash(str);
		}

		/// <summary>
		/// 流请求接口
		/// </summary>
		/// <param name="postUrl"></param>
		/// <param name="paramData"></param>
		/// <param name="dataEncode"></param>
		/// <returns></returns>
		private string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
		{
			string ret = string.Empty;
			try
			{
				byte[] byteArray = dataEncode.GetBytes(paramData); //转化"json=" +
				HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
				webReq.Method = "POST";
				webReq.ContentType = "application/x-www-form-urlencoded";

				webReq.ContentLength = byteArray.Length;
				Stream newStream = webReq.GetRequestStream();
				newStream.Write(byteArray, 0, byteArray.Length);//写入参数
				newStream.Close();
				HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
				StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				ret = sr.ReadToEnd();
				sr.Close();
				response.Close();
				newStream.Close();
			}
			catch (Exception ex)
			{
				OnLog("精真估接口请求错误：" + ex.Message, true);
			}
			return ret;
		}

		#endregion

		/// <summary>
		/// 写Log
		/// </summary>
		/// <param name="logText"></param>
		public void OnLog(string logText, bool nextLine)
		{
			if (Log != null)
				Log(this, new LogArgs(logText, nextLine));
		}
	}


	[Serializable]
	public class BaoZhiLvMsg
	{
		public int MsgCode;
		public string Msg = string.Empty;
		public string Body = string.Empty;
		public string Sign = string.Empty;
	}

	[Serializable]
	public class BaoZhilvLevel
	{
		public int RecCount = 0;
		public List<BaoZhilvObj> ResidualRatioList;
	}

	[Serializable]
	public class BaoZhilvObj
	{
		public int ModelId = 0;
		public int ModelLevel = 0;
		public int OrderId = 0;
		public int ProvinceId = 0;
		public double ResidualRatio1 = 0;
		public double ResidualRatio2 = 0;
		public double ResidualRatio3 = 0;
		public double ResidualRatio4 = 0;
		public double ResidualRatio5 = 0;

		public static int CompareByResidualRatio5(BaoZhilvObj x1,BaoZhilvObj x2)
		{
			return x1.ResidualRatio5 > x2.ResidualRatio5 ? -1 : x1.ResidualRatio5 == x2.ResidualRatio5 ? x1.ModelId - x2.ModelId : 1;
		}
	}
}
