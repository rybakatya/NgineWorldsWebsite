namespace SiteAPI.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;      // required, max len
        public string Slug { get; set; } = string.Empty;       // required, unique (e.g., "my-first-post")
        public string Description { get; set; } = string.Empty;// optional, short summary
        public string Body { get; set; } = string.Empty;       // markdown/html

        // Author relationship
        public int AuthorId { get; set; }
        public User Author { get; set; } = default!;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // optimistic concurrency
        public byte[] RowVersion { get; set; } = default!;
    }
}
