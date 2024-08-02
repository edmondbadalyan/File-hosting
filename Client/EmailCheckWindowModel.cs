using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class EmailCheckWindowModel : BindableBase
    {
        public string Email { get; set; }
        public string Code { get; set; }        
        public bool IsFromRegistration { get; set; }
        public bool IsFromLogin { get; set; }

        public string codeInput;
        public string CodeInput {
            get => codeInput;
            set => SetProperty(ref codeInput, value);
        }

        public EmailCheckWindowModel(string email, string code, string codeInput, bool isFromRegistration, bool isFromLogin) : this(email) {
            Code = code;
            CodeInput = codeInput;
            IsFromRegistration = isFromRegistration;
            IsFromLogin = isFromLogin;
        }

        public EmailCheckWindowModel(string email) {
            Email = email;
            Code = string.Empty;
            CodeInput = string.Empty;
            IsFromRegistration = false;
            IsFromLogin = false;
        }

        public EmailCheckWindowModel() {
            Email = string.Empty;
            Code = string.Empty;
            CodeInput = string.Empty;
            IsFromRegistration = false;
            IsFromLogin = false;
        }
    }
}
