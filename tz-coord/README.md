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
Timezone

Lookup table for IANA timezone names, referenced by City to avoid repeating the string per row.

┌────────┬─────────┬──────────────────┐
│ Column │  Type   │   Constraints    │
├────────┼─────────┼──────────────────┤
│ id     │ INTEGER │ PRIMARY KEY      │
├────────┼─────────┼──────────────────┤
│ name   │ TEXT    │ NOT NULL, UNIQUE │
└────────┴─────────┴──────────────────┘

  ---
Country

┌──────────────┬──────┬──────────────────────────────────────────┐
│    Column    │ Type │               Constraints                │
├──────────────┼──────┼──────────────────────────────────────────┤
│ country_code │ TEXT │ PRIMARY KEY — ISO 2-letter code, e.g. NL │
├──────────────┼──────┼──────────────────────────────────────────┤
│ name         │ TEXT │ NOT NULL                                 │
├──────────────┼──────┼──────────────────────────────────────────┤
│ continent    │ TEXT │ NOT NULL — 2-letter code, e.g. EU        │
└──────────────┴──────┴──────────────────────────────────────────┘

  ---
Region

Administrative regions (states, provinces). Sourced from admin1CodesASCII.txt.

┌─────────────┬──────┬─────────────────────────────────────┐
│   Column    │ Type │             Constraints             │
├─────────────┼──────┼─────────────────────────────────────┤
│ region_code │ TEXT │ PRIMARY KEY — composite, e.g. NL.07 │
├─────────────┼──────┼─────────────────────────────────────┤
│ name        │ TEXT │ NOT NULL                            │
└─────────────┴──────┴─────────────────────────────────────┘

  ---
City

The main table. Contains one row per city name per language — the canonical name plus any distinct Dutch, German,
French and English translations from GeoNames alternate names.

┌──────────────┬─────────┬────────────────────────────────────────────────────┐
│    Column    │  Type   │                    Constraints                     │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ id           │ INTEGER │ PRIMARY KEY AUTOINCREMENT                          │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ country_code │ TEXT    │ NOT NULL, FK → Country                             │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ name         │ TEXT    │ NOT NULL                                           │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ latitude     │ REAL    │ NOT NULL                                           │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ longitude    │ REAL    │ NOT NULL                                           │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ region_code  │ TEXT    │ nullable — not enforced by FK as GeoNames has gaps │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ elevation    │ INTEGER │ nullable, metres                                   │
├──────────────┼─────────┼────────────────────────────────────────────────────┤
│ timezone_id  │ INTEGER │ NOT NULL, FK → Timezone                            │
└──────────────┴─────────┴────────────────────────────────────────────────────┘

Indexes: country_code, timezone_id, name COLLATE NOCASE

  ---
TzData

Historical timezone interval records parsed from the IANA timezone database Zone entries. Multiple rows per zone name,
each representing one historical interval.

┌─────────────┬─────────┬─────────────────────────────────────────────────────┐
│   Column    │  Type   │                     Constraints                     │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ id          │ INTEGER │ PRIMARY KEY AUTOINCREMENT                           │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ zone_name   │ TEXT    │ NOT NULL — IANA name, e.g. Europe/Amsterdam         │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ offset_h    │ INTEGER │ NOT NULL — UTC offset hours                         │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ offset_m    │ INTEGER │ NOT NULL — UTC offset minutes                       │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ offset_s    │ INTEGER │ NOT NULL — UTC offset seconds                       │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ rule        │ TEXT    │ NOT NULL — DST rule name or - for none              │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ format      │ TEXT    │ NOT NULL — abbreviation template, e.g. WE%sT        │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ until_year  │ INTEGER │ NOT NULL — 0 means this interval has no end         │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ until_month │ INTEGER │ NOT NULL                                            │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ until_day   │ TEXT    │ NOT NULL — integer or expression e.g. last6, 6>=1   │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ at_h        │ INTEGER │ NOT NULL DEFAULT 0 — time-of-day for the transition │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ at_m        │ INTEGER │ NOT NULL DEFAULT 0                                  │
├─────────────┼─────────┼─────────────────────────────────────────────────────┤
│ at_s        │ INTEGER │ NOT NULL DEFAULT 0                                  │
└─────────────┴─────────┴─────────────────────────────────────────────────────┘

Index: zone_name

  ---
DstData

DST rules parsed from the IANA timezone database Rule entries. Multiple rows per rule name, each covering a year
range.

┌───────────┬─────────┬─────────────────────────────────────────────────────────┐
│  Column   │  Type   │                       Constraints                       │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ id        │ INTEGER │ PRIMARY KEY AUTOINCREMENT                               │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ rule_name │ TEXT    │ NOT NULL — e.g. Algeria                                 │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ from_year │ INTEGER │ NOT NULL                                                │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ to_year   │ TEXT    │ NOT NULL — year number or only                          │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ month     │ INTEGER │ NOT NULL                                                │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ day       │ TEXT    │ NOT NULL — integer or expression e.g. last6, 6>=1       │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ at_h      │ INTEGER │ NOT NULL                                                │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ at_m      │ INTEGER │ NOT NULL                                                │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ at_s      │ INTEGER │ NOT NULL                                                │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ use_ut    │ TEXT    │ NOT NULL — u if time is UTC, n if local                 │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ save_h    │ INTEGER │ NOT NULL — DST offset to add                            │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ save_m    │ INTEGER │ NOT NULL                                                │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ save_s    │ INTEGER │ NOT NULL                                                │
├───────────┼─────────┼─────────────────────────────────────────────────────────┤
│ letter    │ TEXT    │ NOT NULL — substitution letter for format, e.g. S, D, - │
└───────────┴─────────┴─────────────────────────────────────────────────────────┘

Index: rule_name


## Prerequisites
tz-coord uses hardcoded filepaths which can be found in Domain.cs. 
You will need to adapt these for your own project.

## License

Same as the original project - Enigma is open source.
Copyright (c) Jan Kampherbeek.

