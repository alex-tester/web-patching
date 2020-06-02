using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wsPatching.Models
{
    public class PatchingSettings
    {
        public bool UseLocalPatchScript { get; set; }
        public string LocalPatchingPath { get; set; }
        public string PatchingNetworkShare { get; set; }
    }
}
