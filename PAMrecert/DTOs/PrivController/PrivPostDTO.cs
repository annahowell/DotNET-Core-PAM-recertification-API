using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAMrecert.DTOs.PrivController
{
    public class PrivPostDTO
    {
        [Required]
        public string PrivId { get; set; }

        [Required]
        public string ServiceId { get; set; }

        [Required]
        public string PermissionGroup { get; set; }

        [Required]
        public string ServicePrivSummary { get; set; }

        public string CredentialStorageMethod { get; set; }
    }
}
