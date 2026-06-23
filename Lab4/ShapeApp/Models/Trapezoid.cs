using System.Xml;
using ShapeContracts;

namespace ShapeApp.Models;

/// <summary>Trapezoid defined by two parallel bases and height.</summary>
public class Trapezoid : Shape
{
    public override string ShapeType => "Trapezoid";
    public double Base1  { get; set; }
    public double Base2  { get; set; }
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
        $"Trapezoid    | Base1: {Base1:F2}, Base2: {Base2:F2}, H: {Height:F2} | Area: {(Base1 + Base2) / 2 * Height:F4}";

    public override void InputProperties()
    {
        Console.Write("  Base 1: ");
        Base1  = double.Parse(Console.ReadLine()!);
        Console.Write("  Base 2: ");
        Base2  = double.Parse(Console.ReadLine()!);
        Console.Write("  Height: ");
        Height = double.Parse(Console.ReadLine()!);
    }

    public override IShape CreateInstance() => new Trapezoid();
}
