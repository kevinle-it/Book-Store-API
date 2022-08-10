using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreAPI.Data.Entities
{
    public class BookCart
    {
        [ForeignKey("CurrentCart")]
        public int CartId { get; set; }
        public Cart CurrentCart { get; set; }

        [ForeignKey("CurrentBook")]
        public int BookId { get; set; }
        public Book CurrentBook { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
    }
}

