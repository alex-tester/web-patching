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
    [Route("api/PatchingAvailablePatches")]
    // [Authorize]
    public class PatchingAvailablePatchesController : Controller
    {

        //private static ErrorManagement eMgmt = new ErrorManagement();

        private readonly IMapper mapper;
        public PatchingAvailablePatchesController(IMapper mapper)
        {
            this.mapper = mapper;
        }


        // [Authorize(Roles = "readonly, readwrite")]
        [HttpGet]
        //[EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.All)]
        public async Task<IEnumerable<PatchingAvailablePatches>> GetPatchingAvailablePatches()
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            var AllRecords = await db.PatchingAvailablePatches.ToListAsync();
            return AllRecords.ToList();
        }

        // [Authorize(Roles = "readonly, readwrite")]
        [HttpGet("{id}")]
        //[EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.All)]
        public async Task<IActionResult> GetPatchingAvailablePatchesById(int id)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await db.PatchingAvailablePatches.FindAsync(id);
            if (CurrentRecord == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }
            return Ok(CurrentRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpPost]
        public async Task<IActionResult> InsertPatchingAvailablePatches([FromBody] PatchingAvailablePatches NewRecord)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            if (ModelState.IsValid == false)
            {
                return BadRequest(); // ObjectResult(); //(new ValidationFailedResult(ModelState));
            }


            var MappedRecord = mapper.Map<PatchingAvailablePatches>(NewRecord);
            MappedRecord.CreatedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.CreatedOn = DateTime.Now;
            MappedRecord.ModifiedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.ModifiedOn = DateTime.Now;

            try
            {
                db.PatchingAvailablePatches.Add(MappedRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Insert PatchingAvailablePatches", 0, JsonConvert.SerializeObject(NewRecord));
                return BadRequest("Error Inserting the record into PatchingAvailablePatches.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(MappedRecord);



        }

        //[Authorize(Roles = "readwrite")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatchingAvailablePatches(int id, [FromBody] PatchingAvailablePatches UpdateRecord)
        {
            PatchingAutomationContext dbValidator = new PatchingAutomationContext();
            PatchingAutomationContext db = new PatchingAutomationContext();

            var RecordFound = await dbValidator.PatchingAvailablePatches.FindAsync(id);
            if (RecordFound == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }

            if (ModelState.IsValid == false)
            {
                return BadRequest(); //new ValidationFailedResult(ModelState));
            }


            var MappedRecord = mapper.Map<PatchingAvailablePatches>(UpdateRecord);
            MappedRecord.ModifiedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.ModifiedOn = DateTime.Now;

            try
            {
                db.PatchingAvailablePatches.Update(MappedRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Update PatchingAvailablePatches", id, JsonConvert.SerializeObject(UpdateRecord));
                return BadRequest("Error Updating the record into PatchingAvailablePatches.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(MappedRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUpdatePatchingAvailablePatches(int id, [FromBody]JsonPatchDocument<PatchingAvailablePatches> PatchRecord)
        {
            PatchingAutomationContext dbValidator = new PatchingAutomationContext();
            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await dbValidator.PatchingAvailablePatches.FindAsync(id);
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
                db.PatchingAvailablePatches.Update(CurrentRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Patch Update PatchingAvailablePatches", id, JsonConvert.SerializeObject(PatchRecord));
                return BadRequest("Error Patch Updating the record into PatchingAvailablePatches.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(CurrentRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatchingAvailablePatches(int id)
        {

            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await db.PatchingAvailablePatches.FindAsync(id);
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
                //await eMgmt.SendBasicErrorEmail(ex, "Delete PatchingAvailablePatches", id, "No JSON Body Sent");
                return BadRequest("Error Deleting the record into PatchingAvailablePatches.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok();
        }
    }
}
