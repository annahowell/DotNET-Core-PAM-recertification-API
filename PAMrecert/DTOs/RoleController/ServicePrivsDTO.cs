using System.Collections.Generic;

namespace PAMrecert.DTOs.RoleController
{
    public class ServicePrivsDTO
    {
        // Unique identifier
        public int RolePrivId { get; set; }

        // Service properties
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }

        // Service Priv Link properties
        public string PermissionGroup { get; set; }
        public string ServicePrivSummary { get; set; }
        public string CredentialStorageMethod { get; set; }

        public PreviousPrivDTO PreviousPriv { get; set; }

        // Cerification
        public string RoleOwner_PrivId { get; set; }

        public string RoleOwner_RoleAccessJustification { get; set; }
        public string RoleOwner_RemovalImpact { get; set; }
        public bool RoleOwner_IsRevoked { get; set; }
        public bool? RoleOwner_IsCertified { get; set; }

        public IEnumerable<ServicesAvailablePrivsDTO> ServicesAvailablePrivs { get; set; }

    }
}
