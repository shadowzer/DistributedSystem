using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Node
{
    public class Sender
    {
        
        public string baseAddress;
        public void Put(string key, string value)
        {
            HttpClient client = new HttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(value));
            jsonContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json"); ;
            var response = client.PutAsync(baseAddress + key,
              jsonContent
               ).Result;

        }
        public string Get(string key)
        {
            HttpClient client = new HttpClient();
            var response = client.GetAsync(baseAddress  + key).Result;
            var tmp = response.StatusCode;
            if (response.StatusCode.ToString() != "OK")
            {
                return HttpStatusCode.BadRequest.ToString();
            }
            else
                return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result).ToString();

        }
        public void Delete(string key)
        {
            var result = new HttpClient().DeleteAsync(baseAddress + key).Result;

        }

		public static async Task<HttpResponseMessage> PostAsync(HttpClient client, string method)
		{
			return await client.PostAsync(method, null); //, new StringContent(message, Encoding.UTF8, "application/json"));
		}
	    public static async Task<HttpResponseMessage> DeleteAsync(HttpClient client, string method)
	    {
		    return await client.DeleteAsync(method);
	    }
	}
}
