namespace KimmyEsthi.Reviews;

public sealed class Review
{
    public int ReviewId { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required int Rating { get; set; }
    public required string Content { get; set; }
}
