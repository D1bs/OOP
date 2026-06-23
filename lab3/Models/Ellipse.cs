using System.Xml;

namespace ShapeSerializer.Models;

/// <summary>Represents an ellipse defined by semi-major axis (a) and semi-minor axis (b).</summary>
public class Ellipse : Shape
{
    public override string ShapeType => "Ellipse";

    /// <summary>Semi-major axis.</summary>
    public double A { get; set; }

    /// <summary>Semi-minor axis.</summary>
    public double B { get; set; }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString("A", A.ToString());
        writer.WriteElementString("B", B.ToString());
    }

    public override void ReadXml(XmlElement element)
    {
        A = double.Parse(element["A"]!.InnerText);
        B = double.Parse(element["B"]!.InnerText);
    }

    public override string Describe() =>
        $"Ellipse | A: {A:F2}, B: {B:F2} | Area: {Math.PI * A * B:F4}";

    public override void InputProperties()
    {
        Console.Write("  Semi-major axis (A): ");
        A = double.Parse(Console.ReadLine()!);
        Console.Write("  Semi-minor axis (B): ");
        B = double.Parse(Console.ReadLine()!);
    }

    public override Shape CreateInstance() => new Ellipse();
}
