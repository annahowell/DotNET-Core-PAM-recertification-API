using System;
using System.ComponentModel.DataAnnotations;

namespace PAMrecert.DTOs.UserController
{
    public class UserPostDTO
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string UserFullName { get; set; }

        [Required]
        public string RoleId { get; set; }
    }
}