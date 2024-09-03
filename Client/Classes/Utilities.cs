
﻿using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    public static class Utilities
    {
        public static bool ValidateEmail(string email) {
            try {
                MailAddress m = new MailAddress(email);
                return true;
            }
            catch (FormatException) {
                MessageBox.Show("Incorrect email");
                return false;
            }
        }

        public static bool ValidatePassword(PasswordBox password) {
            string errorMessage = string.Empty;
            bool allowed = false;

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (password is null || string.IsNullOrWhiteSpace(password.Password)) {
                errorMessage = "Password should not be empty";
            }
            else if (!hasLowerChar.IsMatch(password.Password)) {
                errorMessage = "Password should contain At least one lower case letter";
            }
            else if (!hasUpperChar.IsMatch(password.Password)) {
                errorMessage = "Password should contain At least one upper case letter";
            }
            else if (!hasMiniMaxChars.IsMatch(password.Password)) {
                errorMessage = "Password should not be less than or greater than 12 characters";
            }
            else if (!hasNumber.IsMatch(password.Password)) {
                errorMessage = "Password should contain At least one numeric value";
            }
            else if (!hasSymbols.IsMatch(password.Password)) {
                errorMessage = "Password should contain At least one special case characters";
            }
            else {
                allowed = true;
            }

            if (errorMessage != string.Empty) {
                MessageBox.Show(errorMessage);
            }

            return allowed;

        }

        public static string FormatBytes(long bytes) {
            string[] Suffix = { "б", "Кб", "Мб", "Гб", "Тб" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024) {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
    }
}
