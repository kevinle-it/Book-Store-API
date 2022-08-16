using System;
using BookStoreAPI.Dto.User.Response;
using System.Collections.Generic;

namespace BookStoreAPI.Dto.Order.Response
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public UserResponse Owner { get; set; }
        public ICollection<BookOrderResponse> ListBooks { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime Date { get; set; }
    }
}

