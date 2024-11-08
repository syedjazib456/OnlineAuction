using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models
{
    public class Register:IdentityUser
    {
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string FullName { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Address { get; set; }
        [Required]
        [Column(TypeName = "varchar(10)")]
        public string Status { get; set; } = "active";
        [NotMapped]
        public IFormFile ProfileImage { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string ProfilePicture { get; set; }
        [Required]
        [Column(TypeName = "varchar(15)")]
        public string number {  get; set; }
    }
}
