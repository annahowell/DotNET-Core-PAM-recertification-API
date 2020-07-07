using System.ComponentModel.DataAnnotations;

namespace PAMrecert.DTOs.RoleController
{
    public class RoleDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string RoleOwner_RoleId { get; set; }
        public bool FullyCertified { get; set; }
    }
}