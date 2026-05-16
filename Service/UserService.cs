using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class UserService
    {
        private readonly UserRepository  userRepo = new UserRepository();

        public User? Login(string username, string password, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                message = "Tên đăng nhập không được để trống.";
                return null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                message = "Mật khẩu không được để trống.";
                return null;
            }

            var user = userRepo.Login(username.Trim(), password.Trim());
            if (user == null)
            {
                message = "Sai tên đăng nhập hoặc mật khẩu, hoặc tài khoản đang bị khóa.";
                return null;
            }

            return user;
        }
    }
}
