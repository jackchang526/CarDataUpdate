using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class City
    {
        private int m_cityId;
        private int m_cityLevel;
        private string m_cityName;
        private string m_cityEName;

        public City()
        {

        }

        public City(int id, string name)
        {
            m_cityId = id;
            m_cityName = name;
        }

        public int CityId
        {
            get { return m_cityId; }
            set { m_cityId = value; }
        }

        public string CityName
        {
            get { return m_cityName; }
            set { m_cityName = value; }
        }

        public int CityLevel
        {
            get { return m_cityLevel; }
            set { m_cityLevel = value; }
        }
        /// <summary>
        /// 城市的英文名称
        /// </summary>
        public string CityEName
        {
            get { return m_cityEName; }
            set { m_cityEName = value; }
        }
    }
}
