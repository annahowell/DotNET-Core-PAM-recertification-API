using System.ComponentModel.DataAnnotations;

namespace PAMrecert.DTOs.RoleController
{
    public class RolePostDTO
    {
        [Required]
        public string RoleId { get; set; }

        [Required]
        public string RoleName { get; set; }

        public string RoleDescription { get; set; }

        [MinLength(0, ErrorMessage = "A role owner must be submitted, however if the this role is a top-level role (i.e. CEO) then it can be an empty string")]
        public string RoleOwner_RoleId { get; set; }
    }
}