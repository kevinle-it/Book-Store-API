using System;
using System.Collections.Generic;

namespace BookStoreAPI.Dto.Cart.Request
{
    public class CartDto
    {
        public List<BookCartDto> ListBooks { get; set; }
    }
}

