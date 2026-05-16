using Service;
using System.Windows;

namespace ASM
{
    public partial class LoginWindow : Window
    {
        private readonly UserService userService = new UserService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtMessage.Text = ""; // clear message

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            // Validate input
            if (string.IsNullOrWhiteSpace(username))
            {
                txtMessage.Text = "Vui lòng nhập Username.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                txtMessage.Text = "Vui lòng nhập Password.";
                return;
            }

            // Login
            var user = userService.Login(username, password, out string msg);

            if (user == null)
            {
                txtMessage.Text = msg;  // Sai mật khẩu / tài khoản không tồn tại
                return;
            }

            if (user.IsActive == false)
            {
                txtMessage.Text = "Tài khoản đã bị khóa.";
                return;
            }

            // -------- PHÂN QUYỀN --------
            if (user.Role == "Admin")
            {
                var win = new AdminDashboardWindow(user);
                win.Show();
                this.Close();
            }
            else  // Employee
            {
                var win = new EmployeeWindow(user);
                win.Show();
                this.Close();
            }
        }
    }
}
