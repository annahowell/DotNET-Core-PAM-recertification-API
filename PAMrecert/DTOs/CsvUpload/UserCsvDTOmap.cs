using CsvHelper.Configuration;

namespace PAMrecert.DTOs.CsvUpload
{
    public sealed class UserCsvDTOmap : ClassMap<UserCsvDTO>
    {
        public UserCsvDTOmap()
        {
            Map(m => m.UserId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.UserFullName).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.RoleId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.LastCertifiedBy).Optional();
            Map(m => m.LastCertifiedDate).Optional();
        }
    }
}
