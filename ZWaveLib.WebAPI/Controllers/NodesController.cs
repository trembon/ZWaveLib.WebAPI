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
    public class NodesController : ControllerBase
    {
        private ZWaveController controller;

        public NodesController(ZWaveController controller)
        {
            this.controller = controller;
        }

        [HttpGet("/nodes/")]
        public ActionResult<IEnumerable<NodeModel>> GetAll()
        {
            if (controller.Status != ControllerStatus.Ready)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Controller is not ready yet.");

            var devices = controller.Nodes.Select(n => new NodeModel
            {
                ID = n.Id,
                ProtocolInfo = n.ProtocolInfo,
                ManufacturerSpecific = n.ManufacturerSpecific,
                SupportedCommands = n.CommandClasses
            }).ToList();

            return Ok(devices);
        }

        [HttpGet("/nodes/{id}")]
        public ActionResult<NodeModel> Get(byte id)
        {
            if (controller.Status != ControllerStatus.Ready)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Controller is not ready yet.");

            var node = controller.GetNode(id);
            if (node == null)
                return NotFound();

            return Ok(new NodeModel
            {
                ID = node.Id,
                ProtocolInfo = node.ProtocolInfo,
                ManufacturerSpecific = node.ManufacturerSpecific,
                SupportedCommands = node.CommandClasses
            });
        }
    }
}
