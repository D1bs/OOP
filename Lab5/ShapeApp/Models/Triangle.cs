using System.Xml;
using ShapeContracts;

namespace ShapeApp.Models;

/// <summary>Triangle defined by three side lengths; area via Heron's formula.</summary>
public class Triangle : Shape
{
    public override string ShapeType => "Triangle";
    public double SideA { get; set; }
    public double SideB { get; set; }
    public double SideC { get; set; }

    private double Area()
    {
        double s = (SideA + SideB + SideC) / 2.0;
        return Math.Sqrt(s * (s - SideA) * (s - SideB) * (s - SideC));
    }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString("SideA", SideA.ToString());
        writer.WriteElementString("SideB", SideB.ToString());
        writer.WriteElementString("SideC", SideC.ToString());
    }

    public override void ReadXml(XmlElement element)
    {
        SideA = double.Parse(element["SideA"]!.InnerText);
        SideB = double.Parse(element["SideB"]!.InnerText);
        SideC = double.Parse(element["SideC"]!.InnerText);
    }

    public override string Describe() =>
        $"Triangle     | Sides: {SideA:F2}, {SideB:F2}, {SideC:F2} | Area: {Area():F4}";

    public override void InputProperties()
    {
        Console.Write("  Side A: ");
        SideA = double.Parse(Console.ReadLine()!);
        Console.Write("  Side B: ");
        SideB = double.Parse(Console.ReadLine()!);
        Console.Write("  Side C: ");
        SideC = double.Parse(Console.ReadLine()!);
    }

    public override IShape CreateInstance() => new Triangle();
}
