using CommonConfiguration;
using Microsoft.Experimental.Azure.Spark;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.IO;
using System.Collections.Generic;

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

		public override ImmutableDictionary<string, string> ExtraSparkProperties
		{
			get
			{
				return new Dictionary<string, string>() {
					{ "spark.local.dir", @"Q:\SparkLocal" },
					{ "spark.storage.blockManagerTimeoutIntervalMs", (5 * 60 * 1000).ToString() },
					{ "spark.core.connection.connect.threads.max", "60" },
					{ "spark.core.connection.ack.wait.timeout", (10 * 60 * 1000).ToString() },
				}.ToImmutableDictionary();
			}
		}

		/// <summary>
		/// Override executor memory to give all the Large node's memory to Shark.
		/// </summary>
		protected override int ExecutorMemoryMb
		{
			get
			{
				return 6 * 1024 - 512;
			}
		}
	}
}
