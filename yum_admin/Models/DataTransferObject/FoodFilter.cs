using Microsoft.AspNetCore.Mvc;

namespace yum_admin.Models.DataTransferObject
{
	public class FoodFilter
	{
		[FromForm(Name = "Food")]
		public string? name { get; set; }

		[FromForm(Name = "Attr")]
		public byte? attrId { get; set; }
	}
}
