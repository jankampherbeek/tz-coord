# TzCoordCSharp

This is a C# translation of the original Go `tzcoord` project. The project handles coordinates and timezone calculations for the Enigma Astrology Research system.

## Project Structure

- **Program.cs** - Main entry point (corresponds to `main.go`)
- **Domain.cs** - Data structures for DateTime, Country, City (corresponds to `domain.go`)
- **JdCalculator.cs** - Julian Day calculations (corresponds to `jdcalculator.go`)
- **DayDefinition.cs** - Day definition calculations (corresponds to `daydefinition.go`)
- **Conversion.cs** - Utility conversion functions (corresponds to `conversion.go`)
- **Locations.cs** - Location handling for countries and cities (corresponds to `locations.go`)
- **Coordinates.cs** - Coordinate processing functionality (corresponds to `coordinates.go`)

## Key Differences from Go Version

1. **Error Handling**: Uses C# tuples `(result, Exception? error)` instead of Go's multiple return values
2. **Interfaces**: Uses C# interfaces with `I` prefix (e.g., `IDayDefHandler`)
3. **File Operations**: Uses C# `File.ReadAllLines()` and `File.WriteAllLines()` instead of Go's buffered I/O
4. **String Operations**: Uses C# string interpolation and LINQ methods
5. **Collections**: Uses C# `List<T>` instead of Go slices

## Building and Running

```bash
dotnet build
dotnet run
```

## Dependencies

- .NET 8.0
- No external NuGet packages required

## Data Files

The project expects the following data files in the `coord` directory:
- `cities500.txt` - City data
- `admin1CodesAscII.txt` - Region data  
- `countryinfo.txt` - Country data

Output files will be created in the `results` directory.

## License

Same as the original Go project - Enigma is open source.
Copyright (c) Jan Kampherbeek.

