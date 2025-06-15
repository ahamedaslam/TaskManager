using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTOs.Auth
{
    public class RegisterRequestDTO
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string[] Roles { get; set; }

        //[Required]
        public string? TenantId { get; set; } // Optional for Admin, Required for Normal
    }
}
