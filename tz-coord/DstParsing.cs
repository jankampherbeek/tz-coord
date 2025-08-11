/*
 *  Enigma Astrology Research.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using tz_code;
using TzCoordCSharp;

namespace tz_coord
{
    /// <summary>
    /// dstTextLine represents textual data for dst, the names of the fields correspond with definitions in the tz database
    /// </summary>
    public class DstElementsLine
    {
        public string Name { get; set; } = string.Empty;
        public int From { get; set; }
        public int To { get; set; }
        public int In { get; set; }
        public string On { get; set; } = string.Empty;
        public double At { get; set; }
        public double Save { get; set; }
        public string Letter { get; set; } = string.Empty;
    }

    public class DstLine
    {
        public double StartJd { get; set; }
        public double Offset { get; set; }
        public string Letter { get; set; } = string.Empty;
    }

    /// <summary>
    /// DstParser handles parsing dst lines.
    /// </summary>
    public interface IDstParser
    {
        List<DstLine> ProcessDstLines(string[] lines);
    }

    public class DstParsing : IDstParser
    {
        private readonly JdCalculator _jdCalc = new();
        private readonly IDayDefHandler _dayNrCalc = new DayDefHandling();

        public List<DstLine> ProcessDstLines(string[] lines)
        {
            var elementsLines = ParseDstElementsLines(lines);
            var processedLines = ParseDstLines(elementsLines);
            return processedLines;
        }

        private List<DstElementsLine> ParseDstElementsLines(string[] lines)
        {
            var parsedLines = new List<DstElementsLine>();
            
            foreach (var line in lines)
            {
                string dataLine = line;
                string[] items = dataLine.Split(';');
                
                if (items.Length < 12)
                {
                    throw new ArgumentException($"Invalid dataLine: {dataLine}");
                }

                if (!int.TryParse(items[1], out int from))
                {
                    throw new ArgumentException($"Invalid value for from in dataLine: {dataLine}");
                }

                if (!int.TryParse(items[2], out int to))
                {
                    throw new ArgumentException($"Invalid value for to in dataLine: {dataLine}");
                }

                if (!int.TryParse(items[3], out int inValue))
                {
                    throw new ArgumentException($"Invalid value for in in dataLine: {dataLine}");
                }

                var sdt = Conversion.ParseDateTimeFromText(items.Skip(6).Take(3).ToArray());
                double startTime = sdt.dateTime.Hour + sdt.dateTime.Min / 60.0 + sdt.dateTime.Sec / 3600.0;
                
                var oset = Conversion.ParseDateTimeFromText(items.Skip(8).Take(3).ToArray());
                double offset = oset.dateTime.Hour + oset.dateTime.Min / 60.0 + oset.dateTime.Sec / 3600.0;

                var dstLine = new DstElementsLine
                {
                    Name = items[0],
                    From = from,
                    To = to,
                    In = inValue,
                    On = items[4],
                    At = startTime,
                    Save = offset,
                    Letter = items[11]
                };
                
                parsedLines.Add(dstLine);
            }
            
            return parsedLines;
        }

        private List<DstLine> ParseDstLines(List<DstElementsLine> lines)
        {
            var parsedLines = new List<DstLine>();
            
            foreach (var line in lines)
            {
                int startYear = line.From;
                int endYear = line.To;
                
                for (int year = startYear; year <= endYear; year++)
                {
                    var newLine = CreateSingleDstLine(line);
                    parsedLines.Add(newLine);
                }
            }
            
            return parsedLines;
        }

        private DstLine CreateSingleDstLine(DstElementsLine line)
        {
            int day = _dayNrCalc.DayFromDefinition(line.From, line.In, line.On); // resp. year, month and day definition
            double jd = _jdCalc.CalcJd(line.From, line.In, day, line.At);
            
            return new DstLine
            {
                StartJd = jd,
                Offset = line.Save,
                Letter = line.Letter
            };
        }
    }
}
