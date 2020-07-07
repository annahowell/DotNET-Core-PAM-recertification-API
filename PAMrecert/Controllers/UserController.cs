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
using Microsoft.AspNetCore.JsonPatch;
using Swashbuckle.AspNetCore.Annotations;
using CsvHelper;
using PAMrecert.Models;
using PAMrecert.DTOs.CsvUpload;
using PAMrecert.DTOs.UserController;
using PAMrecert.Services;

namespace PAMrecert.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/v1/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly PAML01Context _context;
        private readonly ControllerService _service;
        private ILogger<UserController> _logger { get; }

        public UserController(PAML01Context context, ControllerService service, ILogger<UserController> logger)
        {
            _context = context;
            _service = service;
            _logger  = logger;
        }



        // GET: api/v1/user ----------------------------------------------------
        /// <summary>
        /// Returns all users
        /// </summary>
        /// <returns>
        /// All users
        /// </returns> 
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(IEnumerable<UserDTO>))]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task <ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            try
            {
                ActionResult<IEnumerable<UserDTO>> result = await (
                    from u in _context.UserTable
                    orderby u.UserFullName ascending
                    select new UserDTO
                    {
                        UserId = u.UserId,
                        UserFullName = u.UserFullName,
                        RoleId = u.RoleId,
                        LastCertifiedBy = u.LastCertifiedBy,
                        LastCertifiedDate = u.LastCertifiedDate
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



        // GET: api/v1/user/jacok01 --------------------------------------------------
        /// <summary>
        /// Returns a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a user
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(UserDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task <ActionResult<UserDTO>> GetUser(string id)
        {
            try
            {
                ActionResult<UserDTO> result = await (
                    from u in _context.UserTable
                    where u.UserId == id
                    select new UserDTO
                    {
                        UserId = u.UserId,
                        UserFullName = u.UserFullName,
                        RoleId = u.RoleId,
                        LastCertifiedBy = u.LastCertifiedBy,
                        LastCertifiedDate = u.LastCertifiedDate
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



        // GET: api/v1/jacok01/ownedrolesusers ------------------------------------------
        /// <summary>
        /// Returns users assigned to roles that the user owns.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns users assigned to roles that the user owns.
        /// </returns>
        [HttpGet("{id}/ownedrolesusers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(UserRoleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<UserRoleDTO>>> GetUsersOwnedRolesUsers(string id)
        {
            try
            {
                ActionResult<IEnumerable<UserRoleDTO>> result = await (
                    from u in _context.UserTable
                    join r in _context.RoleTable on u.RoleId equals r.RoleId
                    where r.RoleOwner_RoleId == (from u in _context.UserTable where u.UserId == id select u.RoleId).First()
                    select new UserRoleDTO
                    {
                        UserId = u.UserId,
                        UserFullName = u.UserFullName,

                        RoleId = r.RoleId,
                        RoleName = r.RoleName
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



        // GET: api/v1/user/tablecsv -------------------------------------------
        /// <summary>
        /// Returns a csv of all current data in UserTable
        /// </summary>
        /// <returns>
        /// Returns a csv of all current data in UserTable
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
        public async Task<IActionResult> ExportUserTable()
        {
            try
            {
                List<UserDTO> result = await (
                    from u in _context.UserTable
                    select new UserDTO
                    {
                        UserId = u.UserId,
                        UserFullName = u.UserFullName,
                        RoleId = u.RoleId,
                        LastCertifiedBy = u.LastCertifiedBy,
                        LastCertifiedDate = u.LastCertifiedDate

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

                return File(stream, "application/octet-stream", "UserTable.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/user/deltacsv?baseDateTime=2020-03-24T14:33:03&deltaDateTime=2019-01-19T14:33:03
        /// <summary>
        /// Returns a delta of all recertification data for all users and their roles by datetimes passed as varables in the URL
        /// </summary>
        /// <param name="baseDateTime">The base datetime to use when calculating the delta e.g. 2020-03-24T14:33:03</param>
        /// <param name="deltaDateTime">The delta datetime to use when canculating the delta e.g. 2019-01-19T14:33:03 </param>
        /// <returns>
        /// Returns a delta of all recertification data for all users and their roles by datetimes passed as varables in the URL
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

                List<UserRoleAllDTO> result;
                List<UserRoleAllDTO> baseResult;
                List<UserRoleAllDTO> deltaResult;

                baseResult = await _service.GetUserRoleAllDTObyDateTime(convertedBaseDateTime);
                deltaResult = await _service.GetUserRoleAllDTObyDateTime(convertedDeltaDateTime);

                if (baseResult == null || !baseResult.Any() || deltaResult == null || !deltaResult.Any())
                {
                    return StatusCode(204);
                }

                // We've returned everything using our service, so now get the delta of rows
                result = baseResult.Except(deltaResult).ToList();

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

                return File(stream, "application/octet-stream", "User-Recertification-Data-Delta.csv");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(503);
            }
        }



        // POST: api/v1/user ---------------------------------------------------
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// The newly created user
        /// </returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [SwaggerResponse(201, Type = typeof(UserDTO))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(409, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task <ActionResult<UserDTO>> AddUser(UserPostDTO user)
        {
            try
            {
                var userExists = await _context.UserTable.FindAsync(user.UserId);

                if (userExists != null)
                {
                    return StatusCode(409, "The UserId already exists");
                }

                var roleExists = await _context.RoleTable.FindAsync(user.RoleId);

                if (roleExists == null)
                {
                    return StatusCode(400, "The RoleId does not exist");
                }

                _context.UserTable.Add(
                    new UserTable
                    {
                        UserId            = user.UserId,
                        UserFullName      = user.UserFullName,
                        RoleId            = user.RoleId
                    }
                );

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }



        // PATCH: api/v1/user/jacok01 ------------------------------------------------
        /// <summary>
        /// Updates an existing user by UserId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns>
        /// Updates an existing user by UserId
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
        public async Task<IActionResult> UpdateRole(string id, [FromBody]JsonPatchDocument<UserTable> patch)
        {
            try
            {
                var row = await _context.UserTable.Where(u => u.UserId == id).FirstOrDefaultAsync();

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



        // POST api/v1/user/tablecsv -------------------------------------------
        /// <summary>
        /// Replaces, updates or removes privs by csv upload
        /// </summary>
        /// <param name="csvUpload"></param>
        [HttpPost("tablecsv"), DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [SwaggerResponse(201, Type = typeof(String))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
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

                csv.Configuration.RegisterClassMap<UserCsvDTOmap>();

                var users = csv.GetRecords<UserCsvDTO>();

                if (csvUpload.DeleteAndReplace == true)
                {
                    _context.UserTable.RemoveRange(_context.UserTable.ToList());

                    _context.SaveChanges();
                }

                foreach (UserCsvDTO user in users)
                {
                    var exists = _context.UserTable.AsNoTracking().FirstOrDefault(u => u.UserId == user.UserId);

                    if (csvUpload.DeleteAndReplace == false && exists != null)
                    {
                        _context.UserTable.Update(
                            new UserTable
                            {
                                UserId = user.UserId,
                                UserFullName = user.UserFullName,
                                RoleId = user.RoleId,
                                LastCertifiedBy = user.LastCertifiedBy,
                                LastCertifiedDate = user.LastCertifiedDate
                            }
                        );
                    }
                    else
                    {
                        _context.UserTable.Add(
                            new UserTable
                            {
                                UserId = user.UserId,
                                UserFullName = user.UserFullName,
                                RoleId = user.RoleId,
                                LastCertifiedBy = user.LastCertifiedBy,
                                LastCertifiedDate = user.LastCertifiedDate
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
                            547 => StatusCode(400, "Constraint check violation (a chosen RoleId probably doesn't exist): " + sqlEx.Message.ToString()),

                            // Duplicated key row error / Constraint violation exception
                            2601 => StatusCode(400, "Duplicate key or constraint violation: " + sqlEx.Message.ToString()),

                            // Unique constraint error
                            2627 => StatusCode(400, "Unique constraint error (a chosen UserId probably already exists): " + sqlEx.Message.ToString()),

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



        // DELETE api/user/jacok01 ---------------------------------------------------
        /// <summary>
        /// Deletes a user if it has no dependencies in the database (it shouldn't)
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
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _context.UserTable.FindAsync(id);

                if (user == null)
                {
                    return StatusCode(404);
                }

                _context.UserTable.Remove(user);
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
                            547 => StatusCode(400, "Constraint check violation (something relies on the UserId you are removing): " + sqlEx.Message.ToString()),

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