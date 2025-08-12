/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma.
 *  Copyright (c) 2025, Jan Kampherbeek.
 *  Enigma is open source.
 */

namespace tz_coord;


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
    private const string ContinentPrefix = @"E:\tz-coord\tz-coord\continents\";
    
    private const string Prefix = @"E:\tz-coord\tz-coord\";
    private const string Sep = "\\";
    public const string AfricaInputFile = FilePaths.ContinentPrefix + "africa";
    public const string AntarcticaInputFile = FilePaths.ContinentPrefix + "antarctica";
    public const string AsiaInputFile = FilePaths.ContinentPrefix + "asia";
    public const string AustralasiaInputFile = FilePaths.ContinentPrefix + "australasia";
    public const string BackzoneInputFile = FilePaths.ContinentPrefix + "backzone";
    public const string EuropeInputFile = FilePaths.ContinentPrefix + "europe";
    public const string NorthamericaInputFile = FilePaths.ContinentPrefix + "northamerica";
    public const string SouthamericaInputFile = FilePaths.ContinentPrefix + "southamerica";
    public const string OutputFile1 = Prefix  + Sep + "tz" + Sep + "output1";          // temporary file for intermediate results
    public const string OutputFile2Tz = Prefix + Sep + "tz" + Sep + "output2Tz";        // temporary file for intermediate results timezones
    public const string OutputFile2Dst = Prefix + Sep + "tz" + Sep + "output2Dst";       // temporary file for intermediate results dst
    public const string TzDataFile = Prefix + Sep + "results" + Sep + "tzdata.csv";  // final results for timezones
    public const string DstDataFile = Prefix + Sep + "results" + Sep + "dstdata.csv"; // final results for dst
    public const string ResultsFolder = Prefix + Sep + "results";

    public const string TzOutputFile = Prefix + Sep + "tz" + Sep + "tz.txt";
}