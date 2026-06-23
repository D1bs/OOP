using System.Xml;

namespace ShapeSerializer.Models;

/// <summary>
/// Abstract base class for all geometric shapes.
/// Defines the contract for serialization, deserialization, display, and self-registration.
/// Adding a new shape class NEVER requires modifying this or any other existing class.
/// </summary>
public abstract class Shape
{
    /// <summary>Name label shown in the UI and stored in XML.</summary>
    public abstract string ShapeType { get; }

    /// <summary>
    /// Writes this object's properties as XML child elements into <paramref name="writer"/>.
    /// Called by XmlShapeSerializer – no switch/if needed there.
    /// </summary>
    public abstract void WriteXml(XmlWriter writer);

    /// <summary>
    /// Reads properties from <paramref name="element"/> and populates this instance.
    /// Each concrete class knows its own fields; the serializer does not.
    /// </summary>
    public abstract void ReadXml(XmlElement element);

    /// <summary>
    /// Returns a human-readable, multi-line description of the shape's properties.
    /// </summary>
    public abstract string Describe();

    /// <summary>
    /// Interactively prompts the user to enter values for all properties of this shape.
    /// Each class handles its own fields; no if-else in the caller.
    /// </summary>
    public abstract void InputProperties();

    /// <summary>
    /// Creates a fresh (default) instance of the same concrete type.
    /// Used by ShapeRegistry to instantiate shapes without reflection or switch.
    /// </summary>
    public abstract Shape CreateInstance();
}
