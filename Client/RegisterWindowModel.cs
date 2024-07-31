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
