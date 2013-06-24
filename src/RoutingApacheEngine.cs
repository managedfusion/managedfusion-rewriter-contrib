using System;
using System.Linq;
using System.IO;

namespace ManagedFusion.Rewriter.Contrib
{
	public class RoutingApacheEngine : Engines.ApacheEngine
	{
		protected override void Add(string relativePath, FileInfo file)
		{
			var rule = new RoutingApacheRuleSet(relativePath, file);
			Paths.Add(relativePath, rule);

			// start monitoring the rule set
			AddRuleSetMonitoring(relativePath, file.FullName);
		}
	}
}
