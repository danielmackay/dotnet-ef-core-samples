using Microsoft.Data.SqlTypes;

namespace FullTextSearch;

/// <summary>
/// Fixed sample articles. The embeddings are hand-picked, the same approach as the VectorSearch
/// sample, so the sample runs offline and gives the same results on every run. A real app gets
/// embeddings from an embedding model. Here each of the 4 dimensions stands for one topic:
/// [gardening, healthy food, cars, cooking].
/// </summary>
public static class SampleArticles
{
    public const int Dimensions = 4;

    /// <summary>The embedding of the search text "fresh produce from my garden".</summary>
    public static readonly SqlVector<float> GardenProduce = Vector(0.80f, 0.60f, 0.00f, 0.00f);

    public static Article[] Create() =>
    [
        new()
        {
            Title = "Growing vegetables at home",
            Contents = "A beginner's guide to growing your own vegetables. Vegetables need sun, water and good soil, so plant them in spring.",
            Embedding = Vector(0.90f, 0.40f, 0.00f, 0.10f),
        },
        new()
        {
            Title = "Fruit salad",
            Contents = "Apples, pears and berries make a fresh fruit salad for summer.",
            Embedding = Vector(0.00f, 0.80f, 0.00f, 0.50f),
        },
        new()
        {
            Title = "Car engine maintenance",
            Contents = "Change the oil and check the engine belts every year.",
            Embedding = Vector(0.00f, 0.00f, 1.00f, 0.00f),
        },
        new()
        {
            Title = "Winter soup",
            Contents = "A hearty soup of carrots, leeks and other winter vegetables.",
            Embedding = Vector(0.10f, 0.20f, 0.00f, 0.95f),
        },
        new()
        {
            Title = "Healthy eating",
            Contents = "Eat fruit every day and add greens to every meal.",
            Embedding = Vector(0.30f, 0.90f, 0.00f, 0.20f),
        },
    ];

    private static SqlVector<float> Vector(params float[] values) => new(values);
}
