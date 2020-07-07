using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EfCoreTemporalTable;
using Microsoft.AspNetCore.JsonPatch;
using Swashbuckle.AspNetCore.Annotations;
using CsvHelper;
using PAMrecert.Models;
using PAMrecert.DTOs.CsvUpload;
using PAMrecert.DTOs.RoleController;
using PAMrecert.Services;

namespace PAMrecert.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/v1/role")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly PAML01Context _context;
        private readonly ControllerService _service;
        private ILogger<RoleController> _logger { get; }

        public RoleController(PAML01Context context, ControllerService service, ILogger<RoleController> logger)
        {
            _context = context;
            _service = service;
            _logger = logger;
        }



        // GET: api/v1/role ----------------------------------------------------
        /// <summary>
        /// Returns all roles
        /// </summary>
        /// <returns>
        /// Returns all roles
        /// </returns>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(IEnumerable<RoleDTO>))]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            try
            {
                ActionResult<IEnumerable<RoleDTO>> result = await (
                    from r in _context.RoleTable
                    orderby r.RoleName ascending
                    select new RoleDTO
                    {
                        RoleId          = r.RoleId,
                        RoleName        = r.RoleName,
                        RoleDescription = r.RoleDescription,
                        RoleOwner_RoleId    = r.RoleOwner_RoleId,
                        // Returns false if any of the following are true:
                        // - The ServiceOwner_IsCertified is false
                        // - The RoleOwner_IsCertified is false
                        // - There are no entries in the RolePrivLink table (can't be fully certified if there are no privs to certify)
                        FullyCertified = !(from rp in _context.RolePrivLink
                                           where rp.RoleId == r.RoleId &&
                                           (rp.ServiceOwner_IsCertified == false || rp.RoleOwner_IsCertified == false) ||
                                                !(from rp2 in _context.RolePrivLink
                                                  where rp2.RoleId == r.RoleId
                                                  select rp2.RoleId).Any()
                                           select rp).Any()
                    }
                ).ToListAsync();

                if (result == null || !result.Value.Any())
                {
                    return StatusCode(204);
                }

                return StatusCode(200, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/priv ----------------------------------------------------
        /// <summary>
        /// Returns all roleprivs including RolePrivId
        /// </summary>
        /// <returns>
        /// Returns all roleprivs including RolePrivId
        /// </returns>
        [HttpGet("priv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(IEnumerable<RoleDTO>))]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<RolePrivLinkDTO>>> GetRolePrivs()
        {
            try
            {
                ActionResult<IEnumerable<RolePrivLinkDTO>> result = await (
                    from r in _context.RolePrivLink
                    orderby r.RolePrivId ascending
                    select new RolePrivLinkDTO
                    {
                        RolePrivId = r.RolePrivId,
                        RoleId = r.RoleId,

                        // RoleOwner_ stuff
                        RoleOwner_PrivId = r.RoleOwner_PrivId,
                        RoleOwner_RoleAccessJustification = r.RoleOwner_RoleAccessJustification,
                        RoleOwner_RemovalImpact = r.RoleOwner_RemovalImpact,
                        RoleOwner_IsRevoked = r.RoleOwner_IsRevoked,
                        RoleOwner_IsCertified = r.RoleOwner_IsCertified,
                        RoleOwner_DateCertified = r.RoleOwner_DateCertified,

                        // Service owners stuff
                        ServiceOwner_PrivId = r.ServiceOwner_PrivId,
                        ServiceOwner_RoleAccessJustification = r.ServiceOwner_RoleAccessJustification,
                        ServiceOwner_RemovalImpact = r.ServiceOwner_RemovalImpact,
                        ServiceOwner_IsRevoked = r.ServiceOwner_IsRevoked,
                        ServiceOwner_IsCertified = r.ServiceOwner_IsCertified,
                        ServiceOwner_DateCertified = r.ServiceOwner_DateCertified,

                        // Risk stuff
                        RiskImpact = r.RiskImpact,
                        RiskLikelihood = r.RiskLikelihood,
                        RiskNotes = r.RiskNotes,
                        RiskAssessmentDate = r.RiskAssessmentDate,
                        RiskIsAssessed = r.RiskIsAssessed
                    }
                ).ToListAsync();

                if (result == null || !result.Value.Any())
                {
                    return StatusCode(204);
                }

                return StatusCode(200, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/riskDirector --------------------------------------------------
        /// <summary>
        /// Returns a role
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a role
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(RoleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RoleDTO>> GetRole(string id)
        {

            try
            {
                ActionResult<RoleDTO> result = await (
                    from r in _context.RoleTable
                    where r.RoleId == id
                    select new RoleDTO
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        RoleDescription = r.RoleDescription,
                        RoleOwner_RoleId = r.RoleOwner_RoleId
                    }
                ).FirstOrDefaultAsync();

                if (result.Value == null)
                {
                    return StatusCode(404);
                }

                return StatusCode(200, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/priv/1 ----------------------------------------------------
        /// <summary>
        /// Returns a rolepriv by RolePrivId
        /// </summary>
        /// <returns>
        /// Returns a rolepriv by RolePrivId
        /// </returns>
        [HttpGet("priv/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(IEnumerable<RoleDTO>))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RolePrivLinkDTO>> GetRolePriv(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return StatusCode(400, "id is invalid");
                }

                ActionResult<RolePrivLinkDTO> result = await (
                    from r in _context.RolePrivLink
                    where r.RolePrivId == id
                    select new RolePrivLinkDTO
                    {
                        RolePrivId = r.RolePrivId,
                        RoleId = r.RoleId,

                        // RoleOwner_ stuff
                        RoleOwner_PrivId = r.RoleOwner_PrivId,
                        RoleOwner_RoleAccessJustification = r.RoleOwner_RoleAccessJustification,
                        RoleOwner_RemovalImpact = r.RoleOwner_RemovalImpact,
                        RoleOwner_IsRevoked = r.RoleOwner_IsRevoked,
                        RoleOwner_IsCertified = r.RoleOwner_IsCertified,
                        RoleOwner_DateCertified = r.RoleOwner_DateCertified,

                        // Service owners stuff
                        ServiceOwner_PrivId = r.ServiceOwner_PrivId,
                        ServiceOwner_RoleAccessJustification = r.ServiceOwner_RoleAccessJustification,
                        ServiceOwner_RemovalImpact = r.ServiceOwner_RemovalImpact,
                        ServiceOwner_IsRevoked = r.ServiceOwner_IsRevoked,
                        ServiceOwner_IsCertified = r.ServiceOwner_IsCertified,
                        ServiceOwner_DateCertified = r.ServiceOwner_DateCertified,

                        // Risk stuff
                        RiskImpact = r.RiskImpact,
                        RiskLikelihood = r.RiskLikelihood,
                        RiskNotes = r.RiskNotes,
                        RiskAssessmentDate = r.RiskAssessmentDate,
                        RiskIsAssessed = r.RiskIsAssessed
                    }
                ).FirstOrDefaultAsync();

                if (result.Value == null)
                {
                    return StatusCode(404);
                }

                return StatusCode(200, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/riskDirector/ownedroles ------------------------------------------
        /// <summary>
        /// Returns the roles owned by a role along with whether or not they've been fully certified by the role owner
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns the roles owned by a role along with whether or not they've been fully certified by the role owner
        /// or not they've been fully certified
        /// </returns>
        [HttpGet("{id}/ownedroles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(RoleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRolesOwnedRoles(string id)
        {
            try
            {
                ActionResult<IEnumerable<RoleDTO>> result = await (
                    from r in _context.RoleTable
                    where r.RoleOwner_RoleId == id
                    select new RoleDTO
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        RoleDescription = r.RoleDescription,
                        // Returns false if any of the following are true:
                        // - The RoleOwner_IsCertified is false
                        // - There are no entries in the RolePrivLink table (can't be fully certified if there are no privs to certify)
                        FullyCertified = !(from rp in _context.RolePrivLink
                                           where rp.RoleId == r.RoleId && rp.RoleOwner_IsCertified == false ||
                                                !(from rp2 in _context.RolePrivLink
                                                  where rp2.RoleId == r.RoleId
                                                  select rp2).Any()
                                           select rp).Any()
                    }
                ).Distinct().OrderBy(ur => ur.RoleName).ToListAsync();

                if (result.Value == null || !result.Value.Any())
                {
                    return StatusCode(404);
                }

                return StatusCode(200, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/riskDirector/ownedservices ----------------------------------------------
        /// <summary>
        /// Returns the services owned by a role along with whether or not they've been fully certified by the service owner
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns the services owned by a role along with whether or not they've been fully certified by the service owner
        /// </returns>
        [HttpGet("{id}/ownedservices")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(RoleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetUserServices(string id)
        {
            try
            {
                ActionResult<IEnumerable<ServiceDTO>> result = await (
                    from r in _context.RoleTable
                    join s in _context.ServiceTable on r.RoleId equals s.ServiceOwner_RoleId
                    where r.RoleId == id
                    select new ServiceDTO
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        ServiceDescription = s.ServiceDescription,
                        // Returns false if any of the following are true:
                        // - The ServiceOwner_IsCertified is false
                        // - There are no entries in the RolePrivLink table (can't be fully certified if there are no privs to certify)
                        FullyCertified = !(from rp in _context.RolePrivLink
                                           join p in _context.PrivTable on rp.ServiceOwner_PrivId equals p.PrivId
                                           where p.ServiceId == s.ServiceId && rp.ServiceOwner_IsCertified == false ||
                                                !(from rp2 in _context.RolePrivLink
                                                  join p in _context.PrivTable on rp2.ServiceOwner_PrivId equals p.PrivId
                                                  where p.ServiceId == s.ServiceId
                                                  select rp2.RoleId).Any()
                                           select rp.ServiceOwner_IsCertified).Any()
                    }
                ).Distinct().OrderBy(su => su.ServiceName).ToListAsync();

                if (!result.Value.Any())
                {
                    return StatusCode(404);
                }

                return StatusCode(200, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/riskDirector/serviceprivs -------------------------------------
        /// <summary>
        /// Returns a role, the services assigned to and for each of those services the role owners current certification,
        /// previous certification and all available privileges that could be assigned to the service
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a role, the services assigned to and for each of those services the role owners current certification,
        /// previous certification and all available privileges that could be assigned to the service
        /// </returns>
        [HttpGet("{id}/serviceprivs")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(RoleServiceDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RoleServiceDTO>> GetRoleServicePrivs(string id)
        {
            try
            {
                // Get the end datetime of the second but latest recert cycle or if there is no second but latest recert
                // cycle (there's only 1 cycle) return a datetime of today minus 1000 years. This should guarantee null
                // for PreviousServicePriv without having to handle lots of edge casey nonsense.
                DateTime lastRecertEndDatetimeOrImpossible = _service.GetRecertCycleDatetimeByOffset(1) ?? DateTime.Today.AddYears(-1000);

                ActionResult<RoleServiceDTO> result = await (
                    // Start in role because we're getting info per role and so we can get RoleDescription etc
                    from r in _context.RoleTable
                    where r.RoleId == id
                    select new RoleServiceDTO
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        RoleDescription = r.RoleDescription,
                        RoleOwner_RoleId = r.RoleOwner_RoleId,
                        ServicePrivs = (
                            // Get access to the priv and service tables via RolePrivLink and PrivTable
                            from rp in _context.RolePrivLink
                            join p in _context.PrivTable on rp.RoleOwner_PrivId equals p.PrivId
                            join s in _context.ServiceTable on p.ServiceId equals s.ServiceId
                            where rp.RoleId == id
                            orderby s.ServiceName ascending
                            select new ServicePrivsDTO
                            {
                                RolePrivId = rp.RolePrivId,

                                ServiceId = s.ServiceId,
                                ServiceName = s.ServiceName,
                                ServiceDescription = s.ServiceDescription,

                                PermissionGroup = p.PermissionGroup,
                                ServicePrivSummary = p.ServicePrivSummary,
                                CredentialStorageMethod = p.CredentialStorageMethod,

                                // Get the previous recert cycle's service priv info
                                PreviousPriv = (
                                    from prp in _context.RolePrivLink.AsTemporalAsOf(lastRecertEndDatetimeOrImpossible)
                                    join pp in _context.PrivTable.AsTemporalAsOf(lastRecertEndDatetimeOrImpossible) on prp.RoleOwner_PrivId equals pp.PrivId
                                    where rp.RolePrivId == prp.RolePrivId
                                    select new PreviousPrivDTO
                                    {
                                        PrivId = pp.PrivId,
                                        PermissionGroup = pp.PermissionGroup,
                                        ServicePrivSummary = pp.ServicePrivSummary,
                                        CredentialStorageMethod = pp.CredentialStorageMethod
                                    }
                                ).FirstOrDefault(),

                                RoleOwner_PrivId = rp.RoleOwner_PrivId,
                                RoleOwner_RoleAccessJustification = rp.RoleOwner_RoleAccessJustification,
                                RoleOwner_RemovalImpact = rp.RoleOwner_RemovalImpact,
                                RoleOwner_IsRevoked = rp.RoleOwner_IsRevoked,
                                RoleOwner_IsCertified = rp.RoleOwner_IsCertified,

                                // Now get a list of all potential privs for each service
                                ServicesAvailablePrivs = (
                                    from s2 in  _context.PrivTable
                                    where s2.ServiceId == s.ServiceId
                                    orderby s2.PermissionGroup ascending
                                    select new ServicesAvailablePrivsDTO
                                    {
                                        PrivId = s2.PrivId,
                                        PermissionGroup = s2.PermissionGroup,
                                        ServicePrivSummary = s2.ServicePrivSummary,
                                        CredentialStorageMethod = s2.CredentialStorageMethod
                                    }
                                )
                            }
                        )
                    }
                ).FirstOrDefaultAsync();

                if (result.Value == null)
                {
                    return StatusCode(404);
                }

                return StatusCode(200, result.Value);
            }

            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/riskDirector/riskassessmentserviceprivs/1 ---------------------
        /// <summary>
        /// Returns the role data and an array of each service assigned to it along with their recertification data as
        /// of a datetime using temporal tables. The datetime is defined by the enddate of recert cycle offset passed.
        /// For example role/riskDirector/riskassessmentserviceprivs/1 will use the enddate of the last but 1 recert cycle
        /// </summary>
        /// <param name="id"></param>
        /// <param name="offset"></param>
        /// <returns>
        /// Returns the role data and an array of each service assigned to it along with their recertification data as
        /// of a datetime using temporal tables. The datetime is defined by the enddate of recert cycle offset passed.
        /// For example role/riskDirector/riskassessmentserviceprivs/1 will use the enddate of the last but 1 recert cycle
        /// </returns>
        [HttpGet("{id}/riskassessmentserviceprivs/{offset}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(RiskAssessmentAllDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RiskAssessmentAllDTO>> GetRiskAssessmentRoleServicePrivs(string id, int offset)
        {
            try
            {
                if (offset < 0) // MVC will auto check existence and the type for us
                {
                    return StatusCode(400, "A valid offset must be included");
                }

                // Get the end datetime of the last but offset recert cycle, returns current datetime if offset was 0
                DateTime? nullableRecertEndDatetimeOrNow = _service.GetRecertCycleDatetimeByOffset(offset);

                if (nullableRecertEndDatetimeOrNow == null)
                {
                    return StatusCode(404, "Offset not found");
                }
                else
                {
                    DateTime recertEndDatetimeOrNow = (DateTime) nullableRecertEndDatetimeOrNow;

                    ActionResult<RiskAssessmentAllDTO> result = await (
                        from r in _context.RoleTable.AsTemporalAsOf(recertEndDatetimeOrNow)
                        where r.RoleId == id

                        select new RiskAssessmentAllDTO
                        {
                            // Role stuff
                            RoleId = r.RoleId,
                            RoleName = r.RoleName,
                            RoleDescription = r.RoleDescription,
                            RoleOwner_RoleId = r.RoleOwner_RoleId,

                            RiskAssessment = (
                                from rp in _context.RolePrivLink.AsTemporalAsOf(recertEndDatetimeOrNow)

                                    // msp = RoleOwner_ service priv link
                                    // ms = RoleOwner_ service table
                                    // mp = RoleOwner_ priv table
                                join mp in _context.PrivTable.AsTemporalAsOf(recertEndDatetimeOrNow) on rp.RoleOwner_PrivId equals mp.PrivId
                                join ms in _context.ServiceTable.AsTemporalAsOf(recertEndDatetimeOrNow) on mp.ServiceId equals ms.ServiceId

                                // sosp = Service owners service priv link
                                // sos = Service owners service table
                                // sop = Service owners priv table
                                join sop in _context.PrivTable.AsTemporalAsOf(recertEndDatetimeOrNow) on rp.ServiceOwner_PrivId equals sop.PrivId
                                join sos in _context.ServiceTable.AsTemporalAsOf(recertEndDatetimeOrNow) on sop.ServiceId equals sos.ServiceId
                                where rp.RoleId == id
                                select new RiskAssessmentDTO
                                {
                                    RolePrivId = rp.RolePrivId,

                                    // RoleOwner_ stuff
                                    RoleOwner_PrivId = rp.RoleOwner_PrivId,
                                    RoleOwner_PermissionGroup = mp.PermissionGroup,
                                    RoleOwner_ServicePrivSummary = mp.ServicePrivSummary,
                                    RoleOwner_CredentialStorageMethod = mp.CredentialStorageMethod,

                                    RoleOwner_ServiceId = ms.ServiceId,
                                    RoleOwner_ServiceName = ms.ServiceName,
                                    RoleOwner_ServiceDescription = ms.ServiceDescription,

                                    RoleOwner_RoleAccessJustification = rp.RoleOwner_RoleAccessJustification,
                                    RoleOwner_RemovalImpact = rp.RoleOwner_RemovalImpact,
                                    RoleOwner_IsRevoked = rp.RoleOwner_IsRevoked,
                                    RoleOwner_IsCertified = rp.RoleOwner_IsCertified,
                                    RoleOwner_DateCertified = rp.RoleOwner_DateCertified,

                                    // Service owners stuff
                                    ServiceOwner_PrivId = rp.ServiceOwner_PrivId,
                                    ServiceOwner_PermissionGroup = sop.PermissionGroup,
                                    ServiceOwner_ServicePrivSummary = sop.ServicePrivSummary,
                                    ServiceOwner_CredentialStorageMethod = sop.CredentialStorageMethod,

                                    ServiceOwner_ServiceId = sos.ServiceId,
                                    ServiceOwner_ServiceName = sos.ServiceName,
                                    ServiceOwner_ServiceDescription = sos.ServiceDescription,

                                    ServiceOwner_RoleAccessJustification = rp.ServiceOwner_RoleAccessJustification,
                                    ServiceOwner_RemovalImpact = rp.ServiceOwner_RemovalImpact,
                                    ServiceOwner_IsRevoked = rp.ServiceOwner_IsRevoked,
                                    ServiceOwner_IsCertified = rp.ServiceOwner_IsCertified,
                                    ServiceOwner_DateCertified = rp.ServiceOwner_DateCertified,

                                    // Risk stuff
                                    RiskImpact = rp.RiskImpact,
                                    RiskLikelihood = rp.RiskLikelihood,
                                    RiskRating = rp.RiskImpact * rp.RiskLikelihood,
                                    RiskNotes = rp.RiskNotes,
                                    RiskAssessmentDate = rp.RiskAssessmentDate,
                                    RiskIsAssessed = rp.RiskIsAssessed
                                }
                            ).ToList()
                        }
                    ).FirstOrDefaultAsync();

                    if (result.Value == null)
                    {
                        return StatusCode(404, "Risk Assessment Role Service Privs not found");
                    }

                    return StatusCode(200, result.Value);
                }
            }

            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/tablecsv -------------------------------------------
        /// <summary>
        /// Returns a csv of all current data in RoleTable
        /// </summary>
        /// <returns>
        /// Returns a csv of all current data in RoleTable
        /// </returns>
        [HttpGet("tablecsv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Produces("text/csv", "application/json")]
        [SwaggerResponse(200, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> ExportRoleTable()
        {
            try
            {
                List<RoleTableDTO> result = await (
                    from r in _context.RoleTable
                    select new RoleTableDTO
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        RoleDescription = r.RoleDescription,
                        RoleOwner_RoleId = r.RoleOwner_RoleId
                    }
                ).ToListAsync();

                if (!result.Any())
                {
                    return StatusCode(204);
                }

                var stream = new MemoryStream();
                var writeFile = new StreamWriter(stream);
                var csv = new CsvWriter(writeFile, CultureInfo.CreateSpecificCulture("en-GB"));

                csv.Configuration.ShouldQuote = (field, context) => context.HasHeaderBeenWritten;
                csv.WriteRecords(result);

                writeFile.Flush();
                stream.Position = 0;

                return File(stream, "application/octet-stream", "RoleTable.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/privlinkcsv ----------------------------------------
        /// <summary>
        /// Returns a csv of all data currently in RolePrivLink excluding the RolePrivId
        /// </summary>
        /// <returns>
        /// Returns a csv of all data currently in RolePrivLink excluding the RolePrivId
        /// </returns>
        [HttpGet("privlinkcsv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Produces("text/csv", "application/json")]
        [SwaggerResponse(200, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> ExportRolePrivLink()
        {
            try
            {
                List<RolePrivLinkCsvDTO> result = await (
                    from rp in _context.RolePrivLink
                    select new RolePrivLinkCsvDTO
                    {
                        RoleId = rp.RoleId,

                        // RoleOwner_ stuff
                        RoleOwner_PrivId = rp.RoleOwner_PrivId,
                        RoleOwner_RoleAccessJustification = rp.RoleOwner_RoleAccessJustification,
                        RoleOwner_RemovalImpact = rp.RoleOwner_RemovalImpact,
                        RoleOwner_IsRevoked = rp.RoleOwner_IsRevoked,
                        RoleOwner_IsCertified = rp.RoleOwner_IsCertified,
                        RoleOwner_DateCertified = rp.RoleOwner_DateCertified,

                        // Service owners stuff
                        ServiceOwner_PrivId = rp.ServiceOwner_PrivId,
                        ServiceOwner_RoleAccessJustification = rp.ServiceOwner_RoleAccessJustification,
                        ServiceOwner_RemovalImpact = rp.ServiceOwner_RemovalImpact,
                        ServiceOwner_IsRevoked = rp.ServiceOwner_IsRevoked,
                        ServiceOwner_IsCertified = rp.ServiceOwner_IsCertified,
                        ServiceOwner_DateCertified = rp.ServiceOwner_DateCertified,

                        // Risk stuff
                        RiskImpact = rp.RiskImpact,
                        RiskLikelihood = rp.RiskLikelihood,
                        RiskNotes = rp.RiskNotes,
                        RiskAssessmentDate = rp.RiskAssessmentDate,
                        RiskIsAssessed = rp.RiskIsAssessed
                    }
                ).ToListAsync();

                if (!result.Any())
                {
                    return StatusCode(204);
                }

                var stream = new MemoryStream();
                var writeFile = new StreamWriter(stream);
                var csv = new CsvWriter(writeFile, CultureInfo.CreateSpecificCulture("en-GB"));

                csv.Configuration.ShouldQuote = (field, context) => context.HasHeaderBeenWritten;
                csv.WriteRecords(result);

                writeFile.Flush();
                stream.Position = 0;

                return File(stream, "application/octet-stream", "RolePrivLink.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/csv?baseDateTime=2020-03-24T14:33:03 ----------
        /// <summary>
        /// Returns all recertification data for all roles, including the privileges on each service as submitted by
        /// the role owner and service owner. Data is returned as of a specific point in time using a datetime passed
        /// as a variable in the URL
        /// </summary>
        /// <param name="baseDateTime">The base datetime to use e.g. 2020-03-24T14:33:03</param>
        /// <returns>
        /// Returns all recertification data for all roles, including the privileges on each service as submitted by
        /// the role owner and service owner. Data is returned as of a specific point in time using a datetime passed
        /// as a variable in the URL
        /// </returns>
        [HttpGet("csv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Produces("text/csv", "application/json")]
        [SwaggerResponse(200, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> ExportAllTemporalByDateTime(string baseDateTime)
        {
            try
            {
                DateTime convertedBaseDateTime;

                if (!DateTime.TryParse(baseDateTime, out convertedBaseDateTime))
                {
                    return StatusCode(400, "DateTime is invalid");
                }

                List<RoleServicePrivAllDTO> result = await _service.GetRoleServicePrivAllDTObyDateTime(convertedBaseDateTime);

                if (result == null || !result.Any())
                {
                    return StatusCode(204);
                }

                var stream = new MemoryStream();
                var writeFile = new StreamWriter(stream);
                var csv = new CsvWriter(writeFile, CultureInfo.CreateSpecificCulture("en-GB"));

                csv.Configuration.ShouldQuote = (field, context) => context.HasHeaderBeenWritten;
                csv.WriteRecords(result);

                writeFile.Flush();
                stream.Position = 0;

                return File(stream, "application/octet-stream", "All-Role-Recertification-Data.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/differcsv?baseDateTime=2020-03-24T14:33:03 ---------
        /// <summary>
        /// Returns all recertification data for roles where the role owner and service owners certification differ.
        /// Includes the privileges on each service as submitted by the role owner and service owner. Data is returned
        /// as of a specific point in time using a datetime passed as a variable in the URL
        /// </summary>
        /// <param name="baseDateTime">The base datetime to use e.g. 2020-03-24T14:33:03</param>
        /// <returns>
        /// Returns all recertification data for roles where the role owner and service owners certification differ.
        /// Includes the privileges on each service as submitted by the role owner and service owner. Data is returned
        /// as of a specific point in time using a datetime passed as a variable in the URL
        /// </returns>
        [HttpGet("differcsv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Produces("text/csv", "application/json")]
        [SwaggerResponse(200, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> ExportDiffTemporalByDateTime(string baseDateTime)
        {
            try
            {
                DateTime convertedBaseDateTime;

                if (!DateTime.TryParse(baseDateTime, out convertedBaseDateTime))
                {
                    return StatusCode(400, "DateTime is invalid");
                }

                List<RoleServicePrivAllDTO> result = await _service.GetRoleServicePrivAllDTObyDateTime(convertedBaseDateTime);

                if (result == null || !result.Any())
                {
                    return StatusCode(204);
                }

                // We've returned everything using our service, so now prune items we're not interested in
                result = result.Where(rspa => rspa.RoleOwner_PrivId != rspa.ServiceOwner_PrivId).ToList();

                var stream = new MemoryStream();
                var writeFile = new StreamWriter(stream);
                var csv = new CsvWriter(writeFile, CultureInfo.CreateSpecificCulture("en-GB"));

                csv.Configuration.ShouldQuote = (field, context) => context.HasHeaderBeenWritten;
                csv.WriteRecords(result);

                writeFile.Flush();
                stream.Position = 0;

                return File(stream, "application/octet-stream", "Role-Recertification-Data-Differs.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/role/deltacsv?baseDateTime=2020-03-24T14:33:03&deltaDateTime=2019-03-24T14:33:03
        /// <summary>
        /// Returns a delta all recertification data for all roles, including the privileges on each service as submitted
        /// by the role owner and service owner between two specific points in time using datetimes passed as a variables in the URL
        /// </summary>
        /// <param name="baseDateTime">The base datetime to use when calculating the delta e.g. 2020-03-24T14:33:03</param>
        /// <param name="deltaDateTime">The delta datetime to use when canculating the delta e.g. 2019-01-19T14:33:03 </param>
        /// <returns>
        /// Returns a delta all recertification data for all roles, including the privileges on each service as submitted
        /// by the role owner and service owner between two specific points in time using datetimes passed as a variables in the URL
        /// </returns>
        [HttpGet("deltacsv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Produces("text/csv", "application/json")]
        [SwaggerResponse(200, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> ExportDeltaTemporalByDateTime(string baseDateTime, string deltaDateTime)
        {
            try
            {
                DateTime convertedBaseDateTime;

                if (!DateTime.TryParse(baseDateTime, out convertedBaseDateTime))
                {
                    return StatusCode(400, "Base DateTime is invalid");
                }

                DateTime convertedDeltaDateTime;

                if (!DateTime.TryParse(deltaDateTime, out convertedDeltaDateTime))
                {
                    return StatusCode(400, "Delta DateTime is invalid");
                }

                int convertedBaseDateTimeIsEarlierIfLessThanZero = DateTime.Compare(convertedBaseDateTime, convertedDeltaDateTime);

                if (convertedBaseDateTimeIsEarlierIfLessThanZero == 0)
                {
                    // If they're both the same
                    return StatusCode(400, "Datetimes are identical");
                }
                else if (convertedBaseDateTimeIsEarlierIfLessThanZero > 0)
                {
                    // If the base is later than the delta then swap them
                    (convertedBaseDateTime, convertedDeltaDateTime) = (convertedDeltaDateTime, convertedBaseDateTime);
                }

                List<RoleServicePrivAllDTO> result;
                List<RoleServicePrivAllDTO> baseResult;
                List<RoleServicePrivAllDTO> deltaResult;

                baseResult = await _service.GetRoleServicePrivAllDTObyDateTime(convertedBaseDateTime);
                deltaResult = await _service.GetRoleServicePrivAllDTObyDateTime(convertedDeltaDateTime);

                if (baseResult == null || !baseResult.Any() || deltaResult == null || !deltaResult.Any())
                {
                    return StatusCode(204);
                }

                // We've returned everything using our service, so now get the delta of rows
                result = deltaResult.Except(baseResult).ToList();

                if (result == null || !result.Any())
                {
                    return StatusCode(204);
                }

                var stream = new MemoryStream();
                var writeFile = new StreamWriter(stream);
                var csv = new CsvWriter(writeFile, CultureInfo.CreateSpecificCulture("en-GB"));

                csv.Configuration.ShouldQuote = (field, context) => context.HasHeaderBeenWritten;
                csv.WriteRecords(result);

                writeFile.Flush();
                stream.Position = 0;

                return File(stream, "application/octet-stream", "Role-Recertification-Data-Delta.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // POST api/v1/role ----------------------------------------------------
        /// <summary>
        /// Creates a new role
        /// </summary>
        /// <param name="role"></param>
        /// <returns>
        /// The newly created role
        /// </returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [SwaggerResponse(201, Type = typeof(RoleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(409, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RoleDTO>> AddRole(RolePostDTO role)
        {
            try
            {
                var roleExists = await _context.RoleTable.FindAsync(role.RoleId);

                if (roleExists != null)
                {
                    return StatusCode(409, "The RoleId already exists"); // Conflict
                }

                if (role.RoleOwner_RoleId != "")
                {
                    var ownerExists = await _context.RoleTable.FindAsync(role.RoleOwner_RoleId);

                    if (ownerExists == null)
                    {
                        return StatusCode(400, "The RoleOwner_RoleId does not exist");
                    }
                }

                _context.RoleTable.Add(
                    new RoleTable
                    {
                        RoleId          = role.RoleId,
                        RoleName        = role.RoleName,
                        RoleDescription = role.RoleDescription,
                        RoleOwner_RoleId = role.RoleOwner_RoleId
                    }
                );

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, role);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // POST api/v1/role/priv ----------------------------------------------------
        /// <summary>
        /// Creates a new role priv
        /// </summary>
        /// <param name="rolepriv"></param>
        /// <returns>
        /// The newly created role priv
        /// </returns>
        [HttpPost("priv")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [SwaggerResponse(201, Type = typeof(RoleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(409, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RolePrivLinkPostDTO>> AddRolePriv(RolePrivLinkPostDTO rolepriv)
        {
            try
            {
                // Check to see if this exists based on first unique constraint RoleId and RoleOwner_PrivId
                var roleIdAndRoleOwner_PrivIdExists = _context.RolePrivLink.AsNoTracking().Where(r => r.RoleId == rolepriv.RoleId && r.RoleOwner_PrivId == rolepriv.RoleOwner_PrivId).FirstOrDefault();

                if (roleIdAndRoleOwner_PrivIdExists != null)
                {
                    return StatusCode(409, "The combination of RoleId and RoleOwner_PrivId already exists");
                }
                else
                {
                    // Or check to see if this exists based on second unique constained RoleId and ServiceOwner_PrivId
                    var roleIdAndServiceOwner_PrivIdExists = _context.RolePrivLink.AsNoTracking().Where(r => r.RoleId == rolepriv.RoleId && r.ServiceOwner_PrivId == rolepriv.ServiceOwner_PrivId).FirstOrDefault();

                    if (roleIdAndServiceOwner_PrivIdExists != null)
                    {
                        return StatusCode(409, "The combination of RoleId and ServiceOwner_PrivId already exists");
                    }
                }

                RolePrivLink newRolePriv = new RolePrivLink
                {
                    RoleId = rolepriv.RoleId,

                    // RoleOwner_ stuff
                    RoleOwner_PrivId = rolepriv.RoleOwner_PrivId,
                    RoleOwner_RoleAccessJustification = rolepriv.RoleOwner_RoleAccessJustification,
                    RoleOwner_RemovalImpact = rolepriv.RoleOwner_RemovalImpact,
                    RoleOwner_IsRevoked = rolepriv.RoleOwner_IsRevoked,
                    RoleOwner_IsCertified = rolepriv.RoleOwner_IsCertified,
                    RoleOwner_DateCertified = rolepriv.RoleOwner_DateCertified,

                    // Service owners stuff
                    ServiceOwner_PrivId = rolepriv.ServiceOwner_PrivId,
                    ServiceOwner_RoleAccessJustification = rolepriv.ServiceOwner_RoleAccessJustification,
                    ServiceOwner_RemovalImpact = rolepriv.ServiceOwner_RemovalImpact,
                    ServiceOwner_IsRevoked = rolepriv.ServiceOwner_IsRevoked,
                    ServiceOwner_IsCertified = rolepriv.ServiceOwner_IsCertified,
                    ServiceOwner_DateCertified = rolepriv.ServiceOwner_DateCertified,

                    // Risk stuff
                    RiskImpact = rolepriv.RiskImpact,
                    RiskLikelihood = rolepriv.RiskLikelihood,
                    RiskNotes = rolepriv.RiskNotes,
                    RiskAssessmentDate = rolepriv.RiskAssessmentDate,
                    RiskIsAssessed = rolepriv.RiskIsAssessed
                };

                _context.RolePrivLink.Add(newRolePriv);


                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRolePriv), new { id = newRolePriv.RolePrivId }, newRolePriv);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // PATCH: api/v1/role/riskDirector ------------------------------------------------
        /// <summary>
        /// Updates an existing role by RoleId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns>
        /// Updates an existing role by RoleId
        /// </returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(204)] // No content
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorised
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(500)] // Internal server error
        [SwaggerResponse(204, Type = typeof(UserTable))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody]JsonPatchDocument<RoleTable> patch)
        {
            try
            {
                var row = await _context.RoleTable.Where(u => u.RoleId == id).FirstOrDefaultAsync();

                if (row == null)
                {
                    return StatusCode(404);
                }

                patch.ApplyTo(row, ModelState);
                _context.SaveChanges();

                if (!ModelState.IsValid)
                {
                    string modelStateJson = _service.ToJson(ModelState);

                    _logger.LogInformation("Bad" + modelStateJson);

                    return new ContentResult
                    {
                        Content = modelStateJson,
                        ContentType = "application/json",
                        StatusCode = 400
                    };
                }

                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(401);

            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(500);
            }
        }



        // PATCH: api/v1/role/priv/5 -------------------------------------------
        /// <summary>
        /// Updates an existing rolepriv by RolePrivId
        /// </summary>
        /// <param name="rolePrivId"></param>
        /// <param name="patch"></param>
        [HttpPatch("priv/{rolePrivId}")]
        [ProducesResponseType(204)] // No content
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorised
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(500)] // Internal server error
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> UpdateRoleServicePriv(int rolePrivId, [FromBody]JsonPatchDocument<RolePrivLink> patch)
        {
            try
            {
                var row = await _context.RolePrivLink.Where(rp => rp.RolePrivId == rolePrivId).FirstOrDefaultAsync();

                if (row == null)
                {
                    return StatusCode(404);
                }

                patch.ApplyTo(row, ModelState);
                _context.SaveChanges();

                if (!ModelState.IsValid)
                {

                    string modelStateJson = _service.ToJson(ModelState);

                    _logger.LogInformation("Bad" + modelStateJson);

                    return new ContentResult
                    {
                        Content = modelStateJson,
                        ContentType = "application/json",
                        StatusCode = 400
                    };
                }

                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(400);

            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(500);
            }
        }



        // POST api/v1/role/tablecsv -------------------------------------------
        /// <summary>
        /// Replaces, updates or removes roles by csv upload, a DeleteAndReplace boolean must be
        /// included as part of the request to denote whether or not the csv entirely deletes and replaces
        /// the existing data or simply updates roles that already exist or otherwise creates them 
        /// </summary>
        /// <param name="csvUpload"></param>
        [HttpPost("tablecsv"), DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [SwaggerResponse(201, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(409, Type = typeof(String))]
        public IActionResult PostCsv([FromForm] CsvUploadDTO csvUpload)
        {
            try
            {
                if (csvUpload.File.Length <= 0)
                {
                    return StatusCode(400, "Invalid .csv file");
                }

                if (csvUpload.DeleteAndReplace == null)
                {
                    return StatusCode(400, "DeleteAndReplace boolean must be included as part of the request");
                }

                using var reader = new StreamReader(csvUpload.File.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.CreateSpecificCulture("en-GB"));

                csv.Configuration.RegisterClassMap<RoleCsvDTOmap>();

                var roles = csv.GetRecords<RoleCsvDTO>();

                if (csvUpload.DeleteAndReplace == true)
                {
                    _context.RoleTable.RemoveRange(_context.RoleTable.ToList());

                    _context.SaveChanges();
                }

                foreach (RoleCsvDTO role in roles)
                {
                    var exists = _context.RoleTable.AsNoTracking().FirstOrDefault(r => r.RoleId == role.RoleId);

                    if (csvUpload.DeleteAndReplace == false && exists != null)
                    {
                        _context.RoleTable.Update(
                            new RoleTable
                            {
                                RoleId = role.RoleId,
                                RoleName = role.RoleName,
                                RoleDescription = role.RoleDescription,
                                RoleOwner_RoleId = role.RoleOwner_RoleId
                            }
                        );
                    }
                    else
                    {
                        _context.RoleTable.Add(
                            new RoleTable
                            {
                                RoleId = role.RoleId,
                                RoleName = role.RoleName,
                                RoleDescription = role.RoleDescription,
                                RoleOwner_RoleId = role.RoleOwner_RoleId
                            }
                        );
                    }
                }
                _context.SaveChanges();

                return StatusCode(201);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDebug(ex.ToString());

                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SqlException sqlEx)
                    {
                        return sqlEx.Number switch
                        {
                            // Constraint check violation
                            547 => StatusCode(400, "Constraint check violation (a UserTable, ServiceTable or RolePrivLink entry probably relies on the RoleId you are amending/removing): " + sqlEx.Message.ToString()),

                            // Duplicated key row error / Constraint violation exception
                            2601 => StatusCode(400, "Duplicate key or constraint violation: " + sqlEx.Message.ToString()),

                            // Unique constraint error
                            2627 => StatusCode(400, "Unique constraint error (a chosen RoleId probably already exists): " + sqlEx.Message.ToString()),

                            _ => StatusCode(400, "An unexpected error occured: " + sqlEx.Message.ToString()),
                        };
                    }
                }

                return StatusCode(400, "An unexpected database error occured");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(400, _service.HandleStandardCsvExceptionMessages(ex));
            }
        }



        // POST api/v1/role/privlinkcsv ----------------------------------------
        /// <summary>
        /// Replaces, updates or removes roleprivs by csv upload, a DeleteAndReplace boolean must be
        /// included as part of the request to denote whether or not the csv entirely deletes and replaces
        /// the existing data or simply updates roleprivs that already exist or otherwise creates them 
        /// </summary>
        /// <param name="csvUpload"></param>
        [HttpPost("privlinkcsv"), DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [SwaggerResponse(201, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(409, Type = typeof(String))]
        public IActionResult PostPrivsCsv([FromForm] CsvUploadDTO csvUpload)
        {
            try
            {
                if (csvUpload.File.Length <= 0)
                {
                    return StatusCode(400, "Invalid .csv file");
                }

                if (csvUpload.DeleteAndReplace == null)
                {
                    return StatusCode(400, "DeleteAndReplace boolean must be included as part of the request");
                }

                using var reader = new StreamReader(csvUpload.File.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.CreateSpecificCulture("en-GB"));

                csv.Configuration.RegisterClassMap<RolePrivCsvDTOmap>();

                var roleprivs = csv.GetRecords<RolePrivCsvDTO>();

                if (csvUpload.DeleteAndReplace == true)
                {
                    _context.RolePrivLink.RemoveRange(_context.RolePrivLink.ToList());
                }

                foreach (RolePrivCsvDTO rolepriv in roleprivs)
                {
                    if (csvUpload.DeleteAndReplace == false)
                    {
                        // Check to see if this exists based on first unique constraint RoleId and RoleOwner_PrivId
                        var exists = _context.RolePrivLink.AsNoTracking().Where(r => r.RoleId == rolepriv.RoleId && r.RoleOwner_PrivId == rolepriv.RoleOwner_PrivId).FirstOrDefault();

                        // If it does exist delete it ready for it to be re-added below
                        if (exists != null)
                        {
                            _context.RolePrivLink.Remove(exists);
                        }
                        else
                        {
                            // Or check to see if this exists based on second unique constained RoleId and ServiceOwner_PrivId
                            exists = _context.RolePrivLink.AsNoTracking().Where(r => r.RoleId == rolepriv.RoleId && r.ServiceOwner_PrivId == rolepriv.ServiceOwner_PrivId).FirstOrDefault();

                            // If it does exist delete it ready for it to be re-added below
                            if (exists != null)
                            {
                                _context.RolePrivLink.Remove(exists);
                            }
                        }
                    }

                    _context.RolePrivLink.Add(
                        new RolePrivLink
                        {
                            RoleId = rolepriv.RoleId,

                            // RoleOwner_ stuff
                            RoleOwner_PrivId = rolepriv.RoleOwner_PrivId,
                            RoleOwner_RoleAccessJustification = rolepriv.RoleOwner_RoleAccessJustification,
                            RoleOwner_RemovalImpact = rolepriv.RoleOwner_RemovalImpact,
                            RoleOwner_IsRevoked = rolepriv.RoleOwner_IsRevoked,
                            RoleOwner_IsCertified = rolepriv.RoleOwner_IsCertified,
                            RoleOwner_DateCertified = rolepriv.RoleOwner_DateCertified,

                            // Service owners stuff
                            ServiceOwner_PrivId = rolepriv.ServiceOwner_PrivId,
                            ServiceOwner_RoleAccessJustification = rolepriv.ServiceOwner_RoleAccessJustification,
                            ServiceOwner_RemovalImpact = rolepriv.ServiceOwner_RemovalImpact,
                            ServiceOwner_IsRevoked = rolepriv.ServiceOwner_IsRevoked,
                            ServiceOwner_IsCertified = rolepriv.ServiceOwner_IsCertified,
                            ServiceOwner_DateCertified = rolepriv.ServiceOwner_DateCertified,

                            // Risk stuff
                            RiskImpact = rolepriv.RiskImpact,
                            RiskLikelihood = rolepriv.RiskLikelihood,
                            RiskNotes = rolepriv.RiskNotes,
                            RiskAssessmentDate = rolepriv.RiskAssessmentDate,
                            RiskIsAssessed = rolepriv.RiskIsAssessed
                        }
                    );
                }

                _context.SaveChanges();

                return StatusCode(201);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDebug(ex.ToString());

                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SqlException sqlEx)
                    {
                        return sqlEx.Number switch
                        {
                            // Constraint check violation
                            547 => StatusCode(400, "Constraint check violation: " + sqlEx.Message.ToString()),

                            // Duplicated key row error / Constraint violation exception
                            2601 => StatusCode(400, "Duplicate key or constraint violation: " + sqlEx.Message.ToString()),

                            // Unique constraint error
                            2627 => StatusCode(400, "Unique constraint error (a chosen combination of RoleId + RoleOwner_PrivId or RoleId + ServiceOwner_PrivId probably already exists): " + sqlEx.Message.ToString()),

                            _ => StatusCode(400, "An unexpected error occured: " + sqlEx.Message.ToString()),
                        };
                    }
                }

                return StatusCode(400, "An unexpected database error occured");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(400, _service.HandleStandardCsvExceptionMessages(ex));
            }
        }



        // DELETE api/role/riskDirector ---------------------------------------------------
        /// <summary>
        /// Deletes a role if it has no dependencies in the database
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var role = await _context.RoleTable.FindAsync(id);

                if (role == null)
                {
                    return StatusCode(404);
                }

                _context.RoleTable.Remove(role);
                await _context.SaveChangesAsync();

                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDebug(ex.ToString());

                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SqlException sqlEx)
                    {
                        return sqlEx.Number switch
                        {
                            // Constraint check violation
                            547 => StatusCode(400, "Constraint check violation (a UserTable, ServiceTable or RolePrivLink entry probably relies on the RoleId you are removing): " + sqlEx.Message.ToString()),

                            _ => StatusCode(400, "An unexpected error occured: " + sqlEx.Message.ToString()),
                        };
                    }
                }

                return StatusCode(400, "An unexpected database error occured");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(500);
            }
        }



        // DELETE api/role/priv/5 ---------------------------------------------------
        /// <summary>
        /// Deletes a rolepriv by RolePrivId
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("priv/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> DeleteRolePriv(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return StatusCode(400, "RolePrivId is invalid");
                }

                var rolePriv = await _context.RolePrivLink.FindAsync(id);

                if (rolePriv == null)
                {
                    return StatusCode(404);
                }

                _context.RolePrivLink.Remove(rolePriv);
                await _context.SaveChangesAsync();

                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDebug(ex.ToString());

                if (ex.InnerException != null)
                {
                    if (ex.InnerException is SqlException sqlEx)
                    {
                        return sqlEx.Number switch
                        {
                            // Constraint check violation (shouldn't happen)
                            547 => StatusCode(400, "Constraint check violation (The entry you are deleting is required by another entry): " + sqlEx.Message.ToString()),

                            _ => StatusCode(400, "An unexpected error occured: " + sqlEx.Message.ToString()),
                        };
                    }
                }

                return StatusCode(400, "An unexpected database error occured");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());

                return StatusCode(500);
            }
        }
    }
}
