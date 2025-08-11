/*
 *  Enigma Astrology Research.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using tz_code;
using TzCoordCSharp;

namespace tz_coord
{
    public class DstInfo
    {
        public double Offset { get; set; }
        public string Letter { get; set; } = string.Empty;
    }

    public interface IDstHandler
    {
        DstInfo CurrentDst(DateTimeHms dateTime, string dstRule);
    }

    public class DstHandling : IDstHandler
    {
        private readonly JdCalculator _jdCalc;
        private readonly IDayDefHandler _dayNrCalc;
        private readonly IDstParser _dstLinesParser;
        private static readonly string FilePathRules = $"..{PathSep.Separator}..{PathSep.Separator}data{PathSep.Separator}rules.csv";

        public DstHandling()
        {
            _jdCalc = new JdCalculator();
            _dayNrCalc = new DayDefHandling();
            _dstLinesParser = new DstParsing();
        }

        public DstInfo CurrentDst(DateTimeHms dateTime, string dstRule)
        {
            var emptyDstInfo = new DstInfo
            {
                Offset = 0,
                Letter = string.Empty
            };

            var dstLines = DstData(dstRule);
            if (dstLines == null || !dstLines.Any())
            {
                return emptyDstInfo;
            }

            dstLines = dstLines.OrderBy(x => x.StartJd).ToList();
            
            double clockTime = dateTime.Hour + dateTime.Min / 60.0 + dateTime.Sec / 3600.0;
            double jd = _jdCalc.CalcJd(dateTime.Year, dateTime.Month, dateTime.Day, clockTime);
            
            if (jd < dstLines[0].StartJd)
            {
                return emptyDstInfo;
            }

            var actDstLine = dstLines[0];
            var prevDstLine = dstLines[0];
            
            foreach (var line in dstLines)
            {
                if (line.StartJd < jd)
                {
                    actDstLine = prevDstLine;
                }
                prevDstLine = line;
            }

            return new DstInfo
            {
                Offset = actDstLine.Offset,
                Letter = actDstLine.Letter
            };
        }

        private List<DstLine> DstData(string dstRule)
        {
            try
            {
                var dstTxtLines = ReadDstLines(dstRule);
                var processedLines = _dstLinesParser.ProcessDstLines(dstTxtLines.ToArray());
                return processedLines;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing DST data: {ex.Message}");
                return new List<DstLine>();
            }
        }

        private List<string> ReadDstLines(string ruleName)
        {
            var dstTxtLines = new List<string>();
            
            try
            {
                using (var dstFile = File.OpenText(FilePathRules))
                {
                    string? line;
                    while ((line = dstFile.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith(ruleName))
                        {
                            dstTxtLines.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading DST file: {ex.Message}");
            }
            
            return dstTxtLines;
        }
    }
}
