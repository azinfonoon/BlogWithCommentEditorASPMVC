using System.ComponentModel.DataAnnotations;

namespace BlogWithCommentEditorASPMVC.Areas.Admin.Models.Dtos.Blog
{
    public class CreateBlogDto
    {
        [Required]
        [MaxLength(500)]
        public required string Title { get; set; }

        [Required]
        public required string Content { get; set; }

    }
}
