using System;
using System.Diagnostics.CodeAnalysis;

namespace PAMrecert.DTOs.RoleController
{
    public class RoleServicePrivAllDTO : IEquatable<RoleServicePrivAllDTO>
    {
        // Role stuff
        public int? RolePrivId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string RoleOwner_RoleId { get; set; }

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
        public bool? RoleOwner_IsRevoked { get; set; }
        public bool? RoleOwner_IsCertified { get; set; } // not used for handling delta equality
        public DateTime? RoleOwner_DateCertified { get; set; }

        // Service Owner Stuff
        public string ServiceOwner_PrivId { get; set; }
        public string ServiceOwner_PermissionGroup { get; set; }
        public string ServiceOwner_ServicePrivSummary { get; set; }
        public string ServiceOwner_CredentialStorageMethod { get; set; }
        public string ServiceOwner_ServiceId { get; set; }
        public string ServiceOwner_ServiceName { get; set; }
        public string ServiceOwner_ServiceDescription { get; set; }

        public string ServiceOwner_RoleAccessJustification { get; set; }
        public string ServiceOwner_RemovalImpact { get; set; }
        public bool? ServiceOwner_IsRevoked { get; set; }
        public bool? ServiceOwner_IsCertified { get; set; } // not used for handling delta equality
        public DateTime? ServiceOwner_DateCertified { get; set; }

        // Risk Stuff
        public int? RiskImpact { get; set; }
        public int? RiskLikelihood { get; set; }
        public int? RiskRating { get; set; } // calculated so not used for handling delta equality
        public string RiskNotes { get; set; }
        public DateTime? RiskAssessmentDate { get; set; }



        // The following methods support the delta reports

        // Handle equals for everything but the managers / service owners isCertified and the
        // risk rating. The isCertified stuff gets reset everytime a new cycle happens and
        // risk rating is calculated from threat and likelihood ratings so we can ignore those
        public bool Equals([AllowNull] RoleServicePrivAllDTO other)
        {
            // Check whether the compared object is null.
            if (other is null) return false;

            // Check whether the compared object references the same data.
            if (ReferenceEquals(this, other)) return true;

            // Check whether the objects’ properties are equal.
            // Role stuff
            return RolePrivId.Equals(other.RolePrivId) &&
                   RoleId.Equals(other.RoleId) &&
                   RoleName.Equals(other.RoleName) &&
                   RoleDescription.Equals(other.RoleDescription) &&
                   RoleOwner_RoleId.Equals(other.RoleOwner_RoleId) &&

                   // Manager stuff
                   RoleOwner_PrivId.Equals(other.RoleOwner_PrivId) &&
                   RoleOwner_PermissionGroup.Equals(other.RoleOwner_PermissionGroup) &&
                   RoleOwner_ServicePrivSummary.Equals(other.RoleOwner_ServicePrivSummary) &&
                   RoleOwner_CredentialStorageMethod.Equals(other.RoleOwner_CredentialStorageMethod) &&
                   RoleOwner_ServiceId.Equals(other.RoleOwner_ServiceId) &&
                   RoleOwner_ServiceName.Equals(other.RoleOwner_ServiceName) &&
                   RoleOwner_ServiceDescription.Equals(other.RoleOwner_ServiceDescription) &&

                   RoleOwner_RoleAccessJustification.Equals(other.RoleOwner_RoleAccessJustification) &&
                   RoleOwner_RemovalImpact.Equals(other.RoleOwner_RemovalImpact) &&
                   RoleOwner_IsRevoked.Equals(other.RoleOwner_IsRevoked) &&
                   RoleOwner_DateCertified.Equals(other.RoleOwner_DateCertified);/* &&

                   // Service Owner Stuff
                   ServiceOwner_PrivId.Equals(other.ServiceOwner_PrivId) &&
                   ServiceOwner_PermissionGroup.Equals(other.ServiceOwner_PermissionGroup) &&
                   ServiceOwner_ServicePrivSummary.Equals(other.ServiceOwner_ServicePrivSummary) &&
                   ServiceOwner_CredentialStorageMethod.Equals(other.ServiceOwner_CredentialStorageMethod) &&
                   ServiceOwner_ServiceId.Equals(other.ServiceOwner_ServiceId) &&
                   ServiceOwner_ServiceName.Equals(other.ServiceOwner_ServiceName) &&
                   ServiceOwner_ServiceDescription.Equals(other.ServiceOwner_ServiceDescription) &&

                   ServiceOwner_RoleAccessJustification.Equals(other.ServiceOwner_RoleAccessJustification) &&
                   ServiceOwner_RemovalImpact.Equals(other.ServiceOwner_RemovalImpact) &&
                   ServiceOwner_IsRevoked.Equals(other.ServiceOwner_IsRevoked) &&
                   ServiceOwner_DateCertified.Equals(other.ServiceOwner_DateCertified) &&

                   // Risk stuff
                   RiskImpact.Equals(other.RiskImpact) &&
                   RiskLikelihood.Equals(other.RiskLikelihood) &&
                   RiskNotes.Equals(other.RiskNotes) &&
                   RiskAssessmentDate.Equals(other.RiskAssessmentDate);*/
        }


        // Handle hashes for everything but the managers / service owners isCertified and the
        // risk rating. The isCertified stuff gets reset everytime a new cycle happens and
        // risk rating is calculated from threat and likelihood ratings so we can ignore those
        public override int GetHashCode()
        {
            // Role stuff
            int hashRolePrivId = RolePrivId == null ? 0 : RolePrivId.GetHashCode();

            int hashRoleId = RoleId == null ? 0 : RoleId.GetHashCode();
            int hashRoleName = RoleName == null ? 0 : RoleName.GetHashCode();
            int hashRoleDescription = RoleDescription == null ? 0 : RoleDescription.GetHashCode();
            int hashRoleOwner_RoleId = RoleOwner_RoleId == null ? 0 : RoleOwner_RoleId.GetHashCode();

            // Manager stuff
            int hashRoleOwner_PrivId = RoleOwner_PrivId == null ? 0 : RoleOwner_PrivId.GetHashCode();
            int hashRoleOwner_PermissionGroup = RoleOwner_PermissionGroup == null ? 0 : RoleOwner_PermissionGroup.GetHashCode();
            int hashRoleOwner_ServicePrivSummary = RoleOwner_ServicePrivSummary == null ? 0 : RoleOwner_ServicePrivSummary.GetHashCode();
            int hashRoleOwner_CredentialStorageMethod = RoleOwner_CredentialStorageMethod == null ? 0 : RoleOwner_CredentialStorageMethod.GetHashCode();
            int hashRoleOwner_ServiceId = RoleOwner_ServiceId == null ? 0 : RoleOwner_ServiceId.GetHashCode();
            int hashRoleOwner_ServiceName = RoleOwner_ServiceName == null ? 0 : RoleOwner_ServiceName.GetHashCode();
            int hashRoleOwner_ServiceDescription = RoleOwner_ServiceDescription == null ? 0 : RoleOwner_ServiceDescription.GetHashCode();

            int hashRoleOwner_RoleAccessJustification = RoleOwner_RoleAccessJustification == null ? 0 : RoleOwner_RoleAccessJustification.GetHashCode();
            int hashRoleOwner_RemovalImpact = RoleOwner_RemovalImpact == null ? 0 : RoleOwner_RemovalImpact.GetHashCode();
            int hashRoleOwner_IsRevoked = RoleOwner_IsRevoked == null ? 0 : RoleOwner_IsRevoked.GetHashCode();
            int hashRoleOwner_DateCertified = RoleOwner_DateCertified == null ? 0 : RoleOwner_DateCertified.GetHashCode();

            // Service Owner Stuff
            int hashServiceOwner_PrivId = ServiceOwner_PrivId == null ? 0 : ServiceOwner_PrivId.GetHashCode();
            int hashServiceOwner_PermissionGroup = ServiceOwner_PermissionGroup == null ? 0 : ServiceOwner_PermissionGroup.GetHashCode();
            int hashServiceOwner_ServicePrivSummary = ServiceOwner_ServicePrivSummary == null ? 0 : ServiceOwner_ServicePrivSummary.GetHashCode();
            int hashServiceOwner_CredentialStorageMethod = ServiceOwner_CredentialStorageMethod == null ? 0 : ServiceOwner_CredentialStorageMethod.GetHashCode();
            int hashServiceOwner_ServiceId = ServiceOwner_ServiceId == null ? 0 : ServiceOwner_ServiceId.GetHashCode();
            int hashServiceOwner_ServiceName = ServiceOwner_ServiceName == null ? 0 : ServiceOwner_ServiceName.GetHashCode();
            int hashServiceOwner_ServiceDescription = ServiceOwner_ServiceDescription == null ? 0 : ServiceOwner_ServiceDescription.GetHashCode();

            int hashServiceOwner_RoleAccessJustification = ServiceOwner_RoleAccessJustification == null ? 0 : ServiceOwner_RoleAccessJustification.GetHashCode();
            int hashServiceOwner_RemovalImpact = ServiceOwner_RemovalImpact == null ? 0 : ServiceOwner_RemovalImpact.GetHashCode();
            int hashServiceOwner_IsRevoked = ServiceOwner_IsRevoked == null ? 0 : ServiceOwner_IsRevoked.GetHashCode();
            int hashServiceOwner_DateCertified = ServiceOwner_DateCertified == null ? 0 : ServiceOwner_DateCertified.GetHashCode();


            // Risk Stuff
            int hashRiskImpact = RiskImpact == null ? 0 : RiskImpact.GetHashCode();
            int hashRiskLikelihood = RiskLikelihood == null ? 0 : RiskLikelihood.GetHashCode();
            int hashRiskNotes = RiskNotes == null ? 0 : RiskNotes.GetHashCode();
            int hashRiskAssessmentDate = RiskAssessmentDate == null ? 0 : ServiceOwner_DateCertified.GetHashCode();


            // Calculate the hash code for the object.
            return // Role stuff
                   hashRolePrivId ^
                   hashRoleId ^
                   hashRoleName ^
                   hashRoleDescription ^
                   hashRoleOwner_RoleId ^

                   // Manager stuff
                   hashRoleOwner_PrivId ^
                   hashRoleOwner_PermissionGroup ^
                   hashRoleOwner_ServicePrivSummary ^
                   hashRoleOwner_CredentialStorageMethod ^
                   hashRoleOwner_ServiceId ^
                   hashRoleOwner_ServiceName ^
                   hashRoleOwner_ServiceDescription ^

                   hashRoleOwner_RoleAccessJustification ^
                   hashRoleOwner_RemovalImpact ^
                   hashRoleOwner_IsRevoked ^
                   hashRoleOwner_DateCertified ^

                   // Service Owner Stuff
                   hashServiceOwner_PrivId ^
                   hashServiceOwner_PermissionGroup ^
                   hashServiceOwner_ServicePrivSummary ^
                   hashServiceOwner_CredentialStorageMethod ^
                   hashServiceOwner_ServiceId ^
                   hashServiceOwner_ServiceName ^
                   hashServiceOwner_ServiceDescription ^

                   hashServiceOwner_RoleAccessJustification ^
                   hashServiceOwner_RemovalImpact ^
                   hashServiceOwner_IsRevoked ^
                   hashServiceOwner_DateCertified ^

                   // Risk Stuff
                   hashRiskImpact ^
                   hashRiskLikelihood ^
                   hashRiskNotes ^
                   hashRiskAssessmentDate;
        }
    }
}
