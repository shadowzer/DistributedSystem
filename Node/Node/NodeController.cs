using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Node
{
	public class NodeController : ApiController
	{
		[HttpPost]
		public HttpResponseMessage RegisterNewReplica([FromBody]string port, HttpRequestMessage request)
		{
			var ip = GetClientIp(request);
			if (ip == "127.0.0.1" || ip == "::1")
				ip = "localhost";
			var replicaAddress = ip + ":" + port;
			SendDataToReplica(replicaAddress);
			Console.WriteLine("Registered new replica at " + replicaAddress);
			Storage.Replicas.Add(replicaAddress);
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		private void SendDataToReplica(string address)
		{
			using (var client = new HttpClient() {BaseAddress = new Uri("http://" + address + "/")})
			{
				Console.WriteLine("Copying data to replica [" + Node.Data.Count + " records].");
				foreach (var entry in Node.Data)
				{
					var response = Sender.PostAsync(client, "api/values/" + entry.Key, entry.Value);
					Thread.Sleep(5);
				}
			}
		}

		[HttpGet]
		public HttpResponseMessage GetRecordsCount()
		{
			return Request.CreateResponse(HttpStatusCode.OK, Node.Data.Count);
		}

		private string GetClientIp(HttpRequestMessage request)
		{
			const string HttpContext = "MS_HttpContext";
			const string RemoteEndpointMessage =
			"System.ServiceModel.Channels.RemoteEndpointMessageProperty";
			const string OwinContext = "MS_OwinContext";
			// Web-hosting
			if (request.Properties.ContainsKey(HttpContext))
			{
				HttpContextWrapper ctx =
					(HttpContextWrapper)request.Properties[HttpContext];
				if (ctx != null)
				{
					return ctx.Request.UserHostAddress;
				}
			}

			// Self-hosting
			if (request.Properties.ContainsKey(RemoteEndpointMessage))
			{
				RemoteEndpointMessageProperty remoteEndpoint =
					(RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessage];
				if (remoteEndpoint != null)
				{
					return remoteEndpoint.Address;
				}
			}

			// Self-hosting using Owin
			if (request.Properties.ContainsKey(OwinContext))
			{
				OwinContext owinContext = (OwinContext)request.Properties[OwinContext];
				if (owinContext != null)
				{
					return owinContext.Request.RemoteIpAddress;
				}
			}
			Console.WriteLine("NULL");
			return null;
		}
	}
}
