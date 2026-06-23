using System.Xml;

namespace ShapeContracts;

/// <summary>
/// Contract that every shape plugin must implement.
/// The host application (ShapeApp) depends ONLY on this interface —
/// it never references concrete plugin types directly.
///
/// A plugin DLL registers one or more shapes by returning prototype instances
/// from GetShapes(). The host calls ShapeRegistry.Register() for each prototype.
/// </summary>
public interface IShapePlugin
{
    /// <summary>
    /// Human-readable name of the plugin shown in the UI.
    /// Example: "Pentagon Plugin"
    /// </summary>
    string PluginName { get; }

    /// <summary>
    /// Version string for display and signature verification.
    /// Example: "1.0.0"
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Returns one prototype instance per shape type the plugin provides.
    /// The host iterates this collection and registers each prototype.
    /// </summary>
    IEnumerable<IShape> GetShapes();
}

/// <summary>
/// Shape contract that plugin-defined shapes must fulfill.
/// Mirrors the abstract Shape class from Lab 3 but lives in the shared
/// contracts assembly so plugins can reference it without depending on
/// the host application.
/// </summary>
public interface IShape
{
    /// <summary>Unique type tag stored as the XML "type" attribute.</summary>
    string ShapeType { get; }

    /// <summary>Writes this shape's properties as XML child elements.</summary>
    void WriteXml(XmlWriter writer);

    /// <summary>Populates this shape's properties from an XML element.</summary>
    void ReadXml(XmlElement element);

    /// <summary>Returns a human-readable description with computed geometry.</summary>
    string Describe();

    /// <summary>Interactively prompts the user to enter all properties.</summary>
    void InputProperties();

    /// <summary>Creates a new blank instance of the same concrete type (Prototype pattern).</summary>
    IShape CreateInstance();
}
