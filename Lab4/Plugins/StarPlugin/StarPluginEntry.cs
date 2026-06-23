using ShapeContracts;

namespace StarPlugin;

/// <summary>
/// Plugin entry point for the Star shape.
/// Drop StarPlugin.dll into the plugins/ folder — no host code changes needed.
/// </summary>
public class StarPluginEntry : IShapePlugin
{
    public string PluginName => "Star Plugin";
    public string Version    => "1.0.0";

    public IEnumerable<IShape> GetShapes()
    {
        yield return new Star();
    }
}
