using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Proxy
{
	public class ValuesController : ApiController
	{
		[HttpGet]
		public HttpResponseMessage Get(int id)
		{
			var key = id % Storage.Nodes.Count;
			using (var client = new HttpClient() {BaseAddress = new Uri(Storage.Nodes[key] + "/")})
			{
				var response = Sender.GetAsync(client, "api/values/" + id);
				if (response.Result.StatusCode == HttpStatusCode.OK)
					return Request.CreateResponse(HttpStatusCode.OK, response.Result.Content.ReadAsStringAsync().Result);
			}
			if (Storage.SystemStatus == "resharding")
			{
				key = id % (Storage.Nodes.Count - 1);
				using (var client = new HttpClient() { BaseAddress = new Uri(Storage.Nodes[key] + "/") })
				{
					var response = Sender.GetAsync(client, "api/values/" + id);
					return Request.CreateResponse(HttpStatusCode.OK, response.Result.Content.ReadAsStringAsync().Result);
				}
			}

			return Request.CreateResponse(HttpStatusCode.NotFound, "[ERROR] Не удалось найти искомые данные.");
		}

		[HttpPost]
		public HttpResponseMessage Put(int id, [FromBody] string value)
		{
			var key = id % Storage.Nodes.Count;
			using (var client = new HttpClient() { BaseAddress = new Uri("http://" + Storage.Nodes[key] + "/") })
			{
				var response = Sender.PostAsync(client, "api/values/" + id, value);
				if (response.Result.StatusCode == HttpStatusCode.OK)
					return Request.CreateResponse(HttpStatusCode.OK, response.Result.Content.ReadAsStringAsync().Result);
			}
			if (Storage.SystemStatus == "resharding")
			{
				key = id % (Storage.Nodes.Count - 1);
				using (var client = new HttpClient() { BaseAddress = new Uri("http://" + Storage.Nodes[key] + "/") })
				{
					var response = Sender.PostAsync(client, "api/values/" + id, value);
					return Request.CreateResponse(response.Result.StatusCode, response.Result.Content.ReadAsStringAsync().Result);
				}
			}

			return Request.CreateResponse(HttpStatusCode.NotFound, "[ERROR] Не удалось найти искомые данные.");
		}

		[HttpDelete]
		public HttpResponseMessage Delete(int id)
		{
			var key = id % Storage.Nodes.Count;
			using (var client = new HttpClient() { BaseAddress = new Uri("http://" + Storage.Nodes[key] + "/") })
			{
				var response = Sender.DeleteAsync(client, "api/values/" + id);
				if (response.Result.StatusCode == HttpStatusCode.OK)
					return Request.CreateResponse(HttpStatusCode.OK, response.Result.Content.ReadAsStringAsync().Result);
			}
			if (Storage.SystemStatus == "resharding")
			{
				key = id % (Storage.Nodes.Count - 1);
				using (var client = new HttpClient() { BaseAddress = new Uri("http://" + Storage.Nodes[key] + "/") })
				{
					var response = Sender.DeleteAsync(client, "api/values/" + id);
					return Request.CreateResponse(response.Result.StatusCode, response.Result.Content.ReadAsStringAsync().Result);
				}
			}

			return Request.CreateResponse(HttpStatusCode.NotFound, "[ERROR] Не удалось найти искомые данные.");
		}
	}
}
