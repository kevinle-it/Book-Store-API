using System;
namespace BookStoreAPI.Dto.Cart.Response
{
    public class BookCartResponse
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public double Price { get; set; }
        public string Summary { get; set; }
        public int StockAvailable { get; set; }
        public string CoverImageURL { get; set; }
        public string CoordinateX { get; set; }
        public string CoordinateY { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}

