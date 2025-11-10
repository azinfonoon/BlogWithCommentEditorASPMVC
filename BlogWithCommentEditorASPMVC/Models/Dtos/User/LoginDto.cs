using System.ComponentModel.DataAnnotations;

namespace BlogWithCommentEditorASPMVC.Models.Dtos.User
{
    public class LoginDto
    {
        [Required]
        [MaxLength(50)]
        public required string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}

