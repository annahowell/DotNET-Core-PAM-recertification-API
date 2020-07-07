using CsvHelper.Configuration;

namespace PAMrecert.DTOs.CsvUpload
{
    public sealed class RoleCsvDTOmap : ClassMap<RoleCsvDTO>
    {
        public RoleCsvDTOmap()
        {
            Map(m => m.RoleId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.RoleName).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.RoleDescription).Optional();
            Map(m => m.RoleOwner_RoleId).Validate(field => field != null);
        }
    }
}
