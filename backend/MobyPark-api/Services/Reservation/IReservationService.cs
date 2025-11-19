using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Reservation;


public interface IReservationService
{
    public Task<ReadReservationDto[]> GetAll();
    public Task<ReadReservationDto?> GetById(long id);
    public Task<ReadReservationDto?> Post(WriteReservationDto dto);
    public Task<ReadReservationDto?> Put(long id, WriteReservationDto dto);
    public Task<ReadReservationDto?> Delete(long id);
    public ReadReservationDto? ReservationToReadDto(Reservation? reservation);
}
