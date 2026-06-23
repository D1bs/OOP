using System.Xml;
using ShapeContracts;

namespace StarPlugin;

/// <summary>
/// Regular N-pointed star defined by number of points, outer radius and inner radius.
/// Area is computed as the difference between two regular polygons.
/// </summary>
public class Star : IShape
{
    public string ShapeType => "Star";

    /// <summary>Number of points (tips) on the star. Minimum 3.</summary>
    public int    Points      { get; set; } = 5;

    /// <summary>Outer (tip) radius of the star.</summary>
    public double OuterRadius { get; set; }

    /// <summary>Inner (valley) radius of the star.</summary>
    public double InnerRadius { get; set; }

    /// <summary>
    /// Area formula for a regular star polygon:
    /// A = n/2 * sin(2π/n) * (R² + r²) — where n=points, R=outer, r=inner.
    /// </summary>
    private double Area()
    {
        double angle = 2 * Math.PI / Points;
        return 0.5 * Points * Math.Sin(angle)
               * (OuterRadius * OuterRadius + InnerRadius * InnerRadius);
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString("Points",      Points.ToString());
        writer.WriteElementString("OuterRadius", OuterRadius.ToString());
        writer.WriteElementString("InnerRadius", InnerRadius.ToString());
    }

    public void ReadXml(XmlElement element)
    {
        Points      = int.Parse   (element["Points"]!.InnerText);
        OuterRadius = double.Parse(element["OuterRadius"]!.InnerText);
        InnerRadius = double.Parse(element["InnerRadius"]!.InnerText);
    }

    public string Describe() =>
        $"Star         | Points: {Points}, R: {OuterRadius:F2}, r: {InnerRadius:F2} | Area: {Area():F4}";

    public void InputProperties()
    {
        Console.Write("  Number of points (e.g. 5): ");
        Points = int.Parse(Console.ReadLine()!);
        Console.Write("  Outer radius: ");
        OuterRadius = double.Parse(Console.ReadLine()!);
        Console.Write("  Inner radius: ");
        InnerRadius = double.Parse(Console.ReadLine()!);
    }

    public IShape CreateInstance() => new Star();
}
