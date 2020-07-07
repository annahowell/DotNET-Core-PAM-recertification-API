using System;
using System.Diagnostics.CodeAnalysis;

namespace PAMrecert.DTOs.UserController
{
    public class UserRoleAllDTO : IEquatable<UserRoleAllDTO>
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }

        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string RoleOwner_RoleId { get; set; }

        public string LastCertifiedBy { get; set; }
        public DateTime? LastCertifiedDate { get; set; }



        // The following methods support the delta reports
        public bool Equals([AllowNull] UserRoleAllDTO other)
        {
            // Check whether the compared object is null.
            if (other is null) return false;

            // Check whether the compared object references the same data.
            if (ReferenceEquals(this, other)) return true;

            // Check whether the objects’ properties are equal.
            return UserId.Equals(other.UserId) &&
                   UserFullName.Equals(other.UserFullName) &&

                   RoleId.Equals(other.RoleId) &&
                   RoleName.Equals(other.RoleName) &&
                   RoleDescription.Equals(other.RoleDescription) &&
                   RoleOwner_RoleId.Equals(other.RoleOwner_RoleId) &&

                   LastCertifiedBy.Equals(other.LastCertifiedBy) &&
                   LastCertifiedDate.Equals(other.LastCertifiedDate);
        }


        // Handle hashes for everything but the managers / service owners isCertified and the 
        // risk rating. The isCertified stuff gets reset everytime a new cycle happens and
        // risk rating is calculated from threat and likelihood ratings so we can ignore those
        public override int GetHashCode()
        {
            int hashUserId = UserId == null ? 0 : UserId.GetHashCode();
            int hashUserFullName = UserFullName == null ? 0 : UserFullName.GetHashCode();

            int hashRoleId = RoleId == null ? 0 : RoleId.GetHashCode();
            int hashRoleName = RoleName == null ? 0 : RoleName.GetHashCode();
            int hashRoleDescription = RoleDescription == null ? 0 : RoleDescription.GetHashCode();
            int hashRoleOwner_RoleId = RoleOwner_RoleId == null ? 0 : RoleOwner_RoleId.GetHashCode();

            int hashLastCertifiedBy = LastCertifiedBy == null ? 0 : LastCertifiedBy.GetHashCode();
            int hashLastCertifiedDate = LastCertifiedDate == null ? 0 : LastCertifiedDate.GetHashCode();

            // Calculate the hash code for the object.
            return hashUserId ^
                   hashUserFullName ^

                   hashRoleId ^
                   hashRoleName ^
                   hashRoleDescription ^
                   hashRoleOwner_RoleId ^

                   hashLastCertifiedBy ^
                   hashLastCertifiedDate;
        }
    }
}