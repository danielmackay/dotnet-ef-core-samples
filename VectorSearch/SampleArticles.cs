using Microsoft.Data.SqlTypes;

namespace VectorSearch;

/// <summary>
/// Fixed sample data with hand-picked embeddings, so the sample runs offline and gives the same
/// results on every run. A real app gets embeddings from an embedding model (hundreds or thousands
/// of dimensions). Here each of the 4 dimensions stands for one topic:
/// [databases, cooking, travel, space].
/// </summary>
public static class SampleArticles
{
    public const int Dimensions = 4;

    /// <summary>The embedding of the search text "how do I make my database queries faster?".</summary>
    public static readonly SqlVector<float> DatabaseQuery = Vector(1.00f, 0.00f, 0.00f, 0.05f);

    public static Article[] Create() =>
    [
        new() { Title = "Tuning SQL Server indexes", Body = "How indexes speed up database queries and when to rebuild them.", Embedding = Vector(0.95f, 0.05f, 0.00f, 0.10f) },
        new() { Title = "Vector databases explained", Body = "Storing embeddings in a database and searching them by similarity.", Embedding = Vector(0.85f, 0.00f, 0.00f, 0.40f) },
        new() { Title = "Sourdough bread at home", Body = "Feeding a starter and baking a crusty loaf in your own kitchen.", Embedding = Vector(0.00f, 0.95f, 0.15f, 0.00f) },
        new() { Title = "Street food in Bangkok", Body = "A travel guide to the best noodle and curry stalls in the city.", Embedding = Vector(0.00f, 0.70f, 0.70f, 0.00f) },
        new() { Title = "Backpacking through Patagonia", Body = "Trails, huts and weather tips for a hiking trip at the end of the world.", Embedding = Vector(0.00f, 0.05f, 0.95f, 0.10f) },
        new() { Title = "Life on the International Space Station", Body = "What astronauts eat, how they sleep and how they exercise in orbit.", Embedding = Vector(0.05f, 0.10f, 0.20f, 0.95f) },
        new() { Title = "Querying JSON in SQL Server", Body = "Fast database queries over JSON documents with JSON functions and indexes.", Embedding = Vector(0.90f, 0.00f, 0.00f, 0.00f) },
        new() { Title = "Rocket fuel chemistry", Body = "Why rockets burn liquid hydrogen and what that means for space travel.", Embedding = Vector(0.10f, 0.20f, 0.00f, 0.90f) },
    ];

    private static SqlVector<float> Vector(params float[] values) => new(values);
}
