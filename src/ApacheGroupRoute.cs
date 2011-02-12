﻿using System;
using System.Web;
using System.Web.Routing;
using System.Text.RegularExpressions;

namespace ManagedFusion.Rewriter.Contrib
{
	/// <see href="http://routemagic.codeplex.com"/>
	public class ApacheGroupRoute : RouteBase
	{
		private const string RouteNameKey = "__RouteName";

		public ApacheGroupRoute(string pattern, RegexOptions options, RouteCollection childRoutes)
			: this(new Pattern(pattern, options), childRoutes) { }

		public ApacheGroupRoute(Pattern pattern, RouteCollection childRoutes)
		{
			Pattern = pattern;
			ChildRoutes = childRoutes;
		}

		public Pattern Pattern
		{
			get;
			private set;
		}

		public RouteCollection ChildRoutes
		{
			get;
			private set;
		}

		private string GetRouteName(RouteValueDictionary routeValues)
		{
			if (routeValues == null)
				return null;

			object routeName = null;
			routeValues.TryGetValue(RouteNameKey, out routeName);
			return routeName as string;
		}

		private RouteValueDictionary WithoutRouteName(RouteValueDictionary routeValues)
		{
			routeValues = new RouteValueDictionary(routeValues);
			routeValues.Remove(RouteNameKey);
			return routeValues;
		}

		public override RouteData GetRouteData(HttpContextBase httpContext)
		{
			ApacheRoute.WriteUrlToLog(httpContext);

			var testUrl = httpContext.Request.RawUrl;
			var isMatch = Pattern.IsMatch(testUrl);

			Manager.Log(String.Format("{0}: {1}", (isMatch ? "Matched" : "Not Matched"), Pattern), "Route");

			if (!isMatch)
				return null;

			return ChildRoutes.GetRouteData(httpContext);
		}

		public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
		{
			string routeName = GetRouteName(values);

			var virtualPath = ChildRoutes.GetVirtualPath(requestContext, routeName, WithoutRouteName(values));
			return virtualPath;
		}
	}
}
