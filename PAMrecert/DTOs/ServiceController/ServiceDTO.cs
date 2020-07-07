using System.ComponentModel.DataAnnotations;

namespace PAMrecert.DTOs.ServiceController
{
    public class ServiceDTO
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceOwner_RoleId { get; set; }
        public bool FullyCertified { get; set; }
    }
}