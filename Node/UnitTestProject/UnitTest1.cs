using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Node;

namespace UnitTestProject
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void MainTest()
		{
			var processes = new List<Process>();
			try
			{
				var masterWithReplicas = new string[] {"8080", "8090", "8091"};
				ClearData();
				processes.Add(StartProxy("10000"));

				// simple DB + replication
				processes.Add(StartNode("8080", "10000")); // master 0
				processes.Add(StartNode("8090", "8080")); // replica 0
				processes.Add(StartNode("8091", "8080")); // replica 1
				FillData("8080", 100, 0, 200);
				foreach (var node in masterWithReplicas)
				{
					CheckNodeData(node, 100);
				}
				FillData("8080", 1000, 0, 200); // update
				foreach (var node in masterWithReplicas)
				{
					CheckNodeData(node, 1000);
				}
				Delete0KeyAndCheckFor404("8080");

				// resharding
				processes.Add(StartNode("8081", "10000"));
				Assert.IsTrue(CheckUniformDistributionData(new string[] {"8080", "8081"}));
				FillData("10000", 100, 200, 280);
				processes.Add(StartNode("8082", "10000"));
				processes.Add(StartNode("8083", "10000"));
				Assert.IsTrue(CheckUniformDistributionData(new string[] {"8080", "8081", "8082", "8083"}));
			}
			finally
			{
				foreach (var process in processes)
				{
					process.Kill();
				}
			}
		}

		public void ClearData()
		{
			var path = @"nodes\";
			for (var port = 8080; port < 8092; ++port)
			{
				File.WriteAllText(path + port + ".txt", "");
			}
		}

		public Process StartProxy(string port)
		{
			ProcessStartInfo info = new ProcessStartInfo("Proxy.exe")
			{
				UseShellExecute = true,
				Verb = "runas",
				Arguments = port
			};
			var process = Process.Start(info);
			Thread.Sleep(1000);
			return process;
		}

		public Process StartNode(string port, string masterPort)
		{
			var process = Process.Start("Node.exe", port + " http://localhost:" + masterPort);
			Thread.Sleep(3000);
			return process;
		}

		public void FillData(string port, int addend, int from, int to)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
			{
				for (var i = from; i < to; i++)
				{
					//client.PostAsync("api/values/" + i, new StringContent((addend + i).ToString(), Encoding.UTF8, "application/json"));
					var response = Sender.PostAsync(client, "api/values/" + i, (addend + i).ToString());
					Thread.Sleep(10);
				}
			}
			Thread.Sleep(500);
		}

		public void CheckNodeData(string port, int addend)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
			{
				for (var i = 0; i < 200; i++)
				{
					var response = Sender.GetAsync(client, "api/values/" + i);
					Thread.Sleep(5);
					var result = JsonConvert.DeserializeObject(response.Result.Content.ReadAsStringAsync().Result, typeof(string));
					Assert.AreEqual((addend + i).ToString(), result);
				}
			}
		}

		public void Delete0KeyAndCheckFor404(string port)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
			{
				var response = Sender.DeleteAsync(client, "api/values/0");
				Thread.Sleep(5);
				response = Sender.GetAsync(client, "api/values/0");
				Thread.Sleep(5);
				Assert.AreEqual(HttpStatusCode.NotFound, response.Result.StatusCode);
			}
		}

		/// <returns>true if distribution is normal, false otherwise</returns>
		public bool CheckUniformDistributionData(string[] ports)
		{
			var recordsCountInNodes = new int[ports.Length];
			var sum = 0;
			for (var i = 0; i < ports.Length; ++i)
			{
				using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + ports[i] + "/")})
				{
					var response = client.GetAsync("api/node/");
					if (response.Result.IsSuccessStatusCode)
					{
						var recordsCount = int.Parse(response.Result.Content.ReadAsStringAsync().Result);
						recordsCountInNodes[i] = recordsCount;
						sum += recordsCount;
					}
					else
						recordsCountInNodes[i] = 0;
				}
			}
			var avg = sum / recordsCountInNodes.Length;
			return recordsCountInNodes.Select(recordsCount => recordsCount / avg)
				.All(percent => (percent > 0.85) && (percent < 1.15));
		}
	}
}