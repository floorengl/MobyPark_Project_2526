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
        private ReservationController(IReservationService reservationService) => _reser = reservationService;

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            ReadReservationDto[] dtos = await _reser.GetAll();
            if (dtos == null) 
                return NotFound();
            else
                return Ok(dtos);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id, CancellationToken ct)
        {
            ReadReservationDto? dto = await _reser.GetById(id);
            if (dto == null)
                return NotFound();
            else
                return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WriteReservationDto dto, CancellationToken ct)
        {
            ReadReservationDto? readDto = await _reser.Post(dto);
            if (readDto == null)
                return BadRequest();
            else
                return Ok(readDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromHeader] long id, [FromBody] WriteReservationDto dto, CancellationToken ct)
        {
            ReadReservationDto? readDto = await _reser.Put(id, dto);
            if (readDto == null)
                return BadRequest();
            else
                return Ok(readDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            ReadReservationDto? dto = await _reser.Delete(id);
            if (dto == null)
                return NotFound();
            else
                return Ok(dto);
        }


    }
}
