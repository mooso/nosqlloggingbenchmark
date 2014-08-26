using CommonConfiguration;
using Microsoft.Experimental.Azure.Presto;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestoCoordinator
{
	public class WorkerRole : PrestoNodeBase
	{
		protected override IEnumerable<PrestoCatalogConfig> ConfigurePrestoCatalogs()
		{
			var hiveNode = RoleEnvironment.Roles["HiveMetastore"].Instances
				.Select(GetIPAddress)
				.First();
			var hiveCatalogConfig = new PrestoHiveCatalogConfig(
				metastoreUri: String.Format("thrift://{0}:9083", hiveNode),
				hiveConfigurationProperties: new Dictionary<string, string>()
				{
					{ "fs.azure.skip.metrics", "true" },
					{ "fs.azure.check.block.md5", "false" },
				}.Concat(WasbConfiguration.GetWasbConfigKeys()));
			return new PrestoCatalogConfig[] { hiveCatalogConfig };
		}
		protected override bool IsCoordinator
		{
			get { return true; }
		}
	}
}