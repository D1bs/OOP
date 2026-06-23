using ShapeApp.Models;
using ShapeApp.Plugins;
using ShapeApp.Processors;
using ShapeApp.Serialization;
using ShapeApp.UI;

// ── 1. Register built-in shape prototypes ────────────────────────────────────
ShapeRegistry.Register(new Circle());
ShapeRegistry.Register(new Rectangle());
ShapeRegistry.Register(new Triangle());
ShapeRegistry.Register(new Ellipse());
ShapeRegistry.Register(new Rhombus());
ShapeRegistry.Register(new Trapezoid());

// ── 2. Load shape plugins (Pentagon, Star, etc.) ─────────────────────────────
var shapePluginDir  = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "plugins");
var loadedShapePlugins = PluginLoader.LoadAll(shapePluginDir);

// ── 3. Load processor plugins (XmlToJson, JsonPrettify, Base64, etc.) ────────
// Processor plugins live in a separate sub-folder so they are never confused
// with shape plugins by the shape PluginLoader.
var processorRegistry  = new ProcessorRegistry();
var processorPluginDir = Path.Combine(AppContext.BaseDirectory, "processors");
var loadedProcessors   = ProcessorPluginLoader.LoadAll(processorPluginDir, processorRegistry);

// ── 4. Run the interactive UI ─────────────────────────────────────────────────
var menu = new ConsoleMenu(new ShapeList(), loadedShapePlugins, processorRegistry, loadedProcessors);
menu.Run();
