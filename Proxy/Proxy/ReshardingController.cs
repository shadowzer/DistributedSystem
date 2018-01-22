using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Proxy
{
	public class ReshardingController : ApiController
	{
		[HttpPost]
		public HttpResponseMessage PutReshardedData(int id, [FromBody] string value, HttpRequestMessage request)
		{
			var key = id % Storage.Nodes.Count;
			Console.WriteLine("Record with id " + id + " will be resharded to node " + Storage.Nodes[key]);
			using (var client = new HttpClient() {BaseAddress = new Uri("http://" + Storage.Nodes[key] + "/")})
			{
				var response = Sender.PostAsync(client, "api/resharding/" + id, value);
				Thread.Sleep(5);
				return Request.CreateResponse(response.Result.StatusCode, response.Result.Content.ReadAsStringAsync().Result);
			}
		}
	}
}