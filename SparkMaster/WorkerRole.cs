using CommonConfiguration;
using Microsoft.Experimental.Azure.Spark;
using System.Collections.Immutable;

namespace SparkMaster
{
	public class WorkerRole : SparkNodeBase
	{
		protected override ImmutableDictionary<string, string> GetHadoopConfigProperties()
		{
			return WasbConfiguration.GetWasbConfigKeys();
		}

		protected override bool IsMaster
		{
			get { return true; }
		}
	}
}
