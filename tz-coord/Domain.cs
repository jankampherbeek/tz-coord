namespace TzCoordCSharp;

// DateTimeHms represents a date and time with hour, minute, second
public struct DateTimeHms
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Min { get; set; }
    public int Sec { get; set; }
    public bool Greg { get; set; }
    public double Dst { get; set; }
    public double TZone { get; set; }
}

// Country represents a country with code and name
public struct Country
{
    public string Code { get; set; }
    public string Name { get; set; }
}

// City represents a city with name, coordinates, and timezone
public struct City
{
    public string Country { get; set; }
    public string Name { get; set; }
    public string GeoLat { get; set; }
    public string GeoLong { get; set; }
    public string Region { get; set; }
    public string Elevation { get; set; }
    public string IndicationTz { get; set; }
}

// PathSep is the path separator for the current operating system
public static class PathSep
{
    public static string Separator => Path.DirectorySeparatorChar.ToString();
}

