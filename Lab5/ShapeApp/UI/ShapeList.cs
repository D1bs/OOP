using ShapeContracts;

namespace ShapeApp.UI;

/// <summary>In-memory ordered collection of IShape objects with CRUD operations.</summary>
public class ShapeList
{
    private readonly List<IShape> _items = new();

    public int Count => _items.Count;
    public IReadOnlyList<IShape> Items => _items;

    public void Add(IShape shape)             => _items.Add(shape);
    public void RemoveAt(int index)           => _items.RemoveAt(index);
    public void ReplaceAt(int index, IShape s)=> _items[index] = s;
    public IShape GetAt(int index)            => _items[index];

    /// <summary>Replaces entire list content with <paramref name="shapes"/>.</summary>
    public void ReplaceAll(IEnumerable<IShape> shapes)
    {
        _items.Clear();
        _items.AddRange(shapes);
    }
}
