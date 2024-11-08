using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ReportMessage { get; set; }
        public string FromUserId { get; set; }
        [ForeignKey("FromUserId")]
        public Register fromuser { get; set; }
        public string ToUserId { get; set; }
        [ForeignKey("ToUserId")]
        public Register touser { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string status { get; set; } = "active";
    }
}
