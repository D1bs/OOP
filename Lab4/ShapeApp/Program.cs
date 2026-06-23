using ShapeApp.Models;
using ShapeApp.Plugins;
using ShapeApp.Serialization;
using ShapeApp.UI;

// ── 1. Register built-in shape types (Prototype pattern) ────────────────────
ShapeRegistry.Register(new Circle());
ShapeRegistry.Register(new Rectangle());
ShapeRegistry.Register(new Triangle());
ShapeRegistry.Register(new Ellipse());
ShapeRegistry.Register(new Rhombus());
ShapeRegistry.Register(new Trapezoid());

// ── 2. Dynamically load plugins from the 'plugins' sub-folder ───────────────
// The folder path can be overridden via the first command-line argument:
//   dotnet run -- /path/to/my/plugins
var pluginDir    = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "plugins");
var loadedPlugins = PluginLoader.LoadAll(pluginDir);

// ── 3. Run the interactive UI ────────────────────────────────────────────────
var menu = new ConsoleMenu(new ShapeList(), loadedPlugins);
menu.Run();
