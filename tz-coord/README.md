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
- cities500.zip  (after unzip becomes cities500.txt)
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

## Prerequisites
tz-coord uses hardcoded filepaths which can be found in Domain.cs. 
You will need to adapt these for your own project.

## License

Same as the original project - Enigma is open source.
Copyright (c) Jan Kampherbeek.

