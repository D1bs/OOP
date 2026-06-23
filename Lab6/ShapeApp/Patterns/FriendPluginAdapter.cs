using ShapeContracts;
using FriendContracts;

namespace ShapeApp.Patterns;

/// <summary>
/// PATTERN: Adapter (Object Adapter variant)
/// ==========================================
/// Problem:
///   The classmate's plugin implements IFriendProcessor (foreign interface).
///   Our ProcessorRegistry expects IProcessorPlugin (our interface).
///   We cannot modify either interface.
///
/// Solution — Adapter:
///   FriendPluginAdapter wraps an IFriendProcessor instance and implements
///   IProcessorPlugin, translating every call to the corresponding foreign method:
///
///   Our interface          │  Classmate's interface
///   ───────────────────────┼──────────────────────────────
///   ProcessBeforeSave()    │  Transform()
///   ProcessAfterLoad()     │  Restore()
///   ApplySetting(k, v)     │  Configure("k=v")
///   GetSettings()          │  GetConfig()  (parsed back to dict)
///   ProcessorId / Display  │  Name
///
/// Result: the classmate's plugin works inside our pipeline without any
/// changes to ProcessorRegistry, ConsoleMenu, or the plugin itself.
/// Adding another classmate's plugin only requires one more adapter class.
/// </summary>
public sealed class FriendPluginAdapter : IProcessorPlugin
{
    // The wrapped foreign processor — composition, not inheritance
    private readonly IFriendProcessor _adaptee;

    /// <param name="adaptee">The classmate's processor to wrap.</param>
    public FriendPluginAdapter(IFriendProcessor adaptee)
    {
        _adaptee = adaptee;
    }

    // ── IProcessorPlugin identity — translated from IFriendProcessor.Name ─

    public string ProcessorId  => $"Friend_{_adaptee.Name.Replace(" ", "_")}";
    public string DisplayName  => $"[Adapted] {_adaptee.Name}";
    public string Description  => "Classmate's plugin loaded via Adapter pattern.";
    public string Version      => "1.0.0 (adapted)";
    public bool   IsConfigurable => true;

    // ── Pipeline methods — direct delegation with name translation ─────────

    /// <summary>Delegates to IFriendProcessor.Transform().</summary>
    public string ProcessBeforeSave(string xmlContent) =>
        _adaptee.Transform(xmlContent);

    /// <summary>Delegates to IFriendProcessor.Restore().</summary>
    public string ProcessAfterLoad(string fileContent) =>
        _adaptee.Restore(fileContent);

    // ── Configuration — translates key-value ↔ freeform string ───────────

    /// <summary>
    /// Translates our key-value setting to the classmate's single-string format.
    /// ApplySetting("separator", ";")  →  Configure("separator=;")
    /// </summary>
    public void ApplySetting(string key, string value) =>
        _adaptee.Configure($"{key}={value}");

    /// <summary>
    /// Parses the classmate's freeform config string back into a key-value dict
    /// so our settings menu can display it correctly.
    /// "separator=,"  →  { "separator": "," }
    /// </summary>
    public IReadOnlyDictionary<string, string> GetSettings()
    {
        var result = new Dictionary<string, string>();
        var raw    = _adaptee.GetConfig();

        // Parse "key=value;key2=value2" format
        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2)
                result[kv[0].Trim()] = kv[1].Trim();
            else
                result[part.Trim()] = "";
        }

        return result;
    }
}
