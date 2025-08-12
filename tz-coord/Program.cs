/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma.
 *  Copyright (c) 2025, Jan Kampherbeek.
 *  Enigma is open source.
 */

namespace tz_coord;

class Program
{
    static void Main(string[] args)
    {
        CleanExistingFiles();
        Coordinates.HandleCoordinates();
        TimeZones.HandleTimeZones();
    }


    private static void CleanExistingFiles()
    {
        try
        {
            if (Directory.Exists(FilePaths.ResultsFolder))
            {
                Directory.Delete(FilePaths.ResultsFolder, true);
            }
            Directory.CreateDirectory(FilePaths.ResultsFolder);

            if (File.Exists(FilePaths.OutputFile1))
            {
                File.Delete(FilePaths.OutputFile1);
            }
            if (File.Exists(FilePaths.OutputFile2Tz))
            {
                File.Delete(FilePaths.OutputFile2Tz);
            }
            if (File.Exists(FilePaths.OutputFile2Dst))
            {
                File.Delete(FilePaths.OutputFile2Dst);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning existing files: {ex.Message}");
            throw;
        }
    }
}
