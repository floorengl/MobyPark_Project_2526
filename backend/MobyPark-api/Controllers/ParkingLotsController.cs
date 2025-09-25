using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobyPark_api.DTOs;
using MobyPark_api.Services;

namespace MobyPark_api.Controllers
{
    [ApiController]
    [Route("api/parking-lots")]
    public class ParkingLotsController : ControllerBase
    {
        private readonly IParkingLotService _svc;
        public ParkingLotsController(IParkingLotService svc) => _svc = svc;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParkingLotReadDto>>> List(CancellationToken ct) =>
            Ok(await _svc.ListAsync(ct));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ParkingLotReadDto>> Get(long id, CancellationToken ct)
        {
            var dto = await _svc.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ParkingLotReadDto>> Create([FromBody] CreateParkingLotDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateParkingLotDto dto, CancellationToken ct)
        {
            var ok = await _svc.UpdateAsync(id, dto, ct);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var ok = await _svc.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

    }
}
