using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.DataProcesser;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.Photo
{
    /// <summary>
    /// 车型焦点图消息处理
    /// </summary>
    public class CarFocusImage : BaseProcesser
    {
        public override void Processer(ContentMessage msg)
        {
            int carId = msg.ContentId;
            if (carId <= 0)
            {
                Log.WriteLog("车型焦点图：车型ID<=0");
                return;
            }
            try
            {
                CarEntity car = CommonData.GetCarDataById(carId);
                if (car != null)
                {
                    Log.WriteLog(string.Format("更新车款焦点图,CarId:{0}", carId));
                    PhotoImageService photo = new PhotoImageService();
                    photo.CarFocusImage(car.CsId, carId, car.Year);
                }
                else
                {
                    Log.WriteLog(string.Format("更新车款焦点图,车款为空，CarId:{0}", carId));
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(string.Format("更新车款焦点图异常，CarId:{0},{1}", carId, ex.ToString()));
            }

        }
    }
}
