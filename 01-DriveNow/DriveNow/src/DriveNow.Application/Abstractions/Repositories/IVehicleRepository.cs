using DriveNow.Domain.Entities;

namespace DriveNow.Application.Abstractions.Repositories;

public interface IVehicleRepository
{
    Vehicle? GetById(int id);
    List<Vehicle> GetAll();
    void Update(Vehicle vehicle);
}