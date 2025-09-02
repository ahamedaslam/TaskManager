namespace TaskManager.DTOs.Auth
{
    public class VerifyOtpRequestDTO
    {
        public string UserName { get; set; }
        public string OTP { get; set; }
    }
}
