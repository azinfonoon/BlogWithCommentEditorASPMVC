namespace BlogWithCommentEditorASPMVC.Areas.Admin.Models.Dtos.Blog
{
    public class BlogIndexDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; }
    }
}
