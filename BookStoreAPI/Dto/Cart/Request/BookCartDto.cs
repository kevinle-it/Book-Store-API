using System;
namespace BookStoreAPI.Dto.Cart.Request
{
    public class BookCartDto
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}

