using BlogWithCommentEditorASPMVC.Models.Entities.Comment;
using BlogWithCommentEditorASPMVC.Models.Entities.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWithCommentEditorASPMVC.Models.Entities.Blog
{
    [Table("BlogPosts")]
    public class BlogPst
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Title { get; set; }

        [Required]
        public required string Content { get; set; }

        [Required]
        public required string ImageThumbnail { get; set; }

        public DateTime CreatedAt { get; set; }

        // Foreign key to AppUser
        public Guid AppUserId { get; set; }

        // navigation properties can be added here for relationships with other entities (e.g., comments)
        public AppUser? AppUser { get; set; }
        public List<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
    }
}
