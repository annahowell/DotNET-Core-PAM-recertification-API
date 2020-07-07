using CsvHelper.Configuration;

namespace PAMrecert.DTOs.CsvUpload
{
    public sealed class RolePrivCsvDTOmap : ClassMap<RolePrivCsvDTO>
    {
        public RolePrivCsvDTOmap()
        {
            Map(m => m.RoleId).Validate(field => !string.IsNullOrEmpty(field));

            // Manager stuff
            Map(m => m.RoleOwner_PrivId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.RoleOwner_RoleAccessJustification).Optional();
            Map(m => m.RoleOwner_RemovalImpact).Optional();
            Map(m => m.RoleOwner_IsRevoked).Optional();
            Map(m => m.RoleOwner_IsCertified).Optional();
            Map(m => m.RoleOwner_DateCertified).Optional();

            // Service owner stuff
            Map(m => m.ServiceOwner_PrivId).Validate(field => !string.IsNullOrEmpty(field));
            Map(m => m.ServiceOwner_RoleAccessJustification).Optional();
            Map(m => m.ServiceOwner_RemovalImpact).Optional();
            Map(m => m.ServiceOwner_IsRevoked).Optional();
            Map(m => m.ServiceOwner_IsCertified).Optional();
            Map(m => m.ServiceOwner_DateCertified).Optional();

            // Risk stuff
            Map(m => m.RiskImpact).Optional();
            Map(m => m.RiskLikelihood).Optional();
            Map(m => m.RiskNotes).Optional();
            Map(m => m.RiskAssessmentDate).Optional();
            Map(m => m.RiskIsAssessed).Optional();
        }
    }
}
