namespace PAMrecert.DTOs.ServiceController
{
    public class RolePrivsDTO
    {
        // Unique identifier
        public int RolePrivId { get; set; }

        // Service properties
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }

        // Service Priv Link properties
        public string PermissionGroup { get; set; }
        public string ServicePrivSummary { get; set; }
        public string CredentialStorageMethod { get; set; }

        public ServicePrivDTO PreviousPriv { get; set; }

        // Cerification
        public string ServiceOwner_PrivId { get; set; }
        public string ServiceOwner_RoleAccessJustification { get; set; }
        public string ServiceOwner_RemovalImpact { get; set; }
        public bool ServiceOwner_IsRevoked { get; set; }
        public bool? ServiceOwner_IsCertified { get; set; }
    }
}
