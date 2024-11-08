using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models
{
    public class Ratings
    {
        [Key]
        public int RatingsId { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public Register User { get; set; }

        [Column(TypeName = "decimal(2,1)")]
        public decimal UserRating { get; set; }
        [Column(TypeName = "decimal(2,1)")]
        public decimal ShipRating { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string FromUser { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public int ProductID { get; set; }
    }
}
