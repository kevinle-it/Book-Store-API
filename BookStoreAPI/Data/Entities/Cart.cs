using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace BookStoreAPI.Data.Entities
{
    [SwaggerSchema(ReadOnly = true)]
    public class Cart
    {
        [Key]
        [SwaggerSchema(ReadOnly = true)]
        public int CartId { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public string UserId { get; set; }
        public User Owner { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public ICollection<BookCart> ListBooks { get; set; }
    }
}

