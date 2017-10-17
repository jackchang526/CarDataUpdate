using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.HtmlBuilder;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 子品牌核心看点
    /// </summary>
    public class HeXinKanDian
    {
        public event LogHandler Log;

        public HeXinKanDian()
        {
        }

        public void GetHTML()
        {
            OnLog("开始创建全部核心看点html", true);
            CreateHTML(CommonData.SerialDic.Keys.ToList());
            OnLog("完成创建全部核心看点html", true);
        }
        public void GetHTML(int cs_id)
        {
            OnLog(string.Format("		开始创建子品牌id为：{0}的核心看点html", cs_id.ToString()), true);
            List<int> serialList = new List<int>();
            serialList.Add(cs_id);
            CreateHTML(serialList);
            OnLog(string.Format("		完成创建子品牌id为：{0}的核心看点html", cs_id.ToString()), true);
        }
        private void CreateHTML(List<int> serialList)
        {
            if (serialList == null || serialList.Count <= 0)
                return;

            foreach (int csid in serialList)
            {
                OnLog(string.Format("当前子品牌id为：{0}...", csid.ToString()), true);
				//HeXinKanDianHtmlBuilder builder = new HeXinKanDianHtmlBuilder();
				//builder.BuilderDataOrHtml(csid);
				new SerialHeXinReport().BuilderDataOrHtml(csid);
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
