namespace Online_Auction.Models
{
    public class GetProductsForHomePage
    {
        public List<Products> LatestItems { get; set; }
        public List<Products> TopBidedItems { get; set; }
        public List<CategoryWithProductCount> CategoryWithCount { get; set; }

    }
}
