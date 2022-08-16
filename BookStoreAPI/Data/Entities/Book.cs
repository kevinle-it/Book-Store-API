using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookStoreAPI.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace BookStoreAPI.Data.Entities
{
    public class Book
    {
        [Key]
        [Required]
        [SwaggerSchema(ReadOnly = true)]
        public int BookId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public string Summary { get; set; }

        [Required]
        public int Quantity { get; set; }

        public string CoverImageURL { get; set; }

        [Required]
        public string CoordinateX { get; set; }

        [Required]
        public string CoordinateY { get; set; }

        [SwaggerExclude]
        public ICollection<BookCart> ListCartsHaveThisBook { get; set; }

        [SwaggerExclude]
        public ICollection<BookOrder> ListOrdersHaveThisBook { get; set; }
    }
}

