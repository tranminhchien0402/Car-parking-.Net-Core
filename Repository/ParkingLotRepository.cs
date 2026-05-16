using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ParkingLotRepository
    {
        // ====== TRẠNG THÁI BÃI ======
        public (int totalSlots, int occupiedSlots, int freeSlots) GetParkingStatus()
        {
            using var context = new ParkingManagementDbContext();

            var lot = context.ParkingLots.FirstOrDefault();
            int total = lot?.TotalSlots ?? 0;

            int occupied = context.Tickets.Count(t => t.CheckOutTime == null);
            int free = total - occupied;
            if (free < 0) free = 0;

            return (total, occupied, free);
        }

        // ====== DOANH THU THEO NGÀY (VÉ LƯỢT + VÉ THÁNG) ======
        public decimal GetRevenueByDate(DateTime date)
        {
            using var context = new ParkingManagementDbContext();
            var targetDate = date.Date;
            var targetDateOnly = DateOnly.FromDateTime(targetDate);

            // 1. Vé lượt: tính theo ngày checkout
            var revenueTicket = context.Tickets
                .Where(t => t.CheckOutTime.HasValue
                            && t.TotalAmount != null
                            && t.CheckOutTime.Value.Date == targetDate)
                .Sum(t => (decimal?)t.TotalAmount) ?? 0m;

            // 2. Vé tháng: tính theo StartDate (ngày bắt đầu hiệu lực)
            var revenueMonthly = context.MonthlyTickets
                .Where(m => m.StartDate == targetDateOnly)
                .Sum(m => (decimal?)m.Price) ?? 0m;

            return revenueTicket + revenueMonthly;
        }

        // ====== DOANH THU THEO THÁNG (VÉ LƯỢT + VÉ THÁNG) ======
        public decimal GetRevenueByMonth(int year, int month)
        {
            using var context = new ParkingManagementDbContext();

            // Vé lượt
            var revenueTicket = context.Tickets
                .Where(t => t.CheckOutTime.HasValue
                            && t.TotalAmount != null
                            && t.CheckOutTime.Value.Year == year
                            && t.CheckOutTime.Value.Month == month)
                .Sum(t => (decimal?)t.TotalAmount) ?? 0m;

            // Vé tháng
            var revenueMonthly = context.MonthlyTickets
                .Where(m => m.StartDate.Year == year
                            && m.StartDate.Month == month)
                .Sum(m => (decimal?)m.Price) ?? 0m;

            return revenueTicket + revenueMonthly;
        }

        // ====== DOANH THU THEO NĂM (VÉ LƯỢT + VÉ THÁNG) ======
        public decimal GetRevenueByYear(int year)
        {
            using var context = new ParkingManagementDbContext();

            var revenueTicket = context.Tickets
                .Where(t => t.CheckOutTime.HasValue
                            && t.TotalAmount != null
                            && t.CheckOutTime.Value.Year == year)
                .Sum(t => (decimal?)t.TotalAmount) ?? 0m;
            var revenueMonthly = context.MonthlyTickets
                            .Where(m => m.StartDate.Year == year)
                            .Sum(m => (decimal?)m.Price) ?? 0m;

            return revenueTicket + revenueMonthly;
        }

        // (Nếu bạn có hàm detail theo tháng / năm thì cũng cộng thêm MonthlyTickets tương tự)
    }
}
