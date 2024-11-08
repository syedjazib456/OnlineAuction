using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models
{
    public class Notifications
    {
        [Key]
        public int NotificationId { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string NotificationMessage { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string NotificationTo { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime NotificationTimeStamp { get; set; }
        [Required]
        [Column(TypeName = "varchar(10)")]
        public string NotificationStatus { get; set; } = "unread";
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Link {  get; set; }

    }
}
