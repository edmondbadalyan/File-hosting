using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class RegisterWindowModel : BindableBase
    {
        public string email;
        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        public bool agreeWithTerms;
        public bool AgreeWithTerms {
            get => agreeWithTerms;
            set => SetProperty(ref agreeWithTerms, value);
        }

        public string Terms { get; set; }

        public RegisterWindowModel(string email, bool agreeWithTerms, string terms) {
            Email = email;
            AgreeWithTerms = agreeWithTerms;
            Terms = terms;
        }

        public RegisterWindowModel() {
            Email = string.Empty;
            AgreeWithTerms = false;
            Terms = "For now nothing";
        }
    }
}
