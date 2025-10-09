using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("")]
public sealed class LicenseplateController : ControllerBase
{
    private readonly ILicenseplateService _license;
    public LicenseplateController(ILicenseplateService license) => _license = license;

    [HttpPost("licenseplate")]
    public async Task<IActionResult> Licenseplates([FromBody] LicenseplateDto dto, CancellationToken cto)
    {
        var id = await _license.LicenseplatesAsync(dto, cto);
        return StatusCode(201); // or Created(...)
    }

    [HttpDelete("licenseplate/{plate}")]
    public async Task<IActionResult> Delete(string plate, CancellationToken ct)
    {
        await _license.DeleteAsync(plate, ct);
        return NoContent();
    }
}
