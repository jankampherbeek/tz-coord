# tz-coord

Tz-coord (timezones and coordinates) is a helper project for Enigma. 
This project prepares timezone and coordinate files.

The coordinate files are read from geonames.org and the tz files from the IANA tz database at iana.org

Preparing the files has the following advantages:

- decreasing file size by omitting unnecessary details
- improving performance by pre-calculating times from strings
- increasing reliability by parsing and checking all lines
- decreasing complexity of finding data in the files in the main project enigma-ar
After a change in either the tnsnames data or the IANA tz data, it is sufficient to dowload the files and start enigma-tzcoord.
Put the tnsnames in the folder coord and the tz in the folder tz.

## Specification of the files
GeoNames.org. Select Free Gazetteer Data and download the files:
- allCountries.zip  (after unzip becomes allCountries.txt)
- alternateNamesV2.zip (after unzip becomes alternateNames.txt)
- countryInfo.txt 
- admin1CodesASCII.txt
- 
Iana.org: Select Time Zone Database and download the file:
- tzdata*.tar.gz (after unzipping and untarring becomes a folder).

Use the following files from the tzdata folder:
- africa
- antarctica
- asia
- australasia
- backzone
- europe
- northamerica
- southamerica

## Specification of the output files
The output files are:
- tz.txt
- dstdata.csv
- countries.csv
- regions.csv
- cities.csv

## Database
The database is a SQLite database. It uses the following tables:
---

Country

┌──────────────┬──────┬─────────────┐
│    Column    │ Type │ Constraints │
├──────────────┼──────┼─────────────┤
│ country_code │ TEXT │ PRIMARY KEY │
├──────────────┼──────┼─────────────┤
│ name         │ TEXT │ NOT NULL    │
├──────────────┼──────┼─────────────┤
│ continent    │ TEXT │ NOT NULL    │
└──────────────┴──────┴─────────────┘

One row per country. Holds the canonical (English) name and continent code. Used as the anchor for City foreign keys.

  ---
CountryName

┌──────────────┬─────────┬───────────────────────────┐
│    Column    │  Type   │        Constraints        │
├──────────────┼─────────┼───────────────────────────┤
│ id           │ INTEGER │ PRIMARY KEY AUTOINCREMENT │
├──────────────┼─────────┼───────────────────────────┤
│ country_code │ TEXT    │ NOT NULL, FK → Country    │
├──────────────┼─────────┼───────────────────────────┤
│ name         │ TEXT    │ NOT NULL                  │
└──────────────┴─────────┴───────────────────────────┘

All name variants per country: canonical English name plus translations in Dutch, German, French, and English. Use
this table when searching by name.
Indexes: country_code, name COLLATE NOCASE.

  ---
Region

┌─────────────┬──────┬─────────────┐
│   Column    │ Type │ Constraints │
├─────────────┼──────┼─────────────┤
│ region_code │ TEXT │ PRIMARY KEY │
├─────────────┼──────┼─────────────┤
│ name        │ TEXT │ NOT NULL    │
└─────────────┴──────┴─────────────┘

One row per administrative region (province/state). The code format is CC.XX (e.g. NL.07).

  ---
Timezone

┌────────┬─────────┬─────────────────┐
│ Column │  Type   │   Constraints   │
├────────┼─────────┼─────────────────┤
│ id     │ INTEGER │ PRIMARY KEY     │
├────────┼─────────┼─────────────────┤
│ name   │ TEXT    │ NOT NULL UNIQUE │
└────────┴─────────┴─────────────────┘

Distinct IANA timezone identifiers derived from the cities data (e.g. Europe/Amsterdam).

  ---
City

┌──────────────┬─────────┬───────────────────────────┐
│    Column    │  Type   │        Constraints        │
├──────────────┼─────────┼───────────────────────────┤
│ id           │ INTEGER │ PRIMARY KEY AUTOINCREMENT │
├──────────────┼─────────┼───────────────────────────┤
│ country_code │ TEXT    │ NOT NULL, FK → Country    │
├──────────────┼─────────┼───────────────────────────┤
│ name         │ TEXT    │ NOT NULL                  │
├──────────────┼─────────┼───────────────────────────┤
│ latitude     │ REAL    │ NOT NULL                  │
├──────────────┼─────────┼───────────────────────────┤
│ longitude    │ REAL    │ NOT NULL                  │
├──────────────┼─────────┼───────────────────────────┤
│ region_code  │ TEXT    │ FK → Region (nullable)    │
├──────────────┼─────────┼───────────────────────────┤
│ elevation    │ INTEGER │ nullable                  │
├──────────────┼─────────┼───────────────────────────┤
│ timezone_id  │ INTEGER │ NOT NULL, FK → Timezone   │
└──────────────┴─────────┴───────────────────────────┘

Multiple rows per city when translated names exist — each translation is a separate row with the same coordinates,
region, elevation, and timezone.
Indexes: country_code, timezone_id, name COLLATE NOCASE.

  ---
TzData

┌─────────────┬─────────┬───────────────────────────┐
│   Column    │  Type   │        Constraints        │
├─────────────┼─────────┼───────────────────────────┤
│ id          │ INTEGER │ PRIMARY KEY AUTOINCREMENT │
├─────────────┼─────────┼───────────────────────────┤
│ zone_name   │ TEXT    │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ offset_h    │ INTEGER │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ offset_m    │ INTEGER │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ offset_s    │ INTEGER │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ rule        │ TEXT    │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ format      │ TEXT    │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ until_year  │ INTEGER │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ until_month │ INTEGER │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ until_day   │ TEXT    │ NOT NULL                  │
├─────────────┼─────────┼───────────────────────────┤
│ at_h        │ INTEGER │ NOT NULL, DEFAULT 0       │
├─────────────┼─────────┼───────────────────────────┤
│ at_m        │ INTEGER │ NOT NULL, DEFAULT 0       │
├─────────────┼─────────┼───────────────────────────┤
│ at_s        │ INTEGER │ NOT NULL, DEFAULT 0       │
└─────────────┴─────────┴───────────────────────────┘

Historical UTC offset records from the IANA timezone database. Multiple rows per zone_name, ordered implicitly by the
until_* fields. Index on zone_name.

  ---
DstData

┌───────────┬─────────┬───────────────────────────┐
│  Column   │  Type   │        Constraints        │
├───────────┼─────────┼───────────────────────────┤
│ id        │ INTEGER │ PRIMARY KEY AUTOINCREMENT │
├───────────┼─────────┼───────────────────────────┤
│ rule_name │ TEXT    │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ from_year │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ to_year   │ TEXT    │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ month     │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ day       │ TEXT    │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ at_h      │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ at_m      │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ at_s      │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ use_ut    │ TEXT    │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ save_h    │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ save_m    │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ save_s    │ INTEGER │ NOT NULL                  │
├───────────┼─────────┼───────────────────────────┤
│ letter    │ TEXT    │ NOT NULL                  │
└───────────┴─────────┴───────────────────────────┘

DST (daylight saving time) rules from the IANA database. to_year is TEXT because it can hold the value "only" or "max"
in addition to a year number. day is TEXT to accommodate expressions like lastSun (stored as last6) or Sun>=1 (stored
as 6>=1). Index on rule_name.



## Prerequisites
tz-coord uses hardcoded filepaths which can be found in Domain.cs. 
You will need to adapt these for your own project.

## License

Same as the original project - Enigma is open source.
Copyright (c) Jan Kampherbeek.

