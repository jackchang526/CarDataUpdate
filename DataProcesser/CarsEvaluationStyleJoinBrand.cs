using System;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;


namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 每日更新评测后台StyleJoinBrand表数据
    /// </summary>
    public class CarsEvaluationStyleJoinBrand
    {
        public static bool UpdateStyleJoinBrand()
        {
            try
            {
                string sql = @"DELETE FROM dbo.StyleJoinBrand;
                          INSERT  INTO dbo.StyleJoinBrand ( StyleId, [Year], StyleName, ReferPrice,
                                  EngineExhaust, MasterBrandId,
                                  MasterBrandName, MakeId, MakeName,
                                  MakeCountry, ModelId, ModelName, CreateTime,
                                  UpdateTime, MasterBrandSpell, MakeSpell,
                                  ModelSpell,ModelDisplayName )
                         SELECT  car.Car_Id, car.Car_YearType, car.Car_Name, car.car_ReferPrice,
                                        CASE WHEN CHARINDEX('.', cdb.Pvalue) > 0
                                             THEN cdb.Pvalue + N'L'
                                             ELSE cdb.Pvalue + N'.0L'
                                        END, mb.bs_Id, mb.bs_Name, cb.cb_Id, cb.cb_Name, cb.cb_country,
                                        cs.cs_Id, cs.csName, car.CreateTime, car.UpdateTime, mb.spell,
                                        cb.spell, cs.spell,cs.csShowName
                                FROM    AutoStorageNew.dbo.Car_relation car
                                        LEFT JOIN AutoStorageNew.dbo.Car_Serial cs ON cs.cs_Id = car.Cs_Id
                                        LEFT JOIN AutoStorageNew.dbo.Car_Brand cb ON cb.cb_Id = cs.cb_Id
                                        LEFT JOIN AutoStorageNew.dbo.Car_MasterBrand_Rel cmb ON cmb.cb_Id = cb.cb_Id
                                        LEFT JOIN AutoStorageNew.dbo.Car_MasterBrand mb ON mb.bs_Id = cmb.bs_Id
                                        LEFT JOIN AutoStorageNew.dbo.CarDataBase cdb ON cdb.CarId = car.Car_Id
                                                                                      AND ParamId = 785
                                WHERE   car.IsState = 0
                                        AND cs.IsState = 0
                                        AND cb.IsState = 0
                                        AND mb.IsState = 0";
                
                bool isSuccess = (SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarsEvaluationConnString, CommandType.Text, sql) > 0);
                return isSuccess;
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("评测后台，更新品牌链接表出错：" + ex.ToString());
                return false;
            }
        }
    }
}
