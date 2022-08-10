using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreAPI.Data.Entities
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public string UserId { get; set; }
        public User Owner { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public ICollection<BookCart> ListBooks { get; set; }
    }
}

