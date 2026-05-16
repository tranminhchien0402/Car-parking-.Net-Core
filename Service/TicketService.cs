using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class TicketService
    {
        private readonly MonthlyTicketRepository monthlyTicketRepo = new MonthlyTicketRepository();

        public string CheckInVehicle(string plate, int vehicleTypeId, int userId)
        {
            plate = plate.Trim().ToUpper();

            using var db = new ParkingManagementDbContext();

            // 1. Tìm hoặc tạo Vehicle
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == plate);
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    LicensePlate = plate,
                    VehicleTypeId = vehicleTypeId
                };
                db.Vehicles.Add(vehicle);
                db.SaveChanges();
            }
            else
            {
                // cập nhật loại xe nếu cần
                if (vehicle.VehicleTypeId != vehicleTypeId)
                {
                    vehicle.VehicleTypeId = vehicleTypeId;
                    db.SaveChanges();
                }
            }

            // 2. Kiểm tra còn vé tháng hay không
            var activeMonthly = monthlyTicketRepo.GetActiveByVehicleId(vehicle.VehicleId);
            bool isMonthly = activeMonthly != null;

            // 3. Kiểm tra xe đã ở trong bãi chưa (tránh check-in trùng)
            bool isInPark = db.Tickets.Any(t =>
                t.LicensePlate == plate &&
                t.CheckOutTime == null);

            if (isInPark)
            {
                return "Xe này đang ở trong bãi, không thể check-in lần nữa.";
            }

            // 4. Tạo ticket
            var ticket = new Ticket
            {
                TicketCode = Guid.NewGuid().ToString("N").Substring(0, 8),
                VehicleId = vehicle.VehicleId,
                VehicleTypeId = vehicleTypeId,
                LicensePlate = plate,
                ParkingLotId = db.ParkingLots.First().ParkingLotId, // nếu chỉ có 1 bãi
                UserId = userId,
                CheckInTime = DateTime.Now,
                IsMonthlyTicket = isMonthly,
                // TotalAmount để null, sẽ set lúc checkout
            };

            db.Tickets.Add(ticket);
            db.SaveChanges();

            if (isMonthly)
            {
                return $"Xe {plate} đã check-in (dùng vé tháng, KHÔNG tính tiền).";
            }

            return $"Xe {plate} đã check-in thành công.";
        }
        public string CheckOutVehicle(Ticket uiTicket)
        {
            using var db = new ParkingManagementDbContext();

            // Lấy vé từ DB theo ID 
            var ticket = db.Tickets
                           .Include(t => t.Vehicle)
                           .Include(t => t.VehicleType)
                           .FirstOrDefault(t => t.TicketId == uiTicket.TicketId);

            if (ticket == null)
                return "Không tìm thấy vé trong hệ thống.";

            if (ticket.CheckOutTime != null || ticket.Status == "CheckedOut")
                return "Vé này đã được thanh toán / check-out trước đó.";

            // ===== 1. KIỂM TRA VÉ THÁNG CÒN HẠN CHO XE NÀY =====
            bool hasActiveMonthly = false;

            if (ticket.VehicleId != 0)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                hasActiveMonthly = db.MonthlyTickets.Any(mt =>
                    mt.VehicleId == ticket.VehicleId &&
                    mt.IsActive == true &&
                    mt.StartDate <= today &&
                    mt.EndDate >= today
                );
            }

            // ===== 2. TÍNH TIỀN =====
            decimal amount = 0m;

            if (!hasActiveMonthly)
            {
                // TÍNH TIỀN VÉ LƯỢT 
                var checkIn = ticket.CheckInTime;
                var checkOut = DateTime.Now;

                var hours = (checkOut - checkIn).TotalHours;
                if (hours < 1) hours = 1; // tối thiểu 1 giờ

                // dùng HourlyRate 
                amount = (decimal)Math.Ceiling(hours) * ticket.VehicleType.HourlyRate;
            }
            else
            {
                // Có vé tháng: không tính tiền
                amount = 0m;
                ticket.IsMonthlyTicket = true;   // đánh dấu vé này đi bằng vé tháng 
            }

            // ===== 3. CẬP NHẬT VÉ =====
            ticket.CheckOutTime = DateTime.Now;
            ticket.TotalAmount = amount;
            ticket.Status = "CheckedOut";

            db.SaveChanges();

            // ===== 4. TRẢ LỜI CHO UI =====
            if (hasActiveMonthly)
            {
                return $"Check-out thành công. Xe đang dùng VÉ THÁNG nên không tính tiền.";
            }
            else
            {
                return $"Check-out thành công. Số tiền phải thu: {amount:N0} VND";
            }
        }
    }
}
