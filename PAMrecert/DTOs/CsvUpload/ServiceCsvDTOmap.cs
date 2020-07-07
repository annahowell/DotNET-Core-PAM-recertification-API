using CsvHelper.Configuration;

namespace PAMrecert.DTOs.CsvUpload
{
    public sealed class ServiceCsvDTOmap : ClassMap<ServiceCsvDTO>
    {
        public ServiceCsvDTOmap()
        {
            Map(m => m.ServiceId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.ServiceName).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.ServiceDescription).Optional();
            Map(m => m.ServiceOwner_RoleId).Validate(field => !string.IsNullOrEmpty(field));
        }
    }
}
