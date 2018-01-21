using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
	public static class Storage
	{
		public static Dictionary<int, string> Nodes = new Dictionary<int, string>();
		public static string SystemStatus { get; set; }
	}
}
