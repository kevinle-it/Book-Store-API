using System;
using System.Collections.Generic;
using BookStoreAPI.Dto.User.Response;

namespace BookStoreAPI.Dto.Cart.Response
{
    public class CartResponse
    {
        public int CartId { get; set; }
        public UserResponse Owner { get; set; }
        public ICollection<BookCartResponse> ListBooks { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

