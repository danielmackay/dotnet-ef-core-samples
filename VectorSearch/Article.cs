using Microsoft.Data.SqlTypes;

namespace VectorSearch;

public class Article
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }

    // Non-nullable on purpose: VectorSearch() only accepts a non-nullable vector property.
    // SqlVector<float> is a struct, so "no value loaded" shows up as Embedding.IsNull.
    public SqlVector<float> Embedding { get; set; }
}
