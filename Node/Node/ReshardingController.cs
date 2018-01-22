using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Node
{
	public class ReshardingController : ApiController
	{
		[HttpGet]
		public HttpResponseMessage ReshardData(int id)
		{
			var nodesNumber = id;
			using (var client = new HttpClient() {BaseAddress = new Uri(Storage.MasterIP + "/")})
			{
				var keysToRemove = new List<int>();
				foreach (var entry in Node.Data)
				{
					if (entry.Key % nodesNumber != Node.Number)
					{
						var response = Sender.PostAsync(client, "api/resharding/" + entry.Key, entry.Value);
						if (response.Result.IsSuccessStatusCode)
							keysToRemove.Add(entry.Key);
					}
				}
				foreach (var replica in Storage.Replicas)
				{
					using (var replicaClient = new HttpClient() {BaseAddress = new Uri("http://" + replica + "/")})
					{
						foreach (var key in keysToRemove)
						{
							var response = Sender.DeleteAsync(replicaClient, "api/values/" + key);
							if (response.Result.StatusCode != HttpStatusCode.OK)
								Console.WriteLine(response.Result.StatusCode + ": " + response.Result.Content.ReadAsStringAsync().Result);
						}
					}
				}
				foreach (var key in keysToRemove)
				{
					Node.Data.Remove(key);
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpPost]
		public HttpResponseMessage PutReshardedData(int id, [FromBody] string value)
		{
			Console.WriteLine("Resharding record {" + id + ": " + value + "}");
			if (!Node.Data.ContainsKey(id))
			{
				Node.Data.Add(id, value);
				foreach (var replica in Storage.Replicas)
				{
					using (var client = new HttpClient() {BaseAddress = new Uri("http://" + replica + "/")})
					{
						var response = Sender.PostAsync(client, "api/values/" + id, value);
						if (response.Result.StatusCode != HttpStatusCode.OK)
							Console.WriteLine(response.Result.StatusCode + ": " + response.Result.Content.ReadAsStringAsync().Result);
					}
				}
				return Request.CreateResponse(HttpStatusCode.OK);
			}
			else
			{
				Console.WriteLine("Record with id " + id + " is already exists.");
				return Request.CreateResponse(HttpStatusCode.Accepted);
			}
		}
	}
}