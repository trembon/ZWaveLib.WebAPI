using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZWaveLib.CommandClasses;

namespace ZWaveLib.WebAPI.Models
{
    public class Device
    {
        public byte ID { get; set; }

        public NodeCapabilities ProtocolInfo { get; set; }

        public ManufacturerSpecificInfo ManufacturerSpecific { get; set; }

        public List<string> SupportedCommands { get; set; }
    }
}
