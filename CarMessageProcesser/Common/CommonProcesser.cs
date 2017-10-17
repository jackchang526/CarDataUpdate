using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.DataProcesser;
using BitAuto.CarDataUpdate.Common;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using BitAuto.Utils.Data;
using System.Data.SqlClient;
using BitAuto.Utils;
using System.Net;
using BitAuto.CarDataUpdate.HtmlBuilder;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.Common
{
    public class CommonProcesser
    {
        /// <summary>
        /// 更新树形相关数据
        /// </summary>
        public void UpdateTreeData()
        {
            ContentGetter autoDataXml = new ContentGetter();
            autoDataXml.Log += new LogHandler(WriteLog);
            autoDataXml.UpdateBrandTree();

            CarTreeXmlDataGetter carTree = new CarTreeXmlDataGetter();
            carTree.Log += new LogHandler(WriteLog);
            carTree.UpdateTreeData();
        }

        /// <summary>
        /// 更新子品牌综述页静态块
        /// </summary>
        /// <param name="serialId"></param>
        public void UpdateSerialStaticBlock(int serialId)
        {
            Log.WriteLog(string.Format("start Update Static Block. msg:[csid:{0}]", serialId));

            try
            {
                //核心看点
                HeXinKanDian hxkd = new HeXinKanDian();
                hxkd.Log += new LogHandler(WriteLog);
                hxkd.GetHTML(serialId);

				////买车必看
				//new WatchMustHtmlBuilder().BuilderDataOrHtml(serialId);

				////图释块
				//new SerialColorImage().MakeSerialImageCarsHTML(serialId);

				////点评块
				//DianpingHtmlBuilder dianping = new DianpingHtmlBuilder();
				//dianping.DianpingXmlDocument = CommonFunction.GetLocalXmlDocument(
				//	Path.Combine(CommonData.CommonSettings.SavePath, string.Format("SerialDianping\\Xml\\Dianping_Serial_{0}.xml", serialId))
				//	);
				//dianping.BuilderDataOrHtml(serialId);

				////答疑块
				//new AskHtmlChunkGenerator().Generate(BitAuto.CarDataUpdate.Common.Model.BrandType.Serial, serialId);
            }
            catch (Exception exp)
            {
                Log.WriteErrorLog(exp.ToString());
            }

            Log.WriteLog(string.Format("end Update Static Block. msg:[csid:{0}]", serialId));
        }

        void WriteLog(object sender, LogArgs e)
        {
            Log.WriteLog(e.LogText);
        }
    }
}
