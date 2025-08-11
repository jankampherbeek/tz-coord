/*
 *  Enigma Astrology Research.
 *  Copyright (c) Jan Kampherbeek.
 *  Enigma is open source.
 *  Please check the file copyright.txt in the root of the source for further details.
 */

using TzCoordCSharp;

namespace tz_code;

public static class DayDefinitionConstants
{
    public const string PfLast = "last";
    public const string PfGE1 = ">=1";
    public const string PfGE2 = ">=2";
}

// DayDefHandler handles defining a date based on a definition.
public interface IDayDefHandler
{
    int DayFromDefinition(int year, int month, string def);
}

public class DayDefHandling : IDayDefHandler
{
    private readonly JdCalculator _jdCalc;

    public DayDefHandling()
    {
        _jdCalc = JdCalculator.NewJdCalculator();
    }

    public static IDayDefHandler NewDayDefHandling()
    {
        return new DayDefHandling();
    }

    // DayFromDefinition calculates the day number for a given definition in a given year and month.
    public int DayFromDefinition(int year, int month, string def)
    {
        string? defDay = null;
        string? defType = null;

        if (def.Length <= 2)
        {
            if (int.TryParse(def, out int preDefinedDay))
            {
                return (preDefinedDay);
            }
            throw new Exception($"Error: could not parse day: {def}");

        }

        if (def.Contains(DayDefinitionConstants.PfLast))
        {
            defDay = def[4..];
            defType = DayDefinitionConstants.PfLast;
        }
        else if (def.Contains(DayDefinitionConstants.PfGE1))
        {
            int index = def.IndexOf(DayDefinitionConstants.PfGE1);
            defDay = def[(index - 1)..index];
            defType = DayDefinitionConstants.PfGE1;
        }
        else if (def.Contains(DayDefinitionConstants.PfGE2))
        {
            int index = def.IndexOf(DayDefinitionConstants.PfGE2);
            defDay = def[(index - 1)..index];
            defType = DayDefinitionConstants.PfGE2;
        }
        else
        {
            defType = "Unknown defDay";
        }

        if (!int.TryParse(defDay, out int switchDay))
        {
            throw new Exception($"Error: could not parse defDay: {def}");
        }

        double jd = _jdCalc.CalcJd(year, month, 1, 12.0); // jd for first day of month
        int firstDOW = _jdCalc.DowCalc(jd); // index for first day of month, Mon=0...Sun=7
        int actualDay;

        switch (defType)
        {
            case DayDefinitionConstants.PfLast:
                int[] m31 = { 1, 3, 5, 7, 8, 10, 12 };
                if (Contains(m31, month))
                {
                    int lastDayOfMonth = firstDOW + 30;
                    int diff = lastDayOfMonth % 7 - switchDay;
                    if (diff < 0)
                    {
                        diff += 7;
                    }
                    actualDay = 31 - diff;
                }
                else // assuming the last days of February are never used for a DST switch
                {
                    int lastDayOfMonth = firstDOW + 29;
                    int diff = lastDayOfMonth % 7 - switchDay;
                    if (diff < 0)
                    {
                        diff += 7;
                    }
                    actualDay = 30 - diff;
                }
                break;

            case DayDefinitionConstants.PfGE1:
                int diff1 = switchDay - firstDOW;
                actualDay = 1 + diff1;
                break;

            case DayDefinitionConstants.PfGE2:
                int diff2 = switchDay - firstDOW;
                actualDay = 8 + diff2;
                break;

            default:
                throw new Exception($"Error: unknown def type: {def}");
        }

        return actualDay;
    }

    // contains is a helper function for dayFromdefinition()
    private bool Contains(int[] numbers, int num)
    {
        return numbers.Contains(num);
    }
}

