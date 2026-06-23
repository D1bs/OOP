using ShapeContracts;

namespace PentagonPlugin;

/// <summary>
/// Plugin entry point for the Pentagon shape.
/// The host discovers this class by scanning the DLL for types implementing IShapePlugin.
/// No changes to the host are needed — just drop PentagonPlugin.dll into the plugins/ folder.
/// </summary>
public class PentagonPluginEntry : IShapePlugin
{
    public string PluginName => "Pentagon Plugin";
    public string Version    => "1.0.0";

    /// <summary>
    /// Returns one Pentagon prototype.
    /// The host calls ShapeRegistry.Register() for each returned prototype.
    /// </summary>
    public IEnumerable<IShape> GetShapes()
    {
        yield return new Pentagon();
    }
}
