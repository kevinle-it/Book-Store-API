using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace BookStoreAPI.Data.Entities
{
    public class Order
    {
        [Key]
        [SwaggerSchema(ReadOnly = true)]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public string UserId { get; set; }
        public User Owner { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public ICollection<BookOrder> ListBooks { get; set; }
    }
}
