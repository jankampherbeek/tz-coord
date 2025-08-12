# tz-coord

Tz-ciird (timezones and coordinates) is a helper project for Enigma. 
This project prepares timezone and coordinate files.

The coordinate files are read from tsnames.org adn the tz files from the IANA tz database at iana.org

Preparing the files has the following advantages:

- decreasing file size by omitting unnecessary details
- improving performance by pre-calculating times from strings
- increasing reliability by parsing and checking all lines
- decreasing complexity of finding data in the files in the main project enigma-ar
After a change in either the tnsnames data or the IANA tz data, it is sufficient to dowload the files and start enigma-tzcoord. The files and the application should be in the same directory.

## Prerequisites
tz-coord uses hardcoded filepaths which can be fouond in Domain.cs. 
You will need to adapt these for you own project.

## License

Same as the original project - Enigma is open source.
Copyright (c) Jan Kampherbeek.

