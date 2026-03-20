using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Badminton_Systems_Group3.DAL
{
    internal class UserDAL
    {
        private string connectionString = "Data Source=DESKTOP-GL8IADV\\sqlexpress;Initial Catalog=master;Integrated Security=True;Encrypt=False";

        public bool CheckLogin(UserDTO user)
        {
            bool isValid = false;
            // Lưu ý: Đặt tên bảng là [user] trong ngoặc vuông vì user là từ khóa của SQL
            string query = "SELECT COUNT(*) FROM [user] WHERE username = @username AND password = @password";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@password", user.Password);

                    try
                    {
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            isValid = true; // Tìm thấy user trong database
                        }
                    }
                    catch (SqlException ex)
                    {
                        System.Windows.MessageBox.Show("Lỗi truy vấn CSDL: " + ex.Message);
                    }
                }
            }
            return isValid;
        }
    }
}
