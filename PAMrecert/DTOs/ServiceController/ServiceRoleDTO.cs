using System.Collections.Generic;

namespace PAMrecert.DTOs.ServiceController
{
    public class ServiceRoleDTO
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceOwner_RoleId { get; set; }

        public IEnumerable<ServiceAvailablePrivsDTO> ServicesAvailablePrivs { get; set; }

        public IEnumerable<RolePrivsDTO> RolePrivs { get; set; }
    }
}
