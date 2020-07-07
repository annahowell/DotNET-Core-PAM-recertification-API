using System;
using System.Collections.Generic;

namespace PAMrecert.Models
{
    public partial class ServiceTable
    {
        public ServiceTable()
        {
            PrivTable = new HashSet<PrivTable>();
        }

        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceOwner_RoleId { get; set; }

        public virtual RoleTable ServiceOwner_Role { get; set; }
        public virtual ICollection<PrivTable> PrivTable { get; set; }
    }
}
