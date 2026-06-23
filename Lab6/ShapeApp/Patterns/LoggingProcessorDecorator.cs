using ShapeContracts;

namespace ShapeApp.Patterns;

/// <summary>
/// PATTERN: Decorator
/// ==================
/// Problem:
///   We want to add logging (timing, byte-count) to processor plugins
///   without modifying the plugin classes and without creating subclasses
///   for every combination of plugin + logging.
///
/// Solution — Decorator:
///   LoggingProcessorDecorator wraps any IProcessorPlugin and adds
///   console logging around ProcessBeforeSave / ProcessAfterLoad.
///   The wrapped plugin never knows it is being logged.
///   Multiple decorators can be stacked (e.g. Logging + Caching).
///
/// Usage:
///   var plain   = new XmlToJsonProcessor();
///   var logged  = new LoggingProcessorDecorator(plain);
///   registry.Register(logged);   // same interface — no other changes needed
/// </summary>
public sealed class LoggingProcessorDecorator : IProcessorPlugin
{
    private readonly IProcessorPlugin _inner;
    private bool _loggingEnabled = true;

    public LoggingProcessorDecorator(IProcessorPlugin inner)
    {
        _inner = inner;
    }

    // ── Identity — transparently forwarded ───────────────────────────────

    public string ProcessorId  => _inner.ProcessorId;
    public string DisplayName  => $"{_inner.DisplayName} [+log]";
    public string Description  => _inner.Description;
    public string Version      => _inner.Version;
    public bool   IsConfigurable => true;   // always true — we add our own setting

    // ── Pipeline — wraps inner calls with timing log ──────────────────────

    /// <summary>Logs before/after and delegates to the wrapped processor.</summary>
    public string ProcessBeforeSave(string input)
    {
        if (!_loggingEnabled) return _inner.ProcessBeforeSave(input);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Log($"[SAVE ▶] {_inner.ProcessorId}  in={input.Length} chars");

        var result = _inner.ProcessBeforeSave(input);

        sw.Stop();
        Log($"[SAVE ◀] {_inner.ProcessorId}  out={result.Length} chars  {sw.ElapsedMilliseconds} ms");
        return result;
    }

    /// <summary>Logs before/after and delegates to the wrapped processor.</summary>
    public string ProcessAfterLoad(string input)
    {
        if (!_loggingEnabled) return _inner.ProcessAfterLoad(input);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Log($"[LOAD ▶] {_inner.ProcessorId}  in={input.Length} chars");

        var result = _inner.ProcessAfterLoad(input);

        sw.Stop();
        Log($"[LOAD ◀] {_inner.ProcessorId}  out={result.Length} chars  {sw.ElapsedMilliseconds} ms");
        return result;
    }

    // ── Configuration — merges inner settings with decorator's own ────────

    public IReadOnlyDictionary<string, string> GetSettings()
    {
        // Merge inner settings + our own "Logging" toggle
        var merged = new Dictionary<string, string>(_inner.GetSettings())
        {
            ["Logging"] = _loggingEnabled.ToString()
        };
        return merged;
    }

    public void ApplySetting(string key, string value)
    {
        if (key == "Logging" && bool.TryParse(value, out var b))
            _loggingEnabled = b;
        else
            _inner.ApplySetting(key, value);  // forward to inner
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {DateTime.Now:HH:mm:ss.fff} {message}");
        Console.ResetColor();
    }
}
