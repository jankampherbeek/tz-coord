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

/// <summary>
/// Hardcoded file paths. Change these values and adapt it to your own situation.
/// </summary>
public static class FilePaths
{
    public const string CountriesFile = @"E:\tz-coord\tz-coord\data\countries.csv";
    public const string CitiesFile = @"E:\tz-coord\tz-coord\data\citiesnoreg.csv";
    public const string RegionsFile = @"E:\tz-coord\tz-coord\data\regions.csv";
    public const string CitiesRegionsOutputFile = @"E:\tz-coord\tz-coord\results\cities.csv";
    public const string CitiesOutputFile = @"E:\tz-coord\tz-coord\results\cities.csv";
    public const string RegionsOutputFile = @"E:\tz-coord\tz-coord\results\regions.csv";
    public const string CitiesInputFile = @"E:\tz-coord\tz-coord\coord\cities500.txt";
    public const string RegionsInputFile = @"E:\tz-coord\tz-coord\coord\admin1CodesAscII.txt";
    public const string CountryInputFile = @"E:\tz-coord\tz-coord\coord\countryinfo.txt";
    public const string CountryOutputFile = @"E:\tz-coord\tz-coord\results\countries.csv";
    public const string ContinentPrefix = @"E:\tz-coord\tz-coord\continents\";
    
    public const string prefix = @"E:\tz-coord\tz-coord\";
    public const string Sep = "\\";
    public const string africaInputFile = FilePaths.ContinentPrefix + "africa";
    public const string antarcticaInputFile = FilePaths.ContinentPrefix + "antarctica";
    public const string asiaInputFile = FilePaths.ContinentPrefix + "asia";
    public const string australasiaInputFile = FilePaths.ContinentPrefix + "australasia";
    public const string backzoneInputFile = FilePaths.ContinentPrefix + "backzone";
    public const string europeInputFile = FilePaths.ContinentPrefix + "europe";
    public const string northamericaInputFile = FilePaths.ContinentPrefix + "northamerica";
    public const string southamericaInputFile = FilePaths.ContinentPrefix + "southamerica";
    public const string outputFile1 = prefix  + Sep + "tz" + Sep + "output1";          // temporary file for intermediate results
    public const string outputFile2Tz = prefix + Sep + "tz" + Sep + "output2Tz";        // temporary file for intermediate results timezones
    public const string outputFile2Dst = prefix + Sep + "tz" + Sep + "output2Dst";       // temporary file for intermediate results dst
    public const string tzDataFile = prefix + Sep + "results" + Sep + "tzdata.csv";  // final results for timezones
    public const string dstDataFile = prefix + Sep + "results" + Sep + "dstdata.csv"; // final results for dst
    public const string resultsFolder = prefix + Sep + "results";

    public const string TzOutputFile = prefix + Sep + "tz" + Sep + "tz.txt";
}