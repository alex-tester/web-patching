using System;
using System.Collections.Generic;

namespace wsPatching.Models.DatabaseModels
{
    public partial class ServerObject
    {
        public ServerObject()
        {
            PatchingAvailablePatches = new HashSet<PatchingAvailablePatches>();
            PatchingConfig = new HashSet<PatchingConfig>();
            PatchingResults = new HashSet<PatchingResults>();
        }

        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string Hostname { get; set; }

        public virtual ICollection<PatchingAvailablePatches> PatchingAvailablePatches { get; set; }
        public virtual ICollection<PatchingConfig> PatchingConfig { get; set; }
        public virtual ICollection<PatchingResults> PatchingResults { get; set; }
    }
}
