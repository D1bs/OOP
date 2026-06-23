using ShapeContracts;
using ShapeApp.Plugins;
using ShapeApp.Serialization;

namespace ShapeApp.UI;

/// <summary>
/// Interactive console UI.
/// All command dispatch uses Dictionary&lt;string, Action&gt; — no switch/if chains.
/// Plugins are transparent to the menu: newly registered types appear automatically
/// in the "add shape" type picker without any code change.
/// </summary>
public class ConsoleMenu
{
    private readonly ShapeList        _list;
    private readonly List<PluginInfo> _loadedPlugins;
    private string _currentFile = "shapes.xml";

    private readonly Dictionary<string, (string Label, Action Handler)> _commands;

    public ConsoleMenu(ShapeList list, List<PluginInfo> loadedPlugins)
    {
        _list          = list;
        _loadedPlugins = loadedPlugins;

        _commands = new Dictionary<string, (string, Action)>
        {
            ["1"] = ("List all shapes",      ListShapes),
            ["2"] = ("Add shape",             AddShape),
            ["3"] = ("Remove shape",          RemoveShape),
            ["4"] = ("Edit shape",            EditShape),
            ["5"] = ("Save to XML",           SaveToXml),
            ["6"] = ("Load from XML",         LoadFromXml),
            ["7"] = ("Change file path",      ChangeFilePath),
            ["8"] = ("Show loaded plugins",   ShowPlugins),
            ["0"] = ("Exit",                  () => { }),
        };
    }

    /// <summary>Main loop — runs until the user selects 0.</summary>
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

    // ── Rendering ───────────────────────────────────────────────────────────

    private void PrintBanner()
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║   Shape Serializer  ·  Lab 4 Plugins   ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        if (_loadedPlugins.Count > 0)
        {
            Console.WriteLine($"  Plugins loaded: {_loadedPlugins.Count}");
            foreach (var p in _loadedPlugins)
                Console.WriteLine($"    ✓ {p.Name} v{p.Version}  ({p.ShapesRegistered} shape type(s))");
        }
        else
        {
            Console.WriteLine("  No plugins loaded (place .dll files in the 'plugins' folder).");
        }
        Console.WriteLine();
    }

    private void PrintMenu()
    {
        Console.WriteLine($"── File: {_currentFile}  |  Shapes in memory: {_list.Count} ──");
        foreach (var kv in _commands)
            Console.WriteLine($"  [{kv.Key}] {kv.Value.Label}");
        Console.Write("Choice: ");
    }

    // ── Command handlers ────────────────────────────────────────────────────

    private void ListShapes()
    {
        if (_list.Count == 0) { Console.WriteLine("  (list is empty)"); return; }
        for (int i = 0; i < _list.Count; i++)
            Console.WriteLine($"  [{i + 1}] {_list.GetAt(i).Describe()}");
    }

    private void AddShape()
    {
        var typeName = PickShapeType();
        var shape    = ShapeRegistry.Create(typeName);
        Console.WriteLine($"Enter properties for {typeName}:");
        shape.InputProperties();
        _list.Add(shape);
        Console.WriteLine($"  Added: {shape.Describe()}");
    }

    private void RemoveShape()
    {
        ListShapes();
        if (_list.Count == 0) return;
        int idx     = ReadIndex();
        var removed = _list.GetAt(idx);
        _list.RemoveAt(idx);
        Console.WriteLine($"  Removed: {removed.Describe()}");
    }

    private void EditShape()
    {
        ListShapes();
        if (_list.Count == 0) return;
        int idx = ReadIndex();
        Console.WriteLine($"  Current: {_list.GetAt(idx).Describe()}");
        Console.WriteLine("  Press Enter to keep the same type, or pick a new one:");
        var typeName = PickShapeTypeOrKeep(_list.GetAt(idx).ShapeType);
        var shape    = ShapeRegistry.Create(typeName);
        Console.WriteLine($"Enter new properties for {typeName}:");
        shape.InputProperties();
        _list.ReplaceAt(idx, shape);
        Console.WriteLine($"  Updated: {shape.Describe()}");
    }

    private void SaveToXml()
    {
        XmlShapeSerializer.Serialize(_list.Items, _currentFile);
        Console.WriteLine($"  Saved {_list.Count} shape(s) to '{_currentFile}'.");
    }

    private void LoadFromXml()
    {
        var loaded = XmlShapeSerializer.Deserialize(_currentFile);
        _list.ReplaceAll(loaded);
        Console.WriteLine($"  Loaded {_list.Count} shape(s) from '{_currentFile}'.");
    }

    private void ChangeFilePath()
    {
        Console.Write($"  Current path [{_currentFile}]: ");
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input)) _currentFile = input;
        Console.WriteLine($"  File path set to '{_currentFile}'.");
    }

    private void ShowPlugins()
    {
        if (_loadedPlugins.Count == 0)
        {
            Console.WriteLine("  No plugins loaded.");
            return;
        }
        Console.WriteLine("  Loaded plugins:");
        foreach (var p in _loadedPlugins)
            Console.WriteLine($"    • {p.Name} v{p.Version}  [{p.FileName}]  — {p.ShapesRegistered} type(s)");

        Console.WriteLine("\n  All registered shape types:");
        foreach (var t in ShapeRegistry.RegisteredTypes)
            Console.WriteLine($"    - {t}");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

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

    private static void PrintError(string msg) =>
        Console.WriteLine($"  [ERROR] {msg}");
}
