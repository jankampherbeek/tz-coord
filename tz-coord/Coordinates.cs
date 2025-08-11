/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma-AR.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

namespace TzCoordCSharp;

public static class Coordinates
{
    private const string Sep = "\\";
    private const string CitiesInputFile = "." + Sep + "coord" + Sep + "cities500.txt";
    private const string CitiesOutputFile = "." + Sep + "results" + Sep + "cities.csv";
    private const string CitiesRegionsOutputFile = "." + Sep + "results" + Sep + "citiesregions.csv";
    private const string RegionsInputFile = "." + Sep + "coord" + Sep + "admin1CodesAscII.txt";
    private const string RegionsOutputFile = "." + Sep + "results" + Sep + "regions.csv";
    private const string CountryInputFile = "." + Sep + "coord" + Sep + "countryinfo.txt";
    private const string CountryOutputFile = "." + Sep + "results" + Sep + "countries.csv";

    public static void HandleCoordinates()
    {
        var error = HandleCities();
        if (error != null)
        {
            Console.WriteLine(error.Message);
        }
        
        error = HandleRegions();
        if (error != null)
        {
            Console.WriteLine(error.Message);
        }
        
        error = HandleCountries();
        if (error != null)
        {
            Console.WriteLine(error.Message);
        }

        TimeZones.EnrichCityNames();
    }

    private static Exception? HandleCities()
    {
        try
        {
            var lines = File.ReadAllLines(CitiesInputFile);
            var outputLines = new List<string>();

            foreach (var line in lines)
            {
                var fields = line.Split('\t');
                if (fields.Length < 9)
                {
                    Console.WriteLine("Warning: line has less than 9 fields, skipping");
                    continue;
                }
                
                // Select required fields (0-based index)
                var selectedFields = new[]
                {
                    fields[8],  // country
                    fields[1],  // name location
                    fields[4],  // latitude
                    fields[5],  // longitude
                    fields[10], // admin2 ; province or state
                    fields[16], // elevation in meters
                    fields[17], // indication for timezone
                };
                
                var outputLine = string.Join(";", selectedFields);
                outputLines.Add(outputLine);
            }

            // Ensure directory exists
            var outputDir = Path.GetDirectoryName(CitiesOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(CitiesOutputFile, outputLines);
            Console.WriteLine("Processing completed successfully");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing cities: {ex.Message}");
            return ex;
        }
    }

    private static Exception? HandleRegions()
    {
        try
        {
            var lines = File.ReadAllLines(RegionsInputFile);
            var outputLines = new List<string>();

            foreach (var line in lines)
            {
                var fields = line.Split('\t');
                if (fields.Length < 4)
                {
                    Console.WriteLine($"Warning: line has less than 4 fields, skipping {line}");
                    continue;
                }
                
                // Select required fields (0-based index)
                var selectedFields = new[]
                {
                    fields[0], // code for region
                    fields[1], // name for region
                };

                var outputLine = string.Join(";", selectedFields);
                outputLines.Add(outputLine);
            }

            // Ensure directory exists
            var outputDir = Path.GetDirectoryName(RegionsOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(RegionsOutputFile, outputLines);
            Console.WriteLine("Processing completed successfully");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing regions: {ex.Message}");
            return ex;
        }
    }

    private static Exception? HandleCountries()
    {
        try
        {
            var lines = File.ReadAllLines(CountryInputFile);
            var outputLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    continue; // Skip comment lines
                }
                
                var fields = line.Split('\t');
                if (fields.Length < 4)
                {
                    continue;
                }
                
                // Select required fields (0-based index)
                var selectedFields = new[]
                {
                    fields[0], // country code
                    fields[4], // country name
                    fields[8], // continent
                };

                var outputLine = string.Join(";", selectedFields);
                outputLines.Add(outputLine);
            }

            // Ensure directory exists
            var outputDir = Path.GetDirectoryName(CountryOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(CountryOutputFile, outputLines);
            Console.WriteLine("Processing completed successfully");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing countries: {ex.Message}");
            return ex;
        }
    }


}

