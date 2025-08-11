/*
 *  Enigma Astrology Research.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

namespace TzCoordCSharp;

public interface ILocationHandler
{
    (List<Country> countries, Exception? error) Countries();
    (List<City> cities, Exception? error) Cities(string countryCode);
}

public class LocationHandling : ILocationHandler
{
    private static readonly string CountriesFile = $"..{PathSep.Separator}..{PathSep.Separator}data{PathSep.Separator}countries.csv";
    private static readonly string CitiesFile = $"..{PathSep.Separator}..{PathSep.Separator}data{PathSep.Separator}cities.csv";
    private static readonly string RegionsFile = $"..{PathSep.Separator}..{PathSep.Separator}data{PathSep.Separator}regions.csv";
    private const string ItemSeparator = ";";

    public static ILocationHandler NewLocationHandling()
    {
        return new LocationHandling();
    }

    // Countries returns all available countries
    public (List<Country> countries, Exception? error) Countries()
    {
        try
        {
            var countries = new List<Country>();
            var lines = File.ReadAllLines(CountriesFile);
            
            foreach (var line in lines)
            {
                var fields = line.Split(ItemSeparator);
                if (fields.Length == 3) // there is a field for continent that is currently not used
                {
                    var country = new Country
                    {
                        Code = fields[0].Trim(),
                        Name = fields[1].Trim()
                    };
                    countries.Add(country);
                }
            }
            
            return (countries, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading countries file: {ex.Message}");
            return (new List<Country>(), ex);
        }
    }

    // Cities returns all the cities for a given country
    public (List<City> cities, Exception? error) Cities(string countryCode)
    {
        try
        {
            var cities = new List<City>();
            var lines = File.ReadAllLines(CitiesFile);
            
            foreach (var line in lines)
            {
                var fields = line.Split(ItemSeparator);
                if (fields.Length == 7 && fields[0] == countryCode)
                {
                    var regionCode = (fields[0] + "." + fields[4]).Trim();
                    var (regionName, _) = FindRegionName(regionCode);
                    
                    var city = new City
                    {
                        Country = countryCode,
                        Name = fields[1].Trim(),
                        GeoLat = fields[2].Trim(),
                        GeoLong = fields[3].Trim(),
                        Region = regionName,
                        Elevation = fields[5].Trim(),
                        IndicationTz = fields[6].Trim()
                    };
                    cities.Add(city);
                }
            }
            
            return (cities, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading cities file: {ex.Message}");
            return (new List<City>(), ex);
        }
    }

    // findRegionName looks up the name for a region, using the region code.
    private (string regionName, Exception? error) FindRegionName(string regionCode)
    {
        try
        {
            var lines = File.ReadAllLines(RegionsFile);
            
            foreach (var line in lines)
            {
                var fields = line.Split(ItemSeparator);
                if (fields.Length == 2 && fields[0] == regionCode)
                {
                    return (fields[1].Trim(), null);
                }
            }
            
            return ("", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading regions file: {ex.Message}");
            return ("", ex);
        }
    }
}

