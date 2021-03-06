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
using Microsoft.Extensions.Configuration;
using AutoMapper;

namespace wsPatching.Controllers
{
    [Route("api")]
    // [Authorize]
    public class apiController : Controller
    {
        private IConfiguration _configuration;
        private readonly IMapper mapper;

        public apiController(IConfiguration Configuration, IMapper mapper)
        {
            _configuration = Configuration;
            this.mapper = mapper;
        }

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

        [HttpGet("GetMostRecentExecutionByHostName/{id}")]
        public async Task<IActionResult> GetMostRecentExecutionByHostName(string id)
        {
            PatchingExecution result = new PatchingExecution();
            Scheduling sch = new Scheduling();
            var svr = sch.GetServerByHostName(id);
            if (svr.Id == 0)
            {
                return BadRequest("Server not found");
            }

            List<PatchingExecution> pExec = new List<PatchingExecution>();
            pExec = sch.GetPatchingExecutionHistoryByServerId(svr.Id).OrderByDescending(x => x.CreatedOn).ToList();

            if (pExec.Count() > 0)
            {
                //PatchingExecution mostRecent = pExec.First();
                return Ok(pExec.First());
            }
            else
            {
                return Ok(result);
            }

        }


        [HttpGet("GetPatchingInitializationSettings")]
        public async Task<IActionResult> GetPatchInitializationSettings()
        {
            //PatchingSettings ps = JsonConvert.DeserializeObject<PatchingSettings>(_configuration["InstallationSettings"]);
            //var mappedSettings = mapper.Map<PatchingSettings>(_configuration["InstallationSettings"]);
            return Ok(_configuration.GetSection("InstallationSettings").GetChildren());
            //return Ok(_configuration["InstallationSettings:PatchingNetworkShare"]);
        }

        //FOR TESTING - DELETE
        [HttpGet("connectionstring")]
        public async Task<IActionResult> ConnectionString()
        {
            //PatchingSettings ps = JsonConvert.DeserializeObject<PatchingSettings>(_configuration["InstallationSettings"]);
            //var mappedSettings = mapper.Map<PatchingSettings>(_configuration["InstallationSettings"]);
            return Ok(_configuration.GetSection("ConnectionStrings").GetChildren());
            //return Ok(_configuration["InstallationSettings:PatchingNetworkShare"]);
        }

        [HttpGet("GetOrCreateServerRecordByHostName/{id}/{user}")]
        public async Task<IActionResult> GetOrCreateServerRecordByHostName(string id, string user)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            List<ServerObject> AllRecords = db.ServerObject.Where(x => x.Hostname.ToLower() == id.ToLower()).ToList();
            if (AllRecords.Count() > 0)
            {
                return Ok(AllRecords.FirstOrDefault());
            }
            else
            {
                ServerObject newSvr = new ServerObject()
                {
                    Hostname = id,
                    CreatedOn = DateTime.Now,
                    CreatedBy = user.Replace("%%", "\\"),
                    ModifiedOn = DateTime.Now,
                    ModifiedBy = user.Replace("%%", "\\")
                };
                try
                {
                    db.Add(newSvr);
                    db.SaveChanges();
                    return Ok(newSvr);
                }
                catch
                {
                    return BadRequest();
                }
            }
        }


    }
}
