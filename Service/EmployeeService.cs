using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service
{
    public class EmployeeService
    {
        // Định nghĩa ID của Admin gốc
        private const int ROOT_ADMIN_USER_ID = 1;

        private readonly EmployeeRepository employeeRepo = new EmployeeRepository();
        private readonly UserRepository userRepo = new UserRepository();

        public List<Employee> GetAllEmployees()
            => employeeRepo.GetAllWithUserAndShift();

        public List<Shift> GetAllShifts()
            => employeeRepo.GetAllShifts();

        public void AddEmployeeWithAccount(
            string fullName,
            string? gender,
            string? address,
            string? phone,
            int? shiftId,
            string username,
            string password,
            string role)
        {
            // 1. Kiểm tra nghiệp vụ 
            ValidateFullName(fullName);  
            ValidateAddress(address);     
            ValidatePhoneNumber(phone); 

            if (shiftId == null)
                throw new InvalidOperationException("Vui lòng chọn ca làm.");

            ValidateAndThrow(username, "Tên đăng nhập");
            ValidateAndThrow(password, "Mật khẩu");

            if (password.Length < 6)
                throw new InvalidOperationException("Mật khẩu phải có ít nhất 6 ký tự.");

            ValidateAndThrow(role, "Vai trò");

            // 2. KIỂM TRA TRÙNG LẶP (CHO THÊM MỚI) 
            if (userRepo.IsUsernameDuplicate(username))
            {
                throw new InvalidOperationException("Tên đăng nhập này đã tồn tại.");
            }
            if (employeeRepo.IsPhoneDuplicate(phone)) // phone đã được ValidateAndThrow nên không thể null
            {
                throw new InvalidOperationException("Số điện thoại này đã tồn tại.");
            }
            // KẾT THÚC KIỂM TRA TRÙNG LẶP 

            // 3. Tạo User
            var user = new User
            {
                Username = username.Trim(),
                PasswordHash = password.Trim(), 
                Role = role,
                IsActive = true
            };
            user = userRepo.Add(user);

            // 4. Tạo Employee
            var emp = new Employee
            {
                UserId = user.UserId,
                FullName = fullName.Trim(),
                Gender = gender,
                Address = address.Trim(),
                Phone = phone.Trim(),
                ShiftId = shiftId
            };

            employeeRepo.Add(emp);
        }

        public void UpdateEmployee(
            int employeeId,
string fullName,
            string? gender,
            string? address,
            string? phone,
            int? shiftId,
            string role,
            bool isActive)
        {
            // 1. Validate đầu vào 
            ValidateFullName(fullName);   
            ValidateAddress(address);     
            ValidatePhoneNumber(phone); 

            if (shiftId == null)
                throw new InvalidOperationException("Vui lòng chọn ca làm.");

            ValidateAndThrow(role, "Vai trò");

            var emp = employeeRepo.GetById(employeeId);
            if (emp == null || emp.User == null)
                throw new InvalidOperationException("Không tìm thấy nhân viên.");

            // 2. KIỂM TRA TRÙNG LẶP (CHO CẬP NHẬT)
            // Kiểm tra SĐT (loại trừ chính nhân viên này)
            if (employeeRepo.IsPhoneDuplicate(phone, emp.EmployeeId))
            {
                throw new InvalidOperationException("Số điện thoại này đã tồn tại ở một nhân viên khác.");
            }
            // KẾT THÚC KIỂM TRA TRÙNG LẶP

            // 3. Validate các quy tắc nghiệp vụ
            if (emp.User.UserId == ROOT_ADMIN_USER_ID && !isActive)
            {
                throw new InvalidOperationException("Không thể khóa tài khoản Admin gốc.");
            }

            if (emp.User.UserId == ROOT_ADMIN_USER_ID && role != "Admin")
            {
                throw new InvalidOperationException("Không thể thay đổi vai trò của Admin gốc.");
            }

            // 4. Cập nhật thông tin Employee
            emp.FullName = fullName.Trim();
            emp.Gender = gender;
            emp.Address = address.Trim();
            emp.Phone = phone.Trim();
            emp.ShiftId = shiftId;

            // 5. Cập nhật thông tin User
            emp.User.Role = role;
            emp.User.IsActive = isActive;

            employeeRepo.Update(emp);
        }

        public void DeleteEmployee(int employeeId)
        {
            employeeRepo.DeleteEmployeeAndUser(employeeId);
        }

        public void ResetPassword(int loggedInUserId, int userIdToChange, string newPassword)
        {
            ValidateAndThrow(newPassword, "Mật khẩu mới");
            if (newPassword.Length < 6)
            {
                throw new InvalidOperationException("Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            var passwordHash = newPassword.Trim(); 

            var userToChange = employeeRepo.GetUserById(userIdToChange);
            if (userToChange == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng để reset mật khẩu.");
            }

            employeeRepo.UpdateUserCredentials(
                loggedInUserId,
                userIdToChange,
                userToChange.Username,
                passwordHash);
        }

        // === VALIDATE ===
        private void ValidateAndThrow(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{fieldName} không được để trống.");
            }
        }

        // Validate Họ tên
        private void ValidateFullName(string? fullName)
        {
            ValidateAndThrow(fullName, "Họ tên");
            if (!Regex.IsMatch(fullName.Trim(), @"^[\p{L}\s]+$"))
            {
                throw new InvalidOperationException("Họ tên chỉ được chứa chữ cái và khoảng trắng.");
            }
        }

        // Validate Địa chỉ
        private void ValidateAddress(string? address)
        {
            ValidateAndThrow(address, "Địa chỉ");
            if (!Regex.IsMatch(address.Trim(), @"^[\p{L}\p{N}\s,./-]+$"))
            {
                throw new InvalidOperationException("Địa chỉ không được chứa ký tự đặc biệt (chỉ cho phép số, chữ, và , . / -).");
            }
        }

        // Validate SĐT
        private void ValidatePhoneNumber(string? phone)
        {
            ValidateAndThrow(phone, "Số điện thoại");
            if (!Regex.IsMatch(phone.Trim(), @"^0[0-9]{9,11}$"))
            {
                throw new InvalidOperationException("Số điện thoại phải bắt đầu bằng 0, chỉ chứa số, viết liền, và có 10-12 ký tự.");
            }
        }
    }
}
