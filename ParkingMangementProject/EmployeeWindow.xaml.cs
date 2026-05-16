
using Repository.Models;
using System.Windows;

namespace ASM
{
    public partial class EmployeeWindow : Window
    {
        private readonly User currentUser;

        public EmployeeWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            // Hiển thị tên nhân viên (tùy bạn có trường gì)
            txtEmployeeName.Text = user.Username;
            // hoặc nếu có navigation sang Employee:
            // txtEmployeeName.Text = user.Employee?.FullName ?? user.Username;
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (confirm != MessageBoxResult.Yes)
                return;

            // Mở lại màn hình đăng nhập
            var login = new LoginWindow();
            login.Show();

            // Đóng cửa sổ hiện tại
            this.Close();
        }
    }
}
