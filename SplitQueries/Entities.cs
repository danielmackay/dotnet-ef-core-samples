namespace SplitQueries;

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public List<Post> Posts { get; set; } = new List<Post> { };
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Tag> Tags { get; set; } = new List<Tag> { };
    public List<Blog> Blogs { get; set; } = new List<Blog> { };
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Post> Posts { get; set; } = new List<Post> { };
}
