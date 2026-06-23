using ShapeSerializer.Models;
using ShapeSerializer.Serialization;

namespace ShapeSerializer.UI;

/// <summary>
/// Console-based user interface.
/// All menu dispatch is done via a Dictionary of actions – no if-else chains or switch blocks.
/// </summary>
public class ConsoleMenu
{
    private readonly ShapeList _list = new();
    private string _currentFile = "shapes.xml";

    // Map menu key → (description, action) – no switch needed for dispatch
    private readonly Dictionary<string, (string Label, Action Handler)> _commands;

    public ConsoleMenu()
    {
        _commands = new Dictionary<string, (string, Action)>
        {
            ["1"] = ("List all shapes",         ListShapes),
            ["2"] = ("Add shape",                AddShape),
            ["3"] = ("Remove shape",             RemoveShape),
            ["4"] = ("Edit shape",               EditShape),
            ["5"] = ("Save to XML",              SaveToXml),
            ["6"] = ("Load from XML",            LoadFromXml),
            ["7"] = ("Change file path",         ChangeFilePath),
            ["0"] = ("Exit",                     () => { /* handled in Run() */ }),
        };
    }

    /// <summary>Runs the interactive menu loop until the user chooses to exit.</summary>
    public void Run()
    {
        Console.WriteLine("=== Shape Serializer (XML) ===");
        while (true)
        {
            PrintMenu();
            var key = Console.ReadLine()?.Trim() ?? "";

            if (key == "0") { Console.WriteLine("Goodbye!"); break; }

            if (_commands.TryGetValue(key, out var cmd))
            {
                Console.WriteLine();
                try   { cmd.Handler(); }
                catch (Exception ex) { Error(ex.Message); }
            }
            else
            {
                Error("Unknown option. Try again.");
            }

            Console.WriteLine();
        }
    }

    // ── Menu rendering ──────────────────────────────────────────────────────

    private void PrintMenu()
    {
        Console.WriteLine($"─── File: {_currentFile}  |  Shapes in memory: {_list.Count} ───");
        foreach (var kv in _commands)
            Console.WriteLine($"  [{kv.Key}] {kv.Value.Label}");
        Console.Write("Choice: ");
    }

    // ── Handlers ────────────────────────────────────────────────────────────

    /// <summary>Prints all shapes with their 1-based index.</summary>
    private void ListShapes()
    {
        if (_list.Count == 0) { Console.WriteLine("(list is empty)"); return; }
        for (int i = 0; i < _list.Count; i++)
            Console.WriteLine($"  [{i + 1}] {_list.GetAt(i).Describe()}");
    }

    /// <summary>
    /// Prompts the user to choose a registered type, then calls InputProperties()
    /// on a new blank instance. No switch – the object itself handles input.
    /// </summary>
    private void AddShape()
    {
        var typeName = PickShapeType();
        var shape    = ShapeRegistry.Create(typeName);
        Console.WriteLine($"Enter properties for {typeName}:");
        shape.InputProperties();
        _list.Add(shape);
        Console.WriteLine($"Added: {shape.Describe()}");
    }

    /// <summary>Removes a shape by 1-based index.</summary>
    private void RemoveShape()
    {
        ListShapes();
        if (_list.Count == 0) return;
        int idx = ReadIndex();
        var removed = _list.GetAt(idx);
        _list.RemoveAt(idx);
        Console.WriteLine($"Removed: {removed.Describe()}");
    }

    /// <summary>
    /// Replaces a shape at a chosen index.
    /// User can keep the same type or switch to a different one.
    /// Uses InputProperties() – no if-else for type handling.
    /// </summary>
    private void EditShape()
    {
        ListShapes();
        if (_list.Count == 0) return;

        int idx   = ReadIndex();
        var old   = _list.GetAt(idx);
        Console.WriteLine($"Current: {old.Describe()}");
        Console.WriteLine("Press Enter to keep the same type, or choose a new one:");

        var typeName = PickShapeTypeOrKeep(old.ShapeType);
        var shape    = ShapeRegistry.Create(typeName);
        Console.WriteLine($"Enter new properties for {typeName}:");
        shape.InputProperties();
        _list.ReplaceAt(idx, shape);
        Console.WriteLine($"Updated: {shape.Describe()}");
    }

    /// <summary>Serializes the current list to the active XML file.</summary>
    private void SaveToXml()
    {
        XmlShapeSerializer.Serialize(_list.Items, _currentFile);
        Console.WriteLine($"Saved {_list.Count} shape(s) to '{_currentFile}'.");
    }

    /// <summary>Deserializes shapes from the active XML file, replacing the current list.</summary>
    private void LoadFromXml()
    {
        var loaded = XmlShapeSerializer.Deserialize(_currentFile);
        _list.ReplaceAll(loaded);
        Console.WriteLine($"Loaded {_list.Count} shape(s) from '{_currentFile}'.");
    }

    /// <summary>Lets the user set a different file path for save/load.</summary>
    private void ChangeFilePath()
    {
        Console.Write($"Current file [{_currentFile}]: ");
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input)) _currentFile = input;
        Console.WriteLine($"File path set to '{_currentFile}'.");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Shows registered types and returns the chosen type name.</summary>
    private static string PickShapeType()
    {
        var types = ShapeRegistry.RegisteredTypes.ToList();
        Console.WriteLine("Available shape types:");
        for (int i = 0; i < types.Count; i++)
            Console.WriteLine($"  [{i + 1}] {types[i]}");
        Console.Write("Type number: ");
        int choice = int.Parse(Console.ReadLine()!) - 1;
        return types[choice];
    }

    /// <summary>
    /// Same as PickShapeType but pressing Enter keeps <paramref name="defaultType"/>.
    /// </summary>
    private static string PickShapeTypeOrKeep(string defaultType)
    {
        var types = ShapeRegistry.RegisteredTypes.ToList();
        Console.WriteLine("Available shape types (Enter = keep current):");
        for (int i = 0; i < types.Count; i++)
            Console.WriteLine($"  [{i + 1}] {types[i]}");
        Console.Write("Type number (or Enter): ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input)) return defaultType;
        int choice = int.Parse(input) - 1;
        return types[choice];
    }

    /// <summary>Reads a 1-based index from the console and converts to 0-based.</summary>
    private int ReadIndex()
    {
        Console.Write($"Enter number [1-{_list.Count}]: ");
        int idx = int.Parse(Console.ReadLine()!) - 1;
        if (idx < 0 || idx >= _list.Count)
            throw new ArgumentOutOfRangeException(nameof(idx), "Index out of range.");
        return idx;
    }

    private static void Error(string msg) =>
        Console.WriteLine($"[ERROR] {msg}");
}
