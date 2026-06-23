using System.Reflection;
using ShapeContracts;
using ShapeApp.Serialization;

namespace ShapeApp.Plugins;

/// <summary>
/// Discovers and loads shape plugins from a directory of .dll files.
///
/// Loading process for each .dll:
///   1. Load the assembly with Assembly.LoadFrom().
///   2. Find all public, concrete types that implement IShapePlugin.
///   3. Create one instance via Activator.CreateInstance().
///      (This is the ONLY place reflection is used — it is unavoidable for
///       dynamic loading; no shape-type dispatch logic uses reflection.)
///   4. Call plugin.GetShapes() and register each prototype with ShapeRegistry.
///
/// The host application code (serializer, menu, registry) is never touched
/// when a new plugin is added.
/// </summary>
public static class PluginLoader
{
    /// <summary>
    /// Loads all plugins found in <paramref name="pluginDirectory"/>.
    /// Silently skips files that contain no valid plugin or throw on load.
    /// Returns a list of (pluginName, version, shapeCount) for reporting.
    /// </summary>
    public static List<PluginInfo> LoadAll(string pluginDirectory)
    {
        var loaded = new List<PluginInfo>();

        if (!Directory.Exists(pluginDirectory))
        {
            Console.WriteLine($"[PluginLoader] Directory not found: '{pluginDirectory}'. No plugins loaded.");
            return loaded;
        }

        foreach (var dllPath in Directory.GetFiles(pluginDirectory, "*.dll"))
        {
            var info = TryLoadPlugin(dllPath);
            if (info is not null)
                loaded.Add(info);
        }

        return loaded;
    }

    /// <summary>
    /// Attempts to load a single plugin DLL.
    /// Returns null (and prints a warning) if the DLL contains no valid plugin.
    /// </summary>
    private static PluginInfo? TryLoadPlugin(string dllPath)
    {
        try
        {
            var assembly = Assembly.LoadFrom(dllPath);

            // Find concrete types that implement IShapePlugin
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IShapePlugin).IsAssignableFrom(t)
                            && t is { IsAbstract: false, IsInterface: false })
                .ToList();

            if (pluginTypes.Count == 0)
                return null; // DLL exists but has no plugin entry point

            int totalShapes = 0;
            string pluginName = Path.GetFileName(dllPath);
            string version    = "?";

            foreach (var type in pluginTypes)
            {
                // Activator.CreateInstance is used here — justified because we
                // are loading an unknown type from an external assembly at runtime.
                var plugin = (IShapePlugin)Activator.CreateInstance(type)!;
                pluginName = plugin.PluginName;
                version    = plugin.Version;

                // Register each shape prototype the plugin exposes
                foreach (var shape in plugin.GetShapes())
                {
                    ShapeRegistry.Register(shape);
                    totalShapes++;
                }
            }

            return new PluginInfo(pluginName, version, Path.GetFileName(dllPath), totalShapes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PluginLoader] Failed to load '{Path.GetFileName(dllPath)}': {ex.Message}");
            return null;
        }
    }
}

/// <summary>Immutable record describing a successfully loaded plugin.</summary>
public record PluginInfo(string Name, string Version, string FileName, int ShapesRegistered);
