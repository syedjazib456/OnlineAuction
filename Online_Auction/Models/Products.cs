using Org.BouncyCastle.Bcpg;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ProductTitle { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ProductDescription { get; set; }
        [NotMapped]
        public IFormFile ProductPicture { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ProductImage { get; set; }

        [NotMapped]
        public IFormFile ProductPicture1 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string ProductImage1 { get; set; } = "0";

        [NotMapped]
        public IFormFile ProductPicture2 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string ProductImage2 { get; set; } = "0";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinBidPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CurrBidPrice { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "date")]
        public DateTime AuctionStartDate { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "date")]
        public DateTime AuctionEndDate { get; set; }
        [Required]
        [Column(TypeName = "varchar(10)")]
        public string ProductStatus { get; set; }
        [Required]
        [Column(TypeName = "time")]
        public TimeSpan TimeStamp { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category category { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual Register User { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string SoldToUserId { get; set; } = "none";
    }
}
