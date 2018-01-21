using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace Node
{
    public class ValuesController : ApiController
	{
		[HttpGet]
        public HttpResponseMessage Get(string id)
		{
			Console.WriteLine("[GET] " + id);
			return Node.Data.ContainsKey(id) ? 
				Request.CreateResponse(HttpStatusCode.OK, Node.Data[id]) : 
				Request.CreateResponse(HttpStatusCode.BadRequest, "[ERROR] Данный ключ отсутствует в словаре.");
		}

		[HttpPost]
        public HttpResponseMessage Put(string id, [FromBody]string value)
        {
	        Console.WriteLine("[POST] " + id + ": " + value);
			if (!Node.Data.ContainsKey(id))
		        Node.Data.Add(id, value);
            else
                Node.Data[id] = value;

	        foreach (var replica in Storage.Replicas)
	        {
		        using (var client = new HttpClient() { BaseAddress = new Uri("http://" + replica + "/") })
		        {
			        var response = client.PostAsync("api/values/" + id, new StringContent(value, Encoding.UTF8, "application/json"));
					Console.WriteLine("Sended [POST] request to replica " + client.BaseAddress);
					if (response.Result.StatusCode != HttpStatusCode.OK)
						Console.WriteLine(response.Result.StatusCode + ": " + response.Result.Content.ReadAsStringAsync().Result);
		        }
	        }

			Node.WriteData();
	        return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpDelete]
        public HttpResponseMessage Delete(string id)
		{
			Console.WriteLine("[DELETE] " + id);
			if (Node.Data.ContainsKey(id))
                Node.Data.Remove(id);
            else
	            return Request.CreateResponse(HttpStatusCode.BadRequest, "[ERROR] Данный ключ отсутствует в словаре.");

	        foreach (var replica in Storage.Replicas)
			{
				using (var client = new HttpClient() { BaseAddress = new Uri("http://" + replica + "/") })
				{
					var response = client.DeleteAsync("delete/" + id);
					Console.WriteLine("Sended [DELETE] request to replica " + client.BaseAddress);
					if (response.Result.StatusCode != HttpStatusCode.OK)
						Console.WriteLine(response.Result.StatusCode + ": " + response.Result.Content.ReadAsStringAsync().Result);
				}
	        }

	        Node.WriteData();
	        return Request.CreateResponse(HttpStatusCode.OK);
		}
    }
}
