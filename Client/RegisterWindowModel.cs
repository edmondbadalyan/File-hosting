using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class RegisterWindowModel
    {
        public string Email { get; set; }
        public bool AgreeWithTerms { get; set; }
        public bool Allowed { get; set; }
        public string Terms { get; set; }

        public RegisterWindowModel(string email, bool agreeWithTerms, bool allowed, string terms) {
            Email = email;
            AgreeWithTerms = agreeWithTerms;
            Allowed = allowed;
            Terms = terms;
        }

        public RegisterWindowModel() {
            Email = string.Empty;
            AgreeWithTerms = false;
            Allowed = false;
            Terms = "For now nothing";
        }
    }
}
