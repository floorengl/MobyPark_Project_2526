using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MobyPark_api.Dtos.Reservation;
using MobyPark_api.Dtos.Vehicle;

namespace MobyPark_api.Controllers
{
    [Authorize]
    [Route("reservations")]
    public class ReservationController: ControllerBase
    {
        private readonly IReservationService _reser;
        public ReservationController(IReservationService reservationService) => _reser = reservationService;

        [HttpGet]
       // [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            ReadReservationDto[] dtos = await _reser.GetAll();
            if (dtos == null) 
                return NotFound();
            else
                return Ok(dtos);

        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> Get([FromRoute]string guid, CancellationToken ct)
        {
            ReadReservationDto? dto = await _reser.GetById(guid);
            if (dto == null)
                return NotFound();
            else
                return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WriteReservationDto dto, CancellationToken ct)
        {
            var allowed = await _reser.IsReservationAllowed(dto);
            if (!allowed.Item1)
            {
                return BadRequest(allowed.Item2);
            }
            ReadReservationDto? readDto = await _reser.Post(dto);
            if (readDto == null)
                return BadRequest();
            else
                return Ok(readDto);
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> Put([FromRoute]string guid, [FromBody] WriteReservationDto dto, CancellationToken ct)
        {
            var allowed = await _reser.IsReservationAllowed(dto);
            if (!allowed.Item1)
            {
                return BadRequest(allowed.Item2);
            }
            ReadReservationDto? readDto = await _reser.Put(guid, dto);
            if (readDto == null)
                return BadRequest();
            else
                return Ok(readDto);
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> Delete([FromRoute] string guid, CancellationToken ct)
        {
            ReadReservationDto? dto = await _reser.Delete(guid);
            if (dto == null)
                return NotFound();
            else
                return Ok(dto);
        }

        [HttpGet("For/{plate}/{time}")]
        public async Task<IActionResult> HasActiveReservation([FromRoute] string plate, [FromRoute] DateTime time)
        {
            ReadReservationDto? dto = await _reser.GetActiveReservation(plate, time);
            if (dto == null)
                return NotFound();
            else
                return Ok(dto);
        }
    }
}
