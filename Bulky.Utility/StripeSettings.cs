using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utility
{
	public class StripeSettings
	{

		// make sure its exact same as appsetting.json stripe name
		public string SecretKey { get; set; } 
		public string PublishableKey { get; set; }

	}
}
