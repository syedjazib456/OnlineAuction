namespace Online_Auction.Models
{
    public class ProfileModel
    {
        public Register User { get; set; }
        public List<Products> Products { get; set; }
        public List<Bids> Bid { get; set; }

        public List<Products> ItemsBought { get; set; }
    }
}
