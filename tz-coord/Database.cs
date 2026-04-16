/*
 *  Enigma - Coordinates and Timezones.
 *  A project that supports Enigma.
 *  Copyright (c) 2026, Jan Kampherbeek.
 *  Enigma is open source.
 */

using Microsoft.Data.Sqlite;

namespace tz_coord;

public static class Database
{
    public static void BuildDatabase()
    {
        try
        {
            if (File.Exists(FilePaths.DatabaseFile))
                File.Delete(FilePaths.DatabaseFile);

            using var conn = new SqliteConnection($"Data Source={FilePaths.DatabaseFile}");
            conn.Open();

            CreateSchema(conn);

            using var tx = conn.BeginTransaction();
            LoadCountries(conn);
            LoadRegions(conn);
            var timezoneIds = LoadTimezones(conn);
            LoadCities(conn, timezoneIds);
            LoadTzData(conn);
            LoadDstData(conn);
            tx.Commit();

            Console.WriteLine("Building database completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building database: {ex.Message}");
        }
    }

    private static void CreateSchema(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            PRAGMA journal_mode = WAL;
            PRAGMA foreign_keys = ON;

            CREATE TABLE Country (
                country_code TEXT PRIMARY KEY,
                name         TEXT NOT NULL,
                continent    TEXT NOT NULL
            );

            CREATE TABLE CountryName (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                country_code TEXT    NOT NULL REFERENCES Country(country_code),
                name         TEXT    NOT NULL
            );

            CREATE INDEX idx_countryname_code ON CountryName(country_code);
            CREATE INDEX idx_countryname_name ON CountryName(name COLLATE NOCASE);

            CREATE TABLE Region (
                region_code TEXT PRIMARY KEY,
                name        TEXT NOT NULL
            );

            CREATE TABLE Timezone (
                id   INTEGER PRIMARY KEY,
                name TEXT    NOT NULL UNIQUE
            );

            CREATE TABLE City (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                country_code TEXT    NOT NULL REFERENCES Country(country_code),
                name         TEXT    NOT NULL,
                latitude     REAL    NOT NULL,
                longitude    REAL    NOT NULL,
                region_code  TEXT,
                elevation    INTEGER,
                timezone_id  INTEGER NOT NULL REFERENCES Timezone(id)
            );

            CREATE TABLE TzData (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                zone_name   TEXT    NOT NULL,
                offset_h    INTEGER NOT NULL,
                offset_m    INTEGER NOT NULL,
                offset_s    INTEGER NOT NULL,
                rule        TEXT    NOT NULL,
                format      TEXT    NOT NULL,
                until_year  INTEGER NOT NULL,
                until_month INTEGER NOT NULL,
                until_day   TEXT    NOT NULL,
                at_h        INTEGER NOT NULL DEFAULT 0,
                at_m        INTEGER NOT NULL DEFAULT 0,
                at_s        INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE DstData (
                id        INTEGER PRIMARY KEY AUTOINCREMENT,
                rule_name TEXT    NOT NULL,
                from_year INTEGER NOT NULL,
                to_year   TEXT    NOT NULL,
                month     INTEGER NOT NULL,
                day       TEXT    NOT NULL,
                at_h      INTEGER NOT NULL,
                at_m      INTEGER NOT NULL,
                at_s      INTEGER NOT NULL,
                use_ut    TEXT    NOT NULL,
                save_h    INTEGER NOT NULL,
                save_m    INTEGER NOT NULL,
                save_s    INTEGER NOT NULL,
                letter    TEXT    NOT NULL
            );

            CREATE INDEX idx_city_country   ON City(country_code);
            CREATE INDEX idx_city_timezone  ON City(timezone_id);
            CREATE INDEX idx_city_name      ON City(name COLLATE NOCASE);
            CREATE INDEX idx_tzdata_zone   ON TzData(zone_name);
            CREATE INDEX idx_dstdata_rule  ON DstData(rule_name);
            """;
        cmd.ExecuteNonQuery();
    }

    private static void LoadCountries(SqliteConnection conn)
    {
        var lines = File.ReadAllLines(FilePaths.CountryOutputFile);

        // INSERT into Country — one row per country code (first occurrence = canonical name)
        using var cmdCountry = conn.CreateCommand();
        cmdCountry.CommandText = "INSERT OR IGNORE INTO Country(country_code, name, continent) VALUES($code, $name, $continent)";
        var pCode      = cmdCountry.Parameters.Add("$code",      SqliteType.Text);
        var pName      = cmdCountry.Parameters.Add("$name",      SqliteType.Text);
        var pContinent = cmdCountry.Parameters.Add("$continent", SqliteType.Text);

        // INSERT into CountryName — every row (canonical + translated)
        using var cmdName = conn.CreateCommand();
        cmdName.CommandText = "INSERT INTO CountryName(country_code, name) VALUES($code, $name)";
        var pNCode = cmdName.Parameters.Add("$code", SqliteType.Text);
        var pNName = cmdName.Parameters.Add("$name", SqliteType.Text);

        foreach (var line in lines)
        {
            var f = line.Split(';');
            if (f.Length < 3) continue;
            pCode.Value      = f[0];
            pName.Value      = f[1];
            pContinent.Value = f[2];
            cmdCountry.ExecuteNonQuery(); // silently skips duplicates

            pNCode.Value = f[0];
            pNName.Value = f[1];
            cmdName.ExecuteNonQuery();
        }
        Console.WriteLine("Loaded countries into database");
    }

    private static void LoadRegions(SqliteConnection conn)
    {
        var lines = File.ReadAllLines(FilePaths.RegionsOutputFile);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Region(region_code, name) VALUES($code, $name)";
        var pCode = cmd.Parameters.Add("$code", SqliteType.Text);
        var pName = cmd.Parameters.Add("$name", SqliteType.Text);

        foreach (var line in lines)
        {
            var f = line.Split(';');
            if (f.Length < 2) continue;
            pCode.Value = f[0];
            pName.Value = f[1];
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Loaded regions into database");
    }

    private static Dictionary<string, int> LoadTimezones(SqliteConnection conn)
    {
        // Collect all distinct timezone names from the cities file first
        var names = File.ReadAllLines(FilePaths.CitiesRegionsOutputFile)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Split(';'))
            .Where(f => f.Length >= 7)
            .Select(f => f[6])
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n)
            .ToList();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Timezone(id, name) VALUES($id, $name)";
        var pId   = cmd.Parameters.Add("$id",   SqliteType.Integer);
        var pName = cmd.Parameters.Add("$name", SqliteType.Text);

        var lookup = new Dictionary<string, int>(StringComparer.Ordinal);
        var id = 1;
        foreach (var name in names)
        {
            pId.Value   = id;
            pName.Value = name;
            cmd.ExecuteNonQuery();
            lookup[name] = id;
            id++;
        }
        Console.WriteLine($"Loaded {lookup.Count} timezones into database");
        return lookup;
    }

    private static void LoadCities(SqliteConnection conn, Dictionary<string, int> timezoneIds)
    {
        var lines = File.ReadAllLines(FilePaths.CitiesRegionsOutputFile);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO City(country_code, name, latitude, longitude, region_code, elevation, timezone_id)
            VALUES($country, $name, $lat, $lon, $region, $elev, $tz)
            """;
        var pCountry = cmd.Parameters.Add("$country", SqliteType.Text);
        var pName    = cmd.Parameters.Add("$name",    SqliteType.Text);
        var pLat     = cmd.Parameters.Add("$lat",     SqliteType.Real);
        var pLon     = cmd.Parameters.Add("$lon",     SqliteType.Real);
        var pRegion  = cmd.Parameters.Add("$region",  SqliteType.Text);
        var pElev    = cmd.Parameters.Add("$elev",    SqliteType.Integer);
        var pTz      = cmd.Parameters.Add("$tz",      SqliteType.Integer);

        foreach (var line in lines)
        {
            // format: country;name;lat;lon;regionCode;elevation;timezone
            var f = line.Split(';');
            if (f.Length < 7) continue;

            var countryCode = f[0];
            var rawRegion   = f[4];
            pCountry.Value = countryCode;
            pName.Value    = f[1];
            pLat.Value     = double.TryParse(f[2], System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 0.0;
            pLon.Value     = double.TryParse(f[3], System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : 0.0;
            pRegion.Value  = string.IsNullOrEmpty(rawRegion) ? DBNull.Value : $"{countryCode}.{rawRegion}";
            pElev.Value    = int.TryParse(f[5], out var elev) ? elev : DBNull.Value;
            pTz.Value      = timezoneIds[f[6]];
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Loaded cities into database");
    }

    private static void LoadTzData(SqliteConnection conn)
    {
        var lines = File.ReadAllLines(FilePaths.TzDataFile);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO TzData(zone_name, offset_h, offset_m, offset_s, rule, format,
                               until_year, until_month, until_day, at_h, at_m, at_s)
            VALUES($zone, $oh, $om, $os, $rule, $fmt, $uy, $um, $ud, $ah, $am, $as)
            """;
        var pZone = cmd.Parameters.Add("$zone", SqliteType.Text);
        var pOh   = cmd.Parameters.Add("$oh",   SqliteType.Integer);
        var pOm   = cmd.Parameters.Add("$om",   SqliteType.Integer);
        var pOs   = cmd.Parameters.Add("$os",   SqliteType.Integer);
        var pRule = cmd.Parameters.Add("$rule", SqliteType.Text);
        var pFmt  = cmd.Parameters.Add("$fmt",  SqliteType.Text);
        var pUy   = cmd.Parameters.Add("$uy",   SqliteType.Integer);
        var pUm   = cmd.Parameters.Add("$um",   SqliteType.Integer);
        var pUd   = cmd.Parameters.Add("$ud",   SqliteType.Text);
        var pAh   = cmd.Parameters.Add("$ah",   SqliteType.Integer);
        var pAm   = cmd.Parameters.Add("$am",   SqliteType.Integer);
        var pAs   = cmd.Parameters.Add("$as",   SqliteType.Integer);

        // Zone header lines carry the zone name; continuation lines inherit it.
        var currentZone = "";

        foreach (var line in lines)
        {
            var f = line.Split(';');

            if (f[0] == "Zone")
            {
                // format: Zone;zone_name;oh;om;os;rule;fmt;until_year;until_month;until_day
                if (f.Length < 10) continue;
                currentZone = f[1];
                pZone.Value = currentZone;
                pOh.Value   = IntOrZero(f[2]);
                pOm.Value   = IntOrZero(f[3]);
                pOs.Value   = IntOrZero(f[4]);
                pRule.Value = f[5];
                pFmt.Value  = f[6];
                pUy.Value   = IntOrZero(f[7]);
                pUm.Value   = IntOrZero(f[8]);
                pUd.Value   = f[9];
                pAh.Value   = 0; pAm.Value = 0; pAs.Value = 0;
            }
            else
            {
                // format: oh;om;os;rule;fmt;until_year;until_month;until_day;at_h;at_m;at_s
                if (f.Length < 11) continue;
                pZone.Value = currentZone;
                pOh.Value   = IntOrZero(f[0]);
                pOm.Value   = IntOrZero(f[1]);
                pOs.Value   = IntOrZero(f[2]);
                pRule.Value = f[3];
                pFmt.Value  = f[4];
                pUy.Value   = IntOrZero(f[5]);
                pUm.Value   = IntOrZero(f[6]);
                pUd.Value   = f[7];
                pAh.Value   = IntOrZero(f[8]);
                pAm.Value   = IntOrZero(f[9]);
                pAs.Value   = IntOrZero(f[10]);
            }
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Loaded timezone data into database");
    }

    private static void LoadDstData(SqliteConnection conn)
    {
        var lines = File.ReadAllLines(FilePaths.DstDataFile);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO DstData(rule_name, from_year, to_year, month, day,
                                at_h, at_m, at_s, use_ut, save_h, save_m, save_s, letter)
            VALUES($rule, $from, $to, $mo, $day, $ah, $am, $as, $ut, $sh, $sm, $ss, $letter)
            """;
        var pRule   = cmd.Parameters.Add("$rule",   SqliteType.Text);
        var pFrom   = cmd.Parameters.Add("$from",   SqliteType.Integer);
        var pTo     = cmd.Parameters.Add("$to",     SqliteType.Text);
        var pMo     = cmd.Parameters.Add("$mo",     SqliteType.Integer);
        var pDay    = cmd.Parameters.Add("$day",    SqliteType.Text);
        var pAh     = cmd.Parameters.Add("$ah",     SqliteType.Integer);
        var pAm     = cmd.Parameters.Add("$am",     SqliteType.Integer);
        var pAs     = cmd.Parameters.Add("$as",     SqliteType.Integer);
        var pUt     = cmd.Parameters.Add("$ut",     SqliteType.Text);
        var pSh     = cmd.Parameters.Add("$sh",     SqliteType.Integer);
        var pSm     = cmd.Parameters.Add("$sm",     SqliteType.Integer);
        var pSs     = cmd.Parameters.Add("$ss",     SqliteType.Integer);
        var pLetter = cmd.Parameters.Add("$letter", SqliteType.Text);

        foreach (var line in lines)
        {
            // format: rule_name;from_year;to_year;month;day;at_h;at_m;at_s;use_ut;save_h;save_m;save_s;letter
            var f = line.Split(';');
            if (f.Length < 13) continue;
            pRule.Value   = f[0];
            pFrom.Value   = IntOrZero(f[1]);
            pTo.Value     = f[2];
            pMo.Value     = IntOrZero(f[3]);
            pDay.Value    = f[4];
            pAh.Value     = IntOrZero(f[5]);
            pAm.Value     = IntOrZero(f[6]);
            pAs.Value     = IntOrZero(f[7]);
            pUt.Value     = f[8];
            pSh.Value     = IntOrZero(f[9]);
            pSm.Value     = IntOrZero(f[10]);
            pSs.Value     = IntOrZero(f[11]);
            pLetter.Value = f[12];
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Loaded DST data into database");
    }

    private static int IntOrZero(string s) =>
        int.TryParse(s.Trim(), out var v) ? v : 0;
}
