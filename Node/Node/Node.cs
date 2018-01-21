using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace Node
{
	class Node
	{
		private readonly int Port;
		public int Number { get; private set; }
		public static Dictionary<string, string> Data { get; private set; }

		public Node(string port)
		{
			this.Port = int.Parse(port);
			Data = new Dictionary<string, string>();
			InitDb();

			using (WebApp.Start<Startup>(url: "http://localhost:" + Port + "/")) // WebApp.Start<Startup>()
			{
				Console.WriteLine("Node started on port " + Port);
				RegisterNode();
				while (true) { }
			}
		}

		private void InitDb()
		{
			Storage.FilePath = @"nodes\" + Port + ".txt";

			if (!File.Exists(Storage.FilePath))
			{
				File.Create(Storage.FilePath);
			}
			else
			{
				foreach (var item in File.ReadLines(Storage.FilePath).ToList())
				{
					var words = item.Split(new char[] {' '}, 2);
					Data.Add(words[0], words[1]);
				}
			}
		}

		public static void WriteData()
		{
			using (var writer = new StreamWriter(Storage.FilePath, false))
			{
				foreach (var item in Data)
				{
					writer.Write(item.Key + " ");
					writer.WriteLine(item.Value);
				}
			}
		}

		private void RegisterNode()
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(Storage.MasterIP + "/");
				Task<HttpResponseMessage> response;
				while (true)
				{
					response = client.PostAsync("api/node/", new StringContent(Port.ToString(), Encoding.UTF8, "application/json"));
					if (response.Result.StatusCode == HttpStatusCode.OK)
						break;
					Console.WriteLine("Register status code " + response.Result.StatusCode);
				}
				var result = response.Result.Content.ReadAsStringAsync().Result;
				if (result != null && result.Trim().Length > 0)
				{
					Number = int.Parse(result);
					Console.WriteLine("Node#{0} was registered by proxy.", Number);
				}
				else
				{
					Console.WriteLine("Node was registered as replica.");
				}
			}
		}
	}
}