using System.Text;
using System.Text.Json;
using System.Xml;
using ShapeContracts;

namespace XmlToJsonPlugin;

/// <summary>
/// Processor Plugin — Variant 1: XML → JSON transformation.
///
/// Forward  (before save): converts the XML string produced by XmlShapeSerializer
///                         into a compact JSON representation and writes it to file.
///
/// Reverse  (after load):  reads the JSON file and reconstructs the XML string
///                         that XmlShapeSerializer expects.
///
/// JSON schema produced:
/// {
///   "Shapes": [
///     { "type": "Circle",    "Radius": "5"   },
///     { "type": "Rectangle", "Width": "3", "Height": "4" }
///   ]
/// }
///
/// Configurable setting: Indented (true/false) — pretty-print JSON or compact.
/// </summary>
public class XmlToJsonProcessor : IProcessorPlugin
{
    // ── IProcessorPlugin identity ─────────────────────────────────────────

    public string ProcessorId  => "XmlToJson";
    public string DisplayName  => "XML → JSON Converter";
    public string Description  => "Saves shapes as JSON instead of raw XML; loads JSON back to XML.";
    public string Version      => "1.0.0";
    public bool   IsConfigurable => true;

    // ── Configuration ─────────────────────────────────────────────────────

    /// <summary>When true, the saved JSON is indented (pretty-printed).</summary>
    private bool _indented = true;

    public IReadOnlyDictionary<string, string> GetSettings() =>
        new Dictionary<string, string> { ["Indented"] = _indented.ToString() };

    public void ApplySetting(string key, string value)
    {
        if (key == "Indented" && bool.TryParse(value, out var b))
            _indented = b;
        // Unknown keys are silently ignored
    }

    // ── Forward: XML → JSON ───────────────────────────────────────────────

    /// <summary>
    /// Converts an XML string (from XmlShapeSerializer) to a JSON string.
    /// Called before writing to disk.
    /// </summary>
    public string ProcessBeforeSave(string xmlContent)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xmlContent);

        // Collect all <Shape type="..."> elements into a JSON array
        var options  = new JsonWriterOptions { Indented = _indented };
        var buffer   = new MemoryStream();
        using var jw = new Utf8JsonWriter(buffer, options);

        jw.WriteStartObject();
        jw.WriteStartArray("Shapes");

        foreach (XmlElement shapeEl in doc.DocumentElement!.ChildNodes)
        {
            jw.WriteStartObject();

            // The "type" attribute becomes a JSON field
            jw.WriteString("type", shapeEl.GetAttribute("type"));

            // Every child XML element becomes a key-value string field
            foreach (XmlElement child in shapeEl.ChildNodes)
                jw.WriteString(child.LocalName, child.InnerText);

            jw.WriteEndObject();
        }

        jw.WriteEndArray();
        jw.WriteEndObject();
        jw.Flush();

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    // ── Reverse: JSON → XML ───────────────────────────────────────────────

    /// <summary>
    /// Reconstructs the XML string from JSON file content.
    /// Called after reading from disk, before XmlShapeSerializer parses.
    /// </summary>
    public string ProcessAfterLoad(string fileContent)
    {
        using var doc = JsonDocument.Parse(fileContent);
        var xmlDoc    = new XmlDocument();

        // Do NOT add an XmlDeclaration manually: XmlWriter on a StringBuilder
        // uses UTF-16 internally and will silently produce an empty result when
        // OmitXmlDeclaration=false because the encoding in the declaration
        // ("utf-8") conflicts with the StringBuilder's UTF-16 stream.
        // XmlShapeSerializer.XmlToShapes() only calls doc.LoadXml() which does
        // not need a declaration — the root element is enough.
        var root = xmlDoc.CreateElement("Shapes");
        xmlDoc.AppendChild(root);

        // Rebuild one <Shape type="..."> element per JSON object
        foreach (var shapeJson in doc.RootElement.GetProperty("Shapes").EnumerateArray())
        {
            var shapeEl = xmlDoc.CreateElement("Shape");

            foreach (var prop in shapeJson.EnumerateObject())
            {
                if (prop.Name == "type")
                {
                    // Restore the type attribute
                    shapeEl.SetAttribute("type", prop.Value.GetString()!);
                }
                else
                {
                    // Restore each shape property as an XML child element
                    var fieldEl = xmlDoc.CreateElement(prop.Name);
                    fieldEl.InnerText = prop.Value.GetString() ?? string.Empty;
                    shapeEl.AppendChild(fieldEl);
                }
            }

            root.AppendChild(shapeEl);
        }

        // OmitXmlDeclaration=true is required when writing to a StringBuilder
        // (UTF-16); including the declaration causes XmlWriter to emit nothing.
        var sb       = new StringBuilder();
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
        using (var xw = XmlWriter.Create(sb, settings))
        {
            xmlDoc.DocumentElement!.WriteTo(xw);
        } // Dispose() flushes before sb.ToString()
        return sb.ToString();
    }
}