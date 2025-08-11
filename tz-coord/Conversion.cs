namespace TzCoordCSharp;

public static class Conversion
{
    // ParseHmsFromText parses hours, minutes, seconds from text
    public static double ParseHmsFromText(string hourStr, string minStr, string secStr)
    {
        int.TryParse(hourStr.Trim(), out int hour);
        int.TryParse(minStr.Trim(), out int min);
        int.TryParse(secStr.Trim(), out int sec);

        return hour + min / 60.0 + sec / 3600.0;
    }

    // ParseDateTimeFromText parses date and time from text array
    public static (DateTimeHms dateTime, Exception? error) ParseDateTimeFromText(string[] items)
    {
        int.TryParse(items[0].Trim(), out int year);
        int.TryParse(items[1].Trim(), out int month);
        int.TryParse(items[2].Trim(), out int day);
        int.TryParse(items[3].Trim(), out int hour);
        int.TryParse(items[4].Trim(), out int min);
        int.TryParse(items[5].Trim(), out int sec);

        return (new DateTimeHms
        {
            Year = year,
            Month = month,
            Day = day,
            Hour = hour,
            Min = min,
            Sec = sec,
            Greg = true
        }, null);
    }

    // ParseSexTextFromFloat converts a float to sexagesimal text format
    public static string ParseSexTextFromFloat(double value)
    {
        int hours = (int)value;
        int minutes = (int)((value - hours) * 60);
        int seconds = (int)(((value - hours) * 60 - minutes) * 60);

        return $"{hours}:{minutes}:{seconds}".Replace(":", "").Replace("-", "").Replace("+", "");
    }
}

