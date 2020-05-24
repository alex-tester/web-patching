using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wsPatching.Models.DatabaseModels;
using wsPatching.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using wsPatching.Models.CustomModels;

namespace wsPatching.Controllers
{
    [Route("api")]
    // [Authorize]
    public class apiController : Controller
    {


        // [Authorize(Roles = "readonly, readwrite")]
        [HttpGet("GetPatchingConfigByHostName/{id}")]
        public async Task<IActionResult> GetPatchingConfigByHostName(string id)
        {
            Scheduling sch = new Scheduling();
            var svr = sch.GetServerByHostName(id);
            if (svr.Id == 0)
            {
                return BadRequest("Server not found");
            }

            PatchingConfig config = sch.GetPatchingConfigByServerId(svr.Id);

            if (config.Id == 0)
            {
                return BadRequest("Patching config for this server not found");
            }

            //config.PatchingSource.PatchingConfig = null;
            return Ok(config);
        }

        [HttpGet("GetNextPatchDateByHostName/{id}")]
        public async Task<IActionResult> GetNextPatchDateByHostName(string id)
        {
            Scheduling sch = new Scheduling();
            var svr = sch.GetServerByHostName(id);
            if (svr.Id == 0)
            {
                return BadRequest("Server not found");
            }

            PatchingConfig config = sch.GetPatchingConfigByServerId(svr.Id);

            if (config.Id == 0)
            {
                return BadRequest("Patching config for this server not found");
            }

            var result = sch.GetNextPatchDateByServerId(svr.Id);
            return Ok(result);
        }


    }
}
