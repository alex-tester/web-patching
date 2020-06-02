using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using wsPatching.Models.DatabaseModels;
using wsPatching.Models;
using wsPatching.Models.CustomModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.Entity.Validation;
using System.Web.Helpers;
using System.Threading.Tasks;


namespace wsPatching.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class ManageController : Controller
    {
        //
        // GET: /Manage/

        public IActionResult Index()
        {


            return View();
        }


        public IActionResult Schedule(string id)
        {
            ServerObject svr = new ServerObject();
            bool serverHasSchedule = false;

            if (string.IsNullOrEmpty(id))
            {
                svr.Id = 0;
                svr.Hostname = "na";
            }
            else
            {
                //svr.Id = Convert.ToInt32(id); 
                Scheduling sch = new Scheduling();
                svr = sch.GetServerByHostName(id);
                //if (svr.Id != 0) //only query schedule if server really exists
                if (svr != null)
                {
                    PatchingConfig patchConfig = sch.GetPatchingConfigByServerId(svr.Id); //(Convert.ToInt32(id));
                    if (patchConfig.Id != 0)
                    {
                        serverHasSchedule = true;
                    }
                }
                else
                {
                    svr = new ServerObject();
                }
            }
            ViewBag.serverHasSchedule = serverHasSchedule;

            return View(svr);
        }

        public async Task<PartialViewResult> _ScheduleDetails(string id)
        {
            Scheduling sch = new Scheduling();
            PatchingConfig model = sch.GetPatchingConfigByServerId(Convert.ToInt32(id));
            ServerObject svr = sch.GetServerObjectById(Convert.ToInt32(id));
            List<PatchingExecution> execHistory = sch.GetPatchingExecutionHistoryByServerId(Convert.ToInt32(id));

            ViewBag.execHistory = execHistory.OrderBy(x => x.Id).ToList();
            ViewBag.svr = svr;
            return PartialView(model);
        }

        public JsonResult ServerPatchExecutionHistory(string id)
        {
            Scheduling sch = new Scheduling();
            //PatchingConfig model = sch.GetPatchingConfigByServerId(Convert.ToInt32(id));
            //ServerObject svr = sch.GetServerObjectById(Convert.ToInt32(id));
            List<PatchingExecution> execHistory = sch.GetPatchingExecutionHistoryByServerId(Convert.ToInt32(id));

            //ViewBag.execHistory = execHistory.OrderBy(x => x.Id).ToList();
            //ViewBag.svr = svr;
            return Json(execHistory);
        }

        public ActionResult _ServerPatchingEditor(string id)
        {

            Scheduling sch = new Scheduling();
            PatchingScheduleEdit model = new PatchingScheduleEdit();
            PatchingAutomationContext db = new PatchingAutomationContext();
            //List<ServerObject> serverList = new List<ServerObject>();
            if (id != "0")
            {

                var patchDetails = sch.GetPatchingConfig(Convert.ToInt32(id));

                model.HostName = sch.GetServerObjectById(patchDetails.ServerId).Hostname;
                //db.ServerObject.Find(patchDetails.ServerId).Hostname;
                model.PatchingID = patchDetails.Id;
                model.PatchingSourceID = patchDetails.PatchingSourceId;
                model.StartTime = (patchDetails.StartTime != null) ? patchDetails.StartTime.ToString() : null;
                model.EndTime = (patchDetails.EndTime != null) ? patchDetails.EndTime.ToString() : null;

                model.RebootBeforePatch = patchDetails.RebootBeforePatch;
                model.RebootAfterPatch = patchDetails.RebootAfterPatch;
                //model.ForceRebootAfterPatch = patchDetails.ForceRebootAfterPatch;
                model.EnableSecondAttempt = patchDetails.EnableSecondAttempt;
                model.UpdateVmwareTools = patchDetails.UpdateVmwareTools;
                //model.TempDisablePatch = patchDetails.TempDisablePatch;
                model.EnablePrePatchScript = patchDetails.EnablePrePatchScript;
                model.PrePatchScript = patchDetails.PrePatchScript;
                model.EnablePostPatchScript = patchDetails.EnablePostPatchScript;
                model.PostPatchScript = patchDetails.PostPatchScript;

                if (patchDetails.TelerikRecurrenceRule != null)
                {
                    string rrPattern = @"(?<=RRULE:)(.*)";
                    Match rrMatch = Regex.Match(patchDetails.TelerikRecurrenceRule, rrPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string rrValue = rrMatch.Value;

                    string freqPattern = @"(?<=FREQ=)[^;]*";
                    string intervalPattern = @"(?<=INTERVAL=)[^;]*";
                    string posPattern = @"(?<=BYSETPOS=)[^;]*";
                    string dayPattern = @"(?<=BYDAY=)[^;]*";

                    Match freqMatch = Regex.Match(rrValue, freqPattern, RegexOptions.IgnoreCase);
                    Match intervalMatch = Regex.Match(rrValue, intervalPattern, RegexOptions.IgnoreCase);
                    Match posMatch = Regex.Match(rrValue, posPattern, RegexOptions.IgnoreCase);
                    Match dayMatch = Regex.Match(rrValue, dayPattern, RegexOptions.IgnoreCase);

                    string freqValue = freqMatch.Value;
                    string intervalValue = intervalMatch.Value;
                    string positionValue = posMatch.Value;
                    string dayValue = dayMatch.Value;

                    List<string> dayList = new List<string>();
                    foreach (string day in dayValue.Split(';'))
                    {
                        //replace line break if its the last entry
                        string currentDay = day.Replace("\r", "").ToLower();
                        //switch (day.Replace(@"\r","").ToLower())
                        switch (currentDay)
                        {
                            case "mo":
                                dayList.Add("Monday");
                                break;
                            case "tu":
                                dayList.Add("Tuesday");
                                break;
                            case "we":
                                dayList.Add("Wednesday");
                                break;
                            case "th":
                                dayList.Add("Thursday");
                                break;
                            case "fr":
                                dayList.Add("Friday");
                                break;
                            case "sa":
                                dayList.Add("Saturday");
                                break;
                            case "su":
                                dayList.Add("Sunday");
                                break;
                            default:
                                break;


                        }
                    }

                    model.WeekDay = string.Join(",", dayList);

                    //model.DaysOfWeek = string.Join(",", dayList);


                    switch (freqValue.ToLower())
                    {
                        case "hourly":
                            model.HourlyRecurrence = intervalValue;
                            break;
                        case "daily":
                            model.DailyRecurrence = intervalValue;
                            break;
                        case "weekly":
                            model.WeeklyRecurrence = intervalValue;
                            break;
                        case "monthly":
                            model.WeekNumber = positionValue;
                            break;
                        default:

                            break;

                    }



                    model.Frequency = freqValue.ToLower();
                }
                //finish the rest
            }


            var patchSources = (from p in db.PatchingSource select p).ToList();
            ViewBag.patchSources = patchSources;
            //ViewBag.serverList = serverList;

            //ViewBag.ParentID = ParentID;

            return PartialView(model);
        }

        //use when landing on a config page for a server with no schedule defined
        //manage/schedule should handle scenarios where an invalid server is specified in the url
        public ActionResult _ServerPatchingCreator(string id)
        {

            Scheduling sch = new Scheduling();
            PatchingScheduleEdit model = new PatchingScheduleEdit();
            PatchingAutomationContext db = new PatchingAutomationContext();

            var svr = sch.GetServerObjectById(Convert.ToInt32(id));
            model.HostName = svr.Hostname;
            model.ObjectID = svr.Id;
 


            var patchSources = (from p in db.PatchingSource select p).ToList();
            ViewBag.patchSources = patchSources;
            //ViewBag.serverList = serverList;

            //ViewBag.ParentID = ParentID;

            return PartialView(model);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        //public JsonResult ProcessServerPatching(PatchingSchedule fData)
        public JsonResult ProcessServerPatching(PatchingScheduleEdit fData)
        {
            if (Convert.ToDateTime(fData.StartTime) > Convert.ToDateTime(fData.EndTime))
            {
                return Json(new { Failure = true, Message = "The End Time must be after the Start Time" });
            }

           
            PatchingAutomationContext db = new PatchingAutomationContext();

            PatchingConfig model = new PatchingConfig();

            switch (fData.Frequency)
            {
                case "hourly":
                    fData.Interval = fData.HourlyRecurrence;
                    break;
                case "daily":
                    fData.Interval = fData.DailyRecurrence;
                    break;
                case "weekly":
                    fData.Interval = fData.WeeklyRecurrence;
                    break;
                default:
                    fData.Interval = "1";
                    break;

            }

            fData.Start = Convert.ToDateTime(fData.StartTime);

           
            GenerateNewSchedule newSchedule1 = new GenerateNewSchedule();
          
            newSchedule1.Start = fData.Start;
            newSchedule1.Frequency = fData.Frequency;
            newSchedule1.Interval = fData.Interval;
            newSchedule1.WeekDay = fData.WeekDay.Replace(@"\r", "");
            newSchedule1.RunOnDay = fData.RunOnDay;
            newSchedule1.WeekNumber = fData.WeekNumber;
            newSchedule1.Month = fData.Month;
            newSchedule1.UseReboot = true;
            newSchedule1.AutoReboot = fData.RebootAfterPatch;

            Scheduling sch = new Scheduling();

            //Get server hostname so we can return it later when adding a new entry
            ServerObject returnSvr = sch.GetServerObjectById(fData.ObjectID);

            //requestUri = baseUrl + "Scheduling/PostGenerateNewSchedule";
            //var req = HttpRequestFactory.PostNoToken(requestUri, newSchedule1);
            RecurrenceOutput newSchedule = sch.PostGenerateSchedule(newSchedule1); //req.ContentAsType<RecurrenceOutput>();

            if (fData.PatchingID != 0)
            {
               
                var currentRecord = db.PatchingConfig.Find(fData.PatchingID);
                currentRecord.PatchingSourceId = fData.PatchingSourceID;
                currentRecord.PatchingName = newSchedule.ScheduleName;
                currentRecord.TelerikRecurrenceRule = newSchedule.RecurrenceRule;
                currentRecord.StartTime = Convert.ToDateTime(fData.StartTime);
                currentRecord.EndTime = Convert.ToDateTime(fData.EndTime);
                currentRecord.RebootBeforePatch = fData.RebootBeforePatch;
                currentRecord.RebootAfterPatch = fData.RebootAfterPatch;
                currentRecord.EnableSecondAttempt = fData.EnableSecondAttempt;
                currentRecord.UpdateVmwareTools = fData.UpdateVmwareTools;
                currentRecord.EnablePrePatchScript = fData.EnablePrePatchScript;
                currentRecord.EnablePostPatchScript = fData.EnablePostPatchScript;
                currentRecord.PrePatchScript = fData.PrePatchScript;
                currentRecord.PostPatchScript = fData.PostPatchScript;

                currentRecord.ModifiedOn = DateTime.Now;
                currentRecord.ModifiedBy = "CurrentUser";

                try
                {
                    db.PatchingConfig.Update(currentRecord);
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    return Json(new { Failure = true, Message = ex.Message });
                }
                return Json(new { Success = true });

            }
            else
            {
                //Patching newRecord = new Patching();
                PatchingConfig newRecord = new PatchingConfig();
                newRecord.PatchingSourceId = fData.PatchingSourceID;
                newRecord.ServerId = fData.ObjectID;
                newRecord.StartTime = Convert.ToDateTime(fData.StartTime);
                newRecord.EndTime = Convert.ToDateTime(fData.EndTime);
                newRecord.RebootBeforePatch = fData.RebootBeforePatch;
                newRecord.RebootAfterPatch = fData.RebootAfterPatch;
                //newRecord.ForceRebootAfterPatch = fData.ForceRebootAfterPatch;
                //newRecord.TempDisablePatch = fData.TempDisablePatch;
                newRecord.EnablePrePatchScript = fData.EnablePrePatchScript;
                newRecord.PrePatchScript = fData.PrePatchScript;
                newRecord.EnablePostPatchScript = fData.EnablePostPatchScript;
                newRecord.PostPatchScript = fData.PostPatchScript;
                newRecord.UpdateVmwareTools = fData.UpdateVmwareTools;
                newRecord.EnableSecondAttempt = fData.EnableSecondAttempt;


                newRecord.PatchingName = newSchedule.ScheduleName;
                newRecord.TelerikRecurrenceRule = newSchedule.RecurrenceRule;
                newRecord.CreatedOn = DateTime.Now;
                newRecord.ModifiedOn = DateTime.Now;
                newRecord.CreatedBy = "CurrentUser";
                newRecord.ModifiedBy = "CurrentUser";
                try
                {
                    db.PatchingConfig.Add(newRecord);
                    db.SaveChanges();
                    //JsonResult returnData = Json(new { Success = true, id = newRecord.Id}); //switching id to be hostname - since we use that as id in urls
                    JsonResult returnData = Json(new { Success = true, id = returnSvr.Hostname });
                    return returnData;
                }
                catch (Exception ex)
                {
                    return Json(new { Failure = true, Message = ex.Message + "<br>" + ex.InnerException });
                }
                
            }
        }


        public JsonResult GetAllServers()
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            var servers = db.ServerObject.ToList();
            //SelectListItem empty = new SelectListItem() { Text = "", Value = "" };

            List<SelectListItem> result = new List<SelectListItem>();
            //result.Add(empty);
            //result = ((List<ServerObject>)servers).Select(x => new SelectListItem { Text = x.Hostname, Value = x.Id.ToString() }).ToList();
            result.AddRange(servers.Select(x => new SelectListItem { Text = x.Hostname, Value = x.Id.ToString() }));
            return Json(result); //, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllUnscheduledServers()
        {
            PatchingAutomationContext db = new PatchingAutomationContext();
            var patchConfigs = db.PatchingConfig.ToList();
            var servers = db.ServerObject.ToList();
            //SelectListItem empty = new SelectListItem() { Text = "", Value = "" };

            List<SelectListItem> result = new List<SelectListItem>();
            //result.Add(empty);
            //result = ((List<ServerObject>)servers).Select(x => new SelectListItem { Text = x.Hostname, Value = x.Id.ToString() }).ToList();
            //result.AddRange(servers.Select(x => new SelectListItem { Text = x.Hostname, Value = x.Id.ToString() }));

            foreach (var s in servers)
            {
                if (patchConfigs.Where(x => x.ServerId == s.Id).Count() == 0)
                {
                    result.Add(new SelectListItem { Text = s.Hostname, Value = s.Id.ToString() });
                }
            }
            return Json(result); //, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]  //Delete
        public async Task<JsonResult> DeletePatchConfigByServerId(string id)
        {
            Scheduling sch = new Scheduling();
            bool historyDeleted = await sch.DeletePatchingHistoryByServerId(Convert.ToInt32(id));
            if (!historyDeleted) //something went wrong
            {
                return Json(new { failure = true, message = "Something went wrong when deleting patching history." });
            }

            bool configDeleted = await sch.DeletePatchingConfigurationByServerId(Convert.ToInt32(id));
            if (!configDeleted) //something went wrong
            {
                return Json(new { failure = true, message = "Something went wrong when deleting patching config." });
            }

            return Json(new { result = true });

        }

        [HttpPost]  //Delete
        public async Task<JsonResult> DeletePatchConfigAndServerByServerId(string id)
        {
            Scheduling sch = new Scheduling();
            bool historyDeleted = await sch.DeletePatchingHistoryByServerId(Convert.ToInt32(id));
            if (!historyDeleted) //something went wrong
            {
                return Json(new { failure = true, message = "Something went wrong when deleting patching history." });
            }

            bool configDeleted = await sch.DeletePatchingConfigurationByServerId(Convert.ToInt32(id));
            if (!configDeleted) //something went wrong
            {
                return Json(new { failure = true, message = "Something went wrong when deleting patching config." });
            }

            bool serverDeleted = await sch.DeleteServerObjectByServerId(Convert.ToInt32(id));
            if (!serverDeleted) //something went wrong
            {
                return Json(new { failure = true, message = "Something went wrong when deleting server object." });
            }

            return Json(new { result = true });

        }

        [HttpPost]  //Delete
        public async Task<JsonResult> DeleteServerByServerId(string id)
        {
            Scheduling sch = new Scheduling();

            bool serverDeleted = await sch.DeleteServerObjectByServerId(Convert.ToInt32(id));
            if (!serverDeleted) //something went wrong
            {
                return Json(new { failure = true, message = "Something went wrong when deleting server object." });
            }

            return Json(new { result = true });

        }



    }
}
