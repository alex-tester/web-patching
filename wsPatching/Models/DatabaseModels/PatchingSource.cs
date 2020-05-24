using System;
using System.Collections.Generic;

namespace wsPatching.Models.DatabaseModels
{
    public partial class PatchingSource
    {
        public PatchingSource()
        {
            PatchingConfig = new HashSet<PatchingConfig>();
        }

        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string PatchingSource1 { get; set; }

        public virtual ICollection<PatchingConfig> PatchingConfig { get; set; }
    }
}
