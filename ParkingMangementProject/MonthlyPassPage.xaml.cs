using Repository.Models;
using Service;
using System.Windows;
using System.Windows.Controls;

namespace ASM
{
    public partial class MonthlyPassPage : UserControl
    {
        private readonly MonthlyTicketService monthlyTicketService = new MonthlyTicketService();

        // lớp view model hiển thị trong DataGrid
        private class MonthlyPassRow
        {
            public string LicensePlate { get; set; } = "";
            public string OwnerName { get; set; } = "";
            public string OwnerPhone { get; set; } = "";
            public string VehicleTypeName { get; set; } = "";
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string StatusText
            {
                get
                {
                    if (EndDate == null) return "Test lần 1 - chưa có ngày kết thúc";
                    var days = (EndDate.Value.Date - DateTime.Today).TotalDays; //tính ra số ngày dạng số thực
                    if (days >= 0) return $"Còn {Math.Ceiling(days)} ngày"; // làm tròn lên
                    return "Đã hết hạn";
                }
            }
        }

        public MonthlyPassPage()
        {
            InitializeComponent();
            Loaded += MonthlyPassPage_Loaded;
        }

        private void MonthlyPassPage_Loaded(object sender, RoutedEventArgs e)
        {
            cbVehicleType.ItemsSource = monthlyTicketService.GetVehicleTypes();

            dpStart.SelectedDate = DateTime.Today;
            dpEnd.SelectedDate = DateTime.Today.AddMonths(1).AddDays(-1);

            LoadGrid("");
        }

        private void LoadGrid(string keyword)
        {
            var list = monthlyTicketService.SearchByPlate(keyword);
            var rows = new List<MonthlyPassRow>();

            foreach (var item in list)
            {
                var v = item.vehicle;
                var t = item.ticket;
                var type = item.type;

                rows.Add(new MonthlyPassRow
                {
                    LicensePlate = v.LicensePlate,
                    OwnerName = v.OwnerName ?? "",
                    OwnerPhone = v.OwnerPhone ?? "",
                    VehicleTypeName = type?.TypeName ?? "",
                    StartDate = t?.StartDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = t?.EndDate.ToDateTime(TimeOnly.MinValue)
                });
            }

            dgMonthlyPass.ItemsSource = rows;
        }

        // =============================
        //      VALIDATION CHUẨN HÓA
        // =============================

        private bool ValidateLicensePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
            {
                ShowMessage("Biển số không được để trống.");
                return false;
            }

            if (plate.Length < 8 || plate.Length > 9)
            {
                ShowMessage("lorem is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions.");
                return false;
            }

            string pattern = @"^[0-9]{2}[A-Z]-[0-9]{4,5}$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(plate, pattern))
            {
                ShowMessage("Biển số không đúng định dạng!\nVí dụ: 29A-12345");
                return false;
            }

            return true;
        }

        private bool ValidateOwnerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowMessage("Vui lòng nhập tên chủ xe.");
                return false;
            }

            string pattern = @"^[A-Za-zÀ-ỹ\s]+$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(name, pattern))
            {
                ShowMessage("Tên chủ xe chỉ bao gồm chữ, không chứa số hay ký tự đặc biệt.");
                return false;
            }

            return true;
        }

        private bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                ShowMessage("Vui lòng nhập số điện thoại.");
                return false;
            }

            if (!phone.All(char.IsDigit))
            {
                ShowMessage("Số điện thoại chỉ được chứa số.");
                return false;
            }

            if (!phone.StartsWith("0"))
            {
                ShowMessage("Số điện thoại phải bắt đầu bằng số 0.");
                return false;
            }

            if (phone.Length < 10 || phone.Length > 12)
            {
                ShowMessage("Số điện thoại phải từ 10 đến 12 số.");
                return false;
            }

            return true;
        }


        private bool ValidateDates(DateTime start, DateTime end)
        {
            if (end < start)
            {
                ShowMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
                return false;
            }
            return true;
        }

        // =============================
        //        AUTO UPDATE PRICE
        // =============================

        private void UpdatePrice()
        {
            if (cbVehicleType.SelectedItem is VehicleType type &&
                dpStart.SelectedDate is DateTime start &&
                dpEnd.SelectedDate is DateTime end)
            {
                if (end < start)
                {
                    txtPrice.Text = "";
                    return;
                }

                // Tính số tháng chênh lệch theo tháng, KHÔNG +1
                int monthDiff = ((end.Year - start.Year) * 12) + (end.Month - start.Month);

                // Nếu vẫn là cùng tháng thì mặc định là 1 tháng
                int months = Math.Max(1, monthDiff == 0 ? 1 : monthDiff);

                txtPrice.Text = (months * type.MonthlyRate).ToString("0");
            }
            else
            {
                txtPrice.Text = "";
            }
        }


        private void cbVehicleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePrice();
        }

        private void DateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePrice();
        }

        // =============================
        //        TÁC VỤ CHÍNH
        // =============================

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadGrid(txtSearch.Text.Trim());
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            LoadGrid("");
            ShowMessage("");
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var plate = txtPlate.Text.Trim().ToUpper();
            var owner = txtOwner.Text.Trim();
            var phone = txtPhone.Text.Trim();

            if (!ValidateLicensePlate(plate)) return;
            if (!ValidateOwnerName(owner)) return;
            if (!ValidatePhone(phone)) return;

            if (cbVehicleType.SelectedValue == null)
            {
                ShowMessage("Vui lòng chọn loại xe.");
                return;
            }

            int vehicleTypeId = (int)cbVehicleType.SelectedValue;
            var start = dpStart.SelectedDate ?? DateTime.Today;
            var end = dpEnd.SelectedDate ?? DateTime.Today;

            if (!ValidateDates(start, end)) return;

            if (!decimal.TryParse(txtPrice.Text, out var price))
            {
                ShowMessage("Giá vé không hợp lệ.");
                return;
            }

            var (ok, msg) = monthlyTicketService.RegisterMonthlyPass(plate, vehicleTypeId, owner, phone, start, end, price);
            ShowMessage(msg);
            if (ok)
            {
                LoadGrid(plate);
            }
        }

        private void btnRenew_Click(object sender, RoutedEventArgs e)
        {
            var plate = txtPlate.Text.Trim().ToUpper();
            if (!ValidateLicensePlate(plate)) return;

            if (!int.TryParse(txtMonths.Text, out var months) || months <= 0)
            {
                ShowMessage("Số tháng gia hạn phải > 0.");
                return;
            }

            decimal extraPrice = 0;
            decimal.TryParse(txtPrice.Text, out extraPrice);

            var (ok, msg) = monthlyTicketService.RenewMonthlyPass(plate, months, extraPrice);
            ShowMessage(msg);

            if (ok)
                LoadGrid(plate);
        }

        private void btnDeactivate_Click(object sender, RoutedEventArgs e)
        {
            var plate = txtPlate.Text.Trim().ToUpper();
            if (!ValidateLicensePlate(plate)) return;

            var result = MessageBox.Show(
                $"Bạn chắc chắn muốn hủy vé tháng của xe {plate}?",
                "Xác nhận", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes) return;

            var (ok, msg) = monthlyTicketService.DeactivateMonthlyPass(plate);
            ShowMessage(msg);
            if (ok)
                LoadGrid(plate);
        }

        private void dgMonthlyPass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMonthlyPass.SelectedItem is not MonthlyPassRow row) return;

            txtPlate.Text = row.LicensePlate;
            txtOwner.Text = row.OwnerName;
            txtPhone.Text = row.OwnerPhone;

            dpStart.SelectedDate = row.StartDate;
            dpEnd.SelectedDate = row.EndDate;

            var types = cbVehicleType.ItemsSource.Cast<VehicleType>().ToList();
            var type = types.FirstOrDefault(t => t.TypeName == row.VehicleTypeName);

            if (type != null)
            {
                cbVehicleType.SelectedValue = type.VehicleTypeId;
            }

            UpdatePrice();
        }

        private void ShowMessage(string msg)
        {
            txtMessage.Text = msg;
        }
        private void txtPlate_TextChanged(object sender, TextChangedEventArgs e)
        {
            string plate = txtPlate.Text.Trim().ToUpper();

            // Clear message nếu đang gõ
            txtMessage.Text = "";

            if (plate.Length > 0 && (plate.Length < 8 || plate.Length > 9))
            {
                txtMessage.Text = "Biển số phải từ 8 đến 9 ký tự.";
            }
        }
        private void txtOwner_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtOwner.Text.Trim();

            txtMessage.Text = "";

            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-zÀ-ỹ\s]*$"))
            {
                txtMessage.Text = "Tên chủ xe chỉ được chứa chữ.";
            }
        }
        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            string phone = txtPhone.Text.Trim();

            txtMessage.Text = "";

            if (!phone.All(char.IsDigit))
            {
                txtMessage.Text = "SĐT chỉ được nhập số.";
            }
            else if (phone.Length > 12)
            {
                txtMessage.Text = "SĐT tối đa 12 số.";
            }
        }

    }
}
