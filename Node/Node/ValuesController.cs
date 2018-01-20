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
		[Route("get/{id}")]
        public string Get(string id)
        {
            return Node.Data[id];
        }

		[HttpPost]
		[Route("post")]
        public HttpResponseMessage Put(Record record)
        {
			Console.WriteLine("GOT CALL TO POST " + JsonConvert.SerializeObject(record));
	        if (!Node.Data.ContainsKey(record.key))
	        {
		        Node.Data.Add(record.key, record.value);
	        }
            else
            {
                Node.Data[record.key] = record.value;
            }
	        foreach (var replica in Storage.Replicas)
	        {
		        using (var client = new HttpClient() { BaseAddress = new Uri("http://" + replica + "/") })
		        {
			        var response = client.PostAsync("api/values/post/", new StringContent(JsonConvert.SerializeObject(record), Encoding.UTF8, "application/json"));
					Console.WriteLine("Sended post request to replica " + client.BaseAddress);
					Console.WriteLine(response.Result.StatusCode + ": " + response.Result.Content.ReadAsStringAsync().Result);
		        }
	        }

			Node.WriteData();
	        return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpDelete]
		[Route("delete/{id}")]
        public HttpResponseMessage Delete(string id)
        {
            if (Node.Data.ContainsKey(id))
            {
                Node.Data.Remove(id);
            }
	        foreach (var replica in Storage.Replicas)
			{
				using (var client = new HttpClient() { BaseAddress = new Uri("http://" + replica + "/") })
				{
					var response = client.DeleteAsync("delete/" + id);
				}
	        }

	        Node.WriteData();
	        return Request.CreateResponse(HttpStatusCode.OK);
		}
    }
}
