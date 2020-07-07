using System.ComponentModel.DataAnnotations;

namespace PAMrecert.DTOs.ServiceController
{
    public class ServicePostDTO
    {
        [Required]
        public string ServiceId { get; set; }

        [Required]
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }

        [Required]
        public string ServiceOwner_RoleId { get; set; }
    }
}