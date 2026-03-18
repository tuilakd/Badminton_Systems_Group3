using System;
using System.Collections.Generic;
using System.Text;

namespace Badminton_Systems_Group3.DTO
{
    internal class UserDTO
    {
            public string Username { get; set; }
            public string Password { get; set; }

            public UserDTO(string username, string password)
            {
                Username = username;
                Password = password;
            }
    }
}
