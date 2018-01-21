using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;

namespace Proxy
{
	public class NodeController : ApiController
	{
		[HttpPost]
		public HttpResponseMessage RegisterNewNode([FromBody] string port, HttpRequestMessage request)
		{
			var ip = Sender.GetClientIp(request);
			var nodeAddress = ip + ":" + port;
			Console.WriteLine("Registered new node at " + nodeAddress);
			Storage.Nodes.Add(Storage.Nodes.Count, nodeAddress);


			Storage.SystemStatus = "resharding";
			// todo resharding
			foreach (var entry in Storage.Nodes)
			{
				if (entry.Key != Storage.Nodes.Count - 1)
				{
					using (var client = new HttpClient() {BaseAddress = new Uri("http://" + entry.Value + "/")})
					{
						var response = Sender.GetAsync(client, "api/resharding/" + Storage.Nodes.Count);
						if (response.Result.IsSuccessStatusCode)
							Console.WriteLine("Node " + entry.Key + " at " + entry.Value + " successfully resharded.\n--------------------");
						else
							Console.WriteLine("Node " + entry.Key + " at " + entry.Value + " failed while resharding: " +
							                  response.Result.StatusCode + "\n" + response.Result.Content.ReadAsStringAsync().Result +
											  "\n--------------------");
					}
				}
			}
			Storage.SystemStatus = "ready";

			return request.CreateResponse(HttpStatusCode.OK, Storage.Nodes.Count - 1);
		}

		[HttpGet]
		public string GetAllNodes()
		{
			var answer = "";
			foreach (var node in Storage.Nodes)
			{
				answer += node + ", ";
			}
			return answer.Substring(0, answer.Length - 2);
		}
	}
}