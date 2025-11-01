using BlogWithCommentEditorASPMVC.Models.Entities.Comment;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWithCommentEditorASPMVC.Models.Entities.User
{
    [Table("AppUsers")]
    public class AppUser
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? PasswordHash { get; set; }

        public required string Role { get; set; }

        public DateTime RegisteredAt { get; set; }



        // Navigation properties can be added here for relationships with other entities (comment)
        public List<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
        public List<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    }
}
