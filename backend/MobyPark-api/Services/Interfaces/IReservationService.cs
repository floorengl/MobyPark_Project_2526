using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Reservation;


public interface IReservationService
{
    public Task<ReadReservationDto[]> GetAll();
    public Task<ReadReservationDto?> GetById(string guid);
    public Task<List<ReadReservationDto>?> Post(WriteMultiReservationDto dto);
    public Task<ReadReservationDto?> Post(WriteReservationDto dto);
    public Task<ReadReservationDto?> Put(string guid, WriteReservationDto dto);
    public Task<ReadReservationDto?> Delete(string guid);
    public ReadReservationDto? ReservationToReadDto(Reservation? reservation);
    public Task<(bool, string)> IsReservationAllowed(WriteMultiReservationDto dto);
    public Task<(bool, string)> IsReservationAllowed(WriteReservationDto reservation);
    public Task<ReadReservationDto?> GetActiveReservation(string licensePlate, DateTime time);
    public Task<bool> WillParkingLotOverflow(DateTime start, DateTime end, long parkingLotId, int baseLoad = 0);
    public Task ConsumeReservation(string guid);

}
