using ShapeSerializer.Models;

namespace ShapeSerializer.UI;

/// <summary>
/// In-memory ordered collection of Shape objects.
/// Provides Add, Remove, Edit and enumeration capabilities.
/// </summary>
public class ShapeList
{
    private readonly List<Shape> _items = new();

    /// <summary>Number of shapes currently in the list.</summary>
    public int Count => _items.Count;

    /// <summary>Read-only view of all shapes.</summary>
    public IReadOnlyList<Shape> Items => _items;

    /// <summary>Appends a shape to the end of the list.</summary>
    public void Add(Shape shape) => _items.Add(shape);

    /// <summary>
    /// Removes the shape at the given 0-based <paramref name="index"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public void RemoveAt(int index) => _items.RemoveAt(index);

    /// <summary>
    /// Replaces the shape at <paramref name="index"/> with <paramref name="newShape"/>.
    /// </summary>
    public void ReplaceAt(int index, Shape newShape) => _items[index] = newShape;

    /// <summary>Returns the shape at <paramref name="index"/>.</summary>
    public Shape GetAt(int index) => _items[index];

    /// <summary>Replaces the entire list content with <paramref name="shapes"/>.</summary>
    public void ReplaceAll(IEnumerable<Shape> shapes)
    {
        _items.Clear();
        _items.AddRange(shapes);
    }
}
