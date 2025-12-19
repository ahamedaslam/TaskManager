using System.Security.Cryptography;
using System.Text;

namespace TaskManager.MultiTenant.Helper
{
    public static class OtpHashHelper
    {
        public static string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(otp);
            var hashBytes = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
