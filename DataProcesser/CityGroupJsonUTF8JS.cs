using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using System.IO;
using BitAuto.Services.FileServer.Client;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class CityGroupJsonUTF8JS
    {
        public event LogHandler Log;
        private const string _JsName = "CityGroupJsonUTF8.js";
        private string _JsPath;

        XmlDocument _ServiceDoc = null;
        StringBuilder _Memo1 = new StringBuilder();
        StringBuilder _CityTo = new StringBuilder();
        Dictionary<int, int> _Memo2 = new Dictionary<int, int>();
        Dictionary<int, int> _Memo3 = new Dictionary<int, int>();

        public CityGroupJsonUTF8JS()
        {;
            _JsPath = Path.Combine(CommonData.CommonSettings.SavePath, _JsName);
        }
        public void CreateCityGroupJsonUTF8JS()
        {
            OnLog("Start CreateCityGroupJsonUTF8JS ......", true);
            GetXmlDocument();
            SetValueList();
            SetStringBuilder();
            WriteJsFile();
            UploaderJsFile();
            OnLog("End CreateCityGroupJsonUTF8JS ......", true);
        }

        private void UploaderJsFile()
        {
            OnLog("Start Uploader ......", true);
            if (!File.Exists(_JsPath))
            {
                OnLog("Error Uploader Not File (Path:" + _JsPath + ") ......", true);
            }
            else
            {
                string targetFile = "ycimage/CarChannel/jsnew/" + _JsName;
                try
                {
                    FileRepositoryService.CreateFile(targetFile, _JsPath);
                }
                catch (Exception exp)
                {
                    OnLog("Error Uploader (targetFile:" + targetFile + ";sourceFile:" + _JsPath + ";message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")", true);
                }
            }
            OnLog("End Uploader ......", true);
        }

        private void SetStringBuilder()
        {
            _Memo1.Insert(0, "var city49Array = [").Append("];");

            if (_Memo3.Count > 0 && _Memo2.Count > 0)
            {
                foreach (KeyValuePair<int, int> keyValue in _Memo3)
                {
                    if (_Memo2.ContainsKey(keyValue.Value))
                    {
                        //{"ID":"1803","TO":"1801"}
                        _CityTo.Append(",{");
                        _CityTo.AppendFormat("\"ID\":\"{0}\",\"TO\":\"{1}\"", keyValue.Key.ToString(), _Memo2[keyValue.Value].ToString());
                        _CityTo.Append("}");
                    }
                }
                if (_CityTo.Length > 0)
                    _CityTo.Remove(0, 1);
            }

            _CityTo.Insert(0, "var city348To49Array = [").Append("];");
        }

        private void WriteJsFile()
        {
            OnLog("Start Write JS (Path:" + _JsPath + ")....", true);

            if (_Memo1.Length > 0)
            {
                CommonFunction.SaveFileContent(_Memo1.ToString(), _JsPath, Encoding.UTF8);
                OnLog("success.", true);
            }
            else
            {
                OnLog("error content is empty.", true);
            }
            OnLog("End Write JS ....", true);
        }

        void SetValueList()
        {
            if (_ServiceDoc == null)
                return;
            int current = 0;
            int memo1;
            string memo2Str, memo3Str;
            XmlNodeList nodeList = _ServiceDoc.SelectNodes("/CityValueSet/CityItem");
            OnLog(string.Format("Get Count {0} ...", nodeList.Count.ToString()), true);
            foreach (XmlNode node in nodeList)
            {
                OnLog(string.Format("Cache Data. Current {0} ...", (++current).ToString()), false);
                memo1 = ConvertHelper.GetInteger(node.SelectSingleNode("Memo1").InnerText);
                if (memo1 == 1)
                {
                    //{"ID":"201","Name":"北京","Spell":"beijing"}
                    _Memo1.Append(",{");
                    _Memo1.AppendFormat("\"ID\":\"{0}\",\"Name\":\"{1}\",\"Spell\":\"{2}\""
                        , node.SelectSingleNode("CityId").InnerText
                        , node.SelectSingleNode("CityName").InnerText
                        , node.SelectSingleNode("EngName").InnerText);
                    _Memo1.Append("}");

                    memo2Str = node.SelectSingleNode("Memo2").InnerText;
                    if (!string.IsNullOrEmpty(memo2Str))
                    {
                        _Memo2.Add(ConvertHelper.GetInteger(memo2Str)
                            , ConvertHelper.GetInteger(node.SelectSingleNode("CityId").InnerText));
                    }
                }
                else
                {
                    memo3Str = node.SelectSingleNode("Memo3").InnerText;
                    if (!string.IsNullOrEmpty(memo3Str))
                    {
                        _Memo3.Add(ConvertHelper.GetInteger(node.SelectSingleNode("CityId").InnerText)
                            , ConvertHelper.GetInteger(memo3Str));
                    }
                }
            }
            if (_Memo1.Length > 0)
                _Memo1.Remove(0, 1);
        }

        /// <summary>
        /// 检测XML文件是否存在
        /// </summary>
        /// <returns></returns>
        void GetXmlDocument()
        {
            XmlReader reader = null;
            try
            {
                reader = XmlReader.Create(CommonData.CommonSettings.CityValueSetUrl);
                _ServiceDoc = new XmlDocument();
                _ServiceDoc.Load(reader);
            }
            catch (Exception exp)
            {
                OnLog("Error Read Service XML (Path:" + CommonData.CommonSettings.CityValueSetUrl + ";message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")", true);
            }
            finally
            {
                if (reader != null && reader.ReadState != ReadState.Closed)
                    reader.Close();
            }
        }
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
}
