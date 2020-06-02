//using System;
//using System.Collections.Generic;
using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
//using wsPatching.Models;
//using wsPatching.Models.CustomModels;
//using wsPatching.Models.DatabaseModels;

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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult _PatchingSchedule()
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            List<ServerObject> model = db.ServerObject.ToList(); 

            List<PatchingConfig> patchDetails = db.PatchingConfig.ToList();

            List<PatchingSource> patchSources = db.PatchingSource.ToList();
            ViewBag.patchSources = patchSources;
            foreach (var p in patchDetails)
            {
                p.PatchingSource = patchSources.Where(x => x.Id == p.PatchingSourceId).First();
                p.Server = model.Where(x => x.Id == p.ServerId).First();
            }



            ViewBag.patchDetails = patchDetails;

            List<SelectListItem> unscheduledResult = new List<SelectListItem>();


            foreach (var s in model)
            {
                if (patchDetails.Where(x => x.ServerId == s.Id).Count() == 0)
                {
                    unscheduledResult.Add(new SelectListItem { Text = s.Hostname, Value = s.Id.ToString() });
                }
            }

            ViewBag.unscheduledServers = unscheduledResult.OrderBy(x => x.Text).ToList();

            return PartialView(patchDetails);
        }

        public JsonResult GetPatchingCalendar(string id)
        {
            Scheduling sch = new Scheduling();
            //List<PatchingScheduleDisplay> ps = sch.GetAllPatchSchedules().Where(x => x.ServerId == Convert.ToInt32(id)).ToList();
            List<PatchingScheduleDisplay> ps = sch.GetServerPatchSchedules(Convert.ToInt32(id));

            var events = new List<CalendarEvent>();

            //stupid and lazy way do do this. totally doesnt scale.
            var colorsrting = "#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050";
            var colors = colorsrting.Split(',');
            int calendarId = 0;
            int scheduleId = 0; //create array of different colors and use for each schedule
            foreach (var patchSchedule in ps)
            {
                DateTime startTime = patchSchedule.StartTime ?? DateTime.Now;
                DateTime endTime = patchSchedule.EndTime ?? DateTime.Now;
                double patchingDuration = (endTime - startTime).TotalMinutes;
                if (patchSchedule.Occurrences != null)
                {
                    foreach (var occurrence in patchSchedule.Occurrences.Take(100))
                    {
                        calendarId++;

                        events.Add(new CalendarEvent()
                        {
                            id = calendarId,
                            title = patchSchedule.Hostname + " - " + patchSchedule.ScheduleName,
                            start = occurrence.ToString("o"),
                            end = occurrence.AddMinutes(patchingDuration).ToString("o"),
                            color = colors[scheduleId], //"#3762c3",
                            allDay = false
                        });
                    }
                }
                scheduleId++;
            }

            return Json(events.ToArray()); //, JsonRequestBehavior.AllowGet);
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

        //Scheduling sch = new Scheduling();
        //var ps = sch.GetPatchingSchedule(Convert.ToInt32(id));
        //var events = new List<CalendarEvent>();

        //int calendarId = 0;
        //foreach (var patchSchedule in ps)
        //{
        //    DateTime startTime = patchSchedule.StartTime ?? DateTime.Now;
        //    DateTime endTime = patchSchedule.EndTime ?? DateTime.Now;
        //    double patchingDuration = (endTime - startTime).TotalMinutes;
        //    if (patchSchedule.Occurrences != null)
        //    {
        //        foreach (var occurrence in patchSchedule.Occurrences)
        //        {
        //            calendarId++;

        //            events.Add(new CalendarEvent()
        //            {
        //                id = calendarId,
        //                title = patchSchedule.ScheduleName,
        //                start = occurrence.ToString("o"),
        //                end = occurrence.AddMinutes(patchingDuration).ToString("o"),
        //                color = "#3762c3",
        //                allDay = false
        //            });
        //        }
        //    }
        //}

        //return Json(events.ToArray()); //, JsonRequestBehavior.AllowGet);




        public JsonResult GetPatchingConfigs()
        {
            PatchingAutomationContext db = new PatchingAutomationContext();
            List<PatchingConfig> pConfigs = db.PatchingConfig.ToList();

            List<SelectListItem> output = new List<SelectListItem>();

            foreach (var p in pConfigs)
            {
                output.Add(new SelectListItem() { Value = p.Id.ToString(), Text = p.ServerId.ToString() });
            }

            return Json(output);

        }

        public JsonResult GetAllPatchingCalendar()
        {

            Scheduling sch = new Scheduling();
            List<PatchingScheduleDisplay> ps = sch.GetAllPatchSchedules();

            var events = new List<CalendarEvent>();

            //stupid and lazy way do do this. totally doesnt scale.
            var colorsrting = "#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050";
            var colors = colorsrting.Split(',');
            int calendarId = 0;
            int scheduleId = 0; //create array of different colors and use for each schedule
            foreach (var patchSchedule in ps)
            {
                DateTime startTime = patchSchedule.StartTime ?? DateTime.Now;
                DateTime endTime = patchSchedule.EndTime ?? DateTime.Now;
                double patchingDuration = (endTime - startTime).TotalMinutes;
                if (patchSchedule.Occurrences != null)
                {
                    foreach (var occurrence in patchSchedule.Occurrences.Take(4))
                    {
                        calendarId++;

                        events.Add(new CalendarEvent()
                        {
                            id = calendarId,
                            title = patchSchedule.Hostname + " - " + patchSchedule.ScheduleName,
                            start = occurrence.ToString("o"),
                            end = occurrence.AddMinutes(patchingDuration).ToString("o"),
                            color = colors[scheduleId], //"#3762c3",
                            allDay = false
                        });
                    }
                }
                scheduleId++;
            }

            return Json(events.ToArray()); //, JsonRequestBehavior.AllowGet);
        }

        public IActionResult _NextPatchingInstances()
        {

            Scheduling sch = new Scheduling();
            List<PatchingScheduleDisplay> ps = sch.GetUpcomingPatchSchedules();

            var events = new List<CalendarEvent>();

            //stupid and lazy way do do this. totally doesnt scale.
            var colorsrting = "#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050";
            var colors = colorsrting.Split(',');
            int calendarId = 0;
            int scheduleId = 0; //create array of different colors and use for each schedule
            foreach (var patchSchedule in ps)
            {
                DateTime startTime = patchSchedule.StartTime ?? DateTime.Now;
                DateTime endTime = patchSchedule.EndTime ?? DateTime.Now;
                double patchingDuration = (endTime - startTime).TotalMinutes;
                if (patchSchedule.Occurrences != null)
                {
                    foreach (var occurrence in patchSchedule.Occurrences.Take(4))
                    {
                        calendarId++;

                        events.Add(new CalendarEvent()
                        {
                            id = calendarId,
                            title = patchSchedule.Hostname + " - " + patchSchedule.ScheduleName,
                            start = occurrence.ToString("o"),
                            end = occurrence.AddMinutes(patchingDuration).ToString("o"),
                            color = colors[scheduleId], //"#3762c3",
                            allDay = false
                        });
                    }
                }
                scheduleId++;
            }

            return PartialView(events); //Json(events.ToArray()); //, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllPatchingScheduleGrid()
        {

            Scheduling sch = new Scheduling();
            List<PatchingScheduleDisplay> ps = sch.GetAllPatchSchedules();

            var events = new List<CalendarEvent>();

            //stupid and lazy way do do this. totally doesnt scale.
            var colorsrting = "#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050,#80bfff,#70dbdb,#4dffd2,#c44dff,#ff66b3,#ff9966,#ffff80,#ff5050";
            var colors = colorsrting.Split(',');
            int calendarId = 0;
            int scheduleId = 0; //create array of different colors and use for each schedule
            foreach (var patchSchedule in ps)
            {
                DateTime startTime = patchSchedule.StartTime ?? DateTime.Now;
                DateTime endTime = patchSchedule.EndTime ?? DateTime.Now;
                double patchingDuration = (endTime - startTime).TotalMinutes;
                if (patchSchedule.Occurrences != null)
                {
                    foreach (var occurrence in patchSchedule.Occurrences.Take(4))
                    {
                        calendarId++;

                        events.Add(new CalendarEvent()
                        {
                            id = calendarId,
                            title = patchSchedule.Hostname + " - " + patchSchedule.ScheduleName,
                            start = occurrence.ToString("o"),
                            end = occurrence.AddMinutes(patchingDuration).ToString("o"),
                            color = colors[scheduleId], //"#3762c3",
                            allDay = false
                        });
                    }
                }
                scheduleId++;
            }

            return Json(events.ToArray()); //, JsonRequestBehavior.AllowGet);
        }
    }
}

