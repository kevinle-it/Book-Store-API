using System;
using System.Collections.Generic;

namespace BookStoreAPI.Dto.Order.Request
{
    public class OrderDto
    {
        public List<BookOrderDto> ListBooks { get; set; }
    }
}

