using ShapeContracts;
using ShapeApp.Patterns;
using ShapeApp.Plugins;
using ShapeApp.Processors;
using ShapeApp.Serialization;

namespace ShapeApp.UI;

/// <summary>
/// Interactive console UI for Lab 6.
///
/// Changes from Lab 5:
///   • Save / Load now go through SerializationFacade (Facade pattern)
///   • Banner shows all active patterns
///   • Menu option [A] shows pattern documentation
///
/// No switch/if chains — all command dispatch via Dictionary.
/// </summary>
public class ConsoleMenu
{
    private readonly ShapeList            _list;
    private readonly List<PluginInfo>     _shapePlugins;
    private readonly ProcessorRegistry    _processors;
    private readonly List<ProcessorPluginInfo> _processorPlugins;
    private readonly SerializationFacade  _facade;   // Facade pattern
    private string _currentFile = "shapes.json";

    private readonly Dictionary<string, (string Label, Action Handler)> _commands;

    public ConsoleMenu(ShapeList list,
                       List<PluginInfo> shapePlugins,
                       ProcessorRegistry processors,
                       List<ProcessorPluginInfo> processorPlugins,
                       SerializationFacade facade)
    {
        _list             = list;
        _shapePlugins     = shapePlugins;
        _processors       = processors;
        _processorPlugins = processorPlugins;
        _facade           = facade;

        _commands = new Dictionary<string, (string, Action)>
        {
            ["1"] = ("List all shapes",                  ListShapes),
            ["2"] = ("Add shape",                        AddShape),
            ["3"] = ("Remove shape",                     RemoveShape),
            ["4"] = ("Edit shape",                       EditShape),
            ["5"] = ("Save  (Facade → pipeline)",        SaveFile),
            ["6"] = ("Load  (Facade → pipeline)",        LoadFile),
            ["7"] = ("Change file path",                 ChangeFilePath),
            ["8"] = ("Show plugins",                     ShowPlugins),
            ["9"] = ("Processor plugin settings",        ProcessorSettings),
            ["A"] = ("Pattern documentation",            ShowPatternDocs),
            ["0"] = ("Exit",                             () => { }),
        };
    }

    // ── Main loop ─────────────────────────────────────────────────────────────

    public void Run()
    {
        PrintBanner();
        while (true)
        {
            PrintMenu();
            var key = (Console.ReadLine()?.Trim() ?? "").ToUpperInvariant();
            if (key == "0") { Console.WriteLine("Goodbye!"); break; }

            if (_commands.TryGetValue(key, out var cmd))
            {
                Console.WriteLine();
                try   { cmd.Handler(); }
                catch (Exception ex) { PrintError(ex.Message); }
            }
            else PrintError("Unknown option.");
            Console.WriteLine();
        }
    }

    // ── Banner ────────────────────────────────────────────────────────────────

    private void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔═══════════════════════════════════════════════════╗");
        Console.WriteLine("║   Shape Serializer  ·  Lab 6 — Patterns           ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════╣");
        Console.WriteLine("║  Patterns used:                                    ║");
        Console.WriteLine("║    • Adapter   — classmate's plugin bridged        ║");
        Console.WriteLine("║    • Decorator — logging wrapped around processors ║");
        Console.WriteLine("║    • Facade    — save/load behind one interface    ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════╝");
        Console.ResetColor();

        // Pipeline summary
        if (_processors.All.Count > 0)
        {
            Console.WriteLine($"  Active pipeline: {string.Join(" → ", _processors.All.Select(p => p.ProcessorId))}");
        }
        Console.WriteLine();
    }

    // ── Menu ──────────────────────────────────────────────────────────────────

    private void PrintMenu()
    {
        var pipeline = _processors.All.Count > 0
            ? string.Join(" → ", _processors.All.Select(p => p.ProcessorId))
            : "none";
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"── File: {_currentFile}  |  Shapes: {_list.Count}  |  Pipeline: {pipeline} ──");
        Console.ResetColor();
        foreach (var kv in _commands)
            Console.WriteLine($"  [{kv.Key}] {kv.Value.Label}");
        Console.Write("Choice: ");
    }

    // ── [1] List ──────────────────────────────────────────────────────────────

    private void ListShapes()
    {
        if (_list.Count == 0) { Console.WriteLine("  (list is empty)"); return; }
        for (int i = 0; i < _list.Count; i++)
            Console.WriteLine($"  [{i + 1}] {_list.GetAt(i).Describe()}");
    }

    // ── [2] Add ───────────────────────────────────────────────────────────────

    private void AddShape()
    {
        var typeName = PickShapeType();
        var shape    = ShapeRegistry.Create(typeName);
        Console.WriteLine($"Enter properties for {typeName}:");
        shape.InputProperties();
        _list.Add(shape);
        Console.WriteLine($"  Added: {shape.Describe()}");
    }

    // ── [3] Remove ────────────────────────────────────────────────────────────

    private void RemoveShape()
    {
        ListShapes();
        if (_list.Count == 0) return;
        int idx = ReadIndex();
        var removed = _list.GetAt(idx);
        _list.RemoveAt(idx);
        Console.WriteLine($"  Removed: {removed.Describe()}");
    }

    // ── [4] Edit ──────────────────────────────────────────────────────────────

    private void EditShape()
    {
        ListShapes();
        if (_list.Count == 0) return;
        int idx      = ReadIndex();
        Console.WriteLine($"  Current: {_list.GetAt(idx).Describe()}");
        var typeName = PickShapeTypeOrKeep(_list.GetAt(idx).ShapeType);
        var shape    = ShapeRegistry.Create(typeName);
        Console.WriteLine($"Enter new properties for {typeName}:");
        shape.InputProperties();
        _list.ReplaceAt(idx, shape);
        Console.WriteLine($"  Updated: {shape.Describe()}");
    }

    // ── [5] Save via Facade ───────────────────────────────────────────────────

    /// <summary>
    /// Calls SerializationFacade.Save() — Facade hides XmlShapeSerializer
    /// + ProcessorRegistry + File I/O behind a single method.
    /// </summary>
    private void SaveFile()
    {
        var status = _facade.Save(_list.Items, _currentFile);
        PrintOk(status);
    }

    // ── [6] Load via Facade ───────────────────────────────────────────────────

    /// <summary>
    /// Calls SerializationFacade.Load() — same Facade, reverse pipeline.
    /// </summary>
    private void LoadFile()
    {
        var (shapes, status) = _facade.Load(_currentFile);
        _list.ReplaceAll(shapes);
        PrintOk(status);
    }

    // ── [7] Change file path ──────────────────────────────────────────────────

    private void ChangeFilePath()
    {
        Console.Write($"  Current path [{_currentFile}]: ");
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input)) _currentFile = input;
        Console.WriteLine($"  File path set to '{_currentFile}'.");
    }

    // ── [8] Show plugins ──────────────────────────────────────────────────────

    private void ShowPlugins()
    {
        Console.WriteLine("  Shape plugins:");
        if (_shapePlugins.Count == 0) Console.WriteLine("    (none)");
        else foreach (var p in _shapePlugins)
            Console.WriteLine($"    • {p.Name} v{p.Version}  [{p.FileName}]");

        Console.WriteLine("\n  Processor pipeline:");
        for (int i = 0; i < _processors.All.Count; i++)
        {
            var p = _processors.All[i];
            Console.WriteLine($"    [{i + 1}] {p.DisplayName} v{p.Version}");
        }
        if (_processors.All.Count == 0) Console.WriteLine("    (none)");

        Console.WriteLine("\n  Registered shape types:");
        foreach (var t in ShapeRegistry.RegisteredTypes)
            Console.WriteLine($"    - {t}");
    }

    // ── [9] Processor settings ────────────────────────────────────────────────

    private void ProcessorSettings()
    {
        if (_processors.All.Count == 0)
        {
            Console.WriteLine("  No processor plugins loaded.");
            return;
        }

        while (true)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ── Processor Plugin Settings ──");
            Console.ResetColor();

            var all = _processors.All;
            for (int i = 0; i < all.Count; i++)
            {
                var p   = all[i];
                var cfg = p.IsConfigurable ? "  [configurable]" : "";
                Console.WriteLine($"    [{i + 1}] {p.DisplayName}{cfg}");
                Console.WriteLine($"         {p.Description}");
            }

            Console.WriteLine("\n  Enter processor number to configure, or 0 to go back:");
            Console.Write("  Choice: ");
            var input = Console.ReadLine()?.Trim();
            if (input == "0" || string.IsNullOrEmpty(input)) break;

            if (!int.TryParse(input, out int idx) || idx < 1 || idx > all.Count)
            { PrintError("Invalid number."); continue; }

            ConfigureProcessor(all[idx - 1]);
        }
    }

    private static void ConfigureProcessor(IProcessorPlugin processor)
    {
        if (!processor.IsConfigurable)
        { Console.WriteLine($"  '{processor.DisplayName}' has no configurable settings."); return; }

        Console.WriteLine($"\n  Settings for [{processor.DisplayName}]:");
        var settings = processor.GetSettings();
        var keys     = settings.Keys.ToList();

        for (int i = 0; i < keys.Count; i++)
            Console.WriteLine($"    [{i + 1}] {keys[i]} = {settings[keys[i]]}");

        Console.Write("  Enter setting number to change, or 0 to cancel: ");
        var input = Console.ReadLine()?.Trim();
        if (input == "0" || string.IsNullOrEmpty(input)) return;

        if (!int.TryParse(input, out int si) || si < 1 || si > keys.Count)
        { Console.WriteLine("  Invalid number."); return; }

        var chosenKey = keys[si - 1];
        Console.Write($"  New value for '{chosenKey}' (current: {settings[chosenKey]}): ");
        var newValue = Console.ReadLine()?.Trim() ?? "";
        processor.ApplySetting(chosenKey, newValue);

        var updated = processor.GetSettings();
        PrintOk($"{chosenKey} = {updated.GetValueOrDefault(chosenKey, newValue)}");
    }

    // ── [A] Pattern documentation ─────────────────────────────────────────────

    private static void ShowPatternDocs()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  ═══ Patterns used in Lab 6 ═══");
        Console.ResetColor();

        Console.WriteLine(@"
  1. ADAPTER  (ShapeApp/Patterns/FriendPluginAdapter.cs)
     Problem : classmate's plugin uses IFriendProcessor — different method
               names, different config style, incompatible with our pipeline.
     Solution: FriendPluginAdapter wraps IFriendProcessor and implements
               IProcessorPlugin, translating every call:
                 ProcessBeforeSave() → Transform()
                 ProcessAfterLoad()  → Restore()
                 ApplySetting(k,v)   → Configure(""k=v"")
               The classmate's code is unchanged; our registry is unchanged.

  2. DECORATOR  (ShapeApp/Patterns/LoggingProcessorDecorator.cs)
     Problem : we want timing/size logs for processor calls without
               modifying every plugin class.
     Solution: LoggingProcessorDecorator wraps any IProcessorPlugin,
               logs before/after each call, and forwards the actual work.
               Decorators are stackable: Logging(Adapter(FriendPlugin)).

  3. FACADE  (ShapeApp/Patterns/SerializationFacade.cs)
     Problem : ConsoleMenu had to coordinate XmlShapeSerializer,
               ProcessorRegistry, and File I/O — tight coupling.
     Solution: SerializationFacade exposes two methods: Save() and Load().
               All subsystem coordination is hidden inside the Facade.
               The UI only calls Facade.Save() / Facade.Load().
");
    }

    // ── Shared helpers ────────────────────────────────────────────────────────

    private static string PickShapeType()
    {
        var types = ShapeRegistry.RegisteredTypes.ToList();
        Console.WriteLine("  Available types:");
        for (int i = 0; i < types.Count; i++)
            Console.WriteLine($"    [{i + 1}] {types[i]}");
        Console.Write("  Type number: ");
        return types[int.Parse(Console.ReadLine()!) - 1];
    }

    private static string PickShapeTypeOrKeep(string current)
    {
        var types = ShapeRegistry.RegisteredTypes.ToList();
        Console.WriteLine("  Available types (Enter = keep current):");
        for (int i = 0; i < types.Count; i++)
            Console.WriteLine($"    [{i + 1}] {types[i]}");
        Console.Write("  Type number (or Enter): ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input)) return current;
        return types[int.Parse(input) - 1];
    }

    private int ReadIndex()
    {
        Console.Write($"  Enter number [1–{_list.Count}]: ");
        int idx = int.Parse(Console.ReadLine()!) - 1;
        if (idx < 0 || idx >= _list.Count)
            throw new ArgumentOutOfRangeException(nameof(idx), "Index out of range.");
        return idx;
    }

    private static void PrintOk(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {msg}");
        Console.ResetColor();
    }

    private static void PrintError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [ERROR] {msg}");
        Console.ResetColor();
    }
}
