/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma-AR.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

namespace TzCoordCSharp;

class Program
{
    static void Main(string[] args)
    {
        Coordinates.HandleCoordinates();
        TimeZones.HandleTimeZones();
    }
}
