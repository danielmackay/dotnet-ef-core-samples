using Microsoft.Data.SqlTypes;

namespace FullTextSearch;

public class Article
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required string Contents { get; set; }

    // Used by hybrid search. Non-nullable on purpose: VectorSearch() only accepts a non-nullable
    // vector property. EF Core 11 doesn't read it when it loads an Article.
    public SqlVector<float> Embedding { get; set; }
}
