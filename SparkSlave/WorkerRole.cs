using CommonConfiguration;
using Microsoft.Experimental.Azure.Spark;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace SparkSlave
{
	public class WorkerRole : SparkNodeBase
	{
		protected override ImmutableDictionary<string, string> GetHadoopConfigProperties()
		{
			return WasbConfiguration.GetWasbConfigKeys();
		}

		public override ImmutableDictionary<string, string> ExtraSparkProperties
		{
			get
			{
				const string sparkLocalDir = @"Q:\SparkLocal";
				if (!Directory.Exists(sparkLocalDir))
				{
					Directory.CreateDirectory(sparkLocalDir);
				}
				return new Dictionary<string, string>() {
					{ "spark.local.dir", sparkLocalDir },
				}.ToImmutableDictionary();
			}
		}

		protected override bool IsMaster
		{
			get { return false; }
		}
	}
}
