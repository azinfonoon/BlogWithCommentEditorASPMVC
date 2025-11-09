using System.ComponentModel.DataAnnotations;

namespace BlogWithCommentEditorASPMVC.Models.Dtos.User
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(50)]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
