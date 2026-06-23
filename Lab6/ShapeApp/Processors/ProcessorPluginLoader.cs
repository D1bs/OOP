using System.Reflection;
using ShapeContracts;

namespace ShapeApp.Processors;

/// <summary>
/// Scans a directory for DLLs that implement IProcessorPlugin and loads them
/// into the provided ProcessorRegistry.
///
/// Reflection is used ONLY here (unavoidable for dynamic DLL loading).
/// No shape-type or processor-type dispatch uses reflection.
/// </summary>
public static class ProcessorPluginLoader
{
    /// <summary>
    /// Loads all processor plugins found in <paramref name="directory"/>.
    /// Returns a list of info records for UI display.
    /// Silently skips DLLs that contain no valid processor or throw on load.
    /// </summary>
    public static List<ProcessorPluginInfo> LoadAll(string directory, ProcessorRegistry registry)
    {
        var loaded = new List<ProcessorPluginInfo>();

        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"[ProcessorLoader] Directory not found: '{directory}'. No processor plugins loaded.");
            return loaded;
        }

        // Sort DLLs to guarantee correct pipeline order:
        // XML → [XmlToJson] → [JsonPrettify] → [Base64] → file
        // (CsvAnnotator is registered separately in Program.cs, before this call)
        var dlls = Directory.GetFiles(directory, "*.dll")
            .OrderBy(path =>
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("XmlToJson",    StringComparison.OrdinalIgnoreCase)) return 0;
                if (name.StartsWith("JsonPrettify", StringComparison.OrdinalIgnoreCase)) return 1;
                if (name.StartsWith("Base64",       StringComparison.OrdinalIgnoreCase)) return 2;
                return 3;
            })
            .ThenBy(p => p);

        foreach (var dll in dlls)
        {
            var info = TryLoad(dll, registry);
            if (info is not null) loaded.Add(info);
        }

        return loaded;
    }

    /// <summary>
    /// Attempts to load one DLL as a processor plugin.
    /// Returns null if the DLL contains no IProcessorPlugin implementation.
    /// </summary>
    private static ProcessorPluginInfo? TryLoad(string dllPath, ProcessorRegistry registry)
    {
        try
        {
            var assembly = Assembly.LoadFrom(dllPath);

            // Find all concrete types that implement IProcessorPlugin
            var types = assembly.GetTypes()
                .Where(t => typeof(IProcessorPlugin).IsAssignableFrom(t)
                            && t is { IsAbstract: false, IsInterface: false })
                .ToList();

            if (types.Count == 0) return null;

            string name    = Path.GetFileName(dllPath);
            string version = "?";
            int    count   = 0;

            foreach (var t in types)
            {
                // Only Activator.CreateInstance is used — unavoidable for external types
                var plugin = (IProcessorPlugin)Activator.CreateInstance(t)!;
                registry.Register(plugin);
                name    = plugin.DisplayName;
                version = plugin.Version;
                count++;
            }

            return new ProcessorPluginInfo(name, version, Path.GetFileName(dllPath), count);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProcessorLoader] Failed to load '{Path.GetFileName(dllPath)}': {ex.Message}");
            return null;
        }
    }
}

/// <summary>Info about a successfully loaded processor plugin.</summary>
public record ProcessorPluginInfo(string Name, string Version, string FileName, int ProcessorCount);