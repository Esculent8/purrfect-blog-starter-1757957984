using System;
using System.ComponentModel.DataAnnotations;

namespace purrfect_blog_starter_1757957984.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [Required, StringLength(100)]
        public string AuthorUsername { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}