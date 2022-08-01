﻿using System;
using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Data.Entities
{
    public class Book
    {
        [Key]
        [Required]
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
    }
}

