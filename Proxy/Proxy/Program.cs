using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace Proxy
{
	class Program
	{
		static void Main(string[] args)
		{
			int port = 10000;
			if (args.Length > 0)
				port = int.Parse(args[0]);
			Storage.N = 0;
			using (WebApp.Start<Startup>(url: "http://localhost:" + port + "/"))
			{
				Console.WriteLine("Proxy started on port " + port);
				Console.WriteLine("Press [ENTER] to close app.");
				Console.ReadLine();
			}
		}
	}
}
