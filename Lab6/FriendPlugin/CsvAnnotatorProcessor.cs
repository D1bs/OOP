using System.Text;
using System.Text.Json;

namespace FriendPlugin;

/// <summary>
/// CsvAnnotatorProcessor — a processor plugin written by a classmate.
///
/// It uses the classmate's own IFriendProcessor interface (not our IProcessorPlugin).
/// The Adapter pattern (FriendPluginAdapter) will bridge these interfaces.
///
/// What this processor does:
///   Forward  (Transform): reads the JSON "Shapes" array and prepends a small
///                         CSV summary block as a comment header inside the file,
///                         so the saved file contains both a human-readable CSV
///                         section and the machine-readable JSON.
///
///   Reverse  (Restore):   strips the CSV header block and returns clean JSON.
///
/// Configurable via Configure("separator=;") — changes the CSV column separator.
/// </summary>
public class CsvAnnotatorProcessor : FriendContracts.IFriendProcessor
{
    // ── Identity ──────────────────────────────────────────────────────────
    public string Name => "CSV Annotator (by Alexei Petrov)";

    // ── Configuration ─────────────────────────────────────────────────────
    private char _sep = ',';   // CSV column separator

    private const string HeaderMarker = "//CSV_HEADER_START";
    private const string HeaderEnd    = "//CSV_HEADER_END";

    public void Configure(string configString)
    {
        // Expected format: "separator=;" or "separator=,"
        foreach (var part in configString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Trim() == "separator" && kv[1].Trim().Length == 1)
                _sep = kv[1].Trim()[0];
        }
    }

    public string GetConfig() => $"separator={_sep}";

    // ── Forward: append CSV header ────────────────────────────────────────

    /// <summary>
    /// Builds a CSV summary from the JSON content and prepends it as a
    /// comment block at the top of the file.
    ///
    /// Output format:
    ///   //CSV_HEADER_START
    ///   // type,field1,field2,...
    ///   // Circle,5,...
    ///   //CSV_HEADER_END
    ///   { "Shapes": [...] }
    /// </summary>
    public string Transform(string input)
    {
        try
        {
            var csvBlock = BuildCsvBlock(input);
            return csvBlock + Environment.NewLine + input;
        }
        catch
        {
            // If JSON parsing fails (e.g. not in expected format), pass through
            return input;
        }
    }

    // ── Reverse: strip CSV header ─────────────────────────────────────────

    /// <summary>
    /// Finds and removes the CSV header block, returning clean JSON.
    /// </summary>
    public string Restore(string input)
    {
        var startIdx = input.IndexOf(HeaderMarker, StringComparison.Ordinal);
        var endIdx   = input.IndexOf(HeaderEnd,    StringComparison.Ordinal);
        if (startIdx < 0 || endIdx < 0) return input;  // no header — pass through

        // Skip past HeaderEnd and the following newline
        var afterHeader = endIdx + HeaderEnd.Length;
        while (afterHeader < input.Length && (input[afterHeader] == '\r' || input[afterHeader] == '\n'))
            afterHeader++;

        return input[afterHeader..];
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>Builds the CSV comment block from the shapes JSON array.</summary>
    private string BuildCsvBlock(string json)
    {
        using var doc    = JsonDocument.Parse(json);
        var shapesArray  = doc.RootElement.GetProperty("Shapes");

        // Collect all unique field names across all shapes (for CSV header row)
        var allFields = new LinkedList<string>();
        allFields.AddFirst("type");

        var rows = new List<Dictionary<string, string>>();

        foreach (var shape in shapesArray.EnumerateArray())
        {
            var row = new Dictionary<string, string>();
            foreach (var prop in shape.EnumerateObject())
            {
                row[prop.Name] = prop.Value.GetString() ?? prop.Value.ToString();
                if (!allFields.Contains(prop.Name))
                    allFields.AddLast(prop.Name);
            }
            rows.Add(row);
        }

        var fields = allFields.ToList();
        var sb     = new StringBuilder();

        sb.AppendLine(HeaderMarker);
        // Header row
        sb.Append("// ").AppendLine(string.Join(_sep, fields));
        // Data rows
        foreach (var row in rows)
        {
            var values = fields.Select(f => row.TryGetValue(f, out var v) ? v : "");
            sb.Append("// ").AppendLine(string.Join(_sep, values));
        }
        sb.Append(HeaderEnd);

        return sb.ToString();
    }
}
