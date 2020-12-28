using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZWaveLib.WebAPI.ScheduledJobs
{
    [DisallowConcurrentExecution]
    public class HealNetworkScheduledJob : IJob
    {
        private readonly ZWaveController controller;
        private readonly ILogger<HealNetworkScheduledJob> logger;

        public HealNetworkScheduledJob(ZWaveController controller, ILogger<HealNetworkScheduledJob> logger)
        {
            this.controller = controller;
            this.logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Calling controller.HealNetwork()");
            controller.HealNetwork();

            return Task.CompletedTask;
        }
    }
}
