using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject.Models.Request
{
    public class LoginRequest
    {
        public string email { get; set; }
        public string password { get; set; }
        public bool returnSecureToken { get; set; }
    }
}
