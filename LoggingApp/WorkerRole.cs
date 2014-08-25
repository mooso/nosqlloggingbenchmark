using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace LoggingApp
{
	public class WorkerRole : RoleEntryPoint
	{
		private BlobLogger _logger;

		public override void Run()
		{
			var instanceId = int.Parse(RoleEnvironment.CurrentRoleInstance.Id.Substring(RoleEnvironment.CurrentRoleInstance.Id.LastIndexOf('_') + 1));
			var failureProbability = ((instanceId % 10) == 0) ? 0.1 : 0.01;
			var rand = new Random();
			int numMessagesTraced = 0;
			while (true)
			{
				var failed = rand.NextDouble() < failureProbability;
				if (failed)
				{
					_logger.TraceMessage("Operation failed: I just failed at my super-important job.");
				}
				else
				{
					_logger.TraceMessage("Operation succeeded: I just aced my job as usual.");
				}
				numMessagesTraced++;
				if ((numMessagesTraced % 1000) == 0)
				{
					Trace.TraceInformation("Traced " + numMessagesTraced + " messages so far.");
				}
				//Thread.Sleep(100);
			}
		}

		public override bool OnStart()
		{
			var account = CloudStorageAccount.Parse(
				RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
			var client = account.CreateCloudBlobClient();
			var container = client.GetContainerReference("bloblogs");
			container.CreateIfNotExists();
			var blob = container.GetBlockBlobReference("logs/" + RoleEnvironment.CurrentRoleInstance.Id);
			_logger = new BlobLogger(blob);
			return base.OnStart();
		}
	}
}
