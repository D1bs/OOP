using System.Xml;
using ShapeContracts;

namespace ShapeApp.Serialization;

/// <summary>
/// Serializes and deserializes a list of IShape objects to/from an XML file.
///
/// XML format:
/// <![CDATA[
/// <Shapes>
///   <Shape type="Circle"><Radius>5</Radius></Shape>
///   <Shape type="Pentagon"><Sides>5</Sides><SideLength>3</SideLength></Shape>
/// </Shapes>
/// ]]>
///
/// No if-else, no switch, no reflection.
/// Adding a new plugin shape requires zero changes here.
/// </summary>
public static class XmlShapeSerializer
{
    private const string RootElement  = "Shapes";
    private const string ShapeElement = "Shape";
    private const string TypeAttr     = "type";

    /// <summary>Writes all shapes to <paramref name="filePath"/> as indented XML.</summary>
    public static void Serialize(IEnumerable<IShape> shapes, string filePath)
    {
        var settings = new XmlWriterSettings { Indent = true };
        using var writer = XmlWriter.Create(filePath, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement(RootElement);

        foreach (var shape in shapes)
        {
            writer.WriteStartElement(ShapeElement);
            writer.WriteAttributeString(TypeAttr, shape.ShapeType);
            shape.WriteXml(writer);   // each shape writes its own fields
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    /// <summary>
    /// Reads shapes from <paramref name="filePath"/>.
    /// Uses ShapeRegistry.Create() to instantiate — no switch needed.
    /// </summary>
    public static List<IShape> Deserialize(string filePath)
    {
        var result = new List<IShape>();
        var doc    = new XmlDocument();
        doc.Load(filePath);

        foreach (XmlElement elem in doc.DocumentElement!.ChildNodes)
        {
            var typeName = elem.GetAttribute(TypeAttr);
            var shape    = ShapeRegistry.Create(typeName);
            shape.ReadXml(elem);
            result.Add(shape);
        }

        return result;
    }
}
