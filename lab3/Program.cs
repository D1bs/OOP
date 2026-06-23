using ShapeSerializer.Models;
using ShapeSerializer.Serialization;
using ShapeSerializer.UI;

// ── Shape Registration ───────────────────────────────────────────────────────
// This is the ONLY place that needs to change when a new shape class is added.
// Everything else (serializer, registry, UI) works automatically.
//
// Prototype pattern: each registered object acts as a factory for its own type.
ShapeRegistry.Register(new Circle());
ShapeRegistry.Register(new Rectangle());
ShapeRegistry.Register(new Triangle());
ShapeRegistry.Register(new Ellipse());
ShapeRegistry.Register(new Rhombus());
ShapeRegistry.Register(new Trapezoid());
// ShapeRegistry.Register(new Pentagon());  

// ── Start UI ─────────────────────────────────────────────────────────────────
var menu = new ConsoleMenu();
menu.Run();
