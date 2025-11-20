using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;
using MobyPark_api.Dtos.Vehicle;
using MobyPark_api.Services.VehicleService;

namespace MobyPark_api.Controllers
{
    [ApiController]
    [Route("vehicles")]
    [Authorize]
    public sealed class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vh;

        public VehicleController(IVehicleService vh) => _vh = vh;

        /// <summary>
        /// Retrieves all vehicles belonging to the currently authenticated user.
        /// </summary>
        /// <returns>List of vehicles owned by the logged-in user.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetUserVehiclesAsync()
        {
            var userId = long.Parse(User.FindFirst("id")?.Value ?? "0");
            var vehicles = await _vh.GetUserVehiclesAsync(userId);
            return Ok(vehicles);
        }

        /// <summary>
        /// Retrieves all vehicles associated with a specific username.
        /// </summary>
        /// <remarks>Only accessible to users with the ADMIN role.</remarks>
        /// <param name="username">The username of the account.</param>
        /// <returns>List of vehicles belonging to the specified user.</returns>
        [HttpGet("by-username/{username}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehiclesByUsernameAsync(string username)
        {
            var vehicles = await _vh.GetVehiclesByUsernameAsync(username);
            return Ok(vehicles);
        }

        /// <summary>
        /// Retrieves details of a specific vehicle by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle.</param>
        /// <returns>The requested vehicle details if found.</returns>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehicleDto?>> GetByIdAsync(long id)
        {
            var userId = long.Parse(User.FindFirst("id")?.Value ?? "0");
            var vehicle = await _vh.GetByIdAsync(id, userId);
            if (vehicle is null) return NotFound();
            return Ok(vehicle);
        }

        /// <summary>
        /// Creates a new vehicle for the authenticated user.
        /// </summary>
        /// <param name="dto">Vehicle information to create.</param>
        /// <returns>The created vehicle details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VehicleDto>> CreateAsync([FromBody] VehicleDto dto)
        {
            var userId = long.Parse(User.FindFirst("id")?.Value ?? "0");
            var created = await _vh.CreateAsync(dto, userId);
            if (created is null) 
                return StatusCode(422, "license plate already exists");
            return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing vehicle belonging to the logged-in user.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle.</param>
        /// <param name="dto">Updated vehicle details.</param>
        /// <returns>The updated vehicle data if successful.</returns>
        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehicleDto>> UpdateAsync(long id, [FromBody] VehicleDto dto)
        {
            var userId = long.Parse(User.FindFirst("id")?.Value ?? "0");
            var updated = await _vh.UpdateAsync(id, dto, userId);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a specific vehicle belonging to the logged-in user.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle to delete.</param>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var userId = long.Parse(User.FindFirst("id")?.Value ?? "0");
            var deleted = await _vh.DeleteAsync(id, userId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
