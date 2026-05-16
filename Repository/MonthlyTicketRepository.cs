using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class MonthlyTicketRepository
    {
        // Lấy vé tháng active theo VehicleId
        public MonthlyTicket? GetActiveByVehicleId(int vehicleId)
        {
            using var context = new ParkingManagementDbContext();

            var today = DateOnly.FromDateTime(DateTime.Today);

            return context.MonthlyTickets
                     .FirstOrDefault(m =>
                         m.VehicleId == vehicleId &&
                         m.IsActive == true &&
                         m.StartDate <= today &&
                         m.EndDate >= today);
        }

        // Tạo vé tháng mới
        public MonthlyTicket Add(MonthlyTicket ticket)
        {
            using var context = new ParkingManagementDbContext();
            context.MonthlyTickets.Add(ticket);
            context.SaveChanges();
            return ticket;
        }

        // Cập nhật ngày kết thúc + giá (khi gia hạn)
        public void UpdateDatesAndPrice(int monthlyTicketId, DateOnly newEndDate, decimal newPrice)
        {
            using var context = new ParkingManagementDbContext();
            var t = context.MonthlyTickets.FirstOrDefault(x => x.MonthlyTicketId == monthlyTicketId);
            if (t == null) return;

            t.EndDate = newEndDate;        // EndDate là DateOnly
            t.Price = newPrice;

            context.SaveChanges();
        }


        // Hủy hiệu lực vé tháng
        public void Deactivate(int monthlyTicketId)
        {
            using var context = new ParkingManagementDbContext();
            var t = context.MonthlyTickets.FirstOrDefault(x => x.MonthlyTicketId == monthlyTicketId);
            if (t == null) return;

            t.IsActive = false;
            context.SaveChanges();
        }

        // Tìm list theo biển số (để hiển thị bảng)
        public List<(Vehicle vehicle, MonthlyTicket? ticket, VehicleType type)> SearchByPlate(string keyword)
        {
            using var context = new ParkingManagementDbContext();

            keyword = keyword ?? "";
            keyword = keyword.Trim();

            var query =
                from v in context.Vehicles.Include(v => v.VehicleType)
                join mt in context.MonthlyTickets.Where(m => m.IsActive == true)
                    on v.VehicleId equals mt.VehicleId into gj
                from mt in gj.DefaultIfEmpty()
                where v.LicensePlate.Contains(keyword)
                orderby v.LicensePlate
                select new { v, mt, v.VehicleType };

            var result = new List<(Vehicle, MonthlyTicket?, VehicleType)>();

            foreach (var item in query)
            {
                result.Add((item.v, item.mt, item.VehicleType));
            }

            return result;
        }
    }
}
