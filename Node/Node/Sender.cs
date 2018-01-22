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
		public static async Task<HttpResponseMessage> PostAsync(HttpClient client, string method, string message)
		{
			return await client.PostAsync(method, new StringContent(message, Encoding.UTF8, "application/json"));
		}
	    public static async Task<HttpResponseMessage> GetAsync(HttpClient client, string method)
	    {
		    return await client.GetAsync(method);
		}
	    public static async Task<HttpResponseMessage> DeleteAsync(HttpClient client, string method)
	    {
		    return await client.DeleteAsync(method);
	    }
	}
}
