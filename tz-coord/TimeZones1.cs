/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma-AR.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

using TzCoordCSharp;

namespace tz_coord;

public static class TimeZones1
{
    private const string TzOutputFile = "tz.txt";
    // private const string CitiesRegionsOutputFile = "." + "\\" + "results" + "\\" + "citiesregions.csv";
    // private const string CitiesOutputFile = "." + "\\" + "results" + "\\" + "cities.csv";
    // private const string RegionsOutputFile = "." + "\\" + "results" + "\\" + "regions.csv";

    public static void HandleTimeZones()
    {
        ProcessTzFiles(FilePaths.ContinentPrefix+"africa");
        ProcessTzFiles(FilePaths.ContinentPrefix+"antarctica");
        ProcessTzFiles(FilePaths.ContinentPrefix+"asia");
        ProcessTzFiles(FilePaths.ContinentPrefix+"australasia");
        ProcessTzFiles(FilePaths.ContinentPrefix+"backzone");
        ProcessTzFiles(FilePaths.ContinentPrefix+"europe");
        ProcessTzFiles(FilePaths.ContinentPrefix+"northamerica");
        ProcessTzFiles(FilePaths.ContinentPrefix+"southamerica");
    }
    
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
}
