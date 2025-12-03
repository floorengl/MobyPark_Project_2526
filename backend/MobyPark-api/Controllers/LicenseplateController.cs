using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MobyPark_api.Dtos;

[ApiController]
[Route("licenseplate")]
public sealed class LicenseplateController : ControllerBase
{
    private readonly ILicenseplateService _license;
    public LicenseplateController(ILicenseplateService license) => _license = license;

    // POST /licenseplate
    [HttpPost]
    public async Task<IActionResult> Licenseplates([FromBody] CheckInDto dto, CancellationToken cto)
    {
        var result = await _license.LicenseplatesAsync(dto, cto);
        if (result.Item1 == 0)
            return BadRequest(result.Item2);
        return StatusCode(201);
    }

    // DELETE /licenseplate
    [HttpDelete("{plate}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(string plate, CancellationToken ct)
    {
        await _license.DeleteAsync(plate, ct);
        return NoContent();
    }

    // GET ALL /licenseplate
    [HttpGet("")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<IEnumerable<LicenseplateDto>>> GetAll(CancellationToken ct)
    {
        var list = await _license.GetAllAsync(ct);
        return Ok(list);
    }

    // GET ONE /licenseplate/{plate}
    [HttpGet("{plate}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<LicenseplateDto>> GetOne(string plate, CancellationToken ct)
    {
        var item = await _license.GetByPlateAsync(plate, ct);
        return item is null ? NotFound() : Ok(item);
    }
}
