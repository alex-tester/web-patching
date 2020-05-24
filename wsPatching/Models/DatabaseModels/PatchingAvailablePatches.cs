using System;
using System.Collections.Generic;

namespace wsPatching.Models.DatabaseModels
{
    public partial class PatchingAvailablePatches
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int ServerId { get; set; }
        public int PatchingExecutionId { get; set; }
        public string KbNumber { get; set; }
        public string Title { get; set; }

        public virtual PatchingExecution PatchingExecution { get; set; }
        public virtual ServerObject Server { get; set; }
    }
}
