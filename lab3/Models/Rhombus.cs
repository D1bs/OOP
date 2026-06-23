using System.Xml;

namespace ShapeSerializer.Models;

/// <summary>Represents a rhombus defined by its two diagonals.</summary>
public class Rhombus : Shape
{
    public override string ShapeType => "Rhombus";

    /// <summary>First diagonal.</summary>
    public double Diagonal1 { get; set; }

    /// <summary>Second diagonal.</summary>
    public double Diagonal2 { get; set; }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString("Diagonal1", Diagonal1.ToString());
        writer.WriteElementString("Diagonal2", Diagonal2.ToString());
    }

    public override void ReadXml(XmlElement element)
    {
        Diagonal1 = double.Parse(element["Diagonal1"]!.InnerText);
        Diagonal2 = double.Parse(element["Diagonal2"]!.InnerText);
    }

    public override string Describe() =>
        $"Rhombus | d1: {Diagonal1:F2}, d2: {Diagonal2:F2} | Area: {Diagonal1 * Diagonal2 / 2:F4}";

    public override void InputProperties()
    {
        Console.Write("  Diagonal 1: ");
        Diagonal1 = double.Parse(Console.ReadLine()!);
        Console.Write("  Diagonal 2: ");
        Diagonal2 = double.Parse(Console.ReadLine()!);
    }

    public override Shape CreateInstance() => new Rhombus();
}
