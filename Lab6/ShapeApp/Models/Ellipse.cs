using System.Xml;
using ShapeContracts;

namespace ShapeApp.Models;

/// <summary>Ellipse defined by semi-major axis A and semi-minor axis B.</summary>
public class Ellipse : Shape
{
    public override string ShapeType => "Ellipse";
    public double A { get; set; }
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
        $"Ellipse      | A: {A:F2}, B: {B:F2} | Area: {Math.PI * A * B:F4}";

    public override void InputProperties()
    {
        Console.Write("  Semi-major axis (A): ");
        A = double.Parse(Console.ReadLine()!);
        Console.Write("  Semi-minor axis (B): ");
        B = double.Parse(Console.ReadLine()!);
    }

    public override IShape CreateInstance() => new Ellipse();
}
