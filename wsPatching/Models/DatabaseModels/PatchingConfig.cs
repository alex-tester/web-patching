using System;
using System.Collections.Generic;

namespace wsPatching.Models.DatabaseModels
{
    public partial class PatchingConfig
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int ServerId { get; set; }
        public int PatchingSourceId { get; set; }
        public string TelerikRecurrenceRule { get; set; }
        public string PatchingName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool RebootBeforePatch { get; set; }
        public bool RebootAfterPatch { get; set; }
        public bool EnableSecondAttempt { get; set; }
        public bool UpdateVmwareTools { get; set; }
        public bool EnablePrePatchScript { get; set; }
        public string PrePatchScript { get; set; }
        public bool EnablePostPatchScript { get; set; }
        public string PostPatchScript { get; set; }

        public virtual PatchingSource PatchingSource { get; set; }
        public virtual ServerObject Server { get; set; }
    }
}
