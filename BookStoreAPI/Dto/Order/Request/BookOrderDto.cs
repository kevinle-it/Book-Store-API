using System;
namespace BookStoreAPI.Dto.Order.Request
{
    public class BookOrderDto
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}

