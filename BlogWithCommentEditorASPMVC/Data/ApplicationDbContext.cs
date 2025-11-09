using BlogWithCommentEditorASPMVC.Models.Entities.Comment;
using BlogWithCommentEditorASPMVC.Models.Entities.User;
using BlogWithCommentEditorASPMVC.Models.Entities.Blog;

using Microsoft.EntityFrameworkCore;

namespace BlogWithCommentEditorASPMVC.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<BlogPst> BlogPosts { get; set; }
        public DbSet<CommentEntity> Comments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUser → BlogPosts
            modelBuilder.Entity<BlogPst>()
                .HasOne(bp => bp.AppUser)
                .WithMany(u => u.BlogPsts)
                .HasForeignKey(bp => bp.AppUserId)
                .OnDelete(DeleteBehavior.Restrict); // prevent cascade


            // AppUser → Comments
            modelBuilder.Entity<CommentEntity>()
                .HasOne(c => c.AppUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Restrict); // prevent cascade

            // BlogPost → Comments
            modelBuilder.Entity<CommentEntity>()
                .HasOne(c => c.BlogPst)
                .WithMany(bp=>bp.Comments)
                .HasForeignKey(c => c.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade); // safe to cascade here
        }
    }

   
}
