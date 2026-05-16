using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class EmployeeRepository
    {
        // Định nghĩa ID của Admin gốc (Root Admin)
        private const int ROOT_ADMIN_USER_ID = 1;

        // ... (Các phương thức GetAllWithUserAndShift, GetById, Add không đổi) ...
        // Trong file: EmployeeRepository.cs

        public User? GetUserById(int userId)
        {
            using var context = new ParkingManagementDbContext();
            return context.Users.Find(userId);
        }
        public List<Employee> GetAllWithUserAndShift()
        {
            using var context = new ParkingManagementDbContext();
            return context.Employees
                     .Include(e => e.User)
                     .Include(e => e.Shift)
                     .OrderBy(e => e.FullName)
                     .ToList();
        }

        public Employee? GetById(int employeeId)
        {
            using var context = new ParkingManagementDbContext();
            return context.Employees
                     .Include(e => e.User)
                     .Include(e => e.Shift)
                     .FirstOrDefault(e => e.EmployeeId == employeeId);
        }

        public Employee Add(Employee emp)
        {
            using var context = new ParkingManagementDbContext();
            context.Employees.Add(emp);
            context.SaveChanges();
            return emp;
        }

        public void Update(Employee emp)
        {
            // Phương thức này chỉ nên cập nhật thông tin của Employee
            // (FullName, Gender, Address, Phone, ShiftID)
            // KHÔNG NÊN cập nhật thông tin User (Username, Password, IsActive)
            using var context = new ParkingManagementDbContext();
            context.Employees.Update(emp);
            context.SaveChanges();
        }

        // ==========================================
        // QUY TẮC 1: KHÔNG THỂ XÓA ADMIN GỐC
        // ==========================================
        public void DeleteEmployeeAndUser(int employeeId)
        {
            using var context = new ParkingManagementDbContext();
            var emp = context.Employees.Include(e => e.User)
                                   .FirstOrDefault(e => e.EmployeeId == employeeId);
            if (emp == null) return;

            // *** KIỂM TRA QUY TẮC ***
            if (emp.UserId == ROOT_ADMIN_USER_ID)
            {
                throw new InvalidOperationException("Không thể xóa tài khoản Admin gốc.");
            }
            // *** KẾT THÚC KIỂM TRA ***

            var user = emp.User;
            context.Employees.Remove(emp);
            if (user != null)
                context.Users.Remove(user);

            context.SaveChanges();
        }

        // ==========================================
        // QUY TẮC 2: KHÔNG THỂ LOCK ADMIN GỐC
        // ==========================================
        public void SetUserActiveStatus(int userIdToChange, bool isActive)
        {
            // *** KIỂM TRA QUY TẮC ***
            // Nếu đang cố gắng "lock" (isActive = false) tài khoản Admin gốc
            if (userIdToChange == ROOT_ADMIN_USER_ID && !isActive)
            {
                throw new InvalidOperationException("Không thể khóa tài khoản Admin gốc.");
            }
            // *** KẾT THÚC KIỂM TRA ***

            using var context = new ParkingManagementDbContext();
            var user = context.Users.Find(userIdToChange);
            if (user != null)
            {
                user.IsActive = isActive;
                context.SaveChanges();
            }
        }

        // ==========================================
        // QUY TẮC 3: CHỈ ADMIN GỐC TỰ THAY ĐỔI THÔNG TIN CỦA MÌNH
        // ==========================================
        public void UpdateUserCredentials(int loggedInUserId, int userIdToChange, string newUsername, string newPasswordHash)
        {
            // *** KIỂM TRA QUY TẮC ***
            // Nếu đang cố gắng thay đổi thông tin của Admin gốc (userIdToChange == 1)
            // nhưng người thực hiện KHÔNG PHẢI là Admin gốc (loggedInUserId != 1)
            if (userIdToChange == ROOT_ADMIN_USER_ID && loggedInUserId != ROOT_ADMIN_USER_ID)
            {
                // Dùng SecurityException sẽ phù hợp hơn trong trường hợp phân quyền
                throw new SecurityException("Bạn không có quyền thay đổi thông tin của Admin gốc.");
            }
            // *** KẾT THÚC KIỂM TRA ***

            using var context = new ParkingManagementDbContext();

            // (Nên kiểm tra username mới có bị trùng không)
            var usernameExists = context.Users.Any(u => u.Username == newUsername && u.UserId != userIdToChange);
            if (usernameExists)
            {
                throw new InvalidOperationException("Username đã tồn tại.");
            }

            var user = context.Users.Find(userIdToChange);
            if (user != null)
            {
                user.Username = newUsername;
                user.PasswordHash = newPasswordHash; // Lưu ý: Password nên được hash ở tầng BLL trước khi gọi
                context.SaveChanges();
            }
        }

        public List<Shift> GetAllShifts()
        {
            using var context = new ParkingManagementDbContext();
            return context.Shifts.OrderBy(s => s.ShiftName).ToList();
        }


        public bool IsPhoneDuplicate(string phone, int employeeIdToExclude = 0)
        {
            using var context = new ParkingManagementDbContext();
            var query = context.Employees.Where(e => e.Phone == phone.Trim());

            if (employeeIdToExclude > 0)
            {
                // Khi CẬP NHẬT: Kiểm tra xem có ai KHÁC mình
                // đang sở hữu SĐT này không.
                query = query.Where(e => e.EmployeeId != employeeIdToExclude);
            }

            // Nếu có bất kỳ ai (Any) -> là trùng
            return query.Any();
        }
    }
}
