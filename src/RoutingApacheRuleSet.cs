using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Routing;
using System.Web.Mvc;

namespace ManagedFusion.Rewriter.Contrib
{
	public class RoutingApacheRuleSet : Engines.ApacheRuleSet
	{
		private const RegexOptions FileOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant;

		private static readonly Regex RouteUrlLine = new Regex(@"^RouteUrl[\s]+(?<url>[\S]+)([\s]+""(?<name>[\S]+)"")?[\s]*", FileOptions);
		private static readonly Regex RouteDefaultLine = new Regex(@"^RouteDefault[\s]+(?<name>[\S]+)[\s]+""?(?<value>[\S]*)""?[\s]*", FileOptions);
		private static readonly Regex RouteConstraintLine = new Regex(@"^RouteConstraint[\s]+(?<name>[\S]+)[\s]+(?<value>[\S]+)[\s]*", FileOptions);
		private static readonly Regex RouteNamespaceLine = new Regex(@"^RouteNamespace[\s]+(?<namespace>[\S]+)[\s]*", FileOptions);
		private static readonly Regex RouteIgnoreUrlLine = new Regex(@"^RouteIgnoreUrl[\s]+(?<url>[\S]+)[\s]*", FileOptions);
		private static readonly Regex RouteAreaLine = new Regex(@"^RouteArea[\s]+(?<area>[\S]+)[\s]*", FileOptions);

		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingApacheRuleSet"/> class.
		/// </summary>
		/// <param name="physicalBase">The physical base.</param>
		/// <param name="ruleSetFile">The rule set file.</param>
		public RoutingApacheRuleSet(string physicalBase, FileInfo ruleSetFile)
			: base(physicalBase, ruleSetFile) { }

		protected override void RefreshUnknownLines(ref IList<string> lines)
		{
			var routes = RouteTable.Routes;

			// remove all the current routes
			// TODO: find a way to remove just the routes from the current ruleset not all of them
			routes.Clear();

			string areaName = null;
			IList<string> unknownLines = new List<string>();
			IDictionary<string, object> defaults = new Dictionary<string, object>();
			IDictionary<string, object> constraints = new Dictionary<string, object>();
			IList<string> namespaces = new List<string>();

			foreach (var line in lines)
			{
				if (RouteUrlLine.IsMatch(line))
				{
					Match match = RouteUrlLine.Match(line);
					string url = match.Groups["url"].Value;
					string name = match.Groups["name"].Value;

					if (String.IsNullOrEmpty(name))
						name = Guid.NewGuid().ToString("N");

					//Route route = new Route(url, new MvcContrib.Routing.DebugRouteHandler()) {
					var route = new Route(url, new MvcRouteHandler()) {
						Defaults = new RouteValueDictionary(defaults),
						Constraints = new RouteValueDictionary(constraints),
						DataTokens = new RouteValueDictionary()
					};

					if (namespaces.Count > 0)
						route.DataTokens["Namespaces"] = namespaces.ToArray();

					if (areaName != null && areaName.Trim().Length > 0)
					{
						route.DataTokens["area"] = areaName;

						// disabling the namespace lookup fallback mechanism keeps this areas from accidentally picking up
						// controllers belonging to other areas
						bool useNamespaceFallback = (namespaces.Count == 0);
						route.DataTokens["UseNamespaceFallback"] = useNamespaceFallback;
					}

					routes.Add(name, route);

					areaName = null;
					defaults.Clear();
					constraints.Clear();
					namespaces.Clear();
				}
				else if (RouteIgnoreUrlLine.IsMatch(line))
				{
					Match match = RouteIgnoreUrlLine.Match(line);
					string url = match.Groups["url"].Value;

					Route route = new IgnoreRouteInternal(url) {
						Constraints = new RouteValueDictionary(constraints)
					};

					routes.Add(route);

					defaults.Clear();
					constraints.Clear();
					namespaces.Clear();
				}
				else if (RouteDefaultLine.IsMatch(line))
				{
					Match match = RouteDefaultLine.Match(line);
					string name = match.Groups["name"].Value;
					object value = match.Groups["value"].Value;

					if (String.Equals(value, "?"))
						value = UrlParameter.Optional;

					defaults.Add(name, value);
				}
				else if (RouteConstraintLine.IsMatch(line))
				{
					Match match = RouteConstraintLine.Match(line);
					string name = match.Groups["name"].Value;
					string value = match.Groups["value"].Value;

					constraints.Add(name, value);
				}
				else if (RouteNamespaceLine.IsMatch(line))
				{
					Match match = RouteNamespaceLine.Match(line);
					string ns = match.Groups["namespace"].Value;

					namespaces.Add(ns);
				}
				else if (RouteAreaLine.IsMatch(line))
				{
					Match match = RouteAreaLine.Match(line);
					areaName = match.Groups["area"].Value;
				}
				else
				{
					unknownLines.Add(line);
				}
			}

			lines = unknownLines;
		}

		private sealed class IgnoreRouteInternal : Route
		{
			public IgnoreRouteInternal(string url)
				: base(url, new StopRoutingHandler()) { }

			public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues)
			{
				// Never match during route generation. This avoids the scenario where an IgnoreRoute with
				// fairly relaxed constraints ends up eagerly matching all generated URLs.
				return null;
			}
		}
	}
}