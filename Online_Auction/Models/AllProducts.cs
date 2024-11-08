namespace Online_Auction.Models
{
    public class AllProducts
    {
        public IEnumerable<Products> products { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalProducts { get; set; }
        public int TotalPages { get; set; }
    }
}
