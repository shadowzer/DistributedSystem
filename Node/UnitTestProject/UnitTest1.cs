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

namespace UnitTestProject
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void MainTest()
		{
			var slaves = new string[] {"8090", "8091"};
			ClearData();
			StartProxy("10000");

			// simple DB + replication
			StartMasterNode("8080");
			StartSlaves(slaves);
			FillData("8080", 100, 0, 200);
			CheckMasterNodeData("8080", 100);
			CheckSlaveNodesData(slaves, 100);
			UpdateData("8080", 1000);
			CheckMasterNodeData("8080", 1000);
			CheckSlaveNodesData(slaves, 1000);
			Delete0KeyAndCheckFor404("8080");

			// resharding
			StartMasterNode("8081");
			CheckUniformDistributionData(new string[] {"8080", "8081"});
			FillData("10000", 100, 200, 320);
			StartMasterNode("8082");
			StartMasterNode("8083");
			CheckUniformDistributionData(new string[] {"8080", "8081", "8082", "8083"});
		}

		public void ClearData()
		{
			var path = @"nodes\";
			for (var port = 8080; port < 8090; ++port)
			{
				//File.Create(path + port + ".txt");
				File.WriteAllText(path + port + ".txt", "");
			}
		}

		public void StartProxy(string port)
		{
			ProcessStartInfo info = new ProcessStartInfo("Proxy.exe")
			{
				UseShellExecute = true,
				Verb = "runas",
				Arguments = port
			};
			Process.Start(info);
			Thread.Sleep(1000);
		}

		public void StartMasterNode(string port)
		{
			Process.Start("Node.exe", port + " http://localhost:10000");
			Thread.Sleep(3000);
		}

		public void StartSlaves(string[] ports)
		{
			foreach (var port in ports)
			{
				Process.Start("Node.exe", port + " http://localhost:8080");
			}
		}

		public void FillData(string port, int addend, int from, int to)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
			{
				for (var i = from; i < to; i++)
				{
					client.PostAsync("api/values/" + i, new StringContent((addend + i).ToString(), Encoding.UTF8, "application/json"));
				}
			}
		}

		public void CheckMasterNodeData(string port, int addend)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
			{
				for (var i = 0; i < 200; i++)
				{
					var response = client.GetAsync("api/values/" + i);
					Assert.AreEqual(addend + i, int.Parse(response.Result.Content.ReadAsStringAsync().Result));
				}
			}
		}

		public void CheckSlaveNodesData(string[] ports, int addend)
		{
			foreach (var port in ports)
			{
				using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
				{
					for (var i = 0; i < 200; i++)
					{
						var response = client.GetAsync("api/values/" + i);
						Assert.AreEqual(addend + i, int.Parse(response.Result.Content.ReadAsStringAsync().Result));
					}
				}
			}
		}

		public void UpdateData(string port, int addend)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
			{
				for (var i = 0; i < 200; i++)
				{
					client.PostAsync("api/values/" + i, new StringContent((addend + i).ToString(), Encoding.UTF8, "application/json"));
				}
			}
		}

		public void Delete0KeyAndCheckFor404(string port)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://localhost:" + port + "/")})
			{
				client.DeleteAsync("api/values/0");
				var response = client.GetAsync("api/values/0");
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