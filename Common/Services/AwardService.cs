using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Repository;

namespace BitAuto.CarDataUpdate.Common.Services
{
    public class AwardService
    {
        public static List<Award> GetList(List<string> awardIds,int csId)
        {
            if (awardIds == null || awardIds.Count == 0)
            {
                return null;
            }
            var strAwardIds = string.Join(",", awardIds.ToArray());
            var dt = AwardRepository.GetAwards(strAwardIds);//dt转awards
            var awards = new List<Award>();
            foreach (DataRow dataRow in dt.Rows)
            {
                var award = new Award();
                award.Id = int.Parse(dataRow["Id"].ToString());
                award.LogoUrl = dataRow["LogoUrl"].ToString();
                award.Name = dataRow["AwardsName"].ToString();
                award.YearInfos = GetYearInfos(award.Id,csId);
                awards.Add(award);
            }
            return awards;
        }

        public static List<YearInfo> GetYearInfos(int awardId,int csId)
        {
            var dt = AwardRepository.GetYears(awardId, csId);
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }
            var yearInfos = new List<YearInfo>();
            foreach (DataRow row in dt.Rows)
            {
                var yearInfo = new YearInfo();
                yearInfo.Id = int.Parse(row["Id"].ToString());//判断yearId下是否有匹配车系
                yearInfo.YearName = row["Year"].ToString();
                var remarks = AwardRepository.GetRemarks(yearInfo.Id, csId);
                if (remarks.Length > 30)
                {
                    remarks = remarks.Substring(0, 30) + "...";
                }
                yearInfo.Remarks = remarks;
                var childAwardInfos = AwardRepository.GetChildAwardInfos(yearInfo.Id, csId);
                if (childAwardInfos == null || childAwardInfos.Count == 0){}
                else
                {
                    yearInfo.ChildAwardInfos = childAwardInfos; //把大奖的remark加上
                }
                yearInfos.Add(yearInfo);
                if (yearInfos.Count == 3)
                {
                    return yearInfos;                    
                }
            }
            return yearInfos;
        }

        public static List<string> GetIds(int csId)
        {
            var dt = AwardRepository.GetIds(csId);
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;                   
            }
            var ids = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                var id = row["Id"].ToString();
                ids.Add(id);
            }
            return ids;
        }
    }
}
