using ShapeContracts;
using ShapeApp.Plugins;
using ShapeApp.Processors;
using ShapeApp.Serialization;

namespace ShapeApp.UI;

/// <summary>
/// Interactive console UI for Lab 5.
///
/// Extends Lab 4 menu with:
///   [9] Processor plugin settings  — configure loaded processor plugins
///   Save / Load now route through ProcessorRegistry pipeline automatically.
///
/// No switch/if chains — all command dispatch via Dictionary.
/// </summary>
public class ConsoleMenu
{
    private readonly ShapeList            _list;
    private readonly List<PluginInfo>     _shapePlugins;
    private readonly ProcessorRegistry    _processors;
    private readonly List<ProcessorPluginInfo> _processorPlugins;
    private string _currentFile = "shapes.json"; // default extension reflects JSON output

    private readonly Dictionary<string, (string Label, Action Handler)> _commands;

    public ConsoleMenu(ShapeList list,
                       List<PluginInfo> shapePlugins,
                       ProcessorRegistry processors,
                       List<ProcessorPluginInfo> processorPlugins)
    {
        _list             = list;
        _shapePlugins     = shapePlugins;
        _processors       = processors;
        _processorPlugins = processorPlugins;

        _commands = new Dictionary<string, (string, Action)>
        {
            ["1"] = ("List all shapes",              ListShapes),
            ["2"] = ("Add shape",                    AddShape),
            ["3"] = ("Remove shape",                 RemoveShape),
            ["4"] = ("Edit shape",                   EditShape),
            ["5"] = ("Save  (through processor pipeline)", SaveFile),
            ["6"] = ("Load  (through processor pipeline)", LoadFile),
            ["7"] = ("Change file path",             ChangeFilePath),
            ["8"] = ("Show shape plugins",           ShowShapePlugins),
            ["9"] = ("Processor plugin settings",    ProcessorSettings),
            ["0"] = ("Exit",                         () => { }),
        };
    }

    // ── Main loop ─────────────────────────────────────────────────────────────

    /// <summary>Runs the menu until the user selects 0.</summary>
    public void Run()
    {
        PrintBanner();
        while (true)
        {
            PrintMenu();
            var key = Console.ReadLine()?.Trim() ?? "";
            if (key == "0") { Console.WriteLine("Goodbye!"); break; }

            if (_commands.TryGetValue(key, out var cmd))
            {
                Console.WriteLine();
                try   { cmd.Handler(); }
                catch (Exception ex) { PrintError(ex.Message); }
            }
            else
            {
                PrintError("Unknown option.");
            }
            Console.WriteLine();
        }
    }

    // ── Banner / Menu rendering ───────────────────────────────────────────────

    private void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║  Shape Serializer  ·  Lab 5 — Processor Plugins  ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.ResetColor();

        // Show shape plugins
        if (_shapePlugins.Count > 0)
        {
            Console.WriteLine($"  Shape plugins: {_shapePlugins.Count}");
            foreach (var p in _shapePlugins)
                Console.WriteLine($"    ✓ {p.Name} v{p.Version}  ({p.ShapesRegistered} type(s))");
        }

        // Show processor plugins
        if (_processorPlugins.Count > 0)
        {
            Console.WriteLine($"  Processor plugins: {_processorPlugins.Count}");
            foreach (var p in _processorPlugins)
                Console.WriteLine($"    ✓ {p.Name} v{p.Version}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  No processor plugins loaded — place DLLs in 'processors/' folder.");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    private void PrintMenu()
    {
        // Show active pipeline
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
        var removed = _list.GetAt(ReadIndex());
        _list.RemoveAt(_list.Items.ToList().IndexOf(removed));
        Console.WriteLine($"  Removed: {removed.Describe()}");
    }

    // ── [4] Edit ──────────────────────────────────────────────────────────────

    private void EditShape()
    {
        ListShapes();
        if (_list.Count == 0) return;
        int idx = ReadIndex();
        Console.WriteLine($"  Current: {_list.GetAt(idx).Describe()}");
        var typeName = PickShapeTypeOrKeep(_list.GetAt(idx).ShapeType);
        var shape    = ShapeRegistry.Create(typeName);
        Console.WriteLine($"Enter new properties for {typeName}:");
        shape.InputProperties();
        _list.ReplaceAt(idx, shape);
        Console.WriteLine($"  Updated: {shape.Describe()}");
    }

    // ── [5] Save ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Saves shapes through the full processor pipeline.
    /// XmlShapeSerializer produces XML, then ProcessorRegistry transforms it
    /// (e.g. XML → JSON → Base64) before writing to disk.
    /// </summary>
    private void SaveFile()
    {
        XmlShapeSerializer.Serialize(_list.Items, _currentFile, _processors);
        var info = _processors.All.Count > 0
            ? $" [pipeline: {string.Join(" → ", _processors.All.Select(p => p.ProcessorId))}]"
            : "";
        Console.WriteLine($"  Saved {_list.Count} shape(s) to '{_currentFile}'{info}.");
    }

    // ── [6] Load ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads file and runs the reverse processor pipeline before parsing.
    /// (e.g. Base64 decode → JSON → XML) then XmlShapeSerializer parses shapes.
    /// </summary>
    private void LoadFile()
    {
        var loaded = XmlShapeSerializer.Deserialize(_currentFile, _processors);
        _list.ReplaceAll(loaded);
        Console.WriteLine($"  Loaded {_list.Count} shape(s) from '{_currentFile}'.");
    }

    // ── [7] Change file path ──────────────────────────────────────────────────

    private void ChangeFilePath()
    {
        Console.Write($"  Current path [{_currentFile}]: ");
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input)) _currentFile = input;
        Console.WriteLine($"  File path set to '{_currentFile}'.");
    }

    // ── [8] Show shape plugins ────────────────────────────────────────────────

    private void ShowShapePlugins()
    {
        Console.WriteLine("  Shape plugins:");
        if (_shapePlugins.Count == 0) { Console.WriteLine("    (none)"); }
        else foreach (var p in _shapePlugins)
            Console.WriteLine($"    • {p.Name} v{p.Version}  [{p.FileName}]  — {p.ShapesRegistered} type(s)");

        Console.WriteLine("\n  Registered shape types:");
        foreach (var t in ShapeRegistry.RegisteredTypes)
            Console.WriteLine($"    - {t}");
    }

    // ── [9] Processor settings ────────────────────────────────────────────────

    /// <summary>
    /// Settings sub-menu for processor plugins.
    /// Shows the active pipeline, lets the user pick a processor and tweak its settings.
    /// No switch/if — each processor handles its own ApplySetting().
    /// </summary>
    private void ProcessorSettings()
    {
        if (_processors.All.Count == 0)
        {
            Console.WriteLine("  No processor plugins loaded.");
            Console.WriteLine("  Place processor DLLs in the 'processors/' folder next to the executable.");
            return;
        }

        while (true)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ── Processor Plugin Settings ──");
            Console.ResetColor();

            // List pipeline order
            Console.WriteLine("  Active pipeline (save order):");
            var all = _processors.All;
            for (int i = 0; i < all.Count; i++)
            {
                var p = all[i];
                var cfg = p.IsConfigurable ? "  [configurable]" : "";
                Console.WriteLine($"    [{i + 1}] {p.DisplayName} v{p.Version}{cfg}");
                Console.WriteLine($"         {p.Description}");
            }

            Console.WriteLine();
            Console.WriteLine("  Enter processor number to configure, or 0 to go back:");
            Console.Write("  Choice: ");
            var input = Console.ReadLine()?.Trim();
            if (input == "0" || string.IsNullOrEmpty(input)) break;

            if (!int.TryParse(input, out int idx) || idx < 1 || idx > all.Count)
            {
                PrintError("Invalid number.");
                continue;
            }

            ConfigureProcessor(all[idx - 1]);
        }
    }

    /// <summary>Shows and edits settings for a single processor.</summary>
    private static void ConfigureProcessor(IProcessorPlugin processor)
    {
        if (!processor.IsConfigurable)
        {
            Console.WriteLine($"  '{processor.DisplayName}' has no configurable settings.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"  Settings for [{processor.DisplayName}]:");

        var settings = processor.GetSettings();
        var keys     = settings.Keys.ToList();

        for (int i = 0; i < keys.Count; i++)
            Console.WriteLine($"    [{i + 1}] {keys[i]} = {settings[keys[i]]}");

        Console.WriteLine("  Enter setting number to change, or 0 to cancel:");
        Console.Write("  Choice: ");
        var input = Console.ReadLine()?.Trim();
        if (input == "0" || string.IsNullOrEmpty(input)) return;

        if (!int.TryParse(input, out int si) || si < 1 || si > keys.Count)
        {
            Console.WriteLine("  Invalid number.");
            return;
        }

        var chosenKey = keys[si - 1];
        Console.Write($"  New value for '{chosenKey}' (current: {settings[chosenKey]}): ");
        var newValue = Console.ReadLine()?.Trim() ?? "";

        // Delegate to the plugin — no type-specific logic here
        processor.ApplySetting(chosenKey, newValue);

        // Confirm the change
        var updated = processor.GetSettings();
        Console.WriteLine($"  ✓ {chosenKey} = {updated[chosenKey]}");
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

    private static void PrintError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [ERROR] {msg}");
        Console.ResetColor();
    }
}
