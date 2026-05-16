using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class VehicleRepository
    {
        // Lấy xe theo biển số
        public Vehicle? GetByLicensePlate(string licensePlate)
        {
            using var context = new ParkingManagementDbContext();
            return context.Vehicles
                      .Include(v => v.VehicleType)
                      .FirstOrDefault(v => v.LicensePlate == licensePlate);
        }

        // Thêm mới xe
        public Vehicle Add(Vehicle vehicle)
        {
            using var context = new ParkingManagementDbContext();
            context.Vehicles.Add(vehicle);
            context.SaveChanges();
            return vehicle; // lúc này VehicleId đã có
        }

        // Cập nhật thông tin chủ xe + loại xe
        public void UpdateOwnerAndType(int vehicleId, string? ownerName, string? ownerPhone, int vehicleTypeId)
        {
            using var context = new ParkingManagementDbContext();
            var v = context.Vehicles.FirstOrDefault(x => x.VehicleId == vehicleId);
            if (v == null) return;

            v.OwnerName = ownerName;
            v.OwnerPhone = ownerPhone;
            v.VehicleTypeId = vehicleTypeId;

            context.SaveChanges();
        }

        // Lấy danh sách loại xe (để bind combobox)
        public List<VehicleType> GetVehicleTypes()
        {
            using var context = new ParkingManagementDbContext();
            return context.VehicleTypes.OrderBy(vt => vt.TypeName).ToList();
        }
        public List<Vehicle> GetByPhone(string phone)
        {
            using (var context = new ParkingManagementDbContext())
            {
                return context.Vehicles
                              .Where(v => v.OwnerPhone == phone)
                              .ToList();
            }
        }
    }
}
