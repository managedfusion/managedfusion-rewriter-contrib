using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ManagedFusion.Rewriter.Engines;

namespace ManagedFusion.Rewriter.Contrib
{
	public class RoutingApacheEngine : ManagedFusion.Rewriter.Engines.ApacheEngine
	{
		protected override void Add(string relativePath, FileInfo file)
		{
			RoutingApacheRuleSet rule = new RoutingApacheRuleSet(relativePath, file);
			Paths.Add(relativePath, rule);

			// start monitoring the rule set
			AddRuleSetMonitoring(relativePath, file.FullName);
		}
	}
}
