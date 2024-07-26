using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
	public class BannerVM	
	{
		public BannerImage? Banner {  get; set; }

		public IEnumerable<BannerImage>? BannerImage { get; set; }
		public IEnumerable<Product>? Product { get; set; }

	}
}
