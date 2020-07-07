using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAMrecert.DTOs.PrivController
{
    public class PrivDTO
    {
        public string PrivId { get; set; }
        public string ServiceId { get; set; }
        public string PermissionGroup { get; set; }
        public string ServicePrivSummary { get; set; }
        public string CredentialStorageMethod { get; set; }
    }
}
