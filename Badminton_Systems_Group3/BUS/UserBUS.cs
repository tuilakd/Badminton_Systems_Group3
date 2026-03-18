using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Badminton_Systems_Group3.BUS
{
    internal class UserBUS
    {
        private UserDAL userDAL = new UserDAL();

        public bool Login(UserDTO user)
        {
            // Kiểm tra rỗng: Không cho phép để trống tài khoản hoặc mật khẩu
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                return false;
            }

            // Nếu dữ liệu hợp lệ, gọi DAL để kiểm tra trong Database
            return userDAL.CheckLogin(user);
        }
    }
}
