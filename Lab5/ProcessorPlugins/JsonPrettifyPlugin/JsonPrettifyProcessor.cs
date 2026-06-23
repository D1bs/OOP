using System.Text;
using System.Text.Json;
using ShapeContracts;

namespace JsonPrettifyPlugin;

/// <summary>
/// Processor Plugin 2 — JSON Prettify / Annotate.
///
/// Forward  (before save): re-formats whatever JSON it receives with a
///                         configurable indent size and appends a metadata
///                         header field ("_savedAt", "_shapeCount").
///
/// Reverse  (after load):  strips the metadata fields and returns clean JSON
///                         so the next processor (XmlToJson reverse) can parse it.
///
/// This plugin sits AFTER XmlToJsonPlugin in the pipeline:
///   XML → [XmlToJson] → JSON → [JsonPrettify] → annotated JSON → file
///   file → [JsonPrettify reverse] → clean JSON → [XmlToJson reverse] → XML
///
/// Configurable: IndentSize (number of spaces, 2 or 4).
/// </summary>
public class JsonPrettifyProcessor : IProcessorPlugin
{
    // ── Identity ──────────────────────────────────────────────────────────

    public string ProcessorId  => "JsonPrettify";
    public string DisplayName  => "JSON Prettify & Annotate";
    public string Description  => "Adds metadata header and pretty-prints JSON with configurable indent.";
    public string Version      => "1.0.0";
    public bool   IsConfigurable => true;

    // ── Configuration ─────────────────────────────────────────────────────

    private int _indentSize = 2;

    public IReadOnlyDictionary<string, string> GetSettings() =>
        new Dictionary<string, string> { ["IndentSize"] = _indentSize.ToString() };

    public void ApplySetting(string key, string value)
    {
        if (key == "IndentSize" && int.TryParse(value, out var n) && n is 2 or 4)
            _indentSize = n;
    }

    // ── Forward: add metadata ─────────────────────────────────────────────

    /// <summary>
    /// Parses incoming JSON, injects metadata fields, re-serializes with
    /// the configured indent size.
    /// </summary>
    public string ProcessBeforeSave(string jsonContent)
    {
        using var input  = JsonDocument.Parse(jsonContent);
        var shapesArray  = input.RootElement.GetProperty("Shapes");
        int shapeCount   = shapesArray.GetArrayLength();

        var options  = new JsonWriterOptions { Indented = true };
        var buffer   = new MemoryStream();
        using var jw = new Utf8JsonWriter(buffer, options);

        jw.WriteStartObject();

        // Metadata header — stripped on reverse
        jw.WriteString("_savedAt",    DateTime.UtcNow.ToString("o"));
        jw.WriteNumber("_shapeCount", shapeCount);

        // Re-emit the Shapes array unchanged
        jw.WritePropertyName("Shapes");
        shapesArray.WriteTo(jw);

        jw.WriteEndObject();
        jw.Flush();

        // Re-indent with configurable size (JsonWriter always uses 2; we post-process)
        var raw = Encoding.UTF8.GetString(buffer.ToArray());
        return _indentSize == 2 ? raw : ReIndent(raw, _indentSize);
    }

    // ── Reverse: strip metadata ───────────────────────────────────────────

    /// <summary>
    /// Removes the metadata header fields and returns the core JSON
    /// with just the "Shapes" array.
    /// </summary>
    public string ProcessAfterLoad(string fileContent)
    {
        using var input = JsonDocument.Parse(fileContent);
        var shapesArray = input.RootElement.GetProperty("Shapes");

        var options  = new JsonWriterOptions { Indented = true };
        var buffer   = new MemoryStream();
        using var jw = new Utf8JsonWriter(buffer, options);

        jw.WriteStartObject();
        jw.WritePropertyName("Shapes");
        shapesArray.WriteTo(jw);
        jw.WriteEndObject();
        jw.Flush();

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>Replaces 2-space indentation with the desired indent size.</summary>
    private static string ReIndent(string json, int spaces)
    {
        var indent  = new string(' ', spaces);
        var sb      = new StringBuilder();
        int level   = 0;
        bool inStr  = false;

        foreach (char c in json)
        {
            if (c == '"' ) inStr = !inStr;
            if (!inStr)
            {
                if (c == '{' || c == '[') { sb.Append(c); level++; continue; }
                if (c == '}' || c == ']') { level--; sb.Append(c); continue; }
                if (c == '\n')            { sb.Append(c); sb.Append(string.Concat(Enumerable.Repeat(indent, level))); continue; }
            }
            sb.Append(c);
        }
        return sb.ToString();
    }
}
