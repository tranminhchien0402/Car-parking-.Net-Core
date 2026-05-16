using Repository.Models;
using Service;
using System.Security; // THÊM: Để bắt lỗi SecurityException
using System.Text.RegularExpressions; // THÊM MỚI: Để validate Regex
using System.Windows;
using System.Windows.Controls;

namespace ASM
{
    public partial class AdminDashboardWindow : Window
    {
        private readonly EmployeeService employeeService = new EmployeeService();
        private readonly ParkingLotService parkingLotService = new ParkingLotService();

        private Employee? _selectedEmployee;
        private readonly User _currentUser;

        public AdminDashboardWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            txtCurrentUser.Text = $"{_currentUser.Username} ({_currentUser.Role})";

            LoadEmployees();
            LoadShifts();
            LoadParkingStatus();

            dpRevenueDate.SelectedDate = DateTime.Today;
        }

        #region Load Data
        private void LoadEmployees()
        {
            dgEmployees.ItemsSource = employeeService.GetAllEmployees();
        }

        private void LoadShifts()
        {
            cbShift.ItemsSource = employeeService.GetAllShifts();
        }

        private void LoadParkingStatus()
        {
            var (total, occupied, free) = parkingLotService.GetParkingStatus();
            txtTotalSlots.Text = total.ToString();
            txtOccupiedSlots.Text = occupied.ToString();
            txtFreeSlots.Text = free.ToString();
        }
        #endregion


        // ====== TAB EMPLOYEE ======

        private void dgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEmployee = dgEmployees.SelectedItem as Employee;
            if (_selectedEmployee == null) return;

            txtEmpName.Text = _selectedEmployee.FullName;
            cbGender.Text = _selectedEmployee.Gender;
            txtEmpAddress.Text = _selectedEmployee.Address;
            txtEmpPhone.Text = _selectedEmployee.Phone;
            cbShift.SelectedValue = _selectedEmployee.ShiftId;

            if (_selectedEmployee.User != null)
            {
                txtEmpUsername.Text = _selectedEmployee.User.Username;
                cbRole.Text = _selectedEmployee.User.Role;
                chkActive.IsChecked = _selectedEmployee.User.IsActive;
                txtEmpPassword.Password = "";
            }
        }

        // === CẬP NHẬT VALIDATE NÂNG CAO ===
        private void BtnAddEmp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- BẮT ĐẦU VALIDATE UI (CHO THÊM MỚI) ---
                if (string.IsNullOrWhiteSpace(txtEmpName.Text))
                {
                    txtEmpMessage.Text = "Lỗi: Họ tên không được để trống.";
                    return;
                }
                if (!Regex.IsMatch(txtEmpName.Text.Trim(), @"^[\p{L}\s]+$"))
                {
                    txtEmpMessage.Text = "Lỗi: Họ tên chỉ được chứa chữ cái và khoảng trắng.";
                    return;
                }
                if (cbGender.SelectedItem == null)
                {
                    txtEmpMessage.Text = "Lỗi: Vui lòng chọn giới tính.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmpAddress.Text))
                {
                    txtEmpMessage.Text = "Lỗi: Địa chỉ không được để trống.";
                    return;
                }
                if (!Regex.IsMatch(txtEmpAddress.Text.Trim(), @"^[\p{L}\p{N}\s,./-]+$"))
                {
                    txtEmpMessage.Text = "Lỗi: Địa chỉ không được chứa ký tự đặc biệt (chỉ cho phép số, chữ, và , . / -).";
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmpPhone.Text))
                {
                    txtEmpMessage.Text = "Lỗi: Số điện thoại không được để trống.";
                    return;
                }
                if (!Regex.IsMatch(txtEmpPhone.Text.Trim(), @"^0[0-9]{9,11}$"))
                {
                    txtEmpMessage.Text = "Lỗi: Số điện thoại phải bắt đầu bằng 0, chỉ chứa số, viết liền, và có 10-12 ký tự.";
                    return;
                }
                if (cbShift.SelectedValue == null)
                {
                    txtEmpMessage.Text = "Lỗi: Vui lòng chọn ca làm.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmpUsername.Text))
                    if (string.IsNullOrWhiteSpace(txtEmpUsername.Text))
                    {
                        txtEmpMessage.Text = "Lỗi: Tên đăng nhập không được để trống.";
                        return;
                    }

                // Regex: phải có ít nhất 1 chữ cái
                string pattern = @"^(?=.*[A-Za-z]).+$";

                if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmpUsername.Text, pattern))
                {
                    txtEmpMessage.Text = "Lỗi: Tên đăng nhập phải chứa ít nhất 1 chữ cái (A–Z).";
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmpPassword.Password))
                {
                    txtEmpMessage.Text = "Lỗi: Mật khẩu không được để trống.";
                    return;
                }
                if (txtEmpPassword.Password.Length < 6)
                {
                    txtEmpMessage.Text = "Lỗi: Mật khẩu phải có ít nhất 6 ký tự.";
                    return;
                }
                if (cbRole.SelectedItem == null)
                {
                    txtEmpMessage.Text = "Lỗi: Vui lòng chọn vai trò (Role).";
                    return;
                }
                // --- KẾT THÚC VALIDATE UI ---

                var fullName = txtEmpName.Text;
                var gender = cbGender.Text;
                var address = txtEmpAddress.Text;
                var phone = txtEmpPhone.Text;
                int? shiftId = cbShift.SelectedValue as int?;
                var username = txtEmpUsername.Text;
                var password = txtEmpPassword.Password;
                var role = (cbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Employee";

                employeeService.AddEmployeeWithAccount(
                    fullName, gender, address, phone, shiftId, username, password, role);
                txtEmpMessage.Text = "Thêm nhân viên thành công!";
                LoadEmployees();
            }
            catch (InvalidOperationException ex)
            {
                txtEmpMessage.Text = "Lỗi: " + ex.Message;
            }
            catch (Exception ex)
            {
                txtEmpMessage.Text = "Lỗi hệ thống: " + ex.Message;
            }
        }

        // === CẬP NHẬT VALIDATE NÂNG CAO ===
        private void BtnUpdateEmp_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null)
            {
                txtEmpMessage.Text = "Vui lòng chọn nhân viên để cập nhật.";
                return;
            }

            try
            {
                // --- BẮT ĐẦU VALIDATE UI (CHO CẬP NHẬT) ---
                if (string.IsNullOrWhiteSpace(txtEmpName.Text))
                {
                    txtEmpMessage.Text = "Lỗi: Họ tên không được để trống.";
                    return;
                }
                if (!Regex.IsMatch(txtEmpName.Text.Trim(), @"^[\p{L}\s]+$"))
                {
                    txtEmpMessage.Text = "Lỗi: Họ tên chỉ được chứa chữ cái và khoảng trắng.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmpAddress.Text))
                {
                    txtEmpMessage.Text = "Lỗi: Địa chỉ không được để trống.";
                    return;
                }
                if (!Regex.IsMatch(txtEmpAddress.Text.Trim(), @"^[\p{L}\p{N}\s,./-]+$"))
                {
                    txtEmpMessage.Text = "Lỗi: Địa chỉ không được chứa ký tự đặc biệt (chỉ cho phép số, chữ, và , . / -).";
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmpPhone.Text))
                {
                    txtEmpMessage.Text = "Lỗi: Số điện thoại không được để trống.";
                    return;
                }
                if (!Regex.IsMatch(txtEmpPhone.Text.Trim(), @"^0[0-9]{9,11}$"))
                {
                    txtEmpMessage.Text = "Lỗi: Số điện thoại phải bắt đầu bằng 0, chỉ chứa số, viết liền, và có 10-12 ký tự.";
                    return;
                }
                if (cbShift.SelectedValue == null)
                {
                    txtEmpMessage.Text = "Lỗi: Vui lòng chọn ca làm.";
                    return;
                }
                if (cbRole.SelectedItem == null)
                {
                    txtEmpMessage.Text = "Lỗi: Vui lòng chọn vai trò (Role).";
                    return;
                }
                // --- KẾT THÚC VALIDATE UI ---

                var fullName = txtEmpName.Text;
                var gender = cbGender.Text;
                var address = txtEmpAddress.Text;
                var phone = txtEmpPhone.Text;
                int? shiftId = cbShift.SelectedValue as int?;
                var role = (cbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Employee";
                bool isActive = chkActive.IsChecked == true;

                employeeService.UpdateEmployee(
                    _selectedEmployee.EmployeeId, fullName, gender, address, phone, shiftId, role, isActive);

                txtEmpMessage.Text = "Cập nhật thành công!";
                LoadEmployees();
            }
            catch (InvalidOperationException ex)
            {
                txtEmpMessage.Text = "Lỗi: " + ex.Message;
            }
            catch (Exception ex)
            {
                txtEmpMessage.Text = "Lỗi hệ thống: " + ex.Message;
            }
        }

        private void BtnDeleteEmp_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null)
            {
                txtEmpMessage.Text = "Vui lòng chọn nhân viên để xóa.";
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa nhân viên {_selectedEmployee.FullName}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                employeeService.DeleteEmployee(_selectedEmployee.EmployeeId);
                txtEmpMessage.Text = "Xóa thành công!";
                _selectedEmployee = null;
                LoadEmployees();
            }
            catch (InvalidOperationException ex)
            {
                txtEmpMessage.Text = "Lỗi: " + ex.Message;
            }
            catch (Exception ex)
            {
                txtEmpMessage.Text = "Lỗi hệ thống: " + ex.Message;
            }
        }

        // (Hàm này giữ nguyên validate cũ)
        private void BtnResetPass_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null || _selectedEmployee.User == null)
            {
                txtEmpMessage.Text = "Vui lòng chọn nhân viên để reset mật khẩu.";
                return;
            }

            // --- BẮT ĐẦU VALIDATE UI (CHO RESET PASS) ---
            if (string.IsNullOrWhiteSpace(txtEmpPassword.Password))
            {
                txtEmpMessage.Text = "Lỗi: Mật khẩu mới không được để trống.";
                return;
            }
            if (txtEmpPassword.Password.Length < 6)
            {
                txtEmpMessage.Text = "Lỗi: Mật khẩu mới phải có ít nhất 6 ký tự.";
                return;
            }
            // --- KẾT THÚC VALIDATE UI ---

            try
            {
                var newPass = txtEmpPassword.Password;
                employeeService.ResetPassword(
                    _currentUser.UserId,
                    _selectedEmployee.User.UserId,
                    newPass);

                txtEmpMessage.Text = "Reset mật khẩu thành công!";
            }
            catch (SecurityException ex)
            {
                txtEmpMessage.Text = "Lỗi bảo mật: " + ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                txtEmpMessage.Text = "Lỗi: " + ex.Message;
            }
            catch (Exception ex)
            {
                txtEmpMessage.Text = "Lỗi hệ thống: " + ex.Message;
            }
        }

        #region Other Tabs
        // ====== TAB TRẠNG THÁI BÃI ======
        private void BtnRefreshLot_Click(object sender, RoutedEventArgs e)
        {
            LoadParkingStatus();
        }

        // ====== TAB DOANH THU ======
        private void BtnRevenueByDate_Click(object sender, RoutedEventArgs e)
        {
            var date = dpRevenueDate.SelectedDate ?? DateTime.Today;
            var money = parkingLotService.GetRevenueByDate(date);
            txtRevenueDate.Text = $"Doanh thu ngày {date:dd/MM/yyyy}: {money:N0} VND";
        }

        private void BtnRevenueByMonth_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtYear.Text, out var year) ||
                !int.TryParse(txtMonth.Text, out var month) ||
                month < 1 || month > 12)
            {
                txtRevenueMonth.Text = "Năm / tháng không hợp lệ.";
                return;
            }

            var money = parkingLotService.GetRevenueByMonth(year, month);
            txtRevenueMonth.Text = $"Doanh thu tháng {month:00}/{year}: {money:N0} VND";
        }
        #endregion
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}