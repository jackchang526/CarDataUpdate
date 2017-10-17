using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
    public class PhotoBLL
    {
        private readonly string MessageBody = "<MessageBody>" +
                                                "<From>Photo</From>" +
                                                "<ContentType>{0}</ContentType>" +
                                                "<ContentId>{1}</ContentId>" +
                                                "<UpdateTime>{2}</UpdateTime>" +
                                               "</MessageBody>";

        private readonly string QueueName = CommonData.CommonSettings.QueueName;


        /// <summary>
        /// 车款焦点图消息转发-更新
        /// </summary>
        /// <param name="bodyElement"></param>
        public void CarModelFocusImage_Update(XElement bodyElement)
        {
            string contentId = CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarModelID" });
            int id;
            if (int.TryParse(contentId, out id) && id > 0)
            {
                string msg = string.Format(MessageBody, "CarFocusImage", contentId, DateTime.Now.ToString("yyyy-MM-dd"));
                MessageService.SendMessage(QueueName, msg);
            }
            else
            {
                Log.WriteErrorLog("车款焦点图消息转发异常," + bodyElement.ToString());
            }
        }

        /// <summary>
        /// 车系封面图消息转发-更新
        /// </summary>
        /// <param name="bodyElement"></param>
        public void CoverImage_Update(XElement bodyElement)
        {
            string contentId = CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "SerialBrandID" });
            int id;
            if (int.TryParse(contentId, out id) && id > 0)
            {
                string msg = string.Format(MessageBody, "SerialCoverImage", contentId, DateTime.Now.ToString("yyyy-MM-dd"));
                MessageService.SendMessage(QueueName, msg);
            }
            else
            {
                Log.WriteErrorLog("车系封面图消息转发异常," + bodyElement.ToString());
            }
        }

        /// <summary>
        /// 车系焦点图消息转发-更新
        /// </summary>
        /// <param name="bodyElement"></param>
        public void SerialBrandFocusImage_Update(XElement bodyElement)
        {
            string contentId = CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "SerialBrandID" });
            int id;
            if (int.TryParse(contentId, out id) && id > 0)
            {
                string msg = string.Format(MessageBody, "SerialFocusImage", contentId, DateTime.Now.ToString("yyyy-MM-dd"));
                MessageService.SendMessage(QueueName, msg);
            }
            else
            {
                Log.WriteErrorLog("车系焦点图消息转发异常," + bodyElement.ToString());
            }
        }

        /// <summary>
        /// 车系年度焦点图消息转发-更新
        /// </summary>
        /// <param name="bodyElement"></param>
        public void SerialYearFocusImage_Update(XElement bodyElement)
        {
            string contentId = CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "SerialBrandID" });
            int id;
            if (int.TryParse(contentId, out id) && id > 0)
            {
                string msg = string.Format(MessageBody, "SerialYearFocusImage", contentId, DateTime.Now.ToString("yyyy-MM-dd"));
                MessageService.SendMessage(QueueName, msg);
            }
            else
            {
                Log.WriteErrorLog("车系年度焦点图消息转发异常," + bodyElement.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CarImage_Add(XElement bodyElement)
        {
            string entityId = CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "EntityId" });
            string contentId = GetSerialIdFromInterface(entityId);
            int id;
            if (int.TryParse(contentId, out id) && id > 0)
            {
                string msg = string.Format(MessageBody, "Serial", contentId, DateTime.Now.ToString("yyyy-MM-dd"));
                MessageService.SendMessage(QueueName, msg);
            }
            else
            {
                Log.WriteErrorLog("车系消息转发异常,从接口中获取的车系ID是：" + contentId);
            }
        }

        /// <summary>
        /// 新版图库车型图片更新消息
        /// </summary>
        /// <param name="bodyElement"></param>
        public void CarImagesV2_Update(XElement bodyElement)
        {
            try
            {
                int serialId = ConvertHelper.GetInteger(CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "ModelId" }));
                int carId = ConvertHelper.GetInteger(CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "StyleId" }));

                if (serialId > 0)
                {
                    string msg = string.Format(MessageBody, "Serial", serialId, DateTime.Now.ToString("yyyy-MM-dd"));
                    MessageService.SendMessage(QueueName, msg);
                }
                else
                {
                    Log.WriteLog("车型图片更新  SerialId=0," + bodyElement.ToString());
                }
                if (carId > 0)
                {
                    string msg = string.Format(MessageBody, "Car", serialId, DateTime.Now.ToString("yyyy-MM-dd"));
                    MessageService.SendMessage(QueueName, msg);
                }
                else
                {
                    Log.WriteLog("车型图片更新 CarId=0," + bodyElement.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("车型图片更新消息转发异常," + ex.ToString());
            } 
        }

        /// <summary>
        /// 访问单个车型图片接口
        /// </summary>
        public string GetSerialIdFromInterface(string entityId)
        {
            string contentId = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("http://imgsvr.bitauto.com/CarImage/Get.aspx?entityID=" + entityId);
            if (xmlDoc != null && xmlDoc.HasChildNodes)
            {
                if (xmlDoc.SelectSingleNode("/CarImage/SerialBrandID") != null)
                {
                    contentId = xmlDoc.SelectSingleNode("/CarImage/SerialBrandID").InnerText;
                }
            }
            return contentId;
        }



    }
}
