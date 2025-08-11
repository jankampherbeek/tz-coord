namespace TzCoordCSharp;

// Struct for Julian Day calculations
public class JdCalculator
{
    // NewJdCalculator creates a new JdCalculator instance
    public static JdCalculator NewJdCalculator()
    {
        return new JdCalculator();
    }

    // CalcJd calculates the Julian Day Number for a given Gregorian calendar date
    public double CalcJd(int year, int month, int day, double time)
    {
        if (month <= 2)
        {
            year--;
            month += 12;
        }
        int a = year / 100;
        int b = 2 - a + (a / 4);

        double jd = (int)(365.25 * (year + 4716)) +
                   (int)(30.6001 * (month + 1)) +
                   day + b - 1524.5 + time;

        return jd;
    }

    // Calculates the day of the week from a Julian Day Number
    // Returns 0 for Monday, 1 for Tuesday, ..., 6 for Sunday
    public int DowCalc(double jd)
    {
        int dow = ((int)jd + 1) % 7;
        return dow;
    }
}

