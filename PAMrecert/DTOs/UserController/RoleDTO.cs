namespace PAMrecert.DTOs.UserController
{
    public class RoleDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool FullyCertified { get; set; }
    }
}