namespace PAMrecert.DTOs.RoleController
{
    public class ServiceDTO
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public bool FullyCertified { get; set; }
    }
}