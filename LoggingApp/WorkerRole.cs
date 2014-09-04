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
			var queueLengthBeforeFinishing = ((instanceId % 7) == 0) ? 10000 : 1000;
			var rand = new Random();
			int numMessagesTraced = 0;
			Queue<Guid> operationIds = new Queue<Guid>();
			while (true)
			{
				{
					var newOperationId = Guid.NewGuid();
					_logger.TraceMessage(newOperationId, "Operation started: starting my super-important job.");
					numMessagesTraced++;
					operationIds.Enqueue(newOperationId);
				}
				if (operationIds.Count >= queueLengthBeforeFinishing)
				{
					var finishedId = operationIds.Dequeue();
					var failed = rand.NextDouble() < failureProbability;
					if (failed)
					{
						_logger.TraceMessage(finishedId, "Operation failed: I just failed at my super-important job.");
					}
					else
					{
						_logger.TraceMessage(finishedId, "Operation succeeded: I just aced my job as usual.");
					}
					numMessagesTraced++;
				}
				if ((numMessagesTraced % 10000) == 0)
				{
					Trace.TraceInformation("Traced " + numMessagesTraced + " messages so far.");
				}
			}
		}

		public override bool OnStart()
		{
			var account = CloudStorageAccount.Parse(
				RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
			var client = account.CreateCloudBlobClient();
			var container = client.GetContainerReference("bloblogs2");
			container.CreateIfNotExists();
			var blob = container.GetBlockBlobReference("logs/" + RoleEnvironment.CurrentRoleInstance.Id);
			_logger = new BlobLogger(blob);
			return base.OnStart();
		}
	}
}
