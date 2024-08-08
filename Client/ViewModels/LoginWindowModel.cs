using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class LoginWindowModel : BindableBase
    {
        public string email;
        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        public LoginWindowModel(string email) {
            Email = email;
        }

        public LoginWindowModel() {
            Email = string.Empty;
        }
    }
}
