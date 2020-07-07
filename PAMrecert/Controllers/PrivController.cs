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
using CsvHelper;
using PAMrecert.Models;
using PAMrecert.DTOs.CsvUpload;
using PAMrecert.DTOs.PrivController;
using PAMrecert.Services;
using Microsoft.AspNetCore.JsonPatch;

namespace PAMrecert.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/v1/priv")]
    [ApiController]
    public class PrivController : ControllerBase
    {
        private readonly PAML01Context _context;
        private readonly ControllerService _service;
        private ILogger<PrivController> _logger { get; }

        public PrivController(PAML01Context context, ControllerService service, ILogger<PrivController> logger)
        {
            _context = context;
            _service = service;
            _logger = logger;
        }



        // GET: api/v1/priv ----------------------------------------------------
        /// <summary>
        /// Returns all privileges
        /// </summary>
        /// <returns>
        /// Returns all privileges
        /// </returns> 
        // GET: api/priv
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(IEnumerable<PrivDTO>))]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<PrivDTO>>> GetPrivs()
        {
            try
            {
                ActionResult<IEnumerable<PrivDTO>> result = await (
                    from p in _context.PrivTable
                    orderby p.PermissionGroup ascending
                    select new PrivDTO
                    {
                        PrivId = p.PrivId,
                        ServiceId = p.ServiceId,
                        PermissionGroup = p.PermissionGroup,
                        ServicePrivSummary = p.ServicePrivSummary,
                        CredentialStorageMethod = p.CredentialStorageMethod
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



        // GET: api/v1/priv/qualysGuest --------------------------------------------------
        /// <summary>
        /// Returns a privilege
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a privilege
        /// </returns> 
        // GET: api/priv/5
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(PrivDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<PrivDTO>> GetPriv(string id)
        {
            try
            {
                ActionResult<PrivDTO> result = await (
                    from p in _context.PrivTable
                    where p.PrivId == id
                    select new PrivDTO
                    {
                        PrivId = p.PrivId,
                        ServiceId = p.ServiceId,
                        PermissionGroup = p.PermissionGroup,
                        ServicePrivSummary = p.ServicePrivSummary,
                        CredentialStorageMethod = p.CredentialStorageMethod
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



        // GET: api/v1/priv/tablecsv  ------------------------------------------
        /// <summary>
        /// Returns a csv of all current data in PrivTable
        /// </summary>
        /// <returns>
        /// Returns a csv of all current data in PrivTable
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
        public async Task<IActionResult> ExportPrivTable()
        {
            try
            {
                List<PrivCsvDTO> result = await (
                    from p in _context.PrivTable
                    select new PrivCsvDTO
                    {
                        PrivId = p.PrivId,
                        ServiceId = p.ServiceId,
                        PermissionGroup = p.PermissionGroup,
                        ServicePrivSummary = p.ServicePrivSummary,
                        CredentialStorageMethod = p.CredentialStorageMethod
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

                return File(stream, "application/octet-stream", "PrivTable.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // PATCH: api/v1/priv/qualysGuest ------------------------------------------------
        /// <summary>
        /// Updates an existing priv
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns>
        /// Updates an existing priv
        /// </returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(204)] // No content
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)] // Unauthorised
        [ProducesResponseType(404)] // Not found
        [ProducesResponseType(500)] // Internal server error
        [SwaggerResponse(204, Type = typeof(PrivTable))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> UpdatePriv(string id, [FromBody]JsonPatchDocument<PrivTable> patch)
        {
            try
            {
                var row = await _context.PrivTable.Where(u => u.PrivId == id).FirstOrDefaultAsync();

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



        // POST api/v1/priv ----------------------------------------------------
        /// <summary>
        /// Creates a new privilege
        /// </summary>
        /// <param name="priv"></param>
        /// <returns>
        /// A newly created privilege
        /// </returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [SwaggerResponse(201, Type = typeof(PrivDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(409, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<PrivDTO>> AddPriv(PrivPostDTO priv)
        {
            try
            {
                var privExists = await _context.PrivTable.FindAsync(priv.PrivId);

                if (privExists != null)
                {
                    return StatusCode(409, "The PrivId already exists"); // Conflict
                }

                var serviceExists = await _context.ServiceTable.FindAsync(priv.ServiceId);

                if (serviceExists == null)
                {
                    return StatusCode(400, "The ServiceId does not exist");
                }

                _context.PrivTable.Add(
                    new PrivTable
                    {
                        PrivId = priv.PrivId,
                        ServiceId = priv.ServiceId,
                        PermissionGroup = priv.PermissionGroup,
                        ServicePrivSummary = priv.ServicePrivSummary,
                        CredentialStorageMethod = priv.CredentialStorageMethod
                    }
                );

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPriv), new { id = priv.PrivId }, priv);

            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // POST api/v1/priv/tablecsv ------------------------------------------------
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

                csv.Configuration.RegisterClassMap<PrivCsvDTOmap>();

                var privs = csv.GetRecords<PrivCsvDTO>();

                if (csvUpload.DeleteAndReplace == true)
                {
                    _context.PrivTable.RemoveRange(_context.PrivTable.ToList());

                    _context.SaveChanges();
                }

                foreach (PrivCsvDTO priv in privs)
                {
                    var exists = _context.PrivTable.AsNoTracking().FirstOrDefault(p => p.PrivId == priv.PrivId);

                    if (csvUpload.DeleteAndReplace == false && exists != null)
                    {
                        _context.PrivTable.Update(
                            new PrivTable
                            {
                                PrivId = priv.PrivId,
                                ServiceId = priv.ServiceId,
                                PermissionGroup = priv.PermissionGroup,
                                ServicePrivSummary = priv.ServicePrivSummary,
                                CredentialStorageMethod = priv.CredentialStorageMethod
                            }
                        );
                    }
                    else
                    {
                        _context.PrivTable.Add(
                            new PrivTable
                            {
                                PrivId = priv.PrivId,
                                ServiceId = priv.ServiceId,
                                PermissionGroup = priv.PermissionGroup,
                                ServicePrivSummary = priv.ServicePrivSummary,
                                CredentialStorageMethod = priv.CredentialStorageMethod
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
                            547 => StatusCode(400, "Constraint check violation (a chosen ServiceId probably doesn't exist, or a RolePrivLink entry relies on the PrivId you are amending/removing): " + sqlEx.Message.ToString()),

                            // Duplicated key row error / Constraint violation exception
                            2601 => StatusCode(400, "Duplicate key or constraint violation: " + sqlEx.Message.ToString()),

                            // Unique constraint error
                            2627 => StatusCode(400, "Unique constraint error (a chosen PrivId probably already exists): " + sqlEx.Message.ToString()),

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



        // DELETE api/priv/qualysGuest ---------------------------------------------------
        /// <summary>
        /// Deletes a priv if it has no dependencies in the database
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
        public async Task<IActionResult> DeletePriv(string id)
        {
            try
            {
                var priv = await _context.PrivTable.FindAsync(id);

                if (priv == null)
                {
                    return StatusCode(404);
                }

                _context.PrivTable.Remove(priv);
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
                            547 => StatusCode(400, "Constraint check violation (a RolePrivLink entry probably relies on the PrivId you are removing): " + sqlEx.Message.ToString()),

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
