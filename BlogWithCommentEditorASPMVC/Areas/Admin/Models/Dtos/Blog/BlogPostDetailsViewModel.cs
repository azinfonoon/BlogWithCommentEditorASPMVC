namespace BlogWithCommentEditorASPMVC.Areas.Admin.Models.Dtos.Blog
{
    public class BlogPostDetailsViewModel
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string ImageThumbnail { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = default!;
        public int CommentCount { get; set; }
    }
}
