using System.Collections.Immutable;
using CommonConfiguration;
using Microsoft.Experimental.Azure.Hive;

namespace HiveMetastore
{
	public class WorkerRole : HiveMetastoreNodeBase
	{
		protected override HiveMetastoreConfig GetMetastoreConfig()
		{
			var metastoreInfo = ServiceConfigFile.ReadFile("SqlMetastoreInfo").ToImmutableList();
			return new HiveSqlServerMetastoreConfig(
				serverUri: "//" + metastoreInfo[0] + ".database.windows.net",
				databaseName: metastoreInfo[1],
				userName: metastoreInfo[2] + "@" + metastoreInfo[0],
				password: metastoreInfo[3]);
		}
	}
}