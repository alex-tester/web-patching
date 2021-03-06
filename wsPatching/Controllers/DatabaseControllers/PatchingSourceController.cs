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
    [Route("api/PatchingSource")]
    // [Authorize]
    public class PatchingSourceController : Controller
    {

        //private static ErrorManagement eMgmt = new ErrorManagement();

        private readonly IMapper mapper;
        public PatchingSourceController(IMapper mapper)
        {
            this.mapper = mapper;
        }


        // [Authorize(Roles = "readonly, readwrite")]
        [HttpGet]
        //[EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.All)]
        public async Task<IEnumerable<PatchingSource>> GetPatchingSource()
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            var AllRecords = await db.PatchingSource.ToListAsync();
            return AllRecords.ToList();
        }

        // [Authorize(Roles = "readonly, readwrite")]
        [HttpGet("{id}")]
        //[EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.All)]
        public async Task<IActionResult> GetPatchingSourceById(int id)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await db.PatchingSource.FindAsync(id);
            if (CurrentRecord == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }
            return Ok(CurrentRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpPost]
        public async Task<IActionResult> InsertPatchingSource([FromBody] PatchingSource NewRecord)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            if (ModelState.IsValid == false)
            {
                return BadRequest(); //(new ValidationFailedResult(ModelState));
            }


            var MappedRecord = mapper.Map<PatchingSource>(NewRecord);
            MappedRecord.CreatedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.CreatedOn = DateTime.Now;
            MappedRecord.ModifiedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.ModifiedOn = DateTime.Now;

            try
            {
                db.PatchingSource.Add(MappedRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Insert PatchingSource", 0, JsonConvert.SerializeObject(NewRecord));
                return BadRequest("Error Inserting the record into PatchingSource.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(MappedRecord);



        }

        //[Authorize(Roles = "readwrite")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatchingSource(int id, [FromBody] PatchingSource UpdateRecord)
        {
            PatchingAutomationContext dbValidator = new PatchingAutomationContext();
            PatchingAutomationContext db = new PatchingAutomationContext();

            var RecordFound = await dbValidator.PatchingSource.FindAsync(id);
            if (RecordFound == null)
            {
                return NotFound("No Record Found with ID : " + id.ToString());
            }

            if (ModelState.IsValid == false)
            {
                return BadRequest(); //new ValidationFailedResult(ModelState));
            }


            var MappedRecord = mapper.Map<PatchingSource>(UpdateRecord);
            MappedRecord.ModifiedBy = "API"; // GlobalFunctions.GetUserNameFromToken(User.Claims.ToList());
            MappedRecord.ModifiedOn = DateTime.Now;

            try
            {
                db.PatchingSource.Update(MappedRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Update PatchingSource", id, JsonConvert.SerializeObject(UpdateRecord));
                return BadRequest("Error Updating the record into PatchingSource.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(MappedRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUpdatePatchingSource(int id, [FromBody]JsonPatchDocument<PatchingSource> PatchRecord)
        {
            PatchingAutomationContext dbValidator = new PatchingAutomationContext();
            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await dbValidator.PatchingSource.FindAsync(id);
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
                db.PatchingSource.Update(CurrentRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await eMgmt.SendBasicErrorEmail(ex, "Patch Update PatchingSource", id, JsonConvert.SerializeObject(PatchRecord));
                return BadRequest("Error Patch Updating the record into PatchingSource.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok(CurrentRecord);
        }

        //[Authorize(Roles = "readwrite")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatchingSource(int id)
        {

            PatchingAutomationContext db = new PatchingAutomationContext();

            var CurrentRecord = await db.PatchingSource.FindAsync(id);
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
                //await eMgmt.SendBasicErrorEmail(ex, "Delete PatchingSource", id, "No JSON Body Sent");
                return BadRequest("Error Deleting the record into PatchingSource.  Exception : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            return Ok();
        }
    }
}
