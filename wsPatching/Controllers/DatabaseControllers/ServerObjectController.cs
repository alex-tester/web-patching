using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wsPatching.Models.DatabaseModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace wsPatching.Controllers
{
    [Route("api/ServerObject")]
    // [Authorize]
    public class ServerObjectController : Controller
    {

        //private static ErrorManagement eMgmt = new ErrorManagement();

        private readonly IMapper mapper;
        public ServerObjectController(IMapper mapper)
        {
            this.mapper = mapper;
        }


        // [Authorize(Roles = "readonly, readwrite")]
        [HttpGet]
        //[EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.All)]
        public async Task<IEnumerable<ServerObject>> GetServerObject()
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            var AllRecords = await db.ServerObject.ToListAsync();
            return AllRecords.ToList();
        }

        // [Authorize(Roles = "readonly, readwrite")]
        [HttpGet("{id}")]
        //[EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.All)]
        public async Task<IActionResult> GetServerObjectById(int id)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await db.ServerObject.FindAsync(id);
            if (CurrentRecord == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }
            return Ok(CurrentRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpPost]
        public async Task<IActionResult> InsertServerObject([FromBody] ServerObject NewRecord)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            if (ModelState.IsValid == false)
            {
                return BadRequest(); //(new ValidationFailedResult(ModelState));
            }


            var MappedRecord = mapper.Map<ServerObject>(NewRecord);
            MappedRecord.CreatedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.CreatedOn = DateTime.Now;
            MappedRecord.ModifiedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.ModifiedOn = DateTime.Now;

            try
            {
                db.ServerObject.Add(MappedRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Insert ServerObject", 0, JsonConvert.SerializeObject(NewRecord));
                return BadRequest("Error Inserting the record into ServerObject.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(MappedRecord);



        }

        //[Authorize(Roles = "readwrite")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServerObject(int id, [FromBody] ServerObject UpdateRecord)
        {
            PatchingAutomationContext dbValidator = new PatchingAutomationContext();
            PatchingAutomationContext db = new PatchingAutomationContext();

            var RecordFound = await dbValidator.ServerObject.FindAsync(id);
            if (RecordFound == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }

            if (ModelState.IsValid == false)
            {
                return BadRequest(); //new ValidationFailedResult(ModelState));
            }


            var MappedRecord = mapper.Map<ServerObject>(UpdateRecord);
            MappedRecord.ModifiedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.ModifiedOn = DateTime.Now;

            try
            {
                db.ServerObject.Update(MappedRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Update ServerObject", id, JsonConvert.SerializeObject(UpdateRecord));
                return BadRequest("Error Updating the record into ServerObject.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(MappedRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUpdateServerObject(int id, [FromBody]JsonPatchDocument<ServerObject> PatchRecord)
        {
            PatchingAutomationContext dbValidator = new PatchingAutomationContext();
            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await dbValidator.ServerObject.FindAsync(id);
            if (CurrentRecord == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }

            PatchRecord.ApplyTo(CurrentRecord, ModelState);

            if (ModelState.IsValid == false)
            {
                return BadRequest(); //new ValidationFailedResult(ModelState));
            }

            CurrentRecord.ModifiedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            CurrentRecord.ModifiedOn = DateTime.Now;

            try
            {
                db.ServerObject.Update(CurrentRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Patch Update ServerObject", id, JsonConvert.SerializeObject(PatchRecord));
                return BadRequest("Error Patch Updating the record into ServerObject.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(CurrentRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServerObject(int id)
        {

            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await db.ServerObject.FindAsync(id);
            if (CurrentRecord == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }


            try
            {
                db.Remove(CurrentRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Delete ServerObject", id, "No JSON Body Sent");
                return BadRequest("Error Deleting the record into ServerObject.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok();
        }
    }
}
