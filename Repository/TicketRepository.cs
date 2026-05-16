using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class TicketRepository
    {
        /// <summary>
        /// Lấy danh sách loại xe (bind combobox)
        /// </summary>
        public List<VehicleType> GetVehicleTypes()
        {
            ParkingManagementDbContext context = new ParkingManagementDbContext();
            return context.VehicleTypes
                      .OrderBy(vt => vt.TypeName)
                      .ToList();
        }

        /// <summary>
        /// Lấy danh sách vé đang gửi (Status = CheckedIn)
        /// </summary>
        public List<Ticket> GetActiveTickets()
        {
            ParkingManagementDbContext context = new ParkingManagementDbContext();

            return context.Tickets
                      .Include(t => t.VehicleType)
                      .Include(t => t.Vehicle)
                      .Where(t => t.Status == "CheckedIn")
                      .OrderByDescending(t => t.CheckInTime)
                      .ToList();
        }

        /// <summary>
        /// Check-in xe: tìm/ tạo Vehicle, sau đó tạo Ticket
        /// </summary>
        public bool CheckIn(string licensePlate, int vehicleTypeId, int userId)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                return false;

            licensePlate = licensePlate.Trim().ToUpper();

            ParkingManagementDbContext context = new ParkingManagementDbContext();

            try
            {
                // A. Lấy hoặc tạo Vehicle
                var vehicle = context.Vehicles
                                 .FirstOrDefault(v => v.LicensePlate == licensePlate);

                if (vehicle == null)
                {
                    vehicle = new Vehicle
                    {
                        LicensePlate = licensePlate,
                        VehicleTypeId = vehicleTypeId,
                        OwnerName = "Khách vãng lai"
                    };
                    context.Vehicles.Add(vehicle);
                    context.SaveChanges(); // để có VehicleId
                }

                // B. Xác định bãi xe (lấy bãi đầu tiên, hoặc 1 nếu rỗng)
                int parkingLotId = context.ParkingLots
                                      .Select(p => p.ParkingLotId)
                                      .FirstOrDefault();
                if (parkingLotId == 0)
                    parkingLotId = 1;

                // C. Tạo ticket mới
                var newTicket = new Ticket
                {
                    TicketCode = "T" + DateTime.Now.Ticks.ToString()[^8..], // 8 ký tự cuối
                    LicensePlate = licensePlate,
                    VehicleId = vehicle.VehicleId,
                    VehicleTypeId = vehicleTypeId,
                    ParkingLotId = parkingLotId,
                    UserId = userId,
                    CheckInTime = DateTime.Now,
                    Status = "CheckedIn",
                    IsMonthlyTicket = false
                };

                context.Tickets.Add(newTicket);
                context.SaveChanges();
                return true;
            }
            catch
            {
                // Có thể log Exception ở đây nếu cần
                return false;
            }
        }

        /// <summary>
        /// Check-out vé: cập nhật thời gian ra + tổng tiền
        /// </summary>
        public bool CheckOut(int ticketId, decimal totalAmount)
        {
            ParkingManagementDbContext context = new ParkingManagementDbContext();

            var ticket = context.Tickets.FirstOrDefault(t => t.TicketId == ticketId);
            if (ticket == null) return false;

            ticket.CheckOutTime = DateTime.Now;
            ticket.TotalAmount = totalAmount;
            ticket.Status = "CheckedOut";

            context.SaveChanges();
            return true;
        }
    }
}
