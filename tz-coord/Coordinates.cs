/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma.
 *  Copyright (c) 2026, Jan Kampherbeek.
 *  Enigma is open source.
 */

namespace tz_coord;

public static class Coordinates
{

    public static void HandleCoordinates()
    {
        var translations = LoadTranslations();

        var error = HandleCities(translations);
        if (error != null)
        {
            Console.WriteLine(error.Message);
        }

        error = HandleRegions();
        if (error != null)
        {
            Console.WriteLine(error.Message);
        }

        error = HandleCountries(translations);
        if (error != null)
        {
            Console.WriteLine(error.Message);
        }

        EnrichCityNames();
    }

    /// <summary>
    /// Reads alternateNamesV2.txt and returns a dictionary mapping geoname ID to the set of
    /// distinct translated names in nl, de, fr and en that differ from nothing yet (caller filters
    /// against the canonical name).
    /// </summary>
    private static Dictionary<string, HashSet<string>> LoadTranslations()
    {
        var result = new Dictionary<string, HashSet<string>>();
        var supportedLangs = new HashSet<string> { "nl", "de", "fr", "en" };

        try
        {
            var lines = File.ReadAllLines(FilePaths.AltNamesInputFile);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = line.Split('\t');
                if (fields.Length < 4) continue;

                var lang = fields[2];
                if (!supportedLangs.Contains(lang)) continue;

                var geoId = fields[1];
                var name  = fields[3];
                if (string.IsNullOrWhiteSpace(name)) continue;

                if (!result.TryGetValue(geoId, out var names))
                {
                    names = new HashSet<string>(StringComparer.Ordinal);
                    result[geoId] = names;
                }
                names.Add(name);
            }
            Console.WriteLine("Loading translations completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading translations: {ex.Message}");
        }

        return result;
    }

    private static Exception? HandleCities(Dictionary<string, HashSet<string>> translations)
    {
        const int IDX_ID      = 0;
        const int IDX_COUNTRY = 8;
        const int IDX_NAME    = 1; // allCountries uses col 1 (UTF-8 name)
        const int IDX_LAT     = 4;
        const int IDX_LON     = 5;
        const int IDX_FEATURE = 6; // feature class; only 'P' (populated places) is used
        const int IDX_FCODE   = 7; // feature code; some sub-types are excluded
        const int IDX_ADMIN1  = 10;
        const int IDX_ELEV    = 15; // elevation (may be empty)
        const int IDX_TZ      = 17;

        // Sub-types excluded: not useful as birth locations
        // PPLX  section of populated place (suburb/district)
        // PPLQ  abandoned populated place
        // PPLW  destroyed populated place
        // PPLH  historical populated place
        // PPLCH historical capital
        // PPLL  populated locality
        // PPLF  farm village
        var excludedCodes = new HashSet<string> { "PPLX", "PPLQ", "PPLW", "PPLH", "PPLCH", "PPLL", "PPLF" };

        try
        {
            var lines = File.ReadAllLines(FilePaths.CitiesInputFile);
            var outputLines = new List<string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var fields = line.Split('\t');

                if (fields.Length < 18)
                {
                    Console.WriteLine($"Warning: line has {fields.Length} fields, skipping");
                    continue;
                }

                if (fields[IDX_FEATURE] != "P") continue;
                if (excludedCodes.Contains(fields[IDX_FCODE])) continue;

                var geoId   = fields[IDX_ID];
                var canName = fields[IDX_NAME];

                // Select required fields (0-based index)
                var selectedFields = new[]
                {
                    fields[IDX_COUNTRY],// country
                    canName,            // name location
                    fields[IDX_LAT],    // latitude
                    fields[IDX_LON],    // longitude
                    fields[IDX_ADMIN1], // admin1 ; province or state
                    fields[IDX_ELEV],   // elevation in meters
                    fields[IDX_TZ],     // indication for timezone
                };

                outputLines.Add(string.Join(";", selectedFields));

                // Add extra rows for translated names that differ from the canonical name
                if (translations.TryGetValue(geoId, out var translatedNames))
                {
                    foreach (var translatedName in translatedNames)
                    {
                        if (translatedName == canName) continue;
                        selectedFields[1] = translatedName;
                        outputLines.Add(string.Join(";", selectedFields));
                    }
                }
            }

            // Ensure directory exists
            var outputDir = Path.GetDirectoryName(FilePaths.CitiesOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(FilePaths.CitiesOutputFile, outputLines);
            Console.WriteLine("Processing cities completed successfully");
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
            var lines = File.ReadAllLines(FilePaths.RegionsInputFile);
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
            var outputDir = Path.GetDirectoryName(FilePaths.RegionsOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(FilePaths.RegionsOutputFile, outputLines);
            Console.WriteLine("Processing completed successfully");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing regions: {ex.Message}");
            return ex;
        }
    }

    private static Exception? HandleCountries(Dictionary<string, HashSet<string>> translations)
    {
        const int IDX_CODE      = 0;
        const int IDX_NAME      = 4;
        const int IDX_CONTINENT = 8;
        const int IDX_GEOID     = 16;

        try
        {
            var lines = File.ReadAllLines(FilePaths.CountryInputFile);
            var outputLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    continue; // Skip comment lines
                }

                var fields = line.Split('\t');
                if (fields.Length < 17)
                {
                    continue;
                }

                var canName = fields[IDX_NAME];
                var selectedFields = new[]
                {
                    fields[IDX_CODE],      // country code
                    canName,               // country name
                    fields[IDX_CONTINENT], // continent
                };

                outputLines.Add(string.Join(";", selectedFields));

                // Add extra rows for translated names that differ from the canonical name
                var geoId = fields[IDX_GEOID].Trim();
                if (!string.IsNullOrEmpty(geoId) && translations.TryGetValue(geoId, out var translatedNames))
                {
                    foreach (var translatedName in translatedNames)
                    {
                        if (translatedName == canName) continue;
                        selectedFields[1] = translatedName;
                        outputLines.Add(string.Join(";", selectedFields));
                    }
                }
            }

            // Ensure directory exists
            var outputDir = Path.GetDirectoryName(FilePaths.CountryOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(FilePaths.CountryOutputFile, outputLines);
            Console.WriteLine("Processing completed successfully");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing countries: {ex.Message}");
            return ex;
        }
    }

  // Prompt: Read the content of cities.csv. Replace the name of the cities (2nd field in each line)
    // with the name, and the name of the region between braces. The region can be found in the
    // file regions.csv (third item in each line). Select the correct region for each city by comparing
    // the abbreviation for the country (first field in both cities.csv and regions.csv) and the code for
    // the region (the fith field in cities.csv and the second field in regions.csv ). Write the updated
    // results to a file 'CitiesRegions.csv'

    private static void EnrichCityNames()
    {
        try
        {
            // Read regions.csv to build a map of region codes to region names
            var regionsMap = new Dictionary<string, string>();

            var regionsLines = File.ReadAllLines(FilePaths.RegionsOutputFile);
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
            var citiesLines = File.ReadAllLines(FilePaths.CitiesOutputFile);
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
                    var regionCode = fields[4];    // region code
                    // var elevation = fields[5];     // elevation
                    // var timezone = fields[6];      // timezone

                    // Create the region key by combining country code and region code
                    var regionKey = $"{countryCode}.{regionCode}";

                    // Look up the region name
                    if (regionsMap.TryGetValue(regionKey, out var regionName))
                    {
                        // Replace city name with "cityName [regionName]"
                        fields[1] = $"{cityName} ({regionName})";
                    }

                    // Write the updated line
                    var outputLine = string.Join(";", fields);
                    outputLines.Add(outputLine);
                }
            }

            // Ensure directory exists
            var outputDir = Path.GetDirectoryName(FilePaths.CitiesRegionsOutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllLines(FilePaths.CitiesRegionsOutputFile, outputLines);
            Console.WriteLine("City names enrichment completed successfully"); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enriching city names: {ex.Message}");
        }
    }
}

