using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models
{
    public class Bids
    {
        [Key]
        public int BidId { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual Register User { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Products Products { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BidAmount { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TimeStamp { get; set; }
    }
}
