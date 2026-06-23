using ShapeContracts;
using ShapeApp.Processors;
using ShapeApp.Serialization;

namespace ShapeApp.Patterns;

/// <summary>
/// PATTERN: Facade
/// ===============
/// Problem:
///   Saving and loading involves multiple subsystems:
///     XmlShapeSerializer → ProcessorRegistry → File I/O
///   The UI (ConsoleMenu) has to know and coordinate all of them.
///   Adding a new subsystem (e.g. checksum verification) means touching the UI.
///
/// Solution — Facade:
///   SerializationFacade provides ONE simple method for save and ONE for load.
///   It hides ShapeRegistry, XmlShapeSerializer, ProcessorRegistry, and File I/O
///   behind a clean API.  The UI calls Facade.Save() / Facade.Load() and knows
///   nothing about the internals.
///
/// Adding a new subsystem step (e.g. backup, audit log) only requires a change
/// inside the Facade — the UI is untouched.
/// </summary>
public sealed class SerializationFacade
{
    private readonly ProcessorRegistry _processors;

    public SerializationFacade(ProcessorRegistry processors)
    {
        _processors = processors;
    }

    // ── Public API (the "facade") ─────────────────────────────────────────

    /// <summary>
    /// Saves shapes to a file through the full processor pipeline.
    /// Returns a human-readable status string for display.
    /// </summary>
    public string Save(IEnumerable<IShape> shapes, string filePath)
    {
        var shapeList = shapes.ToList();

        // 1. Delegate to serializer + pipeline
        XmlShapeSerializer.Serialize(shapeList, filePath, _processors);

        // 2. Collect pipeline description for the status message
        var pipeline = _processors.All.Count > 0
            ? string.Join(" → ", _processors.All.Select(p => p.ProcessorId))
            : "plain XML";

        // 3. Report file size
        var bytes = new FileInfo(filePath).Length;

        return $"Saved {shapeList.Count} shape(s) to '{filePath}'  " +
               $"[{bytes} bytes]  pipeline: {pipeline}";
    }

    /// <summary>
    /// Loads shapes from a file through the reverse processor pipeline.
    /// Returns loaded shapes and a human-readable status string.
    /// </summary>
    public (List<IShape> Shapes, string Status) Load(string filePath)
    {
        // 1. Delegate to serializer + reverse pipeline
        var shapes = XmlShapeSerializer.Deserialize(filePath, _processors);

        var pipeline = _processors.All.Count > 0
            ? string.Join(" → ", _processors.All.AsEnumerable().Reverse().Select(p => p.ProcessorId))
            : "plain XML";

        var status = $"Loaded {shapes.Count} shape(s) from '{filePath}'  " +
                     $"pipeline: {pipeline}";

        return (shapes, status);
    }
}
