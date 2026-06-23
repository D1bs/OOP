using ShapeApp.Models;
using ShapeApp.Patterns;
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
var shapePluginDir     = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "plugins");
var loadedShapePlugins = PluginLoader.LoadAll(shapePluginDir);

// ── 3. Build ProcessorRegistry with our own plugins ───────────────────────────
// PATTERN: Decorator — wrap each processor with logging decorator
var processorRegistry = new ProcessorRegistry();
var processorPluginDir = Path.Combine(AppContext.BaseDirectory, "processors");

// ── 4. Load classmate's plugin via Adapter ────────────────────────────────────
// IMPORTANT: CsvAnnotator must be registered BEFORE Base64 in the pipeline.
// Pipeline order (save): XML → XmlToJson → JsonPrettify → CsvAnnotator → Base64 → file
// CsvAnnotator works on JSON; Base64 encodes the result — so CSV comes first.
// PATTERN: Adapter — FriendPluginAdapter bridges IFriendProcessor → IProcessorPlugin
var friendProcessor = new FriendPlugin.CsvAnnotatorProcessor();
var friendAdapter   = new FriendPluginAdapter(friendProcessor);   // <-- Adapter
// Wrap in logging decorator so every call is logged
// PATTERN: Decorator
var loggedFriendAdapter = new LoggingProcessorDecorator(friendAdapter);
processorRegistry.Register(loggedFriendAdapter);

// Load DLL-based processor plugins AFTER CsvAnnotator so Base64 comes last.
var loadedProcessors = ProcessorPluginLoader.LoadAll(processorPluginDir, processorRegistry);

// ── 5. Create Facade ──────────────────────────────────────────────────────────
// PATTERN: Facade — single entry point for save/load operations
var facade = new SerializationFacade(processorRegistry);

// ── 6. Run the interactive UI ─────────────────────────────────────────────────
var menu = new ConsoleMenu(new ShapeList(), loadedShapePlugins, processorRegistry,
                           loadedProcessors, facade);
menu.Run();