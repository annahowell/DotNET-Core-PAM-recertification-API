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
using Swashbuckle.AspNetCore.Annotations;
using EfCoreTemporalTable;
using CsvHelper;
using PAMrecert.Models;
using PAMrecert.DTOs.CsvUpload;
using PAMrecert.DTOs.ServiceController;
using PAMrecert.Services;
using Microsoft.AspNetCore.JsonPatch;

namespace PAMrecert.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/v1/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly PAML01Context _context;
        private readonly ControllerService _service;
        private ILogger<ServiceController> _logger { get; }

        public ServiceController(PAML01Context context, ControllerService service, ILogger<ServiceController> logger)
        {
            _context = context;
            _service = service;
            _logger = logger;
        }



        // GET: api/v1/service -------------------------------------------------
        /// <summary>
        /// Returns all services
        /// </summary>
        /// <returns>
        /// All services
        /// </returns>
        // GET: api/service
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ServiceDTO>))]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServices()
        {
            try
            {
                ActionResult<IEnumerable<ServiceDTO>> result = await (
                    from s in _context.ServiceTable
                    orderby s.ServiceName ascending
                    select new ServiceDTO
                    {
                        ServiceId           = s.ServiceId,
                        ServiceName         = s.ServiceName,
                        ServiceOwner_RoleId = s.ServiceOwner_RoleId,
                        ServiceDescription  = s.ServiceDescription
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



        // GET: api/v1/service/qualys -----------------------------------------------
        /// <summary>
        /// Returns a service
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a service
        /// </returns>
        // GET: api/service/qualys
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(ServiceDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<ServiceDTO>> GetService(string id)
        {
            try
            {
                ActionResult<ServiceDTO> result = await (
                    from s in _context.ServiceTable
                    where s.ServiceId == id
                    select new ServiceDTO
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        ServiceOwner_RoleId = s.ServiceOwner_RoleId,
                        ServiceDescription = s.ServiceDescription
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



        // GET: api/v1/service/qualys/privs -----------------------------------------
        /// <summary>
        /// Returns all privs currently associated with a service 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns all privs currently associated with a service 
        /// </returns>
        [HttpGet("{id}/privs")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(ServicePrivDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<ServicePrivDTO>>> GetServicePrivs(string id)
        {
            try
            {
                ActionResult<IEnumerable<ServicePrivDTO>> result = await (
                    from p in _context.PrivTable
                    where p.ServiceId == id
                    orderby p.PermissionGroup ascending
                    select new ServicePrivDTO
                    {
                        PrivId = p.PrivId,
                        ServiceId = p.ServiceId,
                        PermissionGroup = p.PermissionGroup,
                        ServicePrivSummary = p.ServicePrivSummary,
                        CredentialStorageMethod = p.CredentialStorageMethod
                    }
                ).ToListAsync();

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



        // GET: api/v1/service/qualys/roleprivs -------------------------------------
        /// <summary>
        /// Returns a service, the possible privileges available to the service (if there
        /// are any) along with the current and previous (if available) certification data
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a service, the possible privileges available to the service (if there
        /// are any) along with the current and previous (if available) certification data
        /// </returns>
        [HttpGet("{id}/roleprivs")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(ServiceRoleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<ServiceRoleDTO>> GetServiceRolePrivs(string id)
        {
            try
            {
                // Get the end datetime of the last recert cycle or if there is no last recert cycle (there's only 1 cycle)
                // return a datetime of today minus 1000 years. This should guarantee null for PreviousServicePriv without
                // having to handle lots of edge casey nonsense
                DateTime lastRecertEndDatetimeOrImpossible = _service.GetRecertCycleDatetimeByOffset(1) ?? DateTime.Today.AddYears(-1000);

                ActionResult<ServiceRoleDTO> result = await (
                    // Start in service because we're getting info per service and so we can get ServiceDescription etc
                    from s in _context.ServiceTable
                    where s.ServiceId == id
                    select new ServiceRoleDTO
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        ServiceOwner_RoleId = s.ServiceOwner_RoleId,
                        ServiceDescription = s.ServiceDescription,
                        // Get a list of all potential privs for the service
                        ServicesAvailablePrivs = (
                            from p in _context.PrivTable
                            where s.ServiceId == p.ServiceId
                            orderby p.PermissionGroup ascending
                            select new ServiceAvailablePrivsDTO
                            {
                                PrivId = p.PrivId,
                                PermissionGroup = p.PermissionGroup,
                                ServicePrivSummary = p.ServicePrivSummary,
                                CredentialStorageMethod = p.CredentialStorageMethod
                            }
                        ),
                        RolePrivs = (
                            // Get access to the priv and role tables via ServicePrivLink and RoleServicePrivLink
                            from p2 in _context.PrivTable
                            join rp in _context.RolePrivLink on p2.PrivId equals rp.ServiceOwner_PrivId
                            join r in _context.RoleTable on rp.RoleId equals r.RoleId
                            where p2.ServiceId == id
                            orderby r.RoleName ascending
                            select new RolePrivsDTO
                            {
                                RolePrivId = rp.RolePrivId,

                                RoleId = r.RoleId,
                                RoleName = r.RoleName,
                                RoleDescription = r.RoleDescription,

                                PermissionGroup = p2.PermissionGroup,
                                ServicePrivSummary = p2.ServicePrivSummary,
                                CredentialStorageMethod = p2.CredentialStorageMethod,

                                // Get the previous recert cycle's service priv info
                                PreviousPriv = (
                                    from prp in _context.RolePrivLink.AsTemporalAsOf(lastRecertEndDatetimeOrImpossible)
                                    join pp in _context.PrivTable.AsTemporalAsOf(lastRecertEndDatetimeOrImpossible) on prp.ServiceOwner_PrivId equals pp.PrivId
                                    where rp.RolePrivId == prp.RolePrivId
                                    select new ServicePrivDTO
                                    {
                                        PrivId = pp.PrivId,
                                        ServiceId = pp.ServiceId,
                                        PermissionGroup = pp.PermissionGroup,
                                        ServicePrivSummary = pp.ServicePrivSummary,
                                        CredentialStorageMethod = pp.CredentialStorageMethod
                                    }
                                ).FirstOrDefault(),

                                ServiceOwner_PrivId = rp.ServiceOwner_PrivId,
                                ServiceOwner_RoleAccessJustification = rp.ServiceOwner_RoleAccessJustification,
                                ServiceOwner_RemovalImpact = rp.ServiceOwner_RemovalImpact,
                                ServiceOwner_IsRevoked = rp.ServiceOwner_IsRevoked,
                                ServiceOwner_IsCertified = rp.ServiceOwner_IsCertified,
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



        // GET: api/v1/service/tablecsv ----------------------------------------
        /// <summary>
        /// Returns a csv of all current data in ServiceTable
        /// </summary>
        /// <returns>
        /// Returns a csv of all current data in ServiceTable
        /// </returns>
        [HttpGet("tablecsv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Produces("text/csv", "application/json")]
        [SwaggerResponse(200, Type = typeof(string))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> ExportServiceTable()
        {
            try
            {
                List<ServiceTableDTO> result = await (
                    from s in _context.ServiceTable
                    select new ServiceTableDTO
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        ServiceDescription = s.ServiceDescription,
                        ServiceOwner_RoleId = s.ServiceOwner_RoleId
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

                return File(stream, "application/octet-stream", "ServiceTable.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // PATCH: api/v1/service/5 ------------------------------------------------
        /// <summary>
        /// Updates an existing service
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns>
        /// Updates an existing service
        /// </returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(204)] // No content
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorised
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(500)] // Internal server error
        [SwaggerResponse(204, Type = typeof(ServiceTable))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> UpdateService(string id, [FromBody]JsonPatchDocument<ServiceTable> patch)
        {
            try
            {
                var row = await _context.ServiceTable.Where(u => u.ServiceId == id).FirstOrDefaultAsync();

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



        // POST api/v1/service -------------------------------------------------
        /// <summary>
        /// Creates a new service
        /// </summary>
        /// <param name="service"></param>
        /// <returns>
        /// The newly created service
        /// </returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [SwaggerResponse(201, Type = typeof(ServiceDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(409, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<ServiceDTO>> AddService(ServicePostDTO service)
        {
            try
            {
                var serviceExists = await _context.ServiceTable.FindAsync(service.ServiceId);

                if (serviceExists != null)
                {
                    return StatusCode(409, "The ServiceId already exists");
                }

                var roleExists = await _context.RoleTable.FindAsync(service.ServiceOwner_RoleId);

                if (roleExists == null)
                {
                    return StatusCode(400, "The ServiceOwner_RoleId does not exist");
                }

                _context.ServiceTable.Add(
                    new ServiceTable
                    {
                        ServiceId           = service.ServiceId,
                        ServiceName         = service.ServiceName,
                        ServiceDescription  = service.ServiceDescription,
                        ServiceOwner_RoleId = service.ServiceOwner_RoleId
                    }
                );

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, service);

            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // POST api/v1/service/tablecsv ----------------------------------------
        /// <summary>
        /// Replaces, updates or removes privs by csv upload
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

                csv.Configuration.RegisterClassMap<ServiceCsvDTOmap>();

                var services = csv.GetRecords<ServiceCsvDTO>();

                if (csvUpload.DeleteAndReplace == true)
                {
                    _context.ServiceTable.RemoveRange(_context.ServiceTable.ToList());

                    _context.SaveChanges();
                }

                foreach (ServiceCsvDTO service in services)
                {
                    var exists = _context.ServiceTable.AsNoTracking().FirstOrDefault(s => s.ServiceId == service.ServiceId);

                    if (csvUpload.DeleteAndReplace == false && exists != null)
                    {
                        _context.ServiceTable.Update(
                            new ServiceTable
                            {
                                ServiceId = service.ServiceId,
                                ServiceName = service.ServiceName,
                                ServiceDescription = service.ServiceDescription,
                                ServiceOwner_RoleId = service.ServiceOwner_RoleId
                            }
                        );
                    }
                    else
                    {
                        _context.ServiceTable.Add(
                            new ServiceTable
                            {
                                ServiceId = service.ServiceId,
                                ServiceName = service.ServiceName,
                                ServiceDescription = service.ServiceDescription,
                                ServiceOwner_RoleId = service.ServiceOwner_RoleId
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
                            547 => StatusCode(400, "Constraint check violation (a chosen ServiceOwner_RoleId probably doesn't exist, or a PrivTable entry relies on the ServiceId you are amending/deleting): " + sqlEx.Message.ToString()),

                            // Duplicated key row error / Constraint violation exception
                            2601 => StatusCode(400, "Duplicate key or constraint violation: " + sqlEx.Message.ToString()),

                            // Unique constraint error
                            2627 => StatusCode(400, "Unique constraint error (a chosen ServiceId probably already exists): " + sqlEx.Message.ToString()),

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



        // DELETE api/role/qualys ---------------------------------------------------
        /// <summary>
        /// Deletes a service if it has no dependencies in the database
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
        public async Task<IActionResult> DeleteService(string id)
        {
            try
            {
                var service = await _context.ServiceTable.FindAsync(id);

                if (service == null)
                {
                    return StatusCode(404);
                }

                _context.ServiceTable.Remove(service);
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
                            547 => StatusCode(400, "Constraint check violation (a PrivTable entry probably relies on the ServiceId you are removing): " + sqlEx.Message.ToString()),

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
