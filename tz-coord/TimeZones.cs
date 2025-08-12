/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma.
 *  Copyright (c) 2025, Jan Kampherbeek.
 *  Enigma is open source.
 */

namespace tz_coord;

    public static class TimeZones
    {
        private const string Sep = ";"; // separator to be used in csv
        
        public static void HandleTimeZones()
        {
            try
            {
                ReadInputFiles();
                SplitTzAndDst();
                FinalizeTz();
                FinalizeDst();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleTimeZones: {ex.Message}");
            }
        }

   

        private static void ReadInputFiles()
        {
            ProcessTzFiles(FilePaths.AfricaInputFile);
            ProcessTzFiles(FilePaths.AntarcticaInputFile);
            ProcessTzFiles(FilePaths.AsiaInputFile);
            ProcessTzFiles(FilePaths.AustralasiaInputFile);
            ProcessTzFiles(FilePaths.BackzoneInputFile);
            ProcessTzFiles(FilePaths.EuropeInputFile);
            ProcessTzFiles(FilePaths.NorthamericaInputFile);
            ProcessTzFiles(FilePaths.SouthamericaInputFile);
        }

        // splitTzAndDst read all lines from outputFile1 and writes tz lines to outputFile2Tz and
        // dst lines to outputFile2Dst
        private static void SplitTzAndDst()
        {
            try
            {
                using var inputFile = new StreamReader(FilePaths.OutputFile1);
                using var outputFileTz = new StreamWriter(FilePaths.OutputFile2Tz, true);
                using var outputFileDst = new StreamWriter(FilePaths.OutputFile2Dst, true);
                while (inputFile.ReadLine() is { } line)
                {
                    line = line.Trim();
                    // Check for empty lines
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    if (line.StartsWith("Rule")) // DST line
                    {
                        outputFileDst.WriteLine(line);
                    }
                    else // TZ line
                    {
                        outputFileTz.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SplitTzAndDst: {ex.Message}");
                throw;
            }
        }

        // finalizeTz writes time zone info in the final format to tzDataFile
        private static void FinalizeTz()
        {
            try
            {
                using var inputFile = new StreamReader(FilePaths.OutputFile2Tz);
                using var outputFile = new StreamWriter(FilePaths.TzDataFile, true);
                while (inputFile.ReadLine() is { } line)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    var tzLine = CreateTzLine(line);
                    outputFile.WriteLine(tzLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FinalizeTz: {ex.Message}");
                throw;
            }
        }

        private static void FinalizeDst()
        {
            try
            {
                using var inputFile = new StreamReader(FilePaths.OutputFile2Dst);
                using var outputFile = new StreamWriter(FilePaths.DstDataFile, true);
                while (inputFile.ReadLine() is { } line)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    var dstLine = CreateDstLine(line);
                    outputFile.WriteLine(dstLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FinalizeDst: {ex.Message}");
                throw;
            }
        }

        private static void ProcessTzFiles(string inputFilename)
        {
            try
            {
                using var inputFile = new StreamReader(inputFilename);
                using var outputFile = new StreamWriter(FilePaths.OutputFile1, true);
                while (inputFile.ReadLine() is { } line)
                {
                    line = line.Trim();
                    // Check for empty lines and comments
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    if (line.StartsWith('#'))
                    {
                        continue;
                    }
                    if (line.StartsWith("Link"))
                    {
                        continue;
                    }
                    if (line.Contains('#'))
                    {
                        line = line.Split('#')[0];
                    }
                    outputFile.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing tz file {inputFilename}: {ex.Message}");
                throw;
            }
        }

        private static string CreateTzLine(string line)
        {
            var csvLine = "";
            var strippedLine = line.Trim();
            // line can have a mix of tabs, spaces and multiple spaces
            strippedLine = strippedLine.Replace("\t", " ");
            strippedLine = strippedLine.Replace("     ", " "); // assuming max 5 spaces
            strippedLine = strippedLine.Replace("    ", " ");
            strippedLine = strippedLine.Replace("   ", " ");
            strippedLine = strippedLine.Replace("  ", " ");
            var items = strippedLine.Split(' ');

            if (items[0] == "Zone") // Top line for time zone
            {
                // process line with format: Zone	Africa/Algiers	0:12:12 -	LMT	1891 Mar 16
                // into format : Zone;Africa/Algiers;0;12;12;-;LMT;1891;3;16

                string h = "0", mi = "0", s = "0";
                var topItems = items[2].Split(':');
                if (topItems.Length > 0)
                {
                    h = topItems[0];
                }
                if (topItems.Length > 1)
                {
                    mi = topItems[1];
                }
                if (topItems.Length > 2)
                {
                    s = topItems[2];
                }
                string y = "0", mo = "1", d = "1";
                if (items.Length > 5)
                {
                    y = items[5];
                }
                if (items.Length > 6)
                {
                    var moValue = MonthIdFromText(items[6]);
                    mo = moValue;
                }
                if (items.Length > 7)
                {
                    d = items[7];
                }
                csvLine = items[0] + Sep + items[1] + Sep + h + Sep + mi + Sep + s + Sep + items[4] + Sep + y + Sep + mo + Sep + d;
            }
            else // definition line for time zone
            {
                // process lines with format: 0:09:21	-	PMT	1911 Mar 11
                //                            0:00	Algeria	WE%sT	1940 Feb 25  2:00
                // into:                      0;9;21;-;PMT;1911;3;11;0;0;0
                //                            0;0;0;Algeria;WE%sT;1940;2;25;2;0;0
                // alternative definition line
                // 3:00 RussiaAsia	%z	1992 Sep lastSun  2:00s
                // replace lastSun with last6

                string h = "0", mi = "0", s = "0";
                var topItems = items[0].Split(':');
                if (topItems.Length > 0)
                {
                    h = topItems[0];
                }
                if (topItems.Length > 1)
                {
                    mi = topItems[1];
                }
                if (topItems.Length > 2)
                {
                    s = topItems[2];
                }
                string y = "0", mo = "1", d = "1";
                if (items.Length > 3)
                {
                    y = items[3];
                }
                if (items.Length > 4)
                {
                    var moValue = MonthIdFromText(items[4]);
                    mo = moValue;
                }
                // handle day
                if (items.Length > 5)
                {
                    var dValue = ConstructDayOrRule(items[5]);
                    d = dValue;
                }
                string oh = "0", omi = "0", os = "0";
                if (items.Length > 6)
                {
                    var offsetItems = items[6].Split(':');
                    if (offsetItems.Length > 0)
                    {
                        oh = offsetItems[0];
                    }
                    if (offsetItems.Length > 1)
                    {
                        omi = offsetItems[1];
                    }
                    if (offsetItems.Length > 2)
                    {
                        os = offsetItems[2];
                    }
                }

                csvLine = h + Sep + mi + Sep + s + Sep + items[1] + Sep + items[2] + Sep + y + Sep + mo + Sep + d + Sep + oh + Sep + omi + Sep + os;
            }
            return csvLine;
        }

        private static string CreateDstLine(string line)
        {
            var csvLine = "";
            var strippedLine = line.Trim();
            // line can have a mix of tabs, spaces and multiple spaces
            strippedLine = strippedLine.Replace("\t", " ");
            strippedLine = strippedLine.Replace("     ", " "); // assuming max 5 spaces
            strippedLine = strippedLine.Replace("    ", " ");
            strippedLine = strippedLine.Replace("   ", " ");
            strippedLine = strippedLine.Replace("  ", " ");
            var items = strippedLine.Split(' ');

            // process lines with format: Rule	Algeria	1916	only	-	Jun	14	23:00s	1:00	S
            //                            Rule	Algeria	1916	1919	-	Oct	Sun>=1	23:00s	0	-
            // into:                      Algeria;1916;only;6;14;23;0;0;1;0;0;S
            //                            Algeria;1916;1919;10;6>=1;23;0;0;0;0;0;-

            string h = "0", mi = "0", s = "0", mo = "0", dayOrRule = "0";
            string oh = "0", omi = "0", os = "0";
            string hTest = "0", mTest = "0", sTest = "0";
            var useUt = "n";
            var letterItem = "";
            var toValue = items[3];
            if (toValue == "only")
            {
                toValue = items[2];
            }
            if (items.Length > 5)
            {
                var moValue = MonthIdFromText(items[5]);
                mo = moValue;
            }
            if (items.Length > 6)
            {
                var dorValue = items[6];
                var d = ConstructDayOrRule(dorValue);
                dayOrRule = d;
            }
            if (items.Length > 7)
            {
                var sPos = items[7].IndexOf('s');
                var startTimeText = items[7];
                if (sPos > 0)
                {
                    startTimeText = items[7].Substring(0, sPos);
                }
                var startTimeItems = startTimeText.Split(':');
                if (startTimeItems.Length > 0)
                {
                    hTest = startTimeItems[0];
                    if (hTest.Contains("u"))
                    {
                        useUt = "u";
                    }
                    h = hTest.Replace("u", "", (StringComparison)1);
                }
                if (startTimeItems.Length > 1)
                {
                    mTest = startTimeItems[1];
                    if (mTest.Contains("u"))
                    {
                        useUt = "u";
                    }
                    mi = mTest.Replace("u", "", (StringComparison)1);
                }
                if (startTimeItems.Length > 2)
                {
                    sTest = startTimeItems[2];
                    if (sTest.Contains("u"))
                    {
                        useUt = "u";
                    }
                    s = sTest.Replace("u", "", (StringComparison)1);
                }
            }
            if (items.Length > 8)
            {
                var offsetItems = items[8].Split(':');
                if (offsetItems.Length > 0)
                {
                    oh = offsetItems[0];
                }
                if (offsetItems.Length > 1)
                {
                    omi = offsetItems[1];
                }
                if (offsetItems.Length > 2)
                {
                    os = offsetItems[2];
                }
            }
            if (items.Length > 9)
            {
                letterItem = items[9];
            }
            csvLine = items[1] + Sep + items[2] + Sep + toValue + Sep + mo + Sep + dayOrRule + Sep + h + Sep + mi + Sep + s + Sep + useUt + Sep + oh + Sep + omi + Sep + os + Sep + letterItem;

            return csvLine;
        }

        private static string ConstructDayOrRule(string dorValue)
        {
            var dayOrRule = "error";
            if (dorValue.Contains(">="))
            {
                var pos = dorValue.IndexOf(">=", StringComparison.Ordinal);
                var dayTxt = dorValue.Substring(0, pos);
                var indexTxt = dorValue.Substring(pos + 2);
                var dayNr = NrFromDayText(dayTxt.Trim());
                dayOrRule = dayNr + ">=" + indexTxt;
            }
            else if (dorValue.Contains("<="))
            {
                var pos = dorValue.IndexOf("<=", StringComparison.Ordinal);
                var dayTxt = dorValue.Substring(0, pos);
                var indexTxt = dorValue.Substring(pos + 2);
                var dayNr = NrFromDayText(dayTxt.Trim());
                dayOrRule = dayNr + "<=" + indexTxt;
            }
            else if (dorValue.Contains("last"))
            {
                var dayTxt = dorValue[4..];
                var dayNr = NrFromDayText(dayTxt.Trim());
                dayOrRule = "last" + dayNr;
            }
            else
            {
                dayOrRule = dorValue;
            }
            return dayOrRule;
        }

        private static string NrFromDayText(string dayText)
        {
            switch (dayText)
            {
                case "Mon":
                    return "0";
                case "Tue":
                    return "1";
                case "Wed":
                    return "2";
                case "Thu":
                    return "3";
                case "Fri":
                    return "4";
                case "Sat":
                    return "5";
                case "Sun":
                    return "6";
                default:
                    throw new ArgumentException("day text is invalid");
            }
        }

        private static string MonthIdFromText(string month)
        {
            var monthId = "";
            switch (month)
            {
                case "Jan":
                    monthId = "1";
                    break;
                case "Feb":
                    monthId = "2";
                    break;
                case "Mar":
                    monthId = "3";
                    break;
                case "Apr":
                    monthId = "4";
                    break;
                case "May":
                    monthId = "5";
                    break;
                case "Jun":
                    monthId = "6";
                    break;
                case "Jul":
                    monthId = "7";
                    break;
                case "Aug":
                    monthId = "8";
                    break;
                case "Sep":
                    monthId = "9";
                    break;
                case "Oct":
                    monthId = "10";
                    break;
                case "Nov":
                    monthId = "11";
                    break;
                case "Dec":
                    monthId = "12";
                    break;
                default:
                    throw new ArgumentException("invalid month " + month);
            }
            return monthId;
        }
    }
