namespace BlogWithCommentEditorASPMVC.Areas.Admin.Models.Dtos.Blog
{
    public class PageBlogPostIndexViewModel
    {
        public List<BlogIndexDto> BlogPosts { get; set; } = new List<BlogIndexDto>();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
