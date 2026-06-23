using System.Xml;

namespace ShapeSerializer.Models;

/// <summary>Represents a rectangle defined by width and height.</summary>
public class Rectangle : Shape
{
    public override string ShapeType => "Rectangle";

    public double Width  { get; set; }
    public double Height { get; set; }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString("Width",  Width.ToString());
        writer.WriteElementString("Height", Height.ToString());
    }

    public override void ReadXml(XmlElement element)
    {
        Width  = double.Parse(element["Width"]!.InnerText);
        Height = double.Parse(element["Height"]!.InnerText);
    }

    public override string Describe() =>
        $"Rectangle | Width: {Width:F2}, Height: {Height:F2} | Area: {Width * Height:F4}";

    public override void InputProperties()
    {
        Console.Write("  Width:  ");
        Width  = double.Parse(Console.ReadLine()!);
        Console.Write("  Height: ");
        Height = double.Parse(Console.ReadLine()!);
    }

    public override Shape CreateInstance() => new Rectangle();
}
