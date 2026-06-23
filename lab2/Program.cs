using System;
using System.Collections.Generic;

// ===================== Shape data classes (no Draw method here) =====================

/// <summary>
/// Abstract base class for all shapes.
/// Shapes only store data — they do NOT know how to draw themselves.
/// Drawing is handled by the Renderer (Visitor pattern).
/// </summary>
abstract class Shape
{
    public abstract string Name { get; }
    /// <summary>Accept a renderer — dispatch to the correct Render overload.</summary>
    public abstract void Accept(IRenderer renderer);
}

class Line : Shape
{
    public int X1, Y1, X2, Y2;
    public override string Name => "Line";
    public Line(int x1, int y1, int x2, int y2) { X1=x1; Y1=y1; X2=x2; Y2=y2; }
    public override void Accept(IRenderer r) => r.Render(this);
}

class Rectangle : Shape
{
    public int X, Y, Width, Height;
    public override string Name => "Rectangle";
    public Rectangle(int x, int y, int w, int h) { X=x; Y=y; Width=w; Height=h; }
    public override void Accept(IRenderer r) => r.Render(this);
}

class Square : Rectangle
{
    public override string Name => "Square";
    public Square(int x, int y, int side) : base(x, y, side, side) {}
    public override void Accept(IRenderer r) => r.Render(this);
}

class Ellipse : Shape
{
    public int Cx, Cy, Rx, Ry;
    public override string Name => "Ellipse";
    public Ellipse(int cx, int cy, int rx, int ry) { Cx=cx; Cy=cy; Rx=rx; Ry=ry; }
    public override void Accept(IRenderer r) => r.Render(this);
}

class Circle : Ellipse
{
    public override string Name => "Circle";
    public Circle(int cx, int cy, int radius) : base(cx, cy, radius, radius) {}
    public override void Accept(IRenderer r) => r.Render(this);
}

class Triangle : Shape
{
    public int X1,Y1, X2,Y2, X3,Y3;
    public override string Name => "Triangle";
    public Triangle(int x1,int y1,int x2,int y2,int x3,int y3)
    { X1=x1;Y1=y1;X2=x2;Y2=y2;X3=x3;Y3=y3; }
    public override void Accept(IRenderer r) => r.Render(this);
}

// ===================== Renderer interface =====================

/// <summary>
/// IRenderer — interface for drawing shapes.
/// Adding a new shape type: add one Render overload here + implement in each renderer.
/// Adding a new renderer: implement this interface. No existing code changes needed.
/// </summary>
interface IRenderer
{
    void Render(Line s);
    void Render(Rectangle s);
    void Render(Square s);
    void Render(Ellipse s);
    void Render(Circle s);
    void Render(Triangle s);
}

// ===================== Console renderer =====================

/// <summary>Renders shapes as text strings to the console.</summary>
class ConsoleRenderer : IRenderer
{
    public void Render(Line s)      => Console.WriteLine($"  Line({s.X1},{s.Y1} -> {s.X2},{s.Y2})");
    public void Render(Rectangle s) => Console.WriteLine($"  Rectangle(x={s.X}, y={s.Y}, w={s.Width}, h={s.Height})");
    public void Render(Square s)    => Console.WriteLine($"  Square(x={s.X}, y={s.Y}, side={s.Width})");
    public void Render(Ellipse s)   => Console.WriteLine($"  Ellipse(cx={s.Cx}, cy={s.Cy}, rx={s.Rx}, ry={s.Ry})");
    public void Render(Circle s)    => Console.WriteLine($"  Circle(cx={s.Cx}, cy={s.Cy}, r={s.Rx})");
    public void Render(Triangle s)  => Console.WriteLine($"  Triangle({s.X1},{s.Y1}; {s.X2},{s.Y2}; {s.X3},{s.Y3})");
}

// ===================== Factory =====================

/// <summary>
/// ShapeFactory creates shapes by name from user input.
/// Adding new shapes: add one case here — no other code changes needed (Open/Closed Principle).
/// </summary>
static class ShapeFactory
{
    private static readonly Dictionary<string, Func<int[], Shape>> _creators = new()
    {
        ["line"]      = p => new Line(p[0], p[1], p[2], p[3]),
        ["rectangle"] = p => new Rectangle(p[0], p[1], p[2], p[3]),
        ["square"]    = p => new Square(p[0], p[1], p[2]),
        ["ellipse"]   = p => new Ellipse(p[0], p[1], p[2], p[3]),
        ["circle"]    = p => new Circle(p[0], p[1], p[2]),
        ["triangle"]  = p => new Triangle(p[0], p[1], p[2], p[3], p[4], p[5]),
    };

    public static Shape? Create(string type, int[] p)
    {
        return _creators.TryGetValue(type.ToLower(), out var creator) ? creator(p) : null;
    }
}

// ===================== Shape collection =====================

/// <summary>Stores a list of shapes and draws them all using a given renderer.</summary>
class ShapeCollection
{
    private readonly List<Shape> _shapes = new();

    public void Add(Shape s) => _shapes.Add(s);

    /// <summary>Draw every shape using the provided renderer.</summary>
    public void DrawAll(IRenderer renderer)
    {
        Console.WriteLine($"── {_shapes.Count} shapes ──");
        foreach (Shape s in _shapes)
        {
            Console.Write($"[{s.Name,-10}] → ");
            s.Accept(renderer);   // dispatch to correct Render() overload
        }
        Console.WriteLine(new string('─', 42));
    }
}

// ===================== Entry point =====================

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // --- Static demo ---
        var collection = new ShapeCollection();
        collection.Add(new Line(0, 0, 100, 50));
        collection.Add(new Rectangle(10, 20, 80, 40));
        collection.Add(new Square(5, 5, 60));
        collection.Add(new Ellipse(50, 50, 30, 15));
        collection.Add(new Circle(70, 70, 25));
        collection.Add(new Triangle(10,10, 50,90, 90,10));

        Console.WriteLine("=== Console renderer ===");
        collection.DrawAll(new ConsoleRenderer());
        
        // --- Interactive input ---
        Console.WriteLine("\n=== Add your own shape ===");
        Console.WriteLine("Types: line, rectangle, square, ellipse, circle, triangle");
        Console.Write("Enter type (or press Enter to skip): ");
        
        while (true)
        {
            Console.WriteLine("Types: line, rectangle, square, ellipse, circle, triangle");
            string? input = Console.ReadLine()?.Trim();
            
            if (!string.IsNullOrEmpty(input))
            {
                Console.Write("Enter parameters separated by spaces: ");
                string? paramStr = Console.ReadLine();
                try
                {
                    int[] p = Array.ConvertAll(paramStr!.Trim().Split(' '), int.Parse);
                    Shape? s = ShapeFactory.Create(input, p);
                    if (s != null)
                    {
                        Console.Write($"Created [{s.Name}] → ");
                        s.Accept(new ConsoleRenderer());
                    }
                    else Console.WriteLine("Unknown shape type.");
                }
                catch { Console.WriteLine("Invalid parameters."); }
            }   
        }
        
    }
}
