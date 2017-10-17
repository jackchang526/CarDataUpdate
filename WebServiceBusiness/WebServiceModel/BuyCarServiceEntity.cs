using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.WebServiceModel
{
	public class BuyCarServiceEntity
	{
		public int Id { get; set; }
		public Guid Guid { get; set; }
		public int CarId { get; set; }
		public int CityId { get; set; }
		public int CsId { get; set; }
		public decimal Price { get; set; }
		/// <summary>
		/// add by sk 2015.11.24  业务线（商城） 图片地址 
		/// </summary>
		public string ImageUrl { get; set; }
		/// <summary>
		/// add by sk 2015.11.24 业务线（商城） 车型显示名称 
		/// </summary>
		public string DisplayName { get; set; }
		public string ShortRemarks { get; set; }
		public string Remarks { get; set; }
		public string Url { get; set; }
		public string MUrl { get; set; }
		public DateTime UpdateTime { get; set; }
		public int Status { get; set; }
	}
}
