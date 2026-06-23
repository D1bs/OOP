using System.Xml;

namespace ShapeSerializer.Models;

/// <summary>Represents a circle defined by its radius.</summary>
public class Circle : Shape
{
    public override string ShapeType => "Circle";

    /// <summary>Radius of the circle.</summary>
    public double Radius { get; set; }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString("Radius", Radius.ToString());
    }

    public override void ReadXml(XmlElement element)
    {
        Radius = double.Parse(element["Radius"]!.InnerText);
    }

    public override string Describe() =>
        $"Circle | Radius: {Radius:F2} | Area: {Math.PI * Radius * Radius:F4}";

    public override void InputProperties()
    {
        Console.Write("  Radius: ");
        Radius = double.Parse(Console.ReadLine()!);
    }

    public override Shape CreateInstance() => new Circle();
}
