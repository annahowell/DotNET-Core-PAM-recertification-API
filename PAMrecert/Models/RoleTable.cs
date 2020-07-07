using System;
using System.Collections.Generic;

namespace PAMrecert.Models
{
    public partial class RoleTable
    {
        public RoleTable()
        {
            RolePrivLink = new HashSet<RolePrivLink>();
            ServiceTable = new HashSet<ServiceTable>();
            UserTable = new HashSet<UserTable>();
        }

        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string RoleOwner_RoleId { get; set; }

        public virtual ICollection<RolePrivLink> RolePrivLink { get; set; }
        public virtual ICollection<ServiceTable> ServiceTable { get; set; }
        public virtual ICollection<UserTable> UserTable { get; set; }
    }
}
