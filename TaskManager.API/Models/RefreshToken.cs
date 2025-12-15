using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class RefreshToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;  //initializes the property to an empty string by default.

        [Required]
        public  DateTime Expires { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? Revoked { get; set; }

        //Navigation property to the ApplicationUser
        
        public string UserId { get; set; }
        //This is optional.It allows you to easily navigate from a RefreshToken to its related ApplicationUser ,
        [ForeignKey("UserId")]
        public ApplicationUser User{ get; set; }

    }
}
