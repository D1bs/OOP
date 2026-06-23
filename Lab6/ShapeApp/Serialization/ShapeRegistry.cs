using ShapeContracts;

namespace ShapeApp.Serialization;

/// <summary>
/// Registry of all known shape types — both built-in and plugin-provided.
///
/// Pattern: Prototype
///   Each type is represented by one stored prototype (IShape).
///   CreateInstance() on the prototype produces a new blank object without
///   using reflection or switch-case.
///
/// Thread-safety: registration happens at startup before concurrent access.
/// </summary>
public static class ShapeRegistry
{
    private static readonly Dictionary<string, IShape> _prototypes = new();

    /// <summary>
    /// Registers a shape prototype.
    /// Called at startup for built-in shapes and after plugin load for plugin shapes.
    /// Overwrites any previously registered prototype for the same ShapeType.
    /// </summary>
    public static void Register(IShape prototype)
    {
        _prototypes[prototype.ShapeType] = prototype;
    }

    /// <summary>
    /// Creates a new blank instance of the shape with the given type name.
    /// Uses Prototype.CreateInstance() — no switch, no reflection.
    /// </summary>
    public static IShape Create(string typeName)
    {
        if (!_prototypes.TryGetValue(typeName, out var proto))
            throw new KeyNotFoundException(
                $"Unknown shape type: '{typeName}'. " +
                $"Is the required plugin loaded? Known types: {string.Join(", ", RegisteredTypes)}");
        return proto.CreateInstance();
    }

    /// <summary>Returns registered type names sorted alphabetically.</summary>
    public static IEnumerable<string> RegisteredTypes =>
        _prototypes.Keys.OrderBy(k => k);
}
