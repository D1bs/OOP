using ShapeSerializer.Models;

namespace ShapeSerializer.Serialization;

/// <summary>
/// Central registry of known Shape types.
/// 
/// Design – Prototype pattern:
///   Each concrete Shape registers itself by calling ShapeRegistry.Register(new MyShape()).
///   The registry stores one prototype per type and uses CreateInstance() to produce new objects.
///   
/// Key guarantees required by the assignment:
///   ✓ No if-else / switch-case anywhere in this class.
///   ✓ No reflection.
///   ✓ Adding a new shape class = register it in Program.cs – NOTHING else changes.
/// </summary>
public static class ShapeRegistry
{
    // Maps ShapeType string → prototype instance
    private static readonly Dictionary<string, Shape> _prototypes = new();

    /// <summary>
    /// Registers a prototype for a concrete shape type.
    /// Called once per type at application startup.
    /// </summary>
    public static void Register(Shape prototype)
    {
        _prototypes[prototype.ShapeType] = prototype;
    }

    /// <summary>
    /// Creates a new blank instance of the shape identified by <paramref name="typeName"/>.
    /// Uses the prototype's own CreateInstance() – no switch, no reflection.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the type name is not registered.</exception>
    public static Shape Create(string typeName)
    {
        if (!_prototypes.TryGetValue(typeName, out var proto))
            throw new KeyNotFoundException($"Unknown shape type: '{typeName}'. Register it first.");
        return proto.CreateInstance();
    }

    /// <summary>Returns all registered shape type names, sorted alphabetically.</summary>
    public static IEnumerable<string> RegisteredTypes =>
        _prototypes.Keys.OrderBy(k => k);
}
