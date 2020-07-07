using CsvHelper.Configuration;

namespace PAMrecert.DTOs.CsvUpload
{
    public sealed class PrivCsvDTOmap : ClassMap<PrivCsvDTO>
    {
        public PrivCsvDTOmap()
        {
            Map(m => m.PrivId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.ServiceId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.ServicePrivSummary).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.PermissionGroup).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.CredentialStorageMethod).Optional();
        }
    }
}
