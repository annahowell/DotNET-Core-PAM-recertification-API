using System;
using System.Collections.Generic;

namespace PAMrecert.Models
{
    public partial class RolePrivLink
    {
        public int RolePrivId { get; set; }
        public string RoleId { get; set; }
        public string RoleOwner_PrivId { get; set; }
        public string RoleOwner_RoleAccessJustification { get; set; }
        public string RoleOwner_RemovalImpact { get; set; }
        public bool RoleOwner_IsRevoked { get; set; }
        public bool RoleOwner_IsCertified { get; set; }
        public DateTime? RoleOwner_DateCertified { get; set; }
        public string ServiceOwner_PrivId { get; set; }
        public string ServiceOwner_RoleAccessJustification { get; set; }
        public string ServiceOwner_RemovalImpact { get; set; }
        public bool ServiceOwner_IsRevoked { get; set; }
        public bool ServiceOwner_IsCertified { get; set; }
        public DateTime? ServiceOwner_DateCertified { get; set; }
        public int? RiskImpact { get; set; }
        public int? RiskLikelihood { get; set; }
        public string RiskNotes { get; set; }
        public DateTime? RiskAssessmentDate { get; set; }
        public bool RiskIsAssessed { get; set; }

        public virtual PrivTable RoleOwner_Priv { get; set; }
        public virtual RoleTable Role { get; set; }
    }
}
