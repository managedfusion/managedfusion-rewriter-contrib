using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web;

namespace ManagedFusion.Rewriter.Contrib
{
	public class ApacheRoute : Route
	{
		private readonly IList<string> _flags;
		private const string HttpContextLogWrittenName = "RouteRequestUrlWrittenToManagedFusionRewriterLog";

		internal static void WriteUrlToLog(HttpContextBase httpContext)
		{
			if (httpContext.Items[HttpContextLogWrittenName] != null)
				return;

			var testUrl = httpContext.Request.Url;
			Manager.Log(String.Format("Input: {0}", testUrl), "Route");

			httpContext.Items[HttpContextLogWrittenName] = true;
		}

		public ApacheRoute(string url, string[] flags, IRouteHandler routeHandler)
			: base(url, routeHandler)
		{
			_flags = flags.Select(x => x.ToUpperInvariant()).ToList().AsReadOnly();
		}

		public override RouteData GetRouteData(HttpContextBase httpContext)
		{
			ApacheRoute.WriteUrlToLog(httpContext);
	
			var routeData = base.GetRouteData(httpContext);

			Manager.LogIf(routeData != null, String.Format("Matched: {0}", Url), "Route");
			return routeData;
		}

		public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
		{
			var data = base.GetVirtualPath(requestContext, values);

			if (data != null && _flags.Contains("LC"))
				data.VirtualPath = data.VirtualPath.ToLowerInvariant();

			return data;
		}
	}
}