using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ZWaveLib.CommandClasses;

namespace ZWaveLib.WebAPI.Controllers
{
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly ZWaveController controller;

        public CommandController(ZWaveController controller)
        {
            this.controller = controller;
        }

        [HttpPost("/nodes/{id}/send/{command}")]
        public ActionResult<CallbackStatus> SendCommand(byte id, CommandClass command, string parameter = null)
        {
            if (controller.Status != ControllerStatus.Ready)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Controller is not ready yet.");

            var node = controller.GetNode(id);
            if (node == null)
                return NotFound();

            ZWaveMessage message;
            switch (command)
            {
                case CommandClass.Basic:
                    message = Basic.Set(node, int.Parse(parameter));
                    break;

                default:
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Command not implemented");
            }

            message.Wait();
            return Ok(message.CallbackStatus);
        }
    }
}
