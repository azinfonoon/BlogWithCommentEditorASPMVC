using BlogWithCommentEditorASPMVC.Models.Entities.Blog;
using BlogWithCommentEditorASPMVC.Models.Entities.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWithCommentEditorASPMVC.Models.Entities.Comment
{
    [Table("Comments")]
    public class CommentEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Text { get; set; }

        [Required]
        [MaxLength(100)]
        public required string AuthorName { get; set; }

        public DateTime CreatedAt { get; set; }


        // Foreign key to AppUser
        public Guid AppUserId { get; set; }

        // Navigation property
        public AppUser AppUser { get; set; }



        // Foreign key to BlogPost
        public Guid BlogPostId { get; set; }

        // Navigation property
        public BlogPst BlogPst { get; set; }
    }
}
