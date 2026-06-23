using System.Xml;
using ShapeContracts;
using ShapeApp.Processors;

namespace ShapeApp.Serialization;

public static class XmlShapeSerializer
{
    private const string RootElement  = "Shapes";
    private const string ShapeElement = "Shape";
    private const string TypeAttr     = "type";

    public static void Serialize(IEnumerable<IShape> shapes, string filePath,
                                 ProcessorRegistry? processors = null)
    {
        var xmlContent  = ShapesToXml(shapes);
        var fileContent = processors is not null
            ? processors.ApplyBeforeSave(xmlContent)
            : xmlContent;
        File.WriteAllText(filePath, fileContent, System.Text.Encoding.UTF8);
    }

    public static List<IShape> Deserialize(string filePath,
                                           ProcessorRegistry? processors = null)
    {
        var fileContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        var xmlContent  = processors is not null
            ? processors.ApplyAfterLoad(fileContent)
            : fileContent;
        return XmlToShapes(xmlContent);
    }

    private static string ShapesToXml(IEnumerable<IShape> shapes)
    {
        var sb       = new System.Text.StringBuilder();
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
        // writer must be disposed BEFORE sb.ToString() so the buffer is flushed
        using (var writer = XmlWriter.Create(sb, settings))
        {
            writer.WriteStartElement(RootElement);
            foreach (var shape in shapes)
            {
                writer.WriteStartElement(ShapeElement);
                writer.WriteAttributeString(TypeAttr, shape.ShapeType);
                shape.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        } // <-- Dispose() flushes the buffer here
        return sb.ToString();
    }

    private static List<IShape> XmlToShapes(string xmlContent)
    {
        var result = new List<IShape>();
        var doc    = new XmlDocument();
        doc.LoadXml(xmlContent);
        foreach (XmlElement elem in doc.DocumentElement!.ChildNodes)
        {
            var shape = ShapeRegistry.Create(elem.GetAttribute(TypeAttr));
            shape.ReadXml(elem);
            result.Add(shape);
        }
        return result;
    }
}