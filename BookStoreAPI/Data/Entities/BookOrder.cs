using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreAPI.Data.Entities
{
    public class BookOrder
    {
        [ForeignKey("CurrentOrder")]
        public int OrderId { get; set; }
        public Order CurrentOrder { get; set; }

        [ForeignKey("CurrentBook")]
        public int BookId { get; set; }
        public Book CurrentBook { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
    }
}
