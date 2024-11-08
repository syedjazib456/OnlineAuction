using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Auction.Models
{
    public class Category
    {
        [Key]
        public int CatId { get; set; }

        [Required(ErrorMessage = "Please Enter Category Name")]
        [Column(TypeName = "varchar(100)")]
        public string CatName { get; set; }
        [NotMapped]
        public IFormFile CatPicture { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string CatImg {  get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string CatDescription { get; set; }
    }
}
