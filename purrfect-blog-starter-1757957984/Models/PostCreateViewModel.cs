using System.ComponentModel.DataAnnotations;

namespace purrfect_blog_starter_1757957984.Models
{
    public class PostCreateViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [StringLength(100)]
        public string Category { get; set; }
    }
}