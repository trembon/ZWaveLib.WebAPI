using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZWaveLib.WebAPI.Services
{
    public interface IZWaveEventService
    {
        void Initialize();
    }

    public class ZWaveEventService : IZWaveEventService
    {
        private readonly ZWaveController controller;
        private readonly ILogger<ZWaveEventService> logger;

        public ZWaveEventService(ZWaveController controller, ILogger<ZWaveEventService> logger)
        {
            this.controller = controller;
            this.logger = logger;
        }

        public void Initialize()
        {
            controller.ControllerStatusChanged += Controller_ControllerStatusChanged;
            controller.DiscoveryProgress += Controller_DiscoveryProgress;
            controller.NodeOperationProgress += Controller_NodeOperationProgress;
            controller.NodeUpdated += Controller_NodeUpdated;
            controller.HealProgress += Controller_HealProgress;

            controller.Connect();
        }

        private void Controller_HealProgress(object sender, HealProgressEventArgs args)
        {
            logger.LogInformation($"Heal progress changed to {args.Status}");
        }

        private void Controller_NodeUpdated(object sender, NodeUpdatedEventArgs args)
        {
            logger.LogInformation($"Node {args.NodeId} updated.");
        }

        private void Controller_NodeOperationProgress(object sender, NodeOperationProgressEventArgs args)
        {
            logger.LogInformation($"Node {args.NodeId} operation progressed.");
        }

        private void Controller_DiscoveryProgress(object sender, DiscoveryProgressEventArgs args)
        {
            logger.LogInformation($"Discovery progress changed to {args.Status}.");
            switch (args.Status)
            {
                case DiscoveryStatus.DiscoveryStart:
                    break;
                case DiscoveryStatus.DiscoveryEnd:
                    // system should now be up and running!
                    break;
            }
        }

        private void Controller_ControllerStatusChanged(object sender, ControllerStatusEventArgs args)
        {
            logger.LogInformation($"Controller status changed to {args.Status}.");

            var controller = (sender as ZWaveController);
            switch (args.Status)
            {
                case ControllerStatus.Connected:
                    controller.Initialize();
                    break;
                case ControllerStatus.Disconnected:
                    break;
                case ControllerStatus.Initializing:
                    break;
                case ControllerStatus.Ready:
                    controller.Discovery(); // query controller for all nodes it knows about
                    break;
                case ControllerStatus.Error:
                    break;
            }
        }
    }
}
