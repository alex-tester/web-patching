using System;
using System.Collections.Generic;

namespace wsPatching.Models.DatabaseModels
{
    public partial class PatchingExecution
    {
        public PatchingExecution()
        {
            PatchingAvailablePatches = new HashSet<PatchingAvailablePatches>();
            PatchingResults = new HashSet<PatchingResults>();
        }

        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int ServerId { get; set; }

        public virtual ICollection<PatchingAvailablePatches> PatchingAvailablePatches { get; set; }
        public virtual ICollection<PatchingResults> PatchingResults { get; set; }
    }
}
