using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using Telerik.Web.UI;
using wsPatching.Controllers;
using wsPatching.Models.DatabaseModels;

namespace wsPatching.Models.CustomModels
{
    public class Scheduling
    {
        //i think this is broken/deprecated
        public List<PatchingSchedule> GetPatchingSchedule(int id)
        {
            //stupid i know, need to switch it to findasync
            PatchingAutomationContext db = new PatchingAutomationContext();
            var patchingSchedules = (from s in db.PatchingConfig
                                     where s.Id == id
                                     select s).ToList();


            List<PatchingSchedule> result = new List<PatchingSchedule>();
            foreach (var p in patchingSchedules)
            {
                PatchingSchedule ps = new PatchingSchedule();
                ps.ScheduleName = p.PatchingName;
                ps.StartTime = p.StartTime;
                ps.EndTime = p.EndTime;

                RecurrenceRule rr;
                RecurrenceRule.TryParse(p.TelerikRecurrenceRule.ToString(), out rr);



                if (rr != null)
                {
                    //added this line to ensure we get future occurrences
                    rr.Range.Start = Convert.ToDateTime(DateTime.Now.AddDays(-31).ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                    ps.Occurrences = rr.Occurrences.Where(o => o > DateTime.Now.AddDays(-31)).OrderBy(ob => ob.Date).Take(100).ToList();
                }

                result.Add(ps);
            }
            return result;
        }

        public ServerObject GetServerByHostName(string hostName)
        {
            ServerObject svr = new ServerObject();
            if (string.IsNullOrEmpty(hostName))
            {
                //svr.Id = 0;
                svr.Hostname = "na";
            }
            else
            {
                PatchingAutomationContext db = new PatchingAutomationContext();
                try
                {
                    svr = db.ServerObject.Where(x => x.Hostname.ToLower() == hostName.ToLower()).FirstOrDefault();
                }
                catch
                {
                    return new ServerObject() { Hostname = "na", Id = 0 };
                }
            }

            return svr;
            
        }

        public PatchingConfig GetPatchingConfig(int id)
        {
            if (id == 0)
            {
                return new PatchingConfig() { Id = 0 };
            }
            else
            {
                PatchingAutomationContext db = new PatchingAutomationContext();
                try
                {
                    //var patchingSchedule = (from s in db.PatchingConfig
                    //                         where s.ServerId == id
                    //                         select s).First();
                    var patchingSchedule = db.PatchingConfig.Find(id);
                    var patchingSource = db.PatchingSource.Where(x => x.Id == patchingSchedule.PatchingSourceId).First();
                    patchingSchedule.PatchingSource = patchingSource;
                    return patchingSchedule;

                }
                catch
                {
                    return new PatchingConfig() { Id = 0 };
                }
            }
        }

        public PatchingConfig GetPatchingConfigByServerId(int id)
        {
            if (id == 0)
            {
                return new PatchingConfig() { Id = 0 };
            }
            else
            {
                PatchingAutomationContext db = new PatchingAutomationContext();
                try
                {
                    var patchingSchedule = (from s in db.PatchingConfig
                                            where s.ServerId == id
                                            select s).First();
                    var patchingSource = db.PatchingSource.Where(x => x.Id == patchingSchedule.PatchingSourceId).First();
                    patchingSchedule.PatchingSource = patchingSource;
                    patchingSchedule.PatchingSource.PatchingConfig = null;
                    return patchingSchedule;

                }
                catch
                {
                    return new PatchingConfig() { Id = 0 };
                }
            }
        }

        public ServerObject GetServerObjectById(int id)
        {
            if (id == 0)
            {
                return new ServerObject() { Id = 0 };
            }
            else
            {
                PatchingAutomationContext db = new PatchingAutomationContext();
                try
                {
                    var svr = (from s in db.ServerObject
                                            where s.Id == id
                                            select s).First();
                    //var patchingSource = db.PatchingSource.Where(x => x.Id == patchingSchedule.PatchingSourceId).First();
                    //patchingSchedule.PatchingSource = patchingSource;
                    return svr;

                }
                catch
                {
                    return new ServerObject() { Id = 0 };
                }
            }
        }

        public List<PatchingExecution> GetPatchingExecutionHistoryByServerId(int id)
        {
            List<PatchingExecution> result = new List<PatchingExecution>();
            if (id == 0)
            {
                return result;
            }
            else
            {
                PatchingAutomationContext db = new PatchingAutomationContext();
                try
                {
                    //var svr = (from s in db.PatchingExecution
                    //           where s.Id == id
                    //           select s).First();
                    //var patchingSource = db.PatchingSource.Where(x => x.Id == patchingSchedule.PatchingSourceId).First();
                    //patchingSchedule.PatchingSource = patchingSource;
                    result = db.PatchingExecution.Where(x => x.ServerId == id).ToList();
                    foreach (var h in result)
                    {
                        h.PatchingResults = db.PatchingResults.Where(x => x.PatchingExecutionId == h.Id).ToList();
                        foreach (var n in h.PatchingResults)
                        {
                            n.PatchingExecution = null;
                            n.Server = null;
                        }
                        h.PatchingAvailablePatches = db.PatchingAvailablePatches.Where(x => x.PatchingExecutionId == h.Id).ToList();
                        foreach (var n in h.PatchingAvailablePatches)
                        {
                            n.PatchingExecution = null;
                            n.Server = null;
                        }
                    }
                    return result;

                }
                catch
                {
                    return result;
                }
            }
        }

        public async Task<ServerObject> GetServerObjectIdByPatchingConfig(int id)
        {
            ServerObject svr = new ServerObject();
            if (id == 0)
            {
                svr.Hostname = "NOT FOUND";
                return svr;
            }
            else
            {
                PatchingAutomationContext db = new PatchingAutomationContext();
                svr = await db.ServerObject.FindAsync(id);
                return svr;
            }
        }

        public List<PatchingScheduleDisplay> GetPatchScheduleDisplayByPatchConfigId(int id)
        {

            PatchingAutomationContext db = new PatchingAutomationContext();
            var patchingSchedules = (from s in db.PatchingConfig
                                         where s.Id == id
                                     select s).ToList();


            List<PatchingScheduleDisplay> result = new List<PatchingScheduleDisplay>();
            foreach (var p in patchingSchedules)
            {
                PatchingScheduleDisplay ps = new PatchingScheduleDisplay();
                ps.ScheduleName = p.PatchingName;
                ps.StartTime = p.StartTime;
                ps.EndTime = p.EndTime;

                RecurrenceRule rr;
                RecurrenceRule.TryParse(p.TelerikRecurrenceRule.ToString(), out rr);

                var svr = (from s in db.ServerObject
                           where p.ServerId == s.Id
                           select s).First();

                ps.Hostname = svr.Hostname;
                ps.ServerId = svr.Id;

                if (rr != null)
                {
                    //added this line to ensure we get future occurrences - i think i need to add days-31 to rr.range.start
                    rr.Range.Start = Convert.ToDateTime(DateTime.Today.ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                    ps.Occurrences = rr.Occurrences.Where(o => o > DateTime.Now.AddDays(-31)).OrderBy(ob => ob.Date).Take(20).ToList();
                }

                result.Add(ps);
            }
            return result;
        }

        public List<PatchingScheduleDisplay> GetAllPatchSchedules()
        {
          
            PatchingAutomationContext db = new PatchingAutomationContext();
            var patchingSchedules = (from s in db.PatchingConfig
                                         //where s.ServerId == id
                                     select s).ToList();


            List<PatchingScheduleDisplay> result = new List<PatchingScheduleDisplay>();
            foreach (var p in patchingSchedules)
            {
                PatchingScheduleDisplay ps = new PatchingScheduleDisplay();
                ps.ScheduleName = p.PatchingName;
                ps.StartTime = p.StartTime;
                ps.EndTime = p.EndTime;

                RecurrenceRule rr;
                RecurrenceRule.TryParse(p.TelerikRecurrenceRule.ToString(), out rr);

                var svr = (from s in db.ServerObject
                           where p.ServerId == s.Id
                           select s).First();

                ps.Hostname = svr.Hostname;
                ps.ServerId = svr.Id;

                if (rr != null)
                {
                    //added this line to ensure we get future occurrences - i think i need to add days-31 to rr.range.start
                    rr.Range.Start = Convert.ToDateTime(DateTime.Now.AddDays(-31).ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                    ps.Occurrences = rr.Occurrences.Where(o => o > DateTime.Now.AddDays(-31)).OrderBy(ob => ob.Date).Take(20).ToList();
                }

                result.Add(ps);
            }
            return result;
        }


        public List<PatchingScheduleDisplay> GetServerPatchSchedules(int id)
        {

            PatchingAutomationContext db = new PatchingAutomationContext();
            var patchingSchedules = (from s in db.PatchingConfig
                                         where s.ServerId == id
                                     select s).ToList();


            List<PatchingScheduleDisplay> result = new List<PatchingScheduleDisplay>();
            foreach (var p in patchingSchedules)
            {
                PatchingScheduleDisplay ps = new PatchingScheduleDisplay();
                ps.ScheduleName = p.PatchingName;
                ps.StartTime = p.StartTime;
                ps.EndTime = p.EndTime;

                RecurrenceRule rr;
                RecurrenceRule.TryParse(p.TelerikRecurrenceRule.ToString(), out rr);

                var svr = (from s in db.ServerObject
                           where p.ServerId == s.Id
                           select s).First();

                ps.Hostname = svr.Hostname;
                ps.ServerId = svr.Id;

                if (rr != null)
                {
                    //added this line to ensure we get future occurrences - i think i need to add days-31 to rr.range.start
                    rr.Range.Start = Convert.ToDateTime(DateTime.Now.AddDays(-60).ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                    ps.Occurrences = rr.Occurrences.Where(o => o > DateTime.Now.AddDays(-60)).OrderBy(ob => ob.Date).Take(100).ToList();
                }

                result.Add(ps);
            }
            return result;
        }

        public ClientPatchSettings GetNextPatchDateByServerId(int id)
        {
            var svr = GetServerObjectById(id);
            var p = GetPatchingConfigByServerId(id);

            ClientPatchSettings result = new ClientPatchSettings();


            PatchingScheduleDisplay ps = new PatchingScheduleDisplay();

            if (p.Id == 0)
            {
                result.PatchingName = "Server not configured for patching";
                return result;
            }
            //List <PatchingScheduleDisplay> result = new List<PatchingScheduleDisplay>();
   
            //PatchingScheduleDisplay ps = new PatchingScheduleDisplay();
            //ps.ScheduleName = p.PatchingName;
            //ps.StartTime = p.StartTime;
            //ps.EndTime = p.EndTime;

            result.PatchingName = p.PatchingName;
            result.StartTime = p.StartTime;
            result.EndTime = p.EndTime;
            result.PatchingSource = p.PatchingSource;
            result.RebootAfterPatch = p.RebootAfterPatch;
            result.RebootAfterPatch = p.RebootAfterPatch;
            result.EnableSecondAttempt = p.EnableSecondAttempt;
            result.EnablePrePatchScript = p.EnablePrePatchScript;
            result.PrePatchScript = p.PrePatchScript;
            result.EnablePostPatchScript = p.EnablePostPatchScript;
            result.PostPatchScript = p.PostPatchScript;
            result.Id = p.Id;
            result.UpdateVmwareTools = p.UpdateVmwareTools;
            result.CreatedOn = p.CreatedOn;
            result.CreatedBy = p.CreatedBy;
            result.ModifiedOn = p.ModifiedOn;
            result.ModifiedBy = p.ModifiedBy;


            RecurrenceRule rr;
                RecurrenceRule.TryParse(p.TelerikRecurrenceRule.ToString(), out rr);

            //var svr = (from s in db.ServerObject
            //           where p.ServerId == s.Id
            //           select s).First();

            result.Hostname = svr.Hostname;
            result.ServerId = svr.Id;

                if (rr != null)
                {
                    //added this line to ensure we get future occurrences - i think i need to add days-31 to rr.range.start
                    rr.Range.Start = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                result.Occurrences = rr.Occurrences.Where(o => o > DateTime.Now.AddDays(-1)).OrderBy(ob => ob.Date).Take(1).ToList();
                }

            //I hope this doesnt just compensate for EST
            //for (int i = 0; i < ps.Occurrences.Count(); i++)
            //{
            //    ps.Occurrences[i] = ps.Occurrences[i].AddHours(-4);
            //}

            //result.Add(ps);
            result.NextPatchDate = result.Occurrences[0];
            return result;
        }


        public List<PatchingScheduleDisplay> GetUpcomingPatchSchedules()
        {

            PatchingAutomationContext db = new PatchingAutomationContext();
            var patchingSchedules = (from s in db.PatchingConfig
                                        
                                     select s).ToList();


            List<PatchingScheduleDisplay> result = new List<PatchingScheduleDisplay>();
            foreach (var p in patchingSchedules)
            {
                PatchingScheduleDisplay ps = new PatchingScheduleDisplay();
                ps.ScheduleName = p.PatchingName;
                ps.StartTime = p.StartTime;
                ps.EndTime = p.EndTime;

                RecurrenceRule rr;
                RecurrenceRule.TryParse(p.TelerikRecurrenceRule.ToString(), out rr);

                var svr = (from s in db.ServerObject
                           where p.ServerId == s.Id
                           select s).First();

                ps.Hostname = svr.Hostname;
                ps.ServerId = svr.Id;

                if (rr != null)
                {
                    //added this line to ensure we get future occurrences - i think i need to add days-31 to rr.range.start
                    rr.Range.Start = Convert.ToDateTime(DateTime.Now.AddDays(-31).ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                    ps.Occurrences = rr.Occurrences.Where(o => o >= DateTime.Now).OrderBy(ob => ob.Date).Take(100).ToList();
                }

                result.Add(ps);
            }


            return result.OrderBy(x => x.StartTime).Take(100).ToList();
        }

        public async Task<bool> DeleteServerObjectByServerId(int id)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            //delete Server Object
            try
            {
                db.ServerObject.Remove(await db.ServerObject.FindAsync(id));
                await db.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeletePatchingConfigurationByServerId(int id)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();
            
            //delete patch config 
            try
            {
                db.PatchingConfig.RemoveRange(db.PatchingConfig.Where(x => x.ServerId == id));
                await db.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeletePatchingHistoryByServerId(int id)
        {
            PatchingAutomationContext db = new PatchingAutomationContext();

            //delete available patch history first
            try
            {
                db.PatchingAvailablePatches.RemoveRange(db.PatchingAvailablePatches.Where(x => x.ServerId == id));
                await db.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            //delete installed patch history second
            try
            {
                db.PatchingResults.RemoveRange(db.PatchingResults.Where(x => x.ServerId == id));
                await db.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            //delete patch execution history third
            try
            {
                db.PatchingExecution.RemoveRange(db.PatchingExecution.Where(x => x.ServerId == id));
                await db.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }

       

        static string GenerateScheduleName(RecurrenceRule rule, bool AddReboot, bool Reboot, DateTime Time)
        {

            string ScheduleName = "";
            string DayToRun = "";
            string WeekDay = "";
            string WeekCount = "";


            if (rule.Pattern.DayOrdinal != null)
            {
                WeekCount = rule.Pattern.DayOrdinal.ToString();
            }

            if (rule.Pattern.DayOfMonth != null)
            {
                DayToRun = rule.Pattern.DayOfMonth.ToString();
            }

            if (rule.Pattern.DaysOfWeekMask != null)
            {
                WeekDay = rule.Pattern.DaysOfWeekMask.ToString();
            }



            if (DayToRun != "0")
            {
                if (AddReboot)
                {
                    ScheduleName = string.Format("{0} - {1} Day {2} {3}", rule.Pattern.Frequency, ConvertDayToPlace(DayToRun), Time.ToShortTimeString(), ConvertRebootOption(Reboot));
                }
                else
                {
                    ScheduleName = string.Format("{0} - {1} Day {2}", rule.Pattern.Frequency, ConvertDayToPlace(DayToRun), Time.ToShortTimeString());
                }

            }
            else if (rule.Pattern.Frequency == RecurrenceFrequency.Hourly)
            {
                ScheduleName = string.Format("{0} - Every {2} Hour(s) from {1}", rule.Pattern.Frequency, Time.ToShortTimeString(), rule.Pattern.Interval.ToString());
            }
            else
            {
                if (AddReboot)
                {
                    ScheduleName = string.Format("{0} - {1} {2} {3} {4} ", rule.Pattern.Frequency, ConvertWeeksCount(WeekCount), WeekDay, Time.ToShortTimeString(), ConvertRebootOption(Reboot));
                }
                else
                {
                    ScheduleName = string.Format("{0} - {1} {2} {3}", rule.Pattern.Frequency, ConvertWeeksCount(WeekCount), WeekDay, Time.ToShortTimeString());
                }
            }

            return ScheduleName;
        }
        static string ConvertWeeksCount(string w)
        {
            string returnval = string.Empty;
            switch (w)
            {
                case "1":
                    returnval = "1st";
                    break;
                case "2":
                    returnval = "2nd";
                    break;
                case "3":
                    returnval = "3rd";
                    break;
                case "4":
                    returnval = "4th";
                    break;
                case "5":
                    returnval = "Last";
                    break;
                default:
                    break;
            }
            return returnval;
        }
        static string ConvertRebootOption(bool r)
        {
            string returnval = string.Empty;
            if (r)
            {
                returnval = ""; //"Auto Reboot";
            }
            else
            {
                returnval = ""; //"Owner Reboot";
            }


            return returnval;
        }
        static string ConvertDayToPlace(string d)
        {
            string returnval = string.Empty;
            string tempval = string.Empty;
            string ending = string.Empty;
            char[] splitter = { ',' };
            string[] Dates = d.Replace(" ", "").Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            string[] OddDates = { "11", "12", "13" };

            foreach (string Date in Dates)
            {
                if (OddDates.Contains(Date))
                {
                    ending = "th";
                }
                else
                {

                    switch (Date.Substring(Date.Length - 1, 1))
                    {
                        case "1":
                            ending = "st";
                            break;
                        case "2":
                            ending = "nd";
                            break;
                        case "3":
                            ending = "rd";
                            break;
                        default:
                            ending = "th";
                            break;
                    }
                }
                tempval = Date + ending;


                if (returnval.Length == 0)
                {
                    returnval = tempval;
                }
                else
                {
                    returnval = returnval + ", " + tempval;
                }
            }

            return returnval;
        }

        RecurrenceRule GenerateRecurrenceRule(DateTime Start, string Frequency, string Interval, string WeekDay, string RunOnDay, string WeekNumber, string Month)
        {
            RecurrenceRange newr = new RecurrenceRange();
            newr.Start = Start;
            newr.RecursUntil = DateTime.MaxValue;
            newr.MaxOccurrences = 2147483647;

            RecurrenceRule rr = new MonthlyRecurrenceRule(1, 1, newr);
            #region Create Recurrence Rule
            switch (Frequency.ToLower())
            {
                case "hourly":
                    rr = new HourlyRecurrenceRule(Convert.ToInt32(Interval), newr);
                    break;

                case "daily":

                    rr = new DailyRecurrenceRule(Convert.ToInt32(Interval), newr);

                    break;

                case "weekly":

                    RecurrenceDay day;
                    Enum.TryParse<RecurrenceDay>(WeekDay, out day);

                    rr = new WeeklyRecurrenceRule(Convert.ToInt32(Interval), day, newr);


                    break;
                case "monthly":

                    switch (WeekNumber.ToLower())
                    {
                        case "day":
                            rr = new MonthlyRecurrenceRule(Convert.ToInt32(RunOnDay), Convert.ToInt32(Interval), newr);
                            break;
                        default:

                            RecurrenceDay mday;
                            Enum.TryParse<RecurrenceDay>(WeekDay, out mday);
                            rr = new MonthlyRecurrenceRule(Convert.ToInt32(WeekNumber), mday, Convert.ToInt32(Interval), newr);
                            break;
                    }



                    break;

                case "yearly":

                    RecurrenceMonth ymonth;
                    Enum.TryParse<RecurrenceMonth>(Month, out ymonth);
                    rr = new YearlyRecurrenceRule(ymonth, Convert.ToInt32(RunOnDay), newr);

                    break;
                default:
                    break;
            }

            #endregion

            return rr;

        }

        
        public RecurrenceOutput PostGenerateSchedule(GenerateNewSchedule N)
        //public RecurrenceOutput GenerateSchedule(DateTime Start, string Frequency, string Interval, string WeekDay, string RunOnDay, string WeekNumber, string Month, bool UseReboot, bool AutoReboot)
        {
            DateTime Start = N.Start;
            string Frequency = N.Frequency;
            string Interval = N.Interval;
            string WeekDay = N.WeekDay;
            string RunOnDay = N.RunOnDay;
            string WeekNumber = N.WeekNumber;
            String Month = N.Month;
            bool UseReboot = N.UseReboot;
            bool AutoReboot = N.AutoReboot;


            RecurrenceOutput result = new RecurrenceOutput();

            RecurrenceRule rr = GenerateRecurrenceRule(Start, Frequency, Interval, WeekDay, RunOnDay, WeekNumber, Month);


            result.ScheduleName = GenerateScheduleName(rr, UseReboot, AutoReboot, Start);
            result.RecurrenceRule = rr.ToString();

            var futureOccurances = rr.Occurrences.Where(o => o > DateTime.Now).OrderBy(ob => ob.Date).ToList();
            if (futureOccurances.Count > 0)
            {
                result.NextScheduledDate = futureOccurances.First();
            }

            return result;

        }

        public NextScheduleDate GetNextScheduleDate(string RecRule)
        {
            RecurrenceRule rr;
            RecurrenceRule.TryParse(RecRule.ToString(), out rr);
            NextScheduleDate result = new NextScheduleDate();

            if (rr != null)
            {
                rr.Range.Start = Convert.ToDateTime(DateTime.Today.ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                var futureOccurances = rr.Occurrences.Where(o => o > DateTime.Now).OrderBy(ob => ob.Date).ToList();
                if (futureOccurances.Count > 0)
                {
                    result.Success = true;
                    result.NextScheduledDate = futureOccurances.First();
                }
            }

            return result;
        }



        public NextScheduleDate GetNextScheduleDateAfterSpecificDate(string RecRule, DateTime SpecificDate)
        {
            RecurrenceRule rr;
            RecurrenceRule.TryParse(RecRule.ToString(), out rr);
            NextScheduleDate result = new NextScheduleDate();

            if (rr != null)
            {
                rr.Range.Start = Convert.ToDateTime(DateTime.Today.ToShortDateString() + " " + rr.Range.Start.ToShortTimeString());

                var futureOccurances = rr.Occurrences.Where(o => o > SpecificDate).OrderBy(ob => ob.Date).ToList();
                if (futureOccurances.Count > 0)
                {
                    result.Success = true;
                    result.NextScheduledDate = futureOccurances.First();
                }
            }

            return result;
        }

        public RecurrenceRule GenerateRecurrenceRuleObject(string RecRule)
        {
            RecurrenceRule rr;
            RecurrenceRule.TryParse(RecRule.ToString(), out rr);
            return rr;
        }



        public ScheduleParts GetScheduleParts(string RecRule)
        {
            ScheduleParts result = new ScheduleParts();

            RecurrenceRule rr = GenerateRecurrenceRuleObject(RecRule);



            result.Frequency = rr.Pattern.Frequency.ToString();
            result.Interval = rr.Pattern.Interval.ToString();

            if (rr.Pattern.DaysOfWeekMask != null)
            {
                result.WeekDay = rr.Pattern.DaysOfWeekMask.ToString();
            }

            if (rr.Pattern.DayOfMonth != null)
            {
                result.RunOnDay = rr.Pattern.DayOfMonth.ToString();
            }

            if (rr.Pattern.DayOrdinal != null)
            {
                result.WeekNumber = rr.Pattern.DayOrdinal.ToString();
            }

            if (rr.Pattern.Month != null)
            {
                result.Month = rr.Pattern.Month.ToString();
            }


            result.Start = rr.Range.Start;


            return result;
        }


        public bool Test()
        {
            string rule = @"DTSTART:20180326T110000Z
DTEND:20000102T000000Z
RRULE:FREQ=HOURLY;INTERVAL=2
";


            //GetScheduleParts(rule);
            //GenerateSchedule(Convert.ToDateTime("4/26/18 6:30 PM"), "hourly", "1", "", null, "1", "", false, false);
            //var x = GenerateSchedule(Convert.ToDateTime("4/26/18 6:30 PM"), "hourly", "2", "", "", "", "", false, false);

            var x = GetNextScheduleDateAfterSpecificDate(rule, DateTime.Now.AddMinutes(15));


            return true;
        }

    }


    public class RecurrenceOutput
    {
        public string ScheduleName { get; set; }
        public string RecurrenceRule { get; set; }
        public DateTime NextScheduledDate { get; set; }
    }

    public class NextScheduleDate
    {
        public bool Success { get; set; }
        public DateTime NextScheduledDate { get; set; }
    }

    public class ScheduleParts
    {
        public DateTime Start { get; set; }
        public string Frequency { get; set; }
        public string Interval { get; set; }
        public string WeekDay { get; set; }
        public string RunOnDay { get; set; }
        public string WeekNumber { get; set; }
        public string Month { get; set; }

    }

    public class PatchingSchedule
    {
        public string ScheduleName { get; set; }
        public List<DateTime> Occurrences { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

    }

    public class PatchingScheduleDisplay : PatchingSchedule
    {
        public int ServerId { get; set; }
        public string Hostname { get; set; }

    }

    public class ClientPatchSettings : PatchingConfig
    {
        public int ServerId { get; set; }
        public string Hostname { get; set; }
        public DateTime NextPatchDate { get; set; }
        public List<DateTime> Occurrences { get; set; }
    }

    public class GenerateNewSchedule
    {
        public DateTime Start { get; set; }
        public string Frequency { get; set; }
        public string Interval { get; set; }
        public string WeekDay { get; set; }
        public string RunOnDay { get; set; }
        public string WeekNumber { get; set; }
        public string Month { get; set; }
        public bool UseReboot { get; set; }
        public bool AutoReboot { get; set; }

    }

    public class PatchingScheduleEdit
    {
        public int PatchingID { get; set; }
        public int PatchingSourceID { get; set; }
        public int ObjectID { get; set; }
        public DateTime Start { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Frequency { get; set; }
        public string Interval { get; set; }
        public string WeekDay { get; set; }
        public string RunOnDay { get; set; }
        public string WeekNumber { get; set; }
        public string Month { get; set; }
        //Repeat Every X Hours
        public string HourlyRecurrence { get; set; }
        //Repeat every X days
        public string DailyRecurrence { get; set; }
        //public bool RecurEveryWeekday { get; set; }
        //Repeat every X weeks
        public string WeeklyRecurrence { get; set; }
        //public string DaysOfWeek { get; set; }
        //public string WeekOfMonth { get; set; }
        public bool RebootBeforePatch { get; set; }
        public bool RebootAfterPatch { get; set; }

        public bool ForceRebootAfterPatch { get; set; }
        public bool TempDisablePatch { get; set; }
        public bool EnablePrePatchScript { get; set; }
        public string PrePatchScript { get; set; }
        public bool EnablePostPatchScript { get; set; }
        public string PostPatchScript { get; set; }
        public bool UpdateVmwareTools { get; set; }
        public bool EnableSecondAttempt { get; set; }
        public string HostName { get; set; }
    }
}
