using BitAuto.CarDataUpdate.Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.DataProcesser;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.Photo
{
    public class Car : BaseProcesser
    {
        public override void Processer(ContentMessage msg)
        {
            try
            {
                int carId = msg.ContentId;
                Log.WriteLog(string.Format("开始更新图库车款接口，carid={0}", carId));
                if (carId > 0)
                {  
                    CarEntity car = CommonData.GetCarDataById(carId);
                    if (car != null)
                    { 
                        PhotoImageService photo = new PhotoImageService();
                        if (car.Year > 0)
                        {
                            photo.SerialYear(car.CsId, car.Year); 
                            photo.SerialYearPhotoHtmlNew(car.CsId, car.Year);
                        } 
                        photo.SerialDefaultCarImage(car.CsId, car.CarId); 
                        photo.CarPhotoHtmlNew(car.CarId);
                        //20170926
                        photo.SerialCarReallyImage(car.CsId, car.CarId); 
                    }
                }
                else
                {
                    Log.WriteLog("更新图库车款接口 ID<=0");
                }
                Log.WriteLog(string.Format("结束更新图库车款接口，carid={0}", carId));
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
    }
}
