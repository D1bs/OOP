using System.Xml;
using ShapeContracts;

namespace PentagonPlugin;

/// <summary>
/// Regular pentagon defined by its side length.
/// Lives in a separate plugin DLL — the host app has no compile-time reference to this class.
/// </summary>
public class Pentagon : IShape
{
    public string ShapeType => "Pentagon";

    /// <summary>Length of one side of the regular pentagon.</summary>
    public double SideLength { get; set; }

    /// <summary>
    /// Area of a regular pentagon: (side² * √(25 + 10√5)) / 4
    /// </summary>
    private double Area() =>
        SideLength * SideLength * Math.Sqrt(25 + 10 * Math.Sqrt(5)) / 4.0;

    public void WriteXml(XmlWriter writer) =>
        writer.WriteElementString("SideLength", SideLength.ToString());

    public void ReadXml(XmlElement element) =>
        SideLength = double.Parse(element["SideLength"]!.InnerText);

    public string Describe() =>
        $"Pentagon     | Side: {SideLength:F2} | Area: {Area():F4}";

    public void InputProperties()
    {
        Console.Write("  Side length: ");
        SideLength = double.Parse(Console.ReadLine()!);
    }

    public IShape CreateInstance() => new Pentagon();
}
