
namespace Online_Auction.Models
{
    public class GetAllInfoModel
    {
        public Register User { get; set; }
        public List<Products> Items { get; set; }
        public List<Bids> Bids { get; set; }
        public List<Report> reports { get; set; }
        public List<Products> ItemBought { get; set; }
    }
}
