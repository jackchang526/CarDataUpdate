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
	/// 更新子品牌及旗下所有车型图库相关数据
	/// </summary>
	public class Serial : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			try
			{
				int serialId = msg.ContentId;
				if (serialId > 0)
				{
					//add by sk 2013.05.07 增加综述页图释块 消息及时更新
					Log.WriteLog(string.Format("更新综述页图释块，子品牌ID：{0}", serialId));
					////图释块
					//new SerialColorImage().MakeSerialImageCarsHTML(serialId);

					Log.WriteLog(string.Format("图库接口更新子品牌：{0}", serialId));
					PhotoImageService photo = new PhotoImageService();
					photo.SerialColor(serialId);
					photo.SerialColorAll(serialId);
					photo.SerialPhotoList(serialId);
					photo.SerialPhotoCompare(serialId);
					photo.SerialClass(serialId);
					//photo.SerialStandardImage(serialId);  //del by lisf 2016-01-06
					photo.SerialFocusImage(serialId);
                    //photo.SerialColorCount(serialId);//del by lisf 2016-01-06
					//photo.SerialPhotoHtml(serialId);
                    photo.SerialPhotoHtmlNew(serialId);
					photo.SerialPositionImage(serialId);
					photo.SerialColorImage(serialId);
					photo.SerialElevenImage(serialId);
					photo.SerialDefaultCarFillImage(serialId);
					photo.SerialReallyColorImage(serialId);
                    //photo.SerialThreeStandardImage(serialId);
                    //photo.SerialYearColorUrl(serialId);
                    photo.SerialYearFocusImage(serialId, 0);
                    Dictionary<int, CarEntity> dictCar = CommonData.GetCarDataBySerialId(serialId);
					foreach (CarEntity car in dictCar.Values)
					{
						if (car.Year > 0)
						{
							photo.SerialYear(car.CsId, car.Year);
							//photo.SerialYearFocusImage(car.CsId, car.Year);
							//photo.SerialYearPhotoHtml(car.CsId, car.Year);
                            photo.SerialYearPhotoHtmlNew(car.CsId, car.Year);
						}
                        //photo.CarStandardImage(car.CsId, car.CarId);  //del by lisf 2016-01-06
						//photo.CarFocusImage(car.CsId, car.CarId, car.Year);
						photo.SerialDefaultCarImage(car.CsId, car.CarId);
						//photo.CarPhotoHtml(car.CarId);
                        photo.CarPhotoHtmlNew(car.CarId);
					}
				}
				else
				{
					Log.WriteLog("图库接口更新子品牌ID<=0");
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.Message + ex.StackTrace);
			}
		}
	}
}
