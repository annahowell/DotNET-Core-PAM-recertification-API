using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EfCoreTemporalTable;
using Newtonsoft.Json;
using PAMrecert.Models;
using PAMrecert.DTOs.RoleController;
using PAMrecert.DTOs.UserController;
using CsvHelper;

namespace PAMrecert.Services
{
    public class ControllerService
    {
        private readonly PAML01Context _context;
        private ILogger<ControllerService> _logger { get; }

        public ControllerService(PAML01Context context, ILogger<ControllerService> logger)
        {
            _context = context;
            _logger = logger;
        }



        // Converts modelstate to Json -----------------------------------------
        public string ToJson(ModelStateDictionary modelstate)
        {
            List<string> errors = modelstate.Values
                                    .SelectMany(x => x.Errors)
                                    .Select(x => x.ErrorMessage)
                                    .ToList();
            return JsonConvert.SerializeObject(errors);
        }



        // Returns a string based on exception, used for the csv upload functions
        public string HandleStandardCsvExceptionMessages(Exception ex)
        {
            if (ex is HeaderValidationException)
            {
                return "Required header value(s): " + string.Join(", ", ((HeaderValidationException)ex).HeaderNames) + " not found in the .csv";
            }
            else if (ex is FieldValidationException)
            {
                // Can't pass the Field properly right now, need to spend a lot of time on the maps to get decent data out
                // return "Required field value(s): " + string.Join(", ", ((FieldValidationException)ex).Field) + " not found in the .csv";
                return "Required field value(s) not found in the .csv";
            }
            else if (ex is CsvHelper.MissingFieldException)
            {
                return "Missing field(s) detected in the .csv: " + ex.Message.ToString();
            }
            else if (ex is BadDataException)
            {
                return "Bad data detected in .csv file: " + ex.Message.ToString();
            }
            else if (ex is ParserException)
            {
                return "Error parsing .csv file: " + ex.Message.ToString();
            }
            else
            {
                return "A critical error occured during processing: " + ex.Message.ToString();
            }
        }



        // Returns the ending datetime of a specific recert cycle by offset or
        // the current datetime if offset is 0 (current cycle) or null if cycle
        // offset >= 1 doesn't exist.
        public DateTime? GetRecertCycleDatetimeByOffset(int offset)
        {
            try
            {
                if (offset == 0)
                {
                    return DateTime.Now;
                }
                else
                {
                    DateTime? result = null;

                    result = (
                        from rc in _context.RecertCycleTable
                        orderby rc.RecertCycleId descending
                        select rc.RecertEndedDate
                    ).Skip(offset).FirstOrDefault();

                    if (result != null)
                    {
                        result = DateTime.Parse(result.ToString());
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return null;
            }
        }



        // Returns the ending datetime of a specific recert cycle by id or
        // the current datetime if the id of the curren cycle is passed
        public DateTime? GetRecertCycleDatetimeById(int id)
        {
            DateTime? result = null;

            try
            {
                // get the latest recert cycle id
                int? tmp = (
                    from rc in _context.RecertCycleTable
                    orderby rc.RecertCycleId descending
                    select rc.RecertCycleId
                ).FirstOrDefault();

                if (id == tmp)
                {
                    result = DateTime.Now;
                }
                else
                {
                    result = (
                        from rc in _context.RecertCycleTable
                        where rc.RecertCycleId == id
                        select rc.RecertEndedDate
                    ).FirstOrDefault();

                    if (result != null)
                    {
                        result = DateTime.Parse(result.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return null;
            }

            _logger.LogDebug(result.ToString());

            return result;
        }



        // Returns a list of RoleServicePrivAllDTO as of the date passed -------
        public async Task<List<RoleServicePrivAllDTO>> GetRoleServicePrivAllDTObyDateTime(DateTime dateTime)
        {
            return await (
                from rp in _context.RolePrivLink.AsTemporalAsOf(dateTime)
                join r in _context.RoleTable.AsTemporalAsOf(dateTime) on rp.RoleId equals r.RoleId

                // msp = RoleOwner_ service priv link
                // ms = RoleOwner_ service table
                // mp = RoleOwner_ priv table
                join msp in _context.PrivTable.AsTemporalAsOf(dateTime) on rp.RoleOwner_PrivId equals msp.PrivId
                join ms in _context.ServiceTable.AsTemporalAsOf(dateTime) on msp.ServiceId equals ms.ServiceId

                // sosp = Service owners service priv link
                // sos = Service owners service table
                // sop = Service owners priv table
                join sosp in _context.PrivTable.AsTemporalAsOf(dateTime) on rp.ServiceOwner_PrivId equals sosp.PrivId
                join sos in _context.ServiceTable.AsTemporalAsOf(dateTime) on sosp.ServiceId equals sos.ServiceId

                orderby r.RoleName ascending
                select new RoleServicePrivAllDTO
                {
                    // Role stuff
                    RoleId = r.RoleId ?? "",
                    RoleName = r.RoleName ?? "",
                    RoleDescription = r.RoleDescription ?? "",
                    RoleOwner_RoleId = r.RoleOwner_RoleId ?? "",

                    // Manager stuff
                    RoleOwner_PrivId = rp.RoleOwner_PrivId ?? "",
                    RoleOwner_ServicePrivSummary = msp.ServicePrivSummary ?? "",

                    RoleOwner_PermissionGroup = msp.PermissionGroup ?? "",
                    RoleOwner_CredentialStorageMethod = msp.CredentialStorageMethod ?? "",
                    RoleOwner_ServiceId = msp.ServiceId ?? "",
                    RoleOwner_ServiceName = ms.ServiceName ?? "",
                    RoleOwner_ServiceDescription = ms.ServiceDescription ?? "",

                    RoleOwner_RoleAccessJustification = rp.RoleOwner_RoleAccessJustification ?? "",
                    RoleOwner_RemovalImpact = rp.RoleOwner_RemovalImpact ?? "",
                    RoleOwner_IsRevoked = rp.RoleOwner_IsRevoked,
                    RoleOwner_IsCertified = rp.RoleOwner_IsCertified,
                    RoleOwner_DateCertified = rp.RoleOwner_DateCertified ?? default,

                    // Service Owner stuff
                    ServiceOwner_PrivId = rp.ServiceOwner_PrivId ?? "",
                    ServiceOwner_ServicePrivSummary = sosp.ServicePrivSummary ?? "",

                    ServiceOwner_PermissionGroup = sosp.PermissionGroup ?? "",
                    ServiceOwner_CredentialStorageMethod = sosp.CredentialStorageMethod ?? "",
                    ServiceOwner_ServiceId = sosp.ServiceId ?? "",
                    ServiceOwner_ServiceName = sos.ServiceName ?? "",
                    ServiceOwner_ServiceDescription = sos.ServiceDescription ?? "",

                    ServiceOwner_RoleAccessJustification = rp.ServiceOwner_RoleAccessJustification ?? "",
                    ServiceOwner_RemovalImpact = rp.ServiceOwner_RemovalImpact ?? "",
                    ServiceOwner_IsRevoked = rp.ServiceOwner_IsRevoked,
                    ServiceOwner_IsCertified = rp.ServiceOwner_IsCertified,
                    ServiceOwner_DateCertified = rp.ServiceOwner_DateCertified ?? default,

                    // Risk stuff
                    RiskImpact = rp.RiskImpact ?? 0,
                    RiskLikelihood = rp.RiskLikelihood ?? 0,
                    RiskRating = rp.RiskImpact * rp.RiskLikelihood ?? 0,
                    RiskNotes = rp.RiskNotes ?? "",
                    RiskAssessmentDate = rp.RiskAssessmentDate ?? default,
                }
            ).ToListAsync();
        }



        // Returns a list of UserRoleAllDTO as of the datetime bassed
        public async Task<List<UserRoleAllDTO>> GetUserRoleAllDTObyDateTime(DateTime dateTime)
        {
            return await (
                from u in _context.UserTable.AsTemporalAsOf(dateTime)
                join r in _context.RoleTable.AsTemporalAsOf(dateTime) on u.RoleId equals r.RoleId

                orderby r.RoleName ascending
                select new UserRoleAllDTO
                {
                    UserId = u.UserId ?? "",
                    UserFullName = u.UserFullName ?? "",

                    RoleId = r.RoleId ?? "",
                    RoleName = r.RoleName ?? "",
                    RoleDescription = r.RoleDescription ?? "",
                    RoleOwner_RoleId = r.RoleOwner_RoleId ?? "",

                    LastCertifiedBy = u.LastCertifiedBy ?? "",
                    LastCertifiedDate = u.LastCertifiedDate ?? default,
                }
            ).ToListAsync();
        }
    }
}