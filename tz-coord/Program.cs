/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma-AR.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

using tz_coord;

namespace TzCoordCSharp;

class Program
{
    static void Main(string[] args)
    {
        CleanExistingFiles();
        Coordinates.HandleCoordinates();
        TimeZones.HandleTimeZones();
    }


    public static void CleanExistingFiles()
    {
        try
        {
            if (Directory.Exists(FilePaths.resultsFolder))
            {
                Directory.Delete(FilePaths.resultsFolder, true);
            }
            Directory.CreateDirectory(FilePaths.resultsFolder);

            if (File.Exists(FilePaths.outputFile1))
            {
                File.Delete(FilePaths.outputFile1);
            }
            if (File.Exists(FilePaths.outputFile2Tz))
            {
                File.Delete(FilePaths.outputFile2Tz);
            }
            if (File.Exists(FilePaths.outputFile2Dst))
            {
                File.Delete(FilePaths.outputFile2Dst);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning existing files: {ex.Message}");
            throw;
        }
    }
}
