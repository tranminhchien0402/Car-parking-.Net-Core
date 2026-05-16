using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UserRepository
    {
        public User? GetByUsername(string username)
        {
            using var context = new ParkingManagementDbContext();
            return context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User? GetById(int userId)
        {
            using var context = new ParkingManagementDbContext();
            return context.Users.Find(userId);
        }

        // Đăng nhập: so sánh username + password (tạm thời plain text)
        public User? Login(string username, string password)
        {
            using var context = new ParkingManagementDbContext();
            // TODO: nếu sau này hash mật khẩu thì thay bằng check PasswordHash
            return context.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.PasswordHash == password &&
                u.IsActive == true);
        }

        public List<User> GetAll()
        {
            using var context = new ParkingManagementDbContext();
            return context.Users.OrderBy(u => u.Username).ToList();
        }

        public User Add(User user)
        {
            using var context = new ParkingManagementDbContext();
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        public void Update(User user)
        {
            using var context = new ParkingManagementDbContext();
            context.Users.Update(user);
            context.SaveChanges();
        }

        public void Delete(int userId)
        {
            using var context = new ParkingManagementDbContext();
            var u = context.Users.Find(userId);
            if (u != null)
            {
                context.Users.Remove(u);
                context.SaveChanges();
            }
        }
        public bool IsUsernameDuplicate(string username)
        {
            using var context = new ParkingManagementDbContext();
            // So sánh không phân biệt hoa thường và loại bỏ khoảng trắng
            return context.Users.Any(u => u.Username.ToUpper() == username.ToUpper().Trim());
        }
    }
}
