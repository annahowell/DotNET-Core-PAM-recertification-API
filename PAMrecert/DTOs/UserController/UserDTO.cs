using System;

namespace PAMrecert.DTOs.UserController
{
    public class UserDTO
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string RoleId { get; set; }
        public string LastCertifiedBy { get; set; }
        public DateTime? LastCertifiedDate { get; set; }
    }
}