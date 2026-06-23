namespace ShapeContracts;

/// <summary>
/// Contract for data-processor plugins (Lab 5).
///
/// A processor plugin transforms serialized shape data BEFORE it is written
/// to a file (forward pass) and AFTER it is read from a file (reverse pass).
///
/// Variant 1 — XML → JSON:
///   Forward : XML string  → JSON string  (save)
///   Reverse : JSON string → XML string   (load)
///
/// The pipeline is:
///   Save:  shapes → XmlShapeSerializer → [processor chain] → file
///   Load:  file   → [processor chain reversed] → XmlShapeSerializer → shapes
/// </summary>
public interface IProcessorPlugin
{
    /// <summary>Unique identifier used in the settings menu and pipeline key.</summary>
    string ProcessorId { get; }

    /// <summary>Human-readable display name shown in the plugin settings menu.</summary>
    string DisplayName { get; }

    /// <summary>Short description of what the processor does.</summary>
    string Description { get; }

    /// <summary>Version string.</summary>
    string Version { get; }

    /// <summary>
    /// Forward transform applied BEFORE writing to file.
    /// Input  : raw XML produced by XmlShapeSerializer
    /// Output : transformed content written to the file
    /// </summary>
    string ProcessBeforeSave(string xmlContent);

    /// <summary>
    /// Reverse transform applied AFTER reading from file.
    /// Input  : raw file content read from disk
    /// Output : XML string that XmlShapeSerializer can parse
    /// </summary>
    string ProcessAfterLoad(string fileContent);

    // ── Optional configuration ────────────────────────────────────────────

    /// <summary>
    /// True if this processor exposes configurable parameters.
    /// When true, the settings menu shows a "Configure" option.
    /// </summary>
    bool IsConfigurable { get; }

    /// <summary>
    /// Returns current settings as key-value pairs for display.
    /// Called by the settings menu to show current state.
    /// </summary>
    IReadOnlyDictionary<string, string> GetSettings();

    /// <summary>
    /// Applies a new value for a named setting.
    /// The menu passes the key chosen by the user and the entered value.
    /// Implementations should validate and ignore invalid values.
    /// </summary>
    void ApplySetting(string key, string value);
}
