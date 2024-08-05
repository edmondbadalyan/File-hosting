using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class LoginWindowModel
    {
        public string Email { get; set; }

        public LoginWindowModel(string email) {
            Email = email;
        }

        public LoginWindowModel() {
            Email = string.Empty;
        }
    }
}
