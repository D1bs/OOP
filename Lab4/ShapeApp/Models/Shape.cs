using System.Xml;
using ShapeContracts;

namespace ShapeApp.Models;

/// <summary>
/// Convenience abstract base class for shapes defined inside the host application.
/// Implements IShape so built-in shapes and plugin shapes are treated identically
/// by the registry, serializer, and UI.
/// </summary>
public abstract class Shape : IShape
{
    public abstract string ShapeType { get; }
    public abstract void WriteXml(XmlWriter writer);
    public abstract void ReadXml(XmlElement element);
    public abstract string Describe();
    public abstract void InputProperties();
    public abstract IShape CreateInstance();
}
