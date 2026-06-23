using System;
using System.Collections.Generic;

// ===================== Абстрактный базовый класс =====================

/// <summary>
/// Абстрактный класс Shape — базовый для всех геометрических фигур.
/// Объявляет виртуальный метод Draw(), который каждая фигура переопределяет.
/// </summary>
abstract class Shape
{
    // Свойство с именем фигуры
    public abstract string Name { get; }

    // Виртуальный метод отрисовки — полиморфизм
    public abstract void Draw();

    // Переопределение ToString для удобного вывода
    public override string ToString() => Name;
}

// ===================== Иерархия фигур =====================

/// <summary>Отрезок задаётся двумя точками (x1,y1) и (x2,y2).</summary>
class Line : Shape
{
    public int X1, Y1, X2, Y2;
    public override string Name => "Отрезок";
    public Line(int x1, int y1, int x2, int y2) { X1=x1; Y1=y1; X2=x2; Y2=y2; }
    public override void Draw() =>
        Console.WriteLine($"  Line({X1}, {Y1}, {X2}, {Y2})");
}

/// <summary>Прямоугольник задаётся левым верхним углом, шириной и высотой.</summary>
class Rectangle : Shape
{
    public int X, Y, Width, Height;
    public override string Name => "Прямоугольник";
    public Rectangle(int x, int y, int w, int h) { X=x; Y=y; Width=w; Height=h; }
    public override void Draw() =>
        Console.WriteLine($"  Rectangle({X}, {Y}, {Width}, {Height})");
}

/// <summary>Квадрат — частный случай прямоугольника.</summary>
class Square : Rectangle
{
    public override string Name => "Квадрат";
    public Square(int x, int y, int side) : base(x, y, side, side) { }
    public override void Draw() =>
        Console.WriteLine($"  Square({X}, {Y}, side={Width})");
}

/// <summary>Эллипс задаётся центром и двумя полуосями.</summary>
class Ellipse : Shape
{
    public int Cx, Cy, Rx, Ry;
    public override string Name => "Эллипс";
    public Ellipse(int cx, int cy, int rx, int ry) { Cx=cx; Cy=cy; Rx=rx; Ry=ry; }
    public override void Draw() =>
        Console.WriteLine($"  Ellipse({Cx}, {Cy}, rx={Rx}, ry={Ry})");
}

/// <summary>Круг — частный случай эллипса.</summary>
class Circle : Ellipse
{
    public override string Name => "Круг";
    public Circle(int cx, int cy, int r) : base(cx, cy, r, r) { }
    public override void Draw() =>
        Console.WriteLine($"  Circle({Cx}, {Cy}, r={Rx})");
}

/// <summary>Треугольник задаётся тремя точками.</summary>
class Triangle : Shape
{
    public int X1,Y1, X2,Y2, X3,Y3;
    public override string Name => "Треугольник";
    public Triangle(int x1,int y1,int x2,int y2,int x3,int y3)
    { X1=x1;Y1=y1;X2=x2;Y2=y2;X3=x3;Y3=y3; }
    public override void Draw() =>
        Console.WriteLine($"  Triangle({X1},{Y1}; {X2},{Y2}; {X3},{Y3})");
}

// ===================== Список фигур =====================

/// <summary>
/// ShapeCollection — отдельный класс-коллекция для хранения фигур.
/// Демонстрирует полиморфизм: метод DrawAll() работает с базовым типом Shape.
/// </summary>
class ShapeCollection
{
    private List<Shape> _shapes = new List<Shape>();

    public void Add(Shape s) => _shapes.Add(s);

    /// <summary>Рисует все фигуры через общий интерфейс базового класса.</summary>
    public void DrawAll()
    {
        Console.WriteLine($"{'─',1} Список фигур ({_shapes.Count} шт.) {'─',1}");
        foreach (Shape s in _shapes)
        {
            Console.Write($"[{s.Name,-14}] → ");
            s.Draw(); // динамическое связывание — вызывается нужный Draw()
        }
        Console.WriteLine(new string('─', 40));
    }
}

// ===================== Точка входа =====================

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Статическая инициализация списка фигур
        var collection = new ShapeCollection();
        collection.Add(new Line(0, 0, 100, 50));
        collection.Add(new Rectangle(10, 20, 80, 40));
        collection.Add(new Square(5, 5, 60));
        collection.Add(new Ellipse(50, 50, 30, 15));
        collection.Add(new Circle(70, 70, 25));
        collection.Add(new Triangle(10,10, 50,90, 90,10));

        // Запуск рисования
        collection.DrawAll();
    }
}
