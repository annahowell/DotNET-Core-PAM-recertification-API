using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;
using Swashbuckle.AspNetCore.Annotations;
using PAMrecert.Models;
using PAMrecert.DTOs.RecertCycleController;
using PAMrecert.Services;

namespace PAMrecert.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/v1/recertcycle")]
    [ApiController]
    public class RecertCycleController : ControllerBase
    {
        private readonly PAML01Context _context;
        private readonly ControllerService _service;
        private ILogger<RecertCycleController> _logger { get; }

        public RecertCycleController(PAML01Context context, ControllerService service, ILogger<RecertCycleController> logger)
        {
            _context = context;
            _service = service;
            _logger = logger;

            if (!_context.RecertCycleTable.Any())
            {
                _context.RecertCycleTable.Add(
                    new RecertCycleTable
                    {
                        RecertCycleTitle = "Initial cycle",
                        RecertStartedDate = DateTime.Now,
                        RecertEnabled = false
                    }
                );
                _context.SaveChanges();
            }
        }



        // GET: api/v1/recertcycle ---------------------------------------------
        /// <summary>
        /// Returns all recert cycles
        /// </summary>
        /// <returns>
        /// All recert cycles
        /// </returns> 
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        
        [SwaggerResponse(200, Type = typeof(IEnumerable<RecertCycleDTO>))]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<IEnumerable<RecertCycleDTO>>> GetRecertCycles()
        {
            try
            {
                ActionResult<IEnumerable<RecertCycleDTO>> result = await (
                    from rc in _context.RecertCycleTable
                    orderby rc.RecertStartedDate descending
                    select new RecertCycleDTO
                    {
                        RecertCycleId     = rc.RecertCycleId,
                        RecertCycleTitle  = rc.RecertCycleTitle,
                        RecertStartedDate = rc.RecertStartedDate,
                        RecertEndedDate   = rc.RecertEndedDate,
                        RecertEnabled     = rc.RecertEnabled,
                        RecertCount       = _context.RecertCycleTable.Count()
                    }
                ).ToListAsync();

                if (result == null || !result.Value.Any())
                {
                    return StatusCode(204); // this shouldn't happen because Initial cycle should be seeded
                }

                return StatusCode(200, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }



        // GET: api/v1/recertcycle/5 -------------------------------------------
        /// <summary>
        /// Returns a specific recert cycle by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a specific recert cycle by id
        /// </returns> 
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(RecertCycleDTO))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RecertCycleDTO>> GetRecertCycle(int id)
        {
            try
            {
                if (id < 0)
                {
                    return StatusCode(400, "id is invalid");
                }

                ActionResult<RecertCycleDTO> result = await (
                    from rc in _context.RecertCycleTable
                    where rc.RecertCycleId == id
                    select new RecertCycleDTO
                    {
                        RecertCycleId = rc.RecertCycleId,
                        RecertCycleTitle = rc.RecertCycleTitle,
                        RecertStartedDate = rc.RecertStartedDate,
                        RecertEndedDate = rc.RecertEndedDate,
                        RecertEnabled = rc.RecertEnabled,
                        RecertCount = _context.RecertCycleTable.Count()
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



        // GET: api/v1/recertcycle/latestbut/1 ---------------------------------
        /// <summary>
        /// Returns the most recently started recert cycle minus the offset. So 'latestbut/0' will 
        /// return the very latest recert cycle, and 'latestbut/1' will return the second to latest
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>
        /// Returns the most recently started recert cycle minus the offset. So 'latestbut/0' will 
        /// return the very latest recert cycle, and 'latestbut/1' will return the second to latest
        /// </returns> 
        [HttpGet("latestbut/{offset}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        [SwaggerResponse(200, Type = typeof(RecertCycleDTO))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<ActionResult<RecertCycleDTO>> GetMostRecentRecertCycle(int offset)
        {
            try
            {
                if (offset < 0) // MVC will auto check existence and the type for us
                {
                    return StatusCode(400, "A valid offset must be included");
                }

                ActionResult<RecertCycleDTO> result = await (
                    from rc in _context.RecertCycleTable
                    orderby rc.RecertCycleId descending
                    select new RecertCycleDTO
                    {
                        RecertCycleId = rc.RecertCycleId,
                        RecertCycleTitle = rc.RecertCycleTitle,
                        RecertStartedDate = rc.RecertStartedDate,
                        RecertEndedDate = rc.RecertEndedDate,
                        RecertEnabled = rc.RecertEnabled,
                        RecertCount = _context.RecertCycleTable.Count()
                    }
                ).Skip(offset).FirstOrDefaultAsync();

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



        // POST api/v1/recertcycle ---------------------------------------------
        /// <summary>
        /// Creates a new recert cycle and then sets the end date of the previous cycle
        /// </summary>
        /// <param name="newRecertCycle"></param>
        /// <returns>
        /// The newly created recert cycle
        /// </returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [SwaggerResponse(201, Type = typeof(RecertCycleDTO))]
        [SwaggerResponse(400, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public ActionResult<RecertCycleDTO> AddRecertCycle(RecertCyclePostDTO newRecertCycle)
        {
            try
            {
                if (!_context.RoleTable.Any())
                {
                    return StatusCode(400, "Unable to begin a new recertification cycle: RoleTable in the database is not populated. Please go to the upload .csv page and upload a .csv of RoleTable data.");
                }
                else if (!_context.ServiceTable.Any())
                {
                    return StatusCode(400, "Unable to begin a new recertification cycle: ServiceTable in the database is not populated. Please go to the upload .csv page and upload a .csv of ServiceTable data.");
                }
                else if (!_context.PrivTable.Any())
                {
                    return StatusCode(400, "Unable to begin a new recertification cycle: PrivTable in the database is not populated. Please go to the upload .csv page and upload a .csv of PrivTable data.");
                }
                else if (!_context.RolePrivLink.Any())
                {
                    return StatusCode(400, "Unable to begin a new recertification cycle: RolePrivLink in the database is not populated. Please go to the upload .csv page and upload a .csv of RolePrivLink data.");
                }
                else if (!_context.UserTable.Any())
                {
                    return StatusCode(400, "Unable to begin a new recertification cycle: UserTable in the database is not populated. Please go to the upload .csv page and upload a .csv of UserTable data.");
                }

                // Set the end date of the soon to be old recert cycle, this effectively locks out changes
                // since posts/patches etc check the current cycle end date is null before making amendments
                var currentRecertCycle = _context.RecertCycleTable.OrderByDescending(rc => rc.RecertCycleId).FirstOrDefault();

                if (currentRecertCycle != null)
                {
                    currentRecertCycle.RecertEndedDate = DateTime.Now;

                    _context.SaveChanges();
                }

                // Now set the certified states for role service privs of service owners and role owner to false
                var exists = _context.RolePrivLink.ToList();

                if (exists != null)
                {
                    foreach (RolePrivLink rp in exists)
                    {
                        rp.RoleOwner_IsCertified = false;
                        rp.ServiceOwner_IsCertified = false;
                        rp.RiskIsAssessed = false;
                    }
                }

                _context.SaveChanges();

                // Now we'll make the new cycle. If we ever do a lookup of data at the start of this
                // cycle it will start off with blank recert/risk data which is what we want
                _context.RecertCycleTable.Add(
                    new RecertCycleTable
                    {
                        RecertCycleTitle = newRecertCycle.RecertCycleTitle,
                        RecertStartedDate = DateTime.Now,
                        RecertEnabled = newRecertCycle.RecertEnabled
                    }
                );
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }

            return CreatedAtAction(nameof(GetMostRecentRecertCycle), new { offset = 0 }, newRecertCycle);
        }



        // PATCH: api/v1/recertcycle/5 -----------------------------------------
        /// <summary>
        /// Updates a RecertCycle via the RecertCycleId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns>
        /// Updates a RecertCycle via the RecertCycleId
        /// </returns>
        [HttpPatch("{id}")]
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
        public async Task<IActionResult> UpdateRecertCycleServicePriv(int id, [FromBody]JsonPatchDocument<RecertCycleTable> patch)
        {
            try
            {
                var row = await _context.RecertCycleTable.Where(rc => rc.RecertCycleId == id).FirstOrDefaultAsync();

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



        // DELETE api/v1/recertcycle/5 -----------------------------------------
        /// <summary>
        /// Deletes a RecertCycle
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerResponse(204, Type = typeof(String))]
        [SwaggerResponse(401, Type = typeof(String))]
        [SwaggerResponse(404, Type = typeof(String))]
        [SwaggerResponse(500, Type = typeof(String))]
        public async Task<IActionResult> DeleteRecertCycle(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return StatusCode(400, "id is invalid");
                }

                var RecertCycle = await _context.RecertCycleTable.FindAsync(id);

                if (RecertCycle == null)
                {
                    return StatusCode(404);
                }

                _context.RecertCycleTable.Remove(RecertCycle);
                await _context.SaveChangesAsync();

                return StatusCode(204);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return StatusCode(500);
            }
        }
    }
}
