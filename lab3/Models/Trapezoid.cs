using System.Xml;

namespace ShapeSerializer.Models;

/// <summary>
/// Represents a trapezoid defined by two parallel bases and height.
/// This is the 6th class in the hierarchy – demonstrates that adding it
/// required ZERO changes to Shape, ShapeRegistry, or XmlShapeSerializer.
/// </summary>
public class Trapezoid : Shape
{
    public override string ShapeType => "Trapezoid";

    /// <summary>Length of the first (longer) base.</summary>
    public double Base1 { get; set; }

    /// <summary>Length of the second (shorter) base.</summary>
    public double Base2 { get; set; }

    /// <summary>Perpendicular height between the two bases.</summary>
    public double Height { get; set; }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString("Base1",  Base1.ToString());
        writer.WriteElementString("Base2",  Base2.ToString());
        writer.WriteElementString("Height", Height.ToString());
    }

    public override void ReadXml(XmlElement element)
    {
        Base1  = double.Parse(element["Base1"]!.InnerText);
        Base2  = double.Parse(element["Base2"]!.InnerText);
        Height = double.Parse(element["Height"]!.InnerText);
    }

    public override string Describe() =>
        $"Trapezoid | Base1: {Base1:F2}, Base2: {Base2:F2}, Height: {Height:F2} | Area: {(Base1 + Base2) / 2 * Height:F4}";

    public override void InputProperties()
    {
        Console.Write("  Base 1: ");
        Base1  = double.Parse(Console.ReadLine()!);
        Console.Write("  Base 2: ");
        Base2  = double.Parse(Console.ReadLine()!);
        Console.Write("  Height: ");
        Height = double.Parse(Console.ReadLine()!);
    }

    public override Shape CreateInstance() => new Trapezoid();
}
