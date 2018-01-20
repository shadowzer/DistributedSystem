using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;

namespace Node
{
	class Program
	{
		/// <param name="args">[0] - port, [1] - proxy IP</param>
		static void Main(string[] args)
		{
			if (args.Length == 2)
			{
				Storage.MasterIP = args[1];
				Node node = new Node(port: args[0]);
			}
			else
			{
				Console.WriteLine("[CRITICAL ERROR] You must run this app with 2 parameters:\n1. Port\n2. Proxy IP or node master IP with port\n\nPlease rerun app with correct parameters");
				Console.ReadKey();
			}
		}
	}
}