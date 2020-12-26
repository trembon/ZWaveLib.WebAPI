using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ZWaveLib.WebAPI.Models;

namespace ZWaveLib.WebAPI.Controllers
{
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private ZWaveController controller;

        public DevicesController(ZWaveController controller)
        {
            this.controller = controller;
        }

        [HttpGet("/devices/")]
        public ActionResult<IEnumerable<Device>> GetAll()
        {
            if (controller.Status != ControllerStatus.Ready)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Controller is not ready yet.");

            var devices = controller.Nodes.Select(n => new Device
            {
                ID = n.Id,
                ProtocolInfo = n.ProtocolInfo,
                ManufacturerSpecific = n.ManufacturerSpecific,
                SupportedCommands = n.CommandClasses.Select(cc => cc.CommandClass.ToString()).ToList()
            }).ToList();

            return Ok(devices);
        }

        [HttpGet("/devices/{id}")]
        public ActionResult<Device> Get(byte id)
        {
            if (controller.Status != ControllerStatus.Ready)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Controller is not ready yet.");

            var node = controller.GetNode(id);
            if (node == null)
                return NotFound();

            return Ok(new Device
            {
                ID = node.Id,
                ProtocolInfo = node.ProtocolInfo,
                ManufacturerSpecific = node.ManufacturerSpecific,
                SupportedCommands = node.CommandClasses.Select(cc => cc.CommandClass.ToString()).ToList()
            });
        }
    }
}
