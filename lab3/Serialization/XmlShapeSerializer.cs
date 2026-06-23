using System.Xml;
using ShapeSerializer.Models;

namespace ShapeSerializer.Serialization;

/// <summary>
/// Handles serialization and deserialization of a list of Shape objects to/from XML files.
/// </summary>
public static class XmlShapeSerializer
{
    private const string RootElement  = "Shapes";
    private const string ShapeElement = "Shape";
    private const string TypeAttr     = "type";

    /// <summary>
    /// Serializes <paramref name="shapes"/> to an XML file at <paramref name="filePath"/>.
    /// Each shape writes its own fields via WriteXml() – no dispatch logic here.
    /// </summary>
    public static void Serialize(IEnumerable<Shape> shapes, string filePath)
    {
        var settings = new XmlWriterSettings { Indent = true };
        using var writer = XmlWriter.Create(filePath, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement(RootElement);

        foreach (var shape in shapes)
        {
            writer.WriteStartElement(ShapeElement);
            writer.WriteAttributeString(TypeAttr, shape.ShapeType);
            shape.WriteXml(writer);          // polymorphic – no switch needed
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    /// <summary>
    /// Deserializes a list of shapes from an XML file at <paramref name="filePath"/>.
    /// Uses ShapeRegistry.Create() to instantiate the correct subclass – no switch needed.
    /// </summary>
    public static List<Shape> Deserialize(string filePath)
    {
        var result = new List<Shape>();
        var doc    = new XmlDocument();
        doc.Load(filePath);

        var root = doc.DocumentElement!;
        foreach (XmlElement elem in root.ChildNodes)
        {
            var typeName = elem.GetAttribute(TypeAttr);
            var shape    = ShapeRegistry.Create(typeName); // prototype factory – no switch
            shape.ReadXml(elem);                           // polymorphic read
            result.Add(shape);
        }

        return result;
    }
}
