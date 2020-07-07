using System;
using System.Collections.Generic;

namespace PAMrecert.Models
{
    public partial class PrivTable
    {
        public PrivTable()
        {
            RolePrivLink = new HashSet<RolePrivLink>();
        }

        public string PrivId { get; set; }
        public string ServiceId { get; set; }
        public string ServicePrivSummary { get; set; }
        public string PermissionGroup { get; set; }
        public string CredentialStorageMethod { get; set; }

        public virtual ServiceTable Service { get; set; }
        public virtual ICollection<RolePrivLink> RolePrivLink { get; set; }
    }
}
