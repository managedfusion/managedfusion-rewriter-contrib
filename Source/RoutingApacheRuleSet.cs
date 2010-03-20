using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Routing;
using System.Web.Mvc;

namespace ManagedFusion.Rewriter.Contrib
{
	public class RoutingApacheRuleSet : ManagedFusion.Rewriter.Engines.ApacheRuleSet
	{
		private static readonly RegexOptions FileOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant;

		private static readonly Regex RouteUrlLine = new Regex(@"^RouteUrl[\s]+(?<url>[\S]+)([\s]+""(?<name>[\S]+)"")?[\s]*", FileOptions);
		private static readonly Regex RouteDefaultLine = new Regex(@"^RouteDefault[\s]+(?<name>[\S]+)[\s]+(?<value>[\S]+)[\s]*", FileOptions);
		private static readonly Regex RouteConstraintLine = new Regex(@"^RouteConstraint[\s]+(?<name>[\S]+)[\s]+(?<value>[\S]+)[\s]*", FileOptions);
		private static readonly Regex RouteNamespaceLine = new Regex(@"^RouteNamespace[\s]+(?<namespace>[\S]+)[\s]*", FileOptions);

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
						name =  Guid.NewGuid().ToString("N");

					//Route route = new Route(url, new MvcContrib.Routing.DebugRouteHandler()) {
					Route route = new Route(url, new MvcRouteHandler()) {
						Defaults = new RouteValueDictionary(defaults),
						Constraints = new RouteValueDictionary(constraints)
					};

					if ((namespaces != null) && (namespaces.Count > 0))
					{
						route.DataTokens = new RouteValueDictionary();
						route.DataTokens["Namespaces"] = namespaces.ToArray();
					}

					routes.Add(name, route);

					defaults.Clear();
					constraints.Clear();
					namespaces.Clear();
				}
				else if (RouteDefaultLine.IsMatch(line))
				{
					Match match = RouteDefaultLine.Match(line);
					string name = match.Groups["name"].Value;
					string value = match.Groups["value"].Value;

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
				else
				{
					unknownLines.Add(line);
				}
			}

			lines = unknownLines;
		}
	}
}
