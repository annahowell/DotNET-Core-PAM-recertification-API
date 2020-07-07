using System;
using System.Collections.Generic;

namespace PAMrecert.Models
{
    public partial class UserTable
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string RoleId { get; set; }
        public string LastCertifiedBy { get; set; }
        public DateTime? LastCertifiedDate { get; set; }

        public virtual RoleTable Role { get; set; }
    }
}
