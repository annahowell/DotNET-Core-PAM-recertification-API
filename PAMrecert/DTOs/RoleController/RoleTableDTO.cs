namespace PAMrecert.DTOs.RoleController
{
    public class RoleTableDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string RoleOwner_RoleId { get; set; }
    }
}