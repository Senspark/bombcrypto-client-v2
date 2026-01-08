using System;
using System.Text.RegularExpressions;

namespace Game.Dialog.Connects {
    public static class CreateAccountHelper {
        public const int ForgotPwdTokenLength = 64;
        public const int NickNameMinLength = 4;
        public const int NickNameMaxLength = 10;
        public const int UsrMinLength = 6;
        public const int UsrMaxLength = 20;
        public const int PwdMinLength = 6;
        public const int PwdMaxLength = 20;

        public static bool CheckUserName(string str) {
            return ValidateUserName(str);
        }

        public static bool CheckPassword(string str) {
            return ValidatePassword(str);
        }

        public static bool CheckPassword2(string password1, string password2) {
            CheckPassword(password1);
            CheckPassword(password2);
            if (password1 != password2) {
                throw new Exception("Password does not match");
            }
            return true;
        }

        public static bool CheckNickName(string nickName) {
            const string pattern = @"^[a-zA-Z0-9_-]{4,10}$";
            ValidateString("Nick name", nickName, NickNameMinLength, NickNameMaxLength);
            var isValidUsername = Regex.IsMatch(nickName, pattern);
            if (!isValidUsername) {
                throw new Exception($"Not a valid Nick name");
            }
            return true;
        }

        public static bool CheckEmail(string str) {
            ValidateString("Email", str, 5, 100);
            return ValidateEmail(str);
        }

        public static bool CheckForgotPasswordToken(string str) {
            return ValidateString("Code", str, ForgotPwdTokenLength, ForgotPwdTokenLength);
        }

        private static bool ValidateUserName(string usrName) {
            ValidateString("User name", usrName, UsrMinLength, UsrMaxLength);
            const string pattern = @"^[a-zA-Z0-9_-]{6,20}$";
            var isValidUsername = Regex.IsMatch(usrName, pattern);
            if (!isValidUsername) {
                throw new Exception($"Not a valid User name");
            }
            return true;
        }
        
        private static bool ValidatePassword(string pwd) {
            ValidateString("Password", pwd, PwdMinLength, PwdMaxLength);
            const string pattern = @"^[^\s]{6,20}$";
            var isValidUsername = Regex.IsMatch(pwd, pattern);
            if (!isValidUsername) {
                throw new Exception($"Not a valid User name");
            }
            return true;
        }

        private static bool ValidateString(string kind, string str, int min, int max) {
            if (string.IsNullOrWhiteSpace(str)) {
                throw new Exception($"{kind} must not empty");
            }
            if (str.Length < min || str.Length > max) {
                throw new Exception($"{kind} must be between {min} and {max} characters");
            }
            return true;
        }

        private static bool ValidateEmail(string email) {
            const string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            var isValid = Regex.IsMatch(email, pattern);
            if (!isValid) {
                throw new Exception("Email is not valid");
            }
            return true;
        }
    }
}