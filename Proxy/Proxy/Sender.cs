using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;

namespace Proxy
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

		public static string GetClientIp(HttpRequestMessage request)
		{
			string answer = null;
			const string httpContext = "MS_HttpContext";
			const string remoteEndpointMessage =
				"System.ServiceModel.Channels.RemoteEndpointMessageProperty";
			const string owincontext = "MS_OwinContext";
			// Web-hosting
			if (request.Properties.ContainsKey(httpContext))
			{
				HttpContextWrapper ctx =
					(HttpContextWrapper)request.Properties[httpContext];
				if (ctx != null)
				{
					answer = ctx.Request.UserHostAddress;
				}
			}

			// Self-hosting
			if (request.Properties.ContainsKey(remoteEndpointMessage))
			{
				RemoteEndpointMessageProperty remoteEndpoint =
					(RemoteEndpointMessageProperty)request.Properties[remoteEndpointMessage];
				if (remoteEndpoint != null)
				{
					answer = remoteEndpoint.Address;
				}
			}

			// Self-hosting using Owin
			if (request.Properties.ContainsKey(owincontext))
			{
				OwinContext owinContext = (OwinContext)request.Properties[owincontext];
				if (owinContext != null)
				{
					answer = owinContext.Request.RemoteIpAddress;
				}
			}

			if (answer == "127.0.0.1" || answer == "::1")
				answer = "localhost";
			return answer;
		}
	}
}