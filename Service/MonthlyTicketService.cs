using Repository.Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class MonthlyTicketService
    {
        private readonly VehicleRepository vehicleRepo = new VehicleRepository();
        private readonly MonthlyTicketRepository MonthlyTicketRepo = new MonthlyTicketRepository();

        // Lấy loại xe
        public List<VehicleType> GetVehicleTypes()
        {
            return vehicleRepo.GetVehicleTypes();
        }

        //  ĐĂNG KÝ VÉ THÁNG
        public (bool ok, string message) RegisterMonthlyPass(
            string licensePlate,
            int vehicleTypeId,
            string? ownerName,
            string? ownerPhone,
            DateTime startDate,
            DateTime endDate,
            decimal price)
        {
            // ----- Validate SĐT không trùng giữa hai chủ xe -----
            if (!string.IsNullOrWhiteSpace(ownerPhone))
            {
                var existed = vehicleRepo.GetByPhone(ownerPhone.Trim());

                if (existed.Any(v =>
                    !string.Equals(v.OwnerName ?? "",
                                   ownerName ?? "",
                                   StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "Số điện thoại này đã được đăng ký cho một chủ xe khác.");
                }
            }

            // ----- Validate biển số -----
            if (string.IsNullOrWhiteSpace(licensePlate))
                return (false, "Biển số không được để trống.");

            var lp = licensePlate.Trim().ToUpper();

            // Convert sang DateOnly
            var start = DateOnly.FromDateTime(startDate.Date);
            var finish = DateOnly.FromDateTime(endDate.Date);

            if (finish < start)
                return (false, "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            // ----- Lấy hoặc tạo vehicle -----
            var vehicle = vehicleRepo.GetByLicensePlate(lp);
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    LicensePlate = lp,
                    VehicleTypeId = vehicleTypeId,
                    OwnerName = ownerName,
                    OwnerPhone = ownerPhone
                };
                vehicle = vehicleRepo.Add(vehicle);
            }
            else
            {
                // Cập nhật thông tin chủ xe
                vehicleRepo.UpdateOwnerAndType(vehicle.VehicleId, ownerName, ownerPhone, vehicleTypeId);
            }

            // ----- Kiểm tra vé tháng active -----
            var active = MonthlyTicketRepo.GetActiveByVehicleId(vehicle.VehicleId);
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (active != null && active.IsActive == true && active.EndDate >= today)
            {
                return (false, "Xe này đã có vé tháng còn hiệu lực.");
            }

            // ----- Tạo vé tháng mới -----
            var ticket = new MonthlyTicket
            {
                VehicleId = vehicle.VehicleId,
                VehicleTypeId = vehicleTypeId,
                StartDate = start,
                EndDate = finish,
                Price = price,
                IsActive = true
            };

            MonthlyTicketRepo.Add(ticket);
            return (true, "Đăng ký vé tháng thành công.");
        }

        //  GIA HẠN VÉ THÁNG
        public (bool ok, string message) RenewMonthlyPass(string licensePlate, int monthsToAdd, decimal extraPrice)
        {
            if (monthsToAdd <= 0)
                return (false, "Số tháng gia hạn phải > 0.");

            var lp = licensePlate.Trim().ToUpper();
            var vehicle = vehicleRepo.GetByLicensePlate(lp);
            if (vehicle == null)
                return (false, "Không tìm thấy xe.");

            var active = MonthlyTicketRepo.GetActiveByVehicleId(vehicle.VehicleId);
            if (active == null || active.IsActive != true)
                return (false, "Xe chưa có vé tháng đang hiệu lực.");

            var today = DateOnly.FromDateTime(DateTime.Today);

            // nếu vé đã hết hạn -> gia hạn từ hôm nay
            // nếu còn hạn -> cộng từ enddate
            var baseDate = active.EndDate < today ? today : active.EndDate;

            var newEnd = baseDate.AddMonths(monthsToAdd);
            var newPrice = active.Price + extraPrice;

            MonthlyTicketRepo.UpdateDatesAndPrice(active.MonthlyTicketId, newEnd, newPrice);

            return (true, "Gia hạn vé tháng thành công.");
        }

        //  HỦY VÉ THÁNG
        public (bool ok, string message) DeactivateMonthlyPass(string licensePlate)
        {
            var lp = licensePlate.Trim().ToUpper();
            var vehicle = vehicleRepo.GetByLicensePlate(lp);
            if (vehicle == null)
                return (false, "Không tìm thấy xe.");

            var active = MonthlyTicketRepo.GetActiveByVehicleId(vehicle.VehicleId);
            if (active == null)
                return (false, "Xe không có vé tháng đang hiệu lực.");

            MonthlyTicketRepo.Deactivate(active.MonthlyTicketId);
            return (true, "Đã hủy hiệu lực vé tháng.");
        }

        //  LOAD DỮ LIỆU CHO DATAGRID
        public List<(Vehicle vehicle, MonthlyTicket? ticket, VehicleType type)> SearchByPlate(string keyword)
        {
            return MonthlyTicketRepo.SearchByPlate(keyword);
        }
    }
}
