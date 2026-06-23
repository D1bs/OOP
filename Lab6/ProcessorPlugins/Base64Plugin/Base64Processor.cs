using System.Text;
using ShapeContracts;

namespace Base64Plugin;

/// <summary>
/// Processor Plugin 3 — Base64 Encoding.
///
/// Forward  (before save): encodes the incoming content (JSON string) to Base64.
///                         The file on disk contains only a Base64 string.
///
/// Reverse  (after load):  decodes Base64 back to the original content string.
///
/// Position in pipeline (sits AFTER XmlToJson and JsonPrettify):
///   XML → [XmlToJson] → JSON → [JsonPrettify] → annotated JSON → [Base64] → base64 → file
///   file → [Base64 reverse] → annotated JSON → [JsonPrettify reverse] → JSON → [XmlToJson reverse] → XML
///
/// Configurable: Encoding (utf-8 / ascii) — charset used before Base64 conversion.
/// </summary>
public class Base64Processor : IProcessorPlugin
{
    // ── Identity ──────────────────────────────────────────────────────────

    public string ProcessorId  => "Base64";
    public string DisplayName  => "Base64 Encoder";
    public string Description  => "Encodes file content as Base64 on save; decodes on load.";
    public string Version      => "1.0.0";
    public bool   IsConfigurable => true;

    // ── Configuration ─────────────────────────────────────────────────────

    private string _encodingName = "utf-8";

    private Encoding CurrentEncoding =>
        _encodingName == "ascii" ? Encoding.ASCII : Encoding.UTF8;

    public IReadOnlyDictionary<string, string> GetSettings() =>
        new Dictionary<string, string> { ["Encoding"] = _encodingName };

    public void ApplySetting(string key, string value)
    {
        if (key == "Encoding" && (value == "utf-8" || value == "ascii"))
            _encodingName = value;
    }

    // ── Forward: encode to Base64 ─────────────────────────────────────────

    /// <summary>Encodes the content string to a Base64 representation.</summary>
    public string ProcessBeforeSave(string content)
    {
        var bytes = CurrentEncoding.GetBytes(content);
        return Convert.ToBase64String(bytes);
    }

    // ── Reverse: decode from Base64 ───────────────────────────────────────

    /// <summary>Decodes a Base64 string back to the original content.</summary>
    public string ProcessAfterLoad(string fileContent)
    {
        var bytes = Convert.FromBase64String(fileContent.Trim());
        return CurrentEncoding.GetString(bytes);
    }
}
