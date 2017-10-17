using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;
using System.IO;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.HtmlBuilder;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class EditorComment
    {
        public event LogHandler Log;

        public void CreateEditorComment(int csId)
        {
            OnLog("		Start EditorComment ......", true);
            List<int> serialList = null;
            if (csId > 0)
            {
                serialList = new List<int>();
                serialList.Add(csId);
            }
            else
                serialList = CommonData.SerialDic.Keys.ToList();

            //EditorCommentHtmlBuilder builder = new EditorCommentHtmlBuilder();
            EditorCommentHtmlBuilderNew builderNew = new EditorCommentHtmlBuilderNew();
            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog(String.Format("		Generating EditorComment ......{0}/{1}", counter, serialList.Count), false);
                //builder.BuilderDataOrHtml(serialId);
                builderNew.BuilderDataOrHtml(serialId);
            }
            OnLog("		End EditorComment!", true);
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
