using CommonConfiguration;
using Microsoft.Experimental.Azure.Spark;
using System;
using System.Linq;
using System.Collections.Immutable;

namespace Shark
{
	public class WorkerRole : SharkNodeBase
	{
		protected override ImmutableDictionary<string, string> GetHadoopConfigProperties()
		{
			var rootFs = String.Format("wasb://shark@{0}.blob.core.windows.net",
					WasbConfiguration.ReadWasbAccountsFile().First());
			return WasbConfiguration.GetWasbConfigKeys()
				.Add("hive.exec.scratchdir", rootFs + "/scratch")
				.Add("hive.metastore.warehouse.dir", rootFs + "/warehouse");
		}

		/// <summary>
		/// Override executor memory to give all the A6's node's memory to Shark.
		/// </summary>
		protected override int ExecutorMemoryMb
		{
			get
			{
				return 26 * 1024;
			}
		}
	}
}
