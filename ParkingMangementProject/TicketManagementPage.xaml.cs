using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Service;
using System.Windows;
using System.Windows.Controls;

namespace ASM
{
    public partial class TicketManagementPage : UserControl
    {
        private readonly TicketService ticketService = new TicketService();
        private Ticket? _selectedTicket;

        public TicketManagementPage()
        {
            InitializeComponent();
            Loaded += TicketManagementPage_Loaded;
        }

        private void TicketManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        // ======= HÀM HỖ TRỢ LẤY DỮ LIỆU TRỰC TIẾP TỪ DB =======

        private List<VehicleType> GetVehicleTypes()
        {
            using var context = new ParkingManagementDbContext();
            return context.VehicleTypes.ToList();
        }

        private List<Ticket> GetActiveTickets()
        {
            using var context = new ParkingManagementDbContext();
            return context.Tickets
                      .Where(t => t.CheckOutTime == null)  // đang gửi trong bãi
                      .Include(t=>t.VehicleType)
                      .ToList();
        }

        private void LoadData()
        {
            cboVehicleType.ItemsSource = GetVehicleTypes();
            dgvTickets.ItemsSource = GetActiveTickets();
            ResetCheckoutInfo();
        }

        // ==================== CHECK IN ====================

        private void btnCheckIn_Click(object sender, RoutedEventArgs e)
        {
            string plate = txtLicensePlate.Text.Trim().ToUpper();

            // Validate biển số
            if (!ValidateLicensePlate(plate))
                return;

            // Validate loại xe
            if (cboVehicleType.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn loại xe!");
                return;
            }

            int typeId = (int)cboVehicleType.SelectedValue;

            // ==== KIỂM TRA XE ĐÃ Ở TRONG BÃI CHƯA ====
            var activeTickets = GetActiveTickets();    // lấy các vé đang gửi
            bool existsSameVehicle = activeTickets.Any(t =>
                string.Equals(t.LicensePlate, plate, StringComparison.OrdinalIgnoreCase)
                && t.VehicleTypeId == typeId);

            if (existsSameVehicle)
            {
                MessageBox.Show(
                    "Xe này đang ở trong bãi (đã có vé đang gửi cùng biển số và loại xe).\n" +
                    "Vui lòng kiểm tra lại trước khi check-in.",
                    "Không thể check-in",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            // ==== HẾT PHẦN KIỂM TRA ====

            int userId = 2; // TODO: lấy từ user đăng nhập - truy cứu trách nhiệm

            string result = ticketService.CheckInVehicle(plate, typeId, userId);
            MessageBox.Show(result, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            if (result.Contains("thành công", StringComparison.OrdinalIgnoreCase))
            {
                txtLicensePlate.Clear();
                LoadData();
            }
        }

        // ==================== GRID SELECTION / CHECK OUT ====================

        private void dgvTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTicket = dgvTickets.SelectedItem as Ticket;
            if (_selectedTicket == null)
            {
                ResetCheckoutInfo();
                return;
            }

            var typeName = _selectedTicket.VehicleType?.TypeName ?? "(không rõ)";

            txtCheckOutInfo.Text =
                $"Biển số: {_selectedTicket.LicensePlate}\n" +
                $"Loại: {typeName}\n" +
                $"Giờ vào: {_selectedTicket.CheckInTime:dd/MM/yyyy HH:mm}";

            btnCheckOut.IsEnabled = true;
        }

        private void ResetCheckoutInfo()
        {
            txtCheckOutInfo.Text = "Chọn 1 xe để thanh toán";
            btnCheckOut.IsEnabled = false;
        }

        private void btnCheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTicket == null) return;

            string result = ticketService.CheckOutVehicle(_selectedTicket);
            MessageBox.Show(result);

            LoadData();
        }

        // ==================== VALIDATE BIỂN SỐ ====================

        private bool ValidateLicensePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
            {
                MessageBox.Show("Biển số không được để trống.");
                return false;
            }

            plate = plate.Trim().ToUpper();

            // Độ dài 8–9 ký tự
            if (plate.Length < 8 || plate.Length > 9)
            {
                MessageBox.Show("Biển số phải có 9 ký tự bao gồm cả dấu '-' theo dạng 29A-12345.");
                return false;
            }

            // 2 số + 1 chữ + '-' + 4–5 số
            var regex = @"^[0-9]{2}[A-Z]-[0-9]{4,5}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(plate, regex))
            {
                MessageBox.Show("Biển số không đúng định dạng!\nĐịnh dạng đúng: 29AA-12345");
                return false;
            }

            return true;
        }
    }
}