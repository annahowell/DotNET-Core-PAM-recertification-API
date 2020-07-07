using System.Collections.Generic;

namespace PAMrecert.DTOs.RoleController
{
    public class RoleServiceDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string RoleOwner_RoleId { get; set; }

        public IEnumerable<ServicePrivsDTO> ServicePrivs { get; set; }
    }
}
