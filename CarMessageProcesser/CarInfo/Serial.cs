using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.DataProcesser;
using BitAuto.CarDataUpdate.CarMessageProcesser.Common;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.CarInfo
{
	public class Serial : BaseProcesser
	{
		public override void Processer(ContentMessage msg)
		{
			Log.WriteLog("start Serial processer news [" + msg.ContentId + "] !");

			int serialId = msg.ContentId;
			if (serialId > 0)
			{
				try
				{
					CommonData.GetSerialData();//更新一下缓存

					CommonProcesser commonProcesser = new CommonProcesser();

					////树形相关
					commonProcesser.UpdateTreeData();

					Log.WriteLog(string.Format("开始更新选车工具车型数据，子品牌ID:{0}", serialId));
					UpdateSelectCarData(serialId);
                    //高级选车工具更新
                    UpdateSelectCarDataV2(serialId);
					//更新购车服务选车表
					UpdateBuyCarServiceSelectCarData(serialId);
					Log.WriteLog(string.Format("开始更新互联互通导航，子品牌ID:{0}", serialId));
					GenerateCommonNavigation(serialId);

					//更新子品牌综述页静态块
					commonProcesser.UpdateSerialStaticBlock(serialId);

					#region 向晶赞推送子品牌数据
					/* modified by sk 2014.12.22
                    //<ActionType>Insert|Delete|Update</ActionType>
                    string actionTypeStr = CommonFunction.GetXmlElementInnerText(msg.ContentBody, "MessageBody/ActionType", string.Empty);
                    int actionType;
                    switch (actionTypeStr.ToLower())
                    {
                        case "update":
                            actionType = 2;
                            break;
                        case "insert":
                            actionType = 1;
                            break;
                        case "delete":
                            actionType = 4;
                            break;
                        default:
                            actionType = 0;
                            break;
                    }
                    if (actionType > 0)
                    {
                        new SerialDataToJingZan().PostSerialDataToJingZan(actionType, msg.ContentId);
                    } 
					 */
					#endregion
				}
				catch (Exception ex)
				{
					Log.WriteErrorLog(ex.ToString());
				}
			}

			Log.WriteLog("end Serial processer news [" + msg.ContentId + "] !");
		}
		//更新选车工具表数据
		private void UpdateSelectCarData(int serialId)
		{
			try
			{
				CarInfoForSelecting.UpdateCarDataByCsId(serialId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
        //更新高级选车工具数据
        private void UpdateSelectCarDataV2(int serialId)
        {
            try
            {
                CarInfoForSelecting.UpdateCarDataByCsIdV2(serialId);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
		/// <summary>
		/// 更新购车服务选车工具表数据
		/// </summary>
		/// <param name="serialId"></param>
		private void UpdateBuyCarServiceSelectCarData(int serialId)
		{
			try
			{
				CarInfoForSelecting.UpdateBuyCarServiceSelectCar(serialId, 0);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
		//生成通用导航头
		private void GenerateCommonNavigation(int serialId)
		{
			try
			{
				CommonNavigation nav = new CommonNavigation();
				// nav.GenerateSerialNavigation(serialId);
				nav.GenerateSerialNavigationV2(serialId);
				//nav.GenerateSerialBarInfo(serialId);
				nav.GenerateSerialNavigationM(serialId);
				//更新子品牌旗下车款
				Dictionary<int, CarEntity> dict = CommonData.GetCarDataBySerialId(serialId);
				foreach (CarEntity car in dict.Values)
				{
					// nav.GenerateCarNavigation(car.CarId);
					nav.GenerateCarNavigationV2(car.CarId);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
	}
}
