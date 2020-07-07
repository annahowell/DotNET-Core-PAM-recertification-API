using System;

namespace PAMrecert.DTOs.RoleController
{
    public class RiskAssessmentDTO
    {
        public int RolePrivId { get; set; }

        // Manager stuff
        public string RoleOwner_PrivId { get; set; }
        public string RoleOwner_PermissionGroup { get; set; }
        public string RoleOwner_ServicePrivSummary { get; set; }
        public string RoleOwner_CredentialStorageMethod { get; set; }

        public string RoleOwner_ServiceId { get; set; }
        public string RoleOwner_ServiceName { get; set; }
        public string RoleOwner_ServiceDescription { get; set; }

        public string RoleOwner_RoleAccessJustification { get; set; }
        public string RoleOwner_RemovalImpact { get; set; }
        public bool RoleOwner_IsRevoked { get; set; }
        public bool? RoleOwner_IsCertified { get; set; }
        public DateTime? RoleOwner_DateCertified { get; set; }

        // Service owner stuff
        public string ServiceOwner_PrivId { get; set; }
        public string ServiceOwner_PermissionGroup { get; set; }
        public string ServiceOwner_ServicePrivSummary { get; set; }
        public string ServiceOwner_CredentialStorageMethod { get; set; }

        public string ServiceOwner_ServiceId { get; set; }
        public string ServiceOwner_ServiceName { get; set; }
        public string ServiceOwner_ServiceDescription { get; set; }

        public string ServiceOwner_RoleAccessJustification { get; set; }
        public string ServiceOwner_RemovalImpact { get; set; }
        public bool ServiceOwner_IsRevoked { get; set; }
        public bool? ServiceOwner_IsCertified { get; set; }
        public DateTime? ServiceOwner_DateCertified { get; set; }

        // Risk stuff
        public int? RiskImpact { get; set; }
        public int? RiskLikelihood { get; set; }
        public int? RiskRating { get; set; } // calculated so not used for handling delta equality
        public string RiskNotes { get; set; }
        public DateTime? RiskAssessmentDate { get; set; }
        public bool RiskIsAssessed { get; set; }
    }
}
