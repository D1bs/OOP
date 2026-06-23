using ShapeContracts;

namespace ShapeApp.Processors;

/// <summary>
/// Registry for IProcessorPlugin instances.
///
/// Processors are applied in registration order on save,
/// and in REVERSE order on load (to correctly undo the chain).
///
/// No if-else / switch anywhere — all dispatch goes through the list.
/// </summary>
public class ProcessorRegistry
{
    // Ordered list preserves pipeline sequence
    private readonly List<IProcessorPlugin> _processors = new();

    // ── Registration ──────────────────────────────────────────────────────

    /// <summary>Appends a processor to the end of the pipeline.</summary>
    public void Register(IProcessorPlugin processor)
    {
        _processors.Add(processor);
    }

    /// <summary>All registered processors in pipeline order.</summary>
    public IReadOnlyList<IProcessorPlugin> All => _processors;

    // ── Pipeline execution ────────────────────────────────────────────────

    /// <summary>
    /// Applies all processors in forward order (index 0 … N-1).
    /// Used before writing to file.
    /// </summary>
    public string ApplyBeforeSave(string xmlContent)
    {
        var result = xmlContent;
        foreach (var p in _processors)
            result = p.ProcessBeforeSave(result);
        return result;
    }

    /// <summary>
    /// Applies all processors in REVERSE order (index N-1 … 0).
    /// Used after reading from file to undo the forward transforms.
    /// </summary>
    public string ApplyAfterLoad(string fileContent)
    {
        var result = fileContent;
        foreach (var p in _processors.AsEnumerable().Reverse())
            result = p.ProcessAfterLoad(result);
        return result;
    }
}
