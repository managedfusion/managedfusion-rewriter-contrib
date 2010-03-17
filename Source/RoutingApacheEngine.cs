using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ManagedFusion.Rewriter.Contrib
{
	public class RoutingApacheEngine : ManagedFusion.Rewriter.Engines.ApacheEngine
	{
		protected override void Add(string relativePath, FileInfo file)
		{
			base.Add(relativePath, file);
		}
	}
}
