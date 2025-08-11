/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma-AR.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

namespace TzCoordCSharp;

public static class TimeZones
{
    private const string TzOutputFile = "tz.txt";
    private const string CitiesRegionsOutputFile = "." + "\\" + "results" + "\\" + "citiesregions.csv";
    private const string CitiesOutputFile = "." + "\\" + "results" + "\\" + "cities.csv";
    private const string RegionsOutputFile = "." + "\\" + "results" + "\\" + "regions.csv";

    public static void HandleTimeZones()
    {
        ProcessTzFiles("africa");
        ProcessTzFiles("antarctica");
        ProcessTzFiles("asia");
        ProcessTzFiles("australasia");
        ProcessTzFiles("backzone");
        ProcessTzFiles("europe");
        ProcessTzFiles("northamerica");
        ProcessTzFiles("southamerica");
    }

    // TODO remove this function to timezones ?
    private static void ProcessTzFiles(string inputFilename)
    {
        try
        {
            // Read input file
            var lines = File.ReadAllLines(inputFilename);
            var outputLines = new List<string>();

            // Process each line
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Check if field is not commented out
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }
                
                if (trimmedLine[0] == '#')
                {
                    continue;
                }
                
                if (trimmedLine.Contains('#'))
                {
                    trimmedLine = trimmedLine.Split('#')[0];
                }
                
                outputLines.Add(trimmedLine);
            }

            // Append to output file
            File.AppendAllLines(TzOutputFile, outputLines);
            Console.WriteLine("Processing completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing timezone file {inputFilename}: {ex.Message}");
        }
    }

    // Prompt: Read the content of cities.csv. Replace the name of the cities (2nd field in each line)
    // with the name, a psave and the name of the region between braces. The region can be found in the
    // file regions.csv (third item in each line). Select the correct region for each city by comparing
    // the abbreviation for the country (first field in both cities.csv and regions.csv) and the code for
    // the region (the fith field in cities.csv and the second field in regions.csv ). Write the updated
    // results to a file 'CitiesRegions.csv'
    public static void EnrichCityNames()
    {
        try
        {
            // Read regions.csv to build a map of region codes to region names
            var regionsMap = new Dictionary<string, string>();

            var regionsLines = File.ReadAllLines(RegionsOutputFile);
            foreach (var line in regionsLines)
            {
                var fields = line.Split(';');
                if (fields.Length >= 2)
                {
                    // fields[0] is the region code, fields[1] is the region name
                    regionsMap[fields[0]] = fields[1];
                }
            }

            // Read cities.csv and process each line
            var citiesLines = File.ReadAllLines(CitiesOutputFile);
            var outputLines = new List<string>();

            foreach (var line in citiesLines)
            {
                var fields = line.Split(';');
                if (fields.Length >= 7)
                {
                    var countryCode = fields[0]; // country code
                    var cityName = fields[1];    // city name
                    // var latitude = fields[2];      // latitude
                    // var longitude = fields[3];     // longitude
                    var regionCode = fields[4]; // region code (5th field, 0-based index 4)
                    // var elevation = fields[5];     // elevation
                    // var timezone = fields[6];      // timezone

                    // Create the region key by combining country code and region code
                    var regionKey = countryCode + "." + regionCode;

                    // Look up the region name
                    if (regionsMap.TryGetValue(regionKey, out var regionName))
                    {
                        // Replace city name with "cityName [regionName]"
                        var enrichedCityName = cityName + " (" + regionName + ")";
                        fields[1] = enrichedCityName;
                    }

                    // Write the updated line
                    var outputLine = string.Join(";", fields);
                    outputLines.Add(outputLine);
                }
            }

            // Ensure directory exists
            var outputDir = Path.GetDirectoryName(CitiesRegionsOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(CitiesRegionsOutputFile, outputLines);
            Console.WriteLine("City names enrichment completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enriching city names: {ex.Message}");
        }
    }
}
